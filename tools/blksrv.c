#include <stdio.h>
#include <stdlib.h>
#include <unistd.h>
#include <signal.h>
#include <errno.h>

#include "common.h"

/* Send and receive blocks on demand from a remote. See doc/blksrv.
*/

/* Maximum number of read()==EAGAIN until we consider a timeout */
#define TIMEOUTCNT 10000

static volatile int isrunning = 1;
static int fd;
static FILE *fp;

void intHandler(int dummy) {
    fprintf(stderr, "Shutting down\n");
    isrunning = 0;
}

static int readn(void *buf, size_t count) {
    ssize_t n, ntot=0;
    for (int i=0; i<TIMEOUTCNT; i++) {
        n = read(fd, buf, count-ntot);
        if (n < 0) {
            if (errno == EAGAIN) continue;
            fprintf(stderr, "read() error, %d\n", errno);
            return 1;
        }
        ntot += n;
        buf += n;
        if (ntot == count) return 0;
        i = 0;
    }
    fprintf(stderr, "readn() timeout\n");
    return 1;
}

static int writen(void *buf, size_t count) {
    ssize_t n, ntot=0;
    for (int i=0; i<TIMEOUTCNT; i++) {
        n = write(fd, buf, count-ntot);
        if (n < 0) {
            if (errno == EAGAIN) continue;
            fprintf(stderr, "write() error, %d\n", errno);
            return 1;
        }
        ntot += n;
        buf += n;
        if (ntot == count) return 0;
        i = 0;
    }
    fprintf(stderr, "writen() timeout\n");
    return 1;
}

static int read16(int *dest) {
    char buf[5];

    if (readn(buf, 4) != 0) return 1;
    buf[4] = 0;
    *dest = strtol(buf, NULL, 16);
    return 0;
}

static int write16(int n) {
    char buf[5];

    snprintf(buf, 5, "%04x", n);
    return writen(buf, 4);
}

static int blksum(char *buf) {
    int s = 0;
    for (int i=0; i<1024; i++) s += buf[i];
    return s & 0xffff;
}

static int readblkid() {
    int blkid;

    if (read16(&blkid) != 0) return 1;
    fprintf(stderr, "blkid: %d\n", blkid);
    if (fseek(fp, blkid*1024, SEEK_SET) < 0) {
        fprintf(stderr, "seek error for blkid %d\n", blkid);
        return 1;
    }
    return 0;
}

static int recvblk() {
    char buf[1024];
    int csum;

    if (readn(buf, 1024) != 0) return 1;
    if (read16(&csum) != 0) return 1;
    if (blksum(buf) != csum) return 1;
    if (fwrite(buf, 1024, 1, fp) != 1) return 1;
    fflush(fp);
    return 0;
}

static int sendblk() {
    char buf[1024] = {' '};

    if (fread(buf, 1024, 1, fp) != 1) return 1;
    if (writen(buf, 1024) != 0) return 1;
    return write16(blksum(buf));
}

int main(int argc, char **argv) {
    char c;
    ssize_t n;
    if (argc != 3) {
        fprintf(stderr, "Usage: ./blksrv device blkfs\n");
        return 1;
    }
    fp = fopen(argv[2], "r+");
    if (!fp) {
        fprintf(stderr, "Could not open %s\n", argv[2]);
        return 1;
    }
    fd = ttyopen(argv[1]);
    if (fd < 0) {
        fclose(fp);
        fprintf(stderr, "Could not open %s\n", argv[1]);
        return 1;
    }
    set_blocking(fd, 0);
    signal(SIGINT, intHandler);
    while (isrunning) {
        n = read(fd, &c, 1);
        if (n < 1) {
            if ((n==0) || (errno == EAGAIN)) {
                usleep(1000); // don't consume 100% CPU
                continue;
            }
            fprintf(stderr, "read() error, %d\n", errno);
            break;
        }
        fprintf(stderr, "new cmd: %c\n", c);
        if (c == 'G') {
            if (readblkid() != 0) continue;
            if (sendblk() != 0) {
                fprintf(stderr, "sendblk() error\n", c);
                continue;
            }
            fprintf(stderr, "sendblk() complete\n", c);
        } else if (c == 'P') {
            if (readblkid() != 0) continue;
            if (recvblk() != 0) {
                fprintf(stderr, "recvblk() error\n", c);
                continue;
            }
            fprintf(stderr, "recvblk() complete\n", c);
        }
    }
    close(fd);
    fclose(fp);
}
