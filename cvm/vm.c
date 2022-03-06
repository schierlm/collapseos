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

static word pc16() { word n = gw(vm.PC); vm.PC+=2; return n; }
static word pc8() { byte b = vm.mem[vm.PC]; vm.PC++; return b; }

/* Native words */
static void DUP() { push(peek()); }
static void DROP() { pop(); }
static void SWAP() { word a = pop(); word b = pop(); push(a); push(b); }
static void OVER() { word a = pop(); word b = peek(); push(a); push(b); }
static void ROT() {
    word c = pop(); word b = pop(); word a = pop();
    push(b); push(c); push(a); }
static void RS2PS() { push(popRS()); }
static void PS2RS() { pushRS(pop()); }
static void RFETCH() { push(gw(vm.RS)); }
static void RDROP() { popRS(); }
static void PUSHi() { push(pc16()); }
static void PUSHii() { push(gw(pc16())); }
static void CFETCH() { push(vm.mem[pop()]); }
static void FETCH() { push(gw(pop())); }
static void CSTORE() { word a = pop(); vm.mem[a] = pop(); }
static void STORE() { word a = pop(); sw(a, pop()); }
static void EXECUTE() { vm.PC = pop(); }
static void JMPi() { vm.PC = gw(vm.PC); }
static void JMPii() { vm.PC = gw(gw(vm.PC)); }
static void CALLi() { push(vm.PC+2); JMPi(); }
static void NOT() { push(pop() ? 0 : 1); }
static void AND() { push(pop() & pop()); }
static void OR() { push(pop() | pop()); }
static void XOR() { push(pop() ^ pop()); }
static void PLUS() { word b = pop(); word a = pop(); push(a+b); }
static void SUB() { word b = pop(); word a = pop(); push(a-b); }
static void BR() {
    word off = vm.mem[vm.IP]; if (off > 0x7f) off |= 0xff00; vm.IP += off; }
static void CBR() { if (pop()) { vm.IP++; } else { BR(); } }
static void NEXT() {
    word n = popRS()-1;
    if (n) { pushRS(n); BR(); }
    else { vm.IP++; }
}
static void PCSTORE() {
    word a = pop(); word val = pop();
    io_write(a, val);
}
static void PCFETCH() { push(io_read(pop())); }
static void MULT() { word b = pop(); word a = pop(); push(a * b); }
static void DIVMOD() {
    word b = pop(); word a = pop();
    push(a % b); push(a / b);
}
static void QUIT() { vm.RS = RS_ADDR; vm.PC = gw(0x0a) /* main */; }
static void ABORT() { vm.SP = SP_ADDR; QUIT(); }
static void RCNT() { push((vm.RS - RS_ADDR) / 2); }
static void SCNT() { push((SP_ADDR - vm.SP) / 2); }
static void BYE() { vm.running = false; }
static void EXIT() { vm.IP = popRS(); }
static void CDUP() { word a = peek(); if (a) push(a); }
static void LIT8() { push(vm.mem[vm.IP++]); }
static void LIT16() { push(gw(vm.IP)); vm.IP+=2; }
static void JMPiWR() {
    word a = pop(); vm.mem[a++] = 11 /* JMPi */; sw(a, pop()); push(3); }
static void CALLiWR() {
    word a = pop(); vm.mem[a++] = 10 /* CALLi */; sw(a, pop()); push(3); }
static void LT() { word b = pop(); word a = pop(); push(a<b); }

static void (*halops[67])() = {
    DUP, DROP, PUSHi, PUSHii, SWAP, OVER, ROT, NULL, CBR, NEXT,
    CALLi, JMPi, NULL, EXIT, CDUP, LIT8, LIT16, JMPii, NULL, JMPiWR, NULL,
    EXECUTE, NULL, NULL, NULL, CALLiWR, RDROP, NULL, PLUS, SUB, BR, NULL,
    NULL, LT, NULL, NULL, NULL, NULL, NOT, AND, OR, XOR,
    NULL, NULL, NULL, NULL, NULL, NULL, NULL,
    NULL, NULL, PCSTORE, PCFETCH, MULT, DIVMOD, QUIT, ABORT, RCNT, SCNT, BYE,
    RFETCH, RS2PS, PS2RS, CFETCH, FETCH, STORE, CSTORE
};

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
            vm.PC = 0;
        } else if (vm.PC == 2) { /* does */
            vm.PC = pop();
            push(vm.PC+2);
            vm.PC = gw(vm.PC);
        } else if (vm.PC == 3) { /* value */
            push(gw(pop()));
            vm.PC = 0;
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
