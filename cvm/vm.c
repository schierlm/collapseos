#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include "vm.h"

#define BLKOP_CMD_SZ 4

static VM vm;
static FILE *blkfp;
/* Stores blkop command. Bytes flow from left (byte 0) to right (byte 3)
 * We know we have a full command when last byte is nonzero. After
 * processing the cmd, we reset blkop to 0. */
static byte blkop[BLKOP_CMD_SZ];

/* Read single byte from I/O handler, if set. addr is a word only because of */
/* Forth's cell size, but can't actually address more than a byte-full of ports. */
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

/* I/O hook to read/write a chunk of 1024 byte to blkfs at specified blkid. */
/* This is used by EFS@ and EFS! in xcomp.fs. */
/* See comment above BLK_PORT define for poking convention. */
static void iowr_blk(byte val)
{
    byte rw = blkop[3];
    if (rw) {
        long blkid = (long)blkop[2] << 8 | (long)blkop[1];
        int dest = (int)blkop[0] << 8 | (int)val;
        memset(blkop, 0, BLKOP_CMD_SZ);
        fseek(blkfp, blkid*1024, SEEK_SET);
        if (rw==2) { /* write */
            fwrite(&vm.mem[dest], 1024, 1, blkfp);
        } else { /* read */
            fread(&vm.mem[dest], 1024, 1, blkfp);
        }
    } else {
        memmove(blkop+1, blkop, BLKOP_CMD_SZ-1);
        blkop[0] = val;
    }
}

/* get/set word from/to memory */
static word gw(word addr) { return vm.mem[addr+(word)1] << 8 | vm.mem[addr]; }
static void sw(word addr, word val) {
    vm.mem[addr] = val;
    vm.mem[addr+(word)1] = val >> 8;
}
/* pop word from SP */
static word pop() {
    word n = vm.mem[vm.SP++];
    return n | vm.mem[vm.SP++] << 8;
}
word VM_PS_pop() { return pop(); }

/* push word to SP */
static void push(word x) {
    vm.SP -= 2;
    sw(vm.SP, x);
    if (vm.SP < vm.minSP) { vm.minSP = vm.SP; }
}
void VM_PS_push(word n) { push(n); }
/* pop word from RS */
static word popRS() {
    word x = gw(vm.RS); vm.RS -= 2; return x;
}
/* push word to RS */
static void pushRS(word val) {
    vm.RS += 2;
    sw(vm.RS, val);
    if (vm.RS > vm.maxRS) { vm.maxRS = vm.RS; }
}

static void nativeexec(word idx);

static void execute(word wordref) {
    byte wtype = vm.mem[wordref];
    if (wtype == 0) { /* native */
        nativeexec(vm.mem[wordref+(word)1]);
        return;
    }
    if (wtype & 1) { /* XT or DOES */
        pushRS(vm.IP);
        if (wtype & 0x80) { /* DOES */
            push(wordref+3);
            vm.IP = gw(wordref+1);
        } else { /* regular XT */
            vm.IP = wordref+1;
        }
        if (vm.SP <= vm.RS) {
            vm.SP = SP_ADDR; vm.RS = RS_ADDR;
            execute(gw(0x13)); /* oflw */
        }
        return;
    }
    if (wtype & 2) { /* cell */
        push(wordref+1);
        return;
    }
    if (wtype & 4) { /* alias */
        word addr = gw(wordref+1);
        if (wtype & 0x80) addr = gw(addr); /* indirect */
        execute(addr);
        return;
    }
    if (wtype & 8) { /* value */
        word val = gw(wordref+1);
        if (wtype & 0x80) val = gw(val); /* indirect */
        push(val);
        return;
    }
    fprintf(stderr, "invalid word type %d!\n", wtype);
    vm.running = false;
}

