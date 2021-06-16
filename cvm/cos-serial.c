#include <stdio.h>
#include <unistd.h>
#include <termios.h>
#include "vm.h"

#ifndef BLKFS_PATH
#define BLKFS_PATH "blkfs"
#endif
#ifndef FBIN_PATH
#define FBIN_PATH "serial.bin"
#endif
#define STDIO_PORT 0x00

static byte iord_stdio()
{
	int c = getchar();
    if (c == EOF) {
        c = 0x04; /* EOT */
    }
    return (byte)c;
}

static void iowr_stdio(byte val)
{
    putchar(val);
    fflush(stdout);
}

int main(int argc, char *argv[])
{
    struct termios bkptio, tio;
    VM *vm = VM_init(FBIN_PATH, BLKFS_PATH);
    if (vm == NULL) {
        return 1;
    }
    if (isatty(STDIN_FILENO)) {
        tcgetattr(STDIN_FILENO, &tio);
        bkptio = tio;
        tio.c_lflag &=(~ICANON & ~ECHO); /* no echo, unbuffered */
        tcsetattr(STDIN_FILENO, TCSANOW, &tio);
    }
    vm->iord[STDIO_PORT] = iord_stdio;
    vm->iowr[STDIO_PORT] = iowr_stdio;
    while (VM_steps(1));
    VM_printdbg();
    VM_deinit();
    if (isatty(STDIN_FILENO)) {
        tcsetattr(STDIN_FILENO, TCSANOW, &bkptio);
    }
    return 0;
}
