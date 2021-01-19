#include <stdlib.h>
#include <unistd.h>
#include <termios.h>
#include <errno.h>
#include <stdio.h>
#include <string.h>
#include <fcntl.h>

//#define DEBUG(...) fprintf(stderr, __VA_ARGS__)
#define DEBUG(...)

void mread(int fd, char *s, int count)
{
    while (count) {
        while (read(fd, s, 1) == 0);
        s++;
        count--;
    }
}

// Make sure that nothing is waiting in the pipeline
static void mempty(int fd)
{
    char c;
    while (read(fd, &c, 1) == 1) {
        DEBUG("Emptying %d\n", c);
    }
}

int mexpect(int fd, unsigned char ec)
{
    unsigned char c;
    mread(fd, &c, 1);
    if (c != ec) {
        fprintf(stderr, "Expected %x but got %x\n", ec, c);
        return 0;
    }
    return 1;
}

void readprompt(int fd)
{
    mexpect(fd, ' ');
    mexpect(fd, 'o');
    mexpect(fd, 'k');
    mexpect(fd, '\r');
    mexpect(fd, '\n');
}

void sendcmd(int fd, char *cmd)
{
    DEBUG("Sending %s\n", cmd);
    char junk[2];
    while (*cmd) {
        DEBUG("W: %d\n", *cmd);
        write(fd, cmd, 1);
        read(fd, &junk, 1);
        DEBUG("R: %d\n", *junk);
        cmd++;
    }
    write(fd, "\r", 1);
    mexpect(fd, '\r');
    mexpect(fd, '\n');
}

// Send a cmd and also read the " ok" prompt
void sendcmdp(int fd, char *cmd)
{
    sendcmd(fd, cmd);
    readprompt(fd);
}

void set_blocking(int fd, int should_block)
{
    struct termios tty;
    memset(&tty, 0, sizeof tty);
    if (tcgetattr (fd, &tty) != 0) {
        fprintf(stderr, "error %d from tggetattr", errno);
        return;
    }

    tty.c_cc[VMIN]  = should_block ? 1 : 0;
    tty.c_cc[VTIME] = 1;            // 0.1 seconds read timeout

    if (tcsetattr (fd, TCSANOW, &tty) != 0) {
        fprintf(stderr, "error %d setting term attributes", errno);
    }
}

int ttyopen(char *devname)
{
    int fd = 0;
    if (strcmp(devname, "-") != 0) {
        fd = open(devname, O_RDWR|O_NOCTTY|O_SYNC);
    }
    set_blocking(fd, 0);
    mempty(fd);
    // Communication with device is much more reliable if we
    // begin by sending, asynchronously, a CR to make sure we
    // empty any pending stuff on all sides.
    write(fd, "\r", 1);
    mempty(fd);
    set_blocking(fd, 1);
    return fd;
}
