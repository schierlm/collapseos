#include <stdlib.h>
#include <stdio.h>
#include <fcntl.h>
#include <unistd.h>

#include "common.h"

/* Read specified number of bytes at specified memory address through a Forth
 * remote shell and dump it to stdout.
 */

int main(int argc, char **argv)
{
    if (argc != 4) {
        fprintf(stderr, "Usage: ./memdump device memptr bytecount\n");
        return 1;
    }
    unsigned int memptr = strtol(argv[2], NULL, 16);
    unsigned int bytecount = strtol(argv[3], NULL, 16);
    fprintf(stderr, "memptr: 0x%04x bytecount: 0x%04x.\n", memptr, bytecount);
    if (memptr+bytecount > 0xffff) {
        fprintf(stderr, "memptr+bytecount out of range.\n");
        return 1;
    }
    if (!bytecount) {
        // nothing to spit
        return 0;
    }

    int fd = ttyopen(argv[1]);
    if (fd < 0) {
        fprintf(stderr, "Could not open %s\n", argv[1]);
        return 1;
    }
    char s[0x40];
    sprintf(s, ": _ %d >A %d >R BEGIN AC@+ .x NEXT ; _", memptr, bytecount);
    sendcmd(fd, s);

    for (int i=0; i<bytecount; i++) {
        mread(fd, s, 2); // read hex pair
        s[2] = 0; // null terminate
        unsigned char c = strtol(s, NULL, 16);
        putchar(c);
    }
    readprompt(fd);
    sendcmdp(fd, "FORGET _");
    return 0;
}
