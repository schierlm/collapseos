#include <stdlib.h>
#include <stdio.h>
#include <fcntl.h>
#include <unistd.h>
#include <string.h>

#include "common.h"

/* Pull "blkcnt" blocks from "device", starting at blk "startat" and
 * write it to stdout.
 * If "startat" is omitted, it is 0.
 */

int main(int argc, char **argv)
{
    if ((argc != 4) && (argc != 3)) {
        fprintf(stderr, "Usage: ./blkdown device blkcnt [startat]\n");
        return 1;
    }
    unsigned int blkcnt = strtol(argv[2], NULL, 10);
    unsigned int startat = 0;
    if (argc == 4) {
        startat = strtol(argv[3], NULL, 10);
    }
    int fd = ttyopen(argv[1]);
    if (fd < 0) {
        fprintf(stderr, "Could not open %s\n", argv[1]);
        return 1;
    }
    sendcmdp(fd, ": _ BLK( >A 1024 >R BEGIN AC@+ .x NEXT ;");
    int returncode = 0;
    for (int i=startat; i<blkcnt+startat; i++) {
        char s[0x40];
        sprintf(s, "%d BLK@ _", i);
        sendcmd(fd, s);
        fprintf(stderr, "b%d ", i);

        for (int j=0; j<1024; j++) {
            mread(fd, s, 2); // read hex pair
            s[2] = 0; // null terminate
            char *endptr = NULL;
            unsigned char c = strtol(s, &endptr, 16);
            if (endptr != &s[2]) {
                fprintf(stderr, "Non-hex value (%s) at index %d.\n", s, j);
                // we don't exit now because we need to "consume" our whole program.
                returncode = 1;
            }
            if (returncode == 0) {
                putchar(c);
            }
        }
        readprompt(fd);
    }
    sendcmdp(fd, "FORGET _");
    fprintf(stderr, "Done!\n");
    if (fd > 0) close(fd);
    return returncode;
}