/* The functions below directly map to native forth words defined in the */
/* dictionary (doc/dict.txt) */
static void EXIT() { vm.IP = popRS(); }
static void _br_() {
    word off = vm.mem[vm.IP];
    if (off > 0x7f ) { off -= 0x100; }
    vm.IP += off;
}
static void _cbr_() { if (!pop()) { _br_(); } else { vm.IP++; } }
static void _loop_() {
    word I = gw(vm.RS); I++; sw(vm.RS, I);
    if (I == gw(vm.RS-2)) { /* don't branch */
        popRS(); popRS();
        vm.IP++;
    } else { /* branch */
        _br_();
    }
}
static void SP_to_R_2() { word x = pop(); pushRS(pop()); pushRS(x); }
static void blit() { push(vm.mem[vm.IP]); vm.IP++; }
static void nlit() { push(gw(vm.IP)); vm.IP += 2; }
/* (c) in the CVM doesn't make sense and is a noop. */
static void clit() {
    word off = vm.mem[vm.IP];
    vm.IP += off;
}
static void SP_to_R() { pushRS(pop()); }
static void R_to_SP() { push(popRS()); }
static void R_to_SP_2() { word x = popRS(); push(popRS()); push(x); }
static void EXECUTE() { execute(pop()); }
static void ROT() { /* a b c -- b c a */
    word c = pop(); word b = pop(); word a = pop();
    push(b); push(c); push(a);
}
static void ROTR() { /* a b c -- c a b */
    word c = pop(); word b = pop(); word a = pop();
    push(c); push(a); push(b);
}
static void DUP() { /* a -- a a */
    word a = pop(); push(a); push(a);
}
static void CDUP() {
    word a = pop(); push(a); if (a) { push(a); }
}
static void DROP() { pop(); }
static void SWAP() { /* a b -- b a */
    word b = pop(); word a = pop();
    push(b); push(a);
}
static void OVER() { /* a b -- a b a */
    word b = pop(); word a = pop();
    push(a); push(b); push(a);
}
static void AND() { push(pop() & pop()); }
static void OR() { push(pop() | pop()); }
static void XOR() { push(pop() ^ pop()); }
static void NOT() { push(!pop()); }
static void PLUS() {
    int b = pop(); int a = pop(); int n = a + b;
    vm.carry = n >= 0x10000; push((word)n);
}
static void MINUS() {
    int b = pop(); int a = pop(); int n = a - b;
    vm.carry = n < 0; push((word)n);
}
static void MULT() {
    int b = pop(); int a = pop(); int n = a * b;
    vm.carry = n >= 0x10000; push((word)n);
}
static void DIVMOD() {
    word b = pop(); word a = pop();
    push(a % b); push(a / b);
}
static void STORE() {
    word a = pop(); word val = pop();
    sw(a, val);
}
static void FETCH() { push(gw(pop())); }
static void CSTORE() {
    word a = pop(); word val = pop();
    vm.mem[a] = val;
}
static void CFETCH() { push(vm.mem[pop()]); }
static void IO_OUT() {
    word a = pop(); word val = pop();
    io_write(a, val);
}
static void IO_IN() { push(io_read(pop())); }
static void RI() { push(gw(vm.RS)); }
static void RI_() { push(gw(vm.RS-2)); }
static void RJ() { push(gw(vm.RS-4)); }
static void BYE() { vm.running = false; }
static void QUIT() {
    vm.RS = RS_ADDR;
    vm.IP = gw(0x0a) + 1; /* (main) */
}
static void ABORT() {
    vm.SP = SP_ADDR;
    memset(&vm.mem[SP_ADDR], 0, 4);
    QUIT();
}
static void EQR() {
    word u = pop(); word a2 = pop(); word a1 = pop();
    while (u) {
        byte c1 = vm.mem[a1++];
        byte c2 = vm.mem[a2++];
        if (c1 != c2) { push(0); return; }
        u--;
    }
    push(1);
}
static void EQ() {
    word b = pop(); word a = pop();
    if (a == b) { push(1); } else { push(0); } ;
}
static void LT() {
    word b = pop(); word a = pop();
    if (a < b) { push(1); } else { push(0); } ;
}
static void FIND() {
    word daddr = gw(SYSVARS+0x02); /* CURRENT */
    word len = pop();
    word saddr = pop();
    while (daddr) {
        if ((vm.mem[daddr-(word)1] & 0x7f) == len) {
            word d = daddr-3-len;
            /* Sanity check */
            if ((saddr+len >= MEMSIZE) || (d+len) >= MEMSIZE) break;
            if (strncmp(&vm.mem[saddr], &vm.mem[d], len) == 0) {
                push(daddr); push(1); return;
            }
        }
        daddr -= 3;
        daddr = gw(daddr);
    }
    push(0);
}

