#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <unistd.h>
#include "vm.h"

// Port for block reads. Each read or write has to be done in 5 IO writes:
// 1 - r/w. 1 for read, 2 for write.
// 2 - blkid MSB
// 3 - blkid LSB
// 4 - dest addr MSB
// 5 - dest addr LSB
#define BLK_PORT 0x03
#define CHKPS(cnt) if (!chkPS(cnt)) return;

static VM vm;
static uint64_t blkop = 0; // 5 bytes
static FILE *blkfp;

// Read single byte from I/O handler, if set. addr is a word only because of
// Forth's cell size, but can't actually address more than a byte-ful of ports.
static byte io_read(word addr)
{
    addr &= 0xff;
    IORD fn = vm.iord[addr];
    if (fn != NULL) {
        return fn();
    } else {
        fprintf(stderr, "Out of bounds I/O read: %d\n", addr);
        return 0;
    }
}

static void io_write(word addr, byte val)
{
    addr &= 0xff;
    IOWR fn = vm.iowr[addr];
    if (fn != NULL) {
        fn(val);
    } else {
        fprintf(stderr, "Out of bounds I/O write: %d / %d (0x%x)\n", addr, val, val);
    }
}

// I/O hook to read/write a chunk of 1024 byte to blkfs at specified blkid.
// This is used by EFS@ and EFS! in xcomp.fs.
// See comment above BLK_PORT define for poking convention.
static void iowr_blk(byte val)
{
    blkop <<= 8;
    blkop |= val;
    byte rw = blkop >> 32;
    if (rw) {
        word blkid = (blkop >> 16);
        word dest = blkop & 0xffff;
        blkop = 0;
        fseek(blkfp, blkid*1024, SEEK_SET);
        if (rw==2) { // write
            fwrite(&vm.mem[dest], 1024, 1, blkfp);
        } else { // read
            fread(&vm.mem[dest], 1024, 1, blkfp);
        }
    }
}

// get/set word from/to memory
static word gw(word addr) { return vm.mem[addr+(word)1] << 8 | vm.mem[addr]; }
static void sw(word addr, word val) {
    vm.mem[addr] = val;
    vm.mem[addr+(word)1] = val >> 8;
}
// pop word from SP
static word pop() {
    return vm.mem[vm.SP++] | vm.mem[vm.SP++] << 8;
}
word VM_PS_pop() { return pop(); }

// push word to SP
static void push(word x) {
    vm.SP -= 2;
    sw(vm.SP, x);
    if (vm.SP < vm.minSP) { vm.minSP = vm.SP; }
}
void VM_PS_push(word n) { push(n); }
// pop word from RS
static word popRS() {
    word x = gw(vm.RS); vm.RS -= 2; return x;
}
// push word to RS
static void pushRS(word val) {
    vm.RS += 2;
    sw(vm.RS, val);
    if (vm.RS > vm.maxRS) { vm.maxRS = vm.RS; }
}

// The functions below directly map to native forth words defined in the
// dictionary (doc/dict.txt)
static void execute(word wordref) {
    byte wtype = vm.mem[wordref];
    switch (wtype) {
        case 0: // native
        vm.nativew[vm.mem[wordref+(word)1]]();
        break;

        case 1: // compiled
        pushRS(vm.IP);
        vm.IP = wordref+1;
        if (vm.SP <= vm.RS) {
            vm.SP = SP_ADDR; vm.RS = RS_ADDR;
            execute(gw(0x13)); /* oflw */
        }
        break;

        case 2: // cell
        push(wordref+1);
        break;

        case 3: // does
        push(wordref+3);
        pushRS(vm.IP);
        vm.IP = gw(wordref+1);
        break;

        case 4: // alias
        execute(gw(wordref+1));
        break;

        case 5: // switch
        execute(gw(gw(wordref+1)));
        break;

        case 6: // constant
        push(gw(wordref+1));
        break;
    }
}

static bool chkPS(int cnt) {
    if (vm.SP > (SP_ADDR-cnt*2)) {
        execute(gw(0x06)); /* uflw */
        return false;
    } else {
        return true;
    }
}

static void chkOFLW() {
    if (vm.SP <= vm.RS) {
        vm.SP = SP_ADDR; vm.RS = RS_ADDR;
        execute(gw(0x13)); /* oflw */
    }
}

static word find(word daddr, word waddr) {
    byte len = vm.mem[waddr];
    waddr++;
    while (1) {
        if ((vm.mem[daddr-(word)1] & 0x7f) == len) {
            word d = daddr-3-len;
            // Sanity check
            if ((waddr+len >= MEMSIZE) || (d+len) >= MEMSIZE) return 0;
            if (strncmp(&vm.mem[waddr], &vm.mem[d], len) == 0) {
                return daddr;
            }
        }
        daddr -= 3;
        word offset = gw(daddr);
        if (offset) {
            daddr -= offset;
        } else {
            return 0;
        }
    }
}

