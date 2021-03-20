#include <stdlib.h>
#include <stdio.h>
#include <fcntl.h>
#include <unistd.h>
#include <string.h>
#include <stdbool.h>
#include <stdint.h>

#include "common.h"

/* Push specified file to specified device blk device, starting from blkno
 * and upwards.
 */

int main(int argc, char **argv)
{
    if (argc != 4) {
        fprintf(stderr, "Usage: ./blkup device blkno fname\n");
        return 1;
    }
    unsigned int blkno = strtol(argv[2], NULL, 10);
    FILE *fp = fopen(argv[3], "r");
    if (!fp) {
        fprintf(stderr, "Can't open %s.\n", argv[3]);
        return 1;
    }
    int fd = ttyopen(argv[1]);
    if (fd < 0) {
        fprintf(stderr, "Could not open %s\n", argv[1]);
        return 1;
    }
    char s[0x40];
    char buf[1024] = {0};
    sendcmdp(fd, ": _ 0 1024 0 DO KEY DUP ROT + SWAP I BLK( + C! LOOP .X ;");
    sendcmdp(fd, ": Z BLK( 1024 0 FILL ;");

    int returncode = 0;
    while (fread(buf, 1, 1024, fp)) {
        printf("b%d ", blkno);
        fflush(stdout);
        bool allzero = true;
	uint16_t csum = 0;
        for (int i=0; i<1024; i++) {
	    csum += buf[i];
            if (buf[i] != 0) {
                allzero = false;
            }
        }
        if (allzero) {
            sendcmdp(fd, "Z");
            putchar('Z');
            fflush(stdout);
        } else {
            sendcmd(fd, "_");
            for (int i=0; i<1024; i++) {
                write(fd, &buf[i], 1);
                usleep(1000); // let it breathe
            }
            mread(fd, s, 4); // read double hex pair
            readprompt(fd);
            s[4] = 0; // null terminate
            uint16_t n = strtol(s, NULL, 16);
            if (n != csum) {
                // mismatch!
                fprintf(stderr, "Mismatch %x != %x.\n", n, csum);
                returncode = 1;
                break;
            }
            memset(buf, 0, 1024);
        }
        sprintf(s, "%d BLK> ! BLK!", blkno);
        sendcmdp(fd, s);
        blkno++;
    }
    sendcmdp(fd, "FORGET _");
    printf("Done!\n");
    fclose(fp);
    return returncode;
}