static void PLUS1() { push(pop()+1); }
static void MINUS1() { push(pop()-1); }
/* TICKS in CVM is a noop for now. */
static void TICKS() { pop(); }
static void CRC16() {
    int i;
	word n = pop(); word c = pop();
	c = c ^ n << 8;
	for (i=0; i<8; i++) {
		if (c & 0x8000) {
			c = c << 1 ^ 0x1021;
		} else {
			c = c << 1;
		}
	}
	push(c);
}
static void CARRY() { push(vm.carry); }
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
static void RSH1() { _rsh(1); }
static void LSH1() { _lsh(1); }
static void RSH8() { _rsh(8); }
static void LSH8() { _lsh(8); }
static void Saddr() { push(vm.SP); }
static void Raddr() { push(vm.RS); }
static void FILL() {
  word c = pop(); word u = pop(); word a = pop();
  memset(&vm.mem[a], c, u); }
static void MOVE() {
  word i;
  word u = pop(); word dst = pop(); word src = pop();
  for (i=0; i<u; i++) vm.mem[dst+i] = vm.mem[src+i]; }
static void MOVEMINUS() {
  word i;
  word u = pop(); word dst = pop(); word src = pop();
  for (i=0; i<u; i++) vm.mem[dst+u-i-1] = vm.mem[src+u-i-1]; }

/* Native words in this C Forth VMs are indexed in an array. The word in memory
 * is the typical 0x00 to indicate native, followed by an index byte. The
 * Execute routine will then know which native word to execute.
 * In the same order as in xcomp.fs */
static void (*nativew[])() = {
    EXIT, _br_, _cbr_, _loop_, blit, nlit, clit, SP_to_R, R_to_SP, SP_to_R_2,
    R_to_SP_2, EXECUTE, ROT, DUP, CDUP, DROP, SWAP, OVER, AND, OR, XOR, NOT,
    PLUS, MINUS, MULT, DIVMOD, STORE, FETCH, CSTORE, CFETCH, IO_OUT, IO_IN,
    RI, RI_, RJ, BYE, ABORT, QUIT, EQR, EQ, LT, FIND, PLUS1, MINUS1, TICKS,
    ROTR, CRC16, CARRY, RSH1, LSH1, RSH8, LSH8, Saddr, Raddr, FILL, MOVE,
    MOVEMINUS }; /* 57 words */
static void nativeexec(word idx) {
    if (idx < sizeof(nativew)/sizeof(void*)) {
        nativew[idx]();
    } else {
        fprintf(stderr, "Out of bounds native call %04x. IP: %04x\n", idx, vm.IP);
        vm.running = false;
    }
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
    FILE *bfp = fopen(bin_path, "r");
    if (!bfp) {
        fprintf(stderr, "Can't open forth bin\n");
        return NULL;
    }
    int i = 0;
    int c = getc(bfp);
    while (c != EOF) {
        vm.mem[i++] = c;
        c = getc(bfp);
    }
    fclose(bfp);
    /* initialize rest of memory with random data. Many, many bugs we've seen in
     * Collapse OS were caused by bad initialization and weren't reproducable
     * in CVM because it has a neat zeroed-out memory. Let's make bugs easier
     * to spot. */
    while (i<MEMSIZE) {
        vm.mem[i++] = rand();
    }
    memset(blkop, 0, BLKOP_CMD_SZ);
    vm.SP = SP_ADDR;
    vm.RS = RS_ADDR;
    vm.minSP = SP_ADDR;
    vm.maxRS = RS_ADDR;
    for (i=0; i<0x100; i++) {
        vm.iord[i] = NULL;
        vm.iowr[i] = NULL;
    }
    vm.iowr[BLK_PORT] = iowr_blk;
    vm.IP = gw(0x04) + 1; /* BOOT */
    vm.running = true;
    return &vm;
}

void VM_deinit()
{
    fclose(blkfp);
}

Bool VM_steps(int n) {
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
    fwrite(vm.mem, MEMSIZE, 1, fp);
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