static void EXIT() { vm.IP = popRS(); }
static void _br_() {
    word off = vm.mem[vm.IP];
    if (off > 0x7f ) { off -= 0x100; }
    vm.IP += off;
}
static void _cbr_() { CHKPS(1) if (!pop()) { _br_(); } else { vm.IP++; } }
static void _loop_() {
    word I = gw(vm.RS); I++; sw(vm.RS, I);
    if (I == gw(vm.RS-2)) { // don't branch
        popRS(); popRS();
        vm.IP++;
    } else { // branch
        _br_();
    }
}
static void SP_to_R_2() { CHKPS(2) word x = pop(); pushRS(pop()); pushRS(x); }
static void blit() { push(vm.mem[vm.IP]); vm.IP++; chkOFLW(); }
static void nlit() { push(gw(vm.IP)); vm.IP += 2; chkOFLW(); }
static void slit() { push(vm.IP); vm.IP += vm.mem[vm.IP] + 1; chkOFLW(); }
static void SP_to_R() { CHKPS(1) pushRS(pop()); }
static void R_to_SP() { push(popRS()); }
static void R_to_SP_2() { word x = popRS(); push(popRS()); push(x); }
static void EXECUTE() { CHKPS(1) execute(pop()); }
static void ROT() { CHKPS(3) // a b c -- b c a
    word c = pop(); word b = pop(); word a = pop();
    push(b); push(c); push(a);
}
static void ROTR() { CHKPS(3) // a b c -- c a b
    word c = pop(); word b = pop(); word a = pop();
    push(c); push(a); push(b);
}
static void DUP() { CHKPS(1) // a -- a a
    word a = pop(); push(a); push(a); chkOFLW();
}
static void CDUP() { CHKPS(1)
    word a = pop(); push(a); if (a) { push(a); chkOFLW(); }
}
static void DROP() { CHKPS(1) pop(); }
static void SWAP() { CHKPS(2) // a b -- b a
    word b = pop(); word a = pop();
    push(b); push(a);
}
static void OVER() { CHKPS(2) // a b -- a b a
    word b = pop(); word a = pop();
    push(a); push(b); push(a); chkOFLW();
}
static void AND() { CHKPS(2) push(pop() & pop()); }
static void OR() { CHKPS(2) push(pop() | pop()); }
static void XOR() { CHKPS(2) push(pop() ^ pop()); }
static void NOT() { CHKPS(1) push(!pop()); }
static void PLUS() { CHKPS(2)
    int b = pop(); int a = pop(); int n = a + b;
    vm.carry = n >= 0x10000; push((word)n);
}
static void MINUS() { CHKPS(2)
    int b = pop(); int a = pop(); int n = a - b;
    vm.carry = n < 0; push((word)n);
}
static void MULT() { CHKPS(2)
    int b = pop(); int a = pop(); int n = a * b;
    vm.carry = n >= 0x10000; push((word)n);
}
static void DIVMOD() { CHKPS(2)
    word b = pop(); word a = pop();
    push(a % b); push(a / b);
}
static void STORE() { CHKPS(2)
    word a = pop(); word val = pop();
    sw(a, val);
}
static void FETCH() { CHKPS(1) push(gw(pop())); }
static void CSTORE() { CHKPS(2)
    word a = pop(); word val = pop();
    vm.mem[a] = val;
}
static void CFETCH() { CHKPS(1) push(vm.mem[pop()]); }
static void IO_OUT() { CHKPS(2)
    word a = pop(); word val = pop();
    io_write(a, val);
}
static void IO_IN() { CHKPS(1) push(io_read(pop())); }
static void RI() { push(gw(vm.RS)); chkOFLW(); }
static void RI_() { push(gw(vm.RS-2)); chkOFLW(); }
static void RJ() { push(gw(vm.RS-4)); chkOFLW(); }
static void BYE() { vm.running = false; }
static void QUIT() {
    vm.RS = RS_ADDR;
    vm.IP = gw(0x0a) + 1; // (main)
}
static void ABORT() {
    vm.SP = SP_ADDR;
    QUIT();
}
static void EQR() { CHKPS(3)
    word u = pop(); word a2 = pop(); word a1 = pop();
    while (u) {
        byte c1 = vm.mem[a1++];
        byte c2 = vm.mem[a2++];
        if (c1 != c2) { push(0); return; }
        u--;
    }
    push(1);
}
static void EQ() { CHKPS(2)
    word b = pop(); word a = pop();
    if (a == b) { push(1); } else { push(0); } ;
}
static void LT() { CHKPS(2)
    word b = pop(); word a = pop();
    if (a < b) { push(1); } else { push(0); } ;
}
static void FIND() { CHKPS(1)
    word daddr = gw(SYSVARS+0x02); // CURRENT
    word waddr = pop();
    daddr = find(daddr, waddr);
    if (daddr) {
        push(daddr); push(1);
    } else {
        push(waddr); push(0);
    }
    chkOFLW();
}
static void PLUS1() { CHKPS(1) push(pop()+1); }
static void MINUS1() { CHKPS(1) push(pop()-1); }
static void TICKS() { CHKPS(1) usleep(pop()); }
static void CRC16() { CHKPS(2)
	word n = pop(); word c = pop();
	c = c ^ n << 8;
	for (int i=0; i<8; i++) {
		if (c & 0x8000) {
			c = c << 1 ^ 0x1021;
		} else {
			c = c << 1;
		}
	}
	push(c);
}
static void CARRY() { push(vm.carry); chkOFLW(); }
static void _rsh(word u) {
    int n = pop();
    if (!u) return;
    n >>= u-1;
    vm.carry = n & 1;
    n >>= 1;
    push((word)n);
}
static void _lsh(word u) {
    int n = pop(); n <<= u;
    vm.carry = n >= 0x10000; push((word)n);
}
static void RSH1() { CHKPS(1) _rsh(1); }
static void LSH1() { CHKPS(1) _lsh(1); }
static void RSH8() { CHKPS(1) _rsh(8); }
static void LSH8() { CHKPS(1) _lsh(8); }

