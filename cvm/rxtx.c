#include <stdlib.h>
#include <stdio.h>
#include <string.h>
#include <fcntl.h>
#include <termios.h>
#include "rxtx.h"

/* descriptor of the master's end of the pty for TX> and RX<? */
static int rxtxfd;
/* if >= 0x100, val there is a value waiting in LSB */
static int rxval = 0;

/* To read a byte, we need to poll twice. The first time, it returns 0 or 1.
 * 0 means there's nothing, 1 means there's something. If 1 was returned,
 * the next read of the port will be the value. If 0 was returned, the next
 * read of the port will be another poll.
 */
static byte iord_rxtx() {
    byte b;
    if (rxval) {
        b = rxval & 0xff;
        rxval = 0;
        return b;
    } else {
        if (read(rxtxfd, &b, 1) == 1) {
            rxval = b | 0x100;
            return 1;
        } else {
            return 0;
        }
    }
}

static void iowr_rxtx(byte val) {
    write(rxtxfd, &val, 1);
}

/* 0=success */
int rxtx_init(VM *vm, int port) {
    int slave;
    struct termios termp;
    int r;
    char ptyname[PATH_MAX + 1];

    termp.c_iflag = IGNBRK | IGNPAR;
    termp.c_oflag = 0;
    termp.c_cflag = CS8 | CREAD | CLOCAL;
    termp.c_lflag = 0;
    termp.c_cc[VMIN] = 0;
    termp.c_cc[VTIME] = 0;
    r = openpty(&rxtxfd, &slave, ptyname, &termp, NULL);
    if (r != 0) {
        fprintf(stderr, "Couldn't open RX/TX pty. Aborting.\n");
	return 1;
    }
    close(slave);
    fcntl(rxtxfd, F_SETFL, O_NONBLOCK|O_DIRECT);
    fprintf(stderr, "RX/TX pty opened at %s\n", ptyname);
    vm->iord[port] = iord_rxtx;
    vm->iowr[port] = iowr_rxtx;
    return 0;
}

void rxtx_deinit() {
    close(rxtxfd);
}
