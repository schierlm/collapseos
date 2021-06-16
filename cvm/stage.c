#include <stdio.h>
#include "vm.h"

/* This Collapse OS VM runs Forth code from stdin like a regular VM, but at the
   end of its run, it pops the last 2 elements of its PS: It's the range that it
   should spit to stdout (the binary that was compiled).
*/
#ifndef BLKFS_PATH
#define BLKFS_PATH "blkfs"
#endif
#ifndef FBIN_PATH
#define FBIN_PATH "serial.bin"
#endif
#define STDIO_PORT 0x00

static char *suffixcode = "ORG HERE BYE\r";
static char *suffix = NULL;

static byte iord_stdio()
{
	int c;
	if (suffix) {
		c = *suffix++;
	} else {
		c = getc(stdin);
		if (c == EOF) {
			suffix = suffixcode;
			c = *suffix++;
		}
    }
    return (byte)c;
}

static void iowr_stdio(byte val)
{
    /* comment if you don't like verbose staging output */
    putc(val, stderr);
}

int main(int argc, char *argv[])
{
    int i;
    VM *vm;
    if (argc < 2) {
        vm = VM_init(FBIN_PATH, BLKFS_PATH);
    } else {
        vm = VM_init(FBIN_PATH, argv[1]);
    }
    if (vm == NULL) {
        return 1;
    }
    vm->iord[STDIO_PORT] = iord_stdio;
    vm->iowr[STDIO_PORT] = iowr_stdio;
    while (VM_steps(1)) {}

	word end = VM_PS_pop();
	word start = VM_PS_pop();
    /* We're done, now let's spit dict data */
    for (i=start; i<end; i++) {
        putchar(vm->mem[i]);
    }
    VM_printdbg();
    VM_deinit();
    return 0;
}