static void native(NativeWord func) {
    vm.nativew[vm.nativew_count++] = func;
}

VM* VM_init(char *bin_path, char *blkfs_path)
{
    fprintf(stderr, "Using blkfs %s\n", blkfs_path);
    blkfp = fopen(blkfs_path, "r+");
    if (!blkfp) {
        fprintf(stderr, "Can't open\n");
        return NULL;
    }
    fseek(blkfp, 0, SEEK_END);
    if (ftell(blkfp) < 100 * 1024) {
        fclose(blkfp);
        fprintf(stderr, "emul/blkfs too small, something's wrong, aborting.\n");
        return NULL;
    }
    fseek(blkfp, 0, SEEK_SET);
    FILE *bfp = fopen(bin_path, "r");
    if (!bfp) {
        fprintf(stderr, "Can't open forth.bin\n");
        return NULL;
    }
    int i = 0;
    int c = getc(bfp);
    while (c != EOF) {
        vm.mem[i++] = c;
        c = getc(bfp);
    }
    fclose(bfp);
    // initialize rest of memory with random data. Many, many bugs we've seen in
    // Collapse OS were caused by bad initialization and weren't reproducable
    // in CVM because it has a neat zeroed-out memory. Let's make bugs easier
    // to spot.
    while (i<0x10000) {
        vm.mem[i++] = random();
    }
    vm.SP = SP_ADDR;
    vm.RS = RS_ADDR;
    vm.minSP = SP_ADDR;
    vm.maxRS = RS_ADDR;
    vm.nativew_count = 0;
    for (int i=0; i<0x100; i++) {
        vm.iord[i] = NULL;
        vm.iowr[i] = NULL;
    }
    vm.iowr[BLK_PORT] = iowr_blk;
    // Added in the same order as in xcomp.fs
    native(EXIT);
    native(_br_);
    native(_cbr_);
    native(_loop_);
    native(blit);
    native(nlit);
    native(slit);
    native(SP_to_R);
    native(R_to_SP);
    native(SP_to_R_2);
    native(R_to_SP_2);
    native(EXECUTE);
    native(ROT);
    native(DUP);
    native(CDUP);
    native(DROP);
    native(SWAP);
    native(OVER);
    native(AND);
    native(OR);
    native(XOR);
    native(NOT);
    native(PLUS);
    native(MINUS);
    native(MULT);
    native(DIVMOD);
    native(STORE);
    native(FETCH);
    native(CSTORE);
    native(CFETCH);
    native(IO_OUT);
    native(IO_IN);
    native(RI);
    native(RI_);
    native(RJ);
    native(BYE);
    native(ABORT);
    native(QUIT);
    native(EQR);
    native(EQ);
    native(LT);
    native(FIND);
    native(PLUS1);
    native(MINUS1);
    native(TICKS);
    native(ROTR);
    native(CRC16);
    native(CARRY);
    native(RSH1);
    native(LSH1);
    native(RSH8);
    native(LSH8);
    vm.IP = gw(0x04) + 1; // BOOT
    sw(SYSVARS+0x02, gw(0x08)); // CURRENT
    sw(SYSVARS+0x04, gw(0x08)); // HERE
    vm.running = true;
    return &vm;
}

void VM_deinit()
{
    fclose(blkfp);
}

bool VM_steps(int n) {
    if (!vm.running) {
        fprintf(stderr, "machine halted!\n");
        return false;
    }
    while (n && vm.running) {
        word wordref = gw(vm.IP);
        vm.IP += 2;
        execute(wordref);
        n--;
    }
    return vm.running;
}

void VM_memdump() {
    fprintf(stderr, "Dumping memory to memdump. IP %04x\n", vm.IP);
    FILE *fp = fopen("memdump", "w");
    fwrite(vm.mem, 0x10000, 1, fp);
    fclose(fp);
}

void VM_debugstr(char *s) {
    sprintf(s, "SP %04x (%04x) RS %04x (%04x)",
        vm.SP, vm.minSP, vm.RS, vm.maxRS);
}

void VM_printdbg() {
    char buf[0x100];
    VM_debugstr(buf);
    fprintf(stderr, "%s\n", buf);
}
