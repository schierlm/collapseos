#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include "vm.h"

#define BLKOP_CMD_SZ 4
#define NEXT vm.PC=0

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
static word peek() { return gw(vm.SP); }
/* pop word from SP */
static word pop() { word n = peek(); vm.SP+=2; return n; }
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

static void FIND() {
    byte len = pop();
    word waddr = pop();
    word daddr = gw(SYSVARS+0x02); /* CURRENT */
    while (daddr) {
        if ((vm.mem[daddr-(word)1] & 0x7f) == len) {
            word d = daddr-3-len;
            if (strncmp(&vm.mem[waddr], &vm.mem[d], len) == 0) {
                push(daddr); push(1); NEXT; return;
            }
        }
        daddr = gw(daddr-3);
    }
    push(0); NEXT;
}
static void EQR() {
    word u = pop(); word a2 = pop(); word a1 = pop();
    while (u) {
        byte c1 = vm.mem[a1++];
        byte c2 = vm.mem[a2++];
        if (c1 != c2) { push(0); NEXT; return; }
        u--;
    }
    push(1); NEXT;
}
static void PCSTORE() {
    word a = pop(); word val = pop();
    io_write(a, val); NEXT;
}
static void PCFETCH() { push(io_read(pop())); NEXT; }
static void MULT() {
    int b = pop(); int a = pop(); int n = a * b;
    vm.zero = n == 0; vm.carry = n >= 0x10000; push(n);
    NEXT;
}
static void DIVMOD() {
    word b = pop(); word a = pop();
    push(a % b); push(a / b);
    NEXT;
}
static void QUIT() { vm.RS = RS_ADDR; vm.PC = gw(0x0a) /* main */; }
static void ABORT() { vm.SP = SP_ADDR; QUIT(); }
static void RCNT() { push((vm.RS - RS_ADDR) / 2); NEXT; }
static void SCNT() { push((SP_ADDR - vm.SP) / 2); NEXT; }
static void BYE() { vm.running = false; }

static void (*nativew[11])() = {
    FIND, EQR, PCSTORE, PCFETCH, MULT, DIVMOD, QUIT, ABORT, RCNT, SCNT, BYE };

/* HAL ops */
/* Stack */
static void DUPp() { push(peek()); }
static void DROPp() { pop(); }
static void POPp() { vm.W = pop(); }
static void PUSHp() { push(vm.W); }
static void POPf() { word a = pop(); vm.W = pop(); push(a); }
static void PUSHf() { word a = pop(); push(vm.W); push(a); }
static void POPr() { vm.W = popRS(); }
static void PUSHr() { pushRS(vm.W); }
static void SWAPwp() { word a = pop(); push(vm.W); vm.W = a; }
static void SWAPwf() { word a = pop(); SWAPwp(); push(a); }
/* Transfer */
static void w2p() { pop(); push(vm.W); }
static void p2w() { POPp(); PUSHp(); }
static void i2w() { vm.W = gw(vm.PC); vm.PC+=2; }
static void ii2w() { vm.W = gw(gw(vm.PC)); vm.PC+=2; }
static void CFETCHw() { vm.W = vm.mem[vm.W]; }
static void FETCHw() { vm.W = gw(vm.W); }
static void CSTOREwp() { vm.mem[vm.W] = peek(); }
static void STOREwp() { sw(vm.W, peek()); }
static void w2IP() { vm.IP = vm.W; }
static void IP2w() { vm.W = vm.IP; }
static void IPplusoff() {
    word off = vm.mem[vm.IP]; if (off > 0x7f) off |= 0xff00; vm.IP += off; }
static void IPplusone() { vm.IP++; }
/* Flags */
static void wZ() { vm.zero = vm.W == 0; }
static void pZ() { vm.zero = peek() == 0; }
static void Z2w() { vm.W = vm.zero; }
static void C2w() { vm.W = vm.carry; }
static void ZSel() { vm.jcond = vm.zero; }
static void CSel() { vm.jcond = vm.carry; }
static void InvSel() { vm.jcond = !vm.jcond; }
/* Jump */
static void JMPw() { vm.PC = vm.W; }
static void JMPi() { vm.PC = gw(vm.PC); }
static void CALLi() { push(vm.PC+2); JMPi(); }
static void JRi() {
    byte off = vm.mem[vm.PC]; vm.PC+=off; if (off&0x80) vm.PC-=0x100; }
static void JRCONDi() { if (vm.jcond) { JRi(); } else { vm.PC++; } }
/* Arithmetic */
static void INCw() { vm.W++; }
static void DECw() { vm.W--; }
static void INCp() { push(pop()+1); }
static void DECp() { push(pop()-1); }
static void CMPwp() { vm.zero=peek()==vm.W; vm.carry=peek()>vm.W; }
static void ANDwp() { vm.W &= peek(); }
static void ANDwi() { word i = gw(vm.PC); vm.PC += 2; vm.W &= i; }
static void ORwp() { vm.W |= peek(); }
static void XORwp() { vm.W ^= peek(); }
static void XORwi() { word i = gw(vm.PC); vm.PC += 2; vm.W ^= i; }
static void PLUSpw() {
    int b = vm.W; int a = peek(); int n = a + b;
    vm.zero = n == 0; vm.carry = n >= 0x10000; vm.W = n;
}
static void SUBwp() {
    int b = vm.W; int a = peek(); int n = b - a;
    vm.zero = n == 0; vm.carry=n<0; vm.W = n;
}
static void SHRw() { vm.carry = vm.W & 1; vm.W >>= 1; }
static void SHLw() { vm.carry = (vm.W & 0x8000) >> 15; vm.W <<= 1; }
static void SHR8w() { vm.W >>= 8; }
static void SHL8w() { vm.W <<= 8; }

static void (*halops[49])() = {
    DUPp, DROPp, POPp, PUSHp, POPr, PUSHr, POPf, PUSHf, SWAPwp, SWAPwf,
    JMPw, JMPi, JRi, JRCONDi, ZSel, CSel, InvSel, wZ, pZ, Z2w, C2w,
    w2p, p2w, i2w, CFETCHw, FETCHw, CSTOREwp, STOREwp, w2IP, IP2w,
    IPplusoff, IPplusone,
    INCw, DECw, INCp, DECp, PLUSpw, SUBwp, CMPwp, ANDwp, ORwp, XORwp, XORwi,
    SHRw, SHLw, SHR8w, SHL8w, CALLi, ii2w };
static void halexec(byte op) {
    if (op < sizeof(halops)/sizeof(void*)) {
        halops[op]();
    } else {
        fprintf(stderr, "Out of bounds HAL op %04x. PC: %04x\n", op, vm.PC);
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
    vm.PC = gw(0x04); /* BOOT */
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
        if (vm.PC == 0) { /* next */
            vm.PC = gw(vm.IP);
            vm.IP += 2;
        } else if (vm.PC == 1) { /* xt */
            pushRS(vm.IP);
            vm.IP = pop();
            NEXT;
        } else if (vm.PC == 2) { /* does */
            pushRS(vm.IP);
            vm.IP = vm.W;
            NEXT;
        } else if (vm.PC == 3) { /* push */
            push(vm.W);
            NEXT;
        } else if (vm.PC < (sizeof(nativew)/sizeof(void*)+4)) { /* native word */
            nativew[vm.PC-4]();
        } else {
            halexec(vm.mem[vm.PC++]);
        }
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
