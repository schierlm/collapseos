#include <stdlib.h>
#include <stdio.h>
#include <fcntl.h>
#include <unistd.h>

#include "common.h"

/* Push specified file to specified device running Forth and verify
 * that the sent contents is correct.
 */

int main(int argc, char **argv)
{
    if (argc != 4) {
        fprintf(stderr, "Usage: ./upload device memptr fname\n");
        return 1;
    }
    unsigned int memptr = strtol(argv[2], NULL, 16);
    FILE *fp = fopen(argv[3], "r");
    if (!fp) {
        fprintf(stderr, "Can't open %s.\n", argv[3]);
        return 1;
    }
    fseek(fp, 0, SEEK_END);
    unsigned int bytecount = ftell(fp);
    fprintf(stderr, "memptr: 0x%04x bytecount: 0x%04x.\n", memptr, bytecount);
    if (!bytecount) {
        // Nothing to read
        fclose(fp);
        return 0;
    }
    if (memptr+bytecount > 0xffff) {
        fprintf(stderr, "memptr+bytecount out of range.\n");
        fclose(fp);
        return 1;
    }
    rewind(fp);
    int fd = ttyopen(argv[1]);
    if (fd < 0) {
        fprintf(stderr, "Could not open %s\n", argv[1]);
        return 1;
    }
    char s[0x40];
    sendcmdp(fd, ": K KEY DUP EMIT ;");
    // P: parse digit. We assume '0-9' or 'a-f' range and return a 0-15 value.
    sendcmdp(fd, ": P DUP '0' '9' =><= IF '0' ELSE $57 THEN - ;");
    // R: receive hex pairs. we receive values in hex pairs, re-emit them upon
    //    reception, and then write them to memory.
    sprintf(s,
        ": R %d %d DO K P 16 * K P OR I C! LOOP ; R",
        memptr+bytecount, memptr);
    sendcmd(fd, s);

    int returncode = 0;
    while (fread(s, 1, 1, fp)) {
        unsigned char c1, c2;
        c1 = s[0];
        sprintf(s, "%02x", c1);
        for (int i=0; i<2; i++) {
            c1 = s[i];
            write(fd, &c1, 1);
            usleep(1000); // let it breathe
            mread(fd, &c2, 1); // read ping back
            if (c1 != c2) {
                // mismatch!
                unsigned int pos = ftell(fp);
                fprintf(stderr, "Mismatch at byte %d! %d != %d.\n", pos, c1, c2);
                // we don't exit now because we need to "consume" our whole program.
                returncode = 1;
            }
        }
        putc('.', stderr);
        fflush(stderr);
    }
    readprompt(fd);
    sendcmdp(fd, "FORGET K");
    fprintf(stderr, "Done!\n");
    fclose(fp);
    if (fd > 0) {
        close(fd);
    }
    return returncode;
}

