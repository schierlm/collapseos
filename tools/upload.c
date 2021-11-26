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
    // P: Parse digit. We assume '0-9' or 'a-f' range and return a 0-15 value.
    sendcmdp(fd, ": P DUP '0' '9' =><= IF '0' ELSE 87 THEN - ;");
    // PP: parse pair
    sendcmdp(fd, ": PP KEY KEY P SWAP P << << << << OR ;");
    // R: receive hex pairs. we receive values in hex pairs, combine them, write
    //    them, reread them and spit them back.
    // We *have* to use C! instead of stuff like AC!+ to allow for C! overrides.
    sprintf(s,
        ": R %d >A %d >R BEGIN PP A> C! AC@+ .x NEXT ; R",
        memptr, bytecount);
    sendcmd(fd, s);
    usleep(100000); // let it breathe

    int returncode = 0;
    unsigned char c1, c2;
    while (fread(&c1, 1, 1, fp)) {
        sprintf(s, "%02x", c1);
        write(fd, s, 1);
        usleep(1000); // let it breathe
        write(fd, s+1, 1);
        usleep(1000); // let it breathe
        s[2] = 0;
        mread(fd, s, 2); // read ping back
        c2 = strtol(s, NULL, 16);
        if (c1 != c2) {
            // mismatch!
            unsigned int pos = ftell(fp);
            fprintf(stderr, "Mismatch at byte %d! %02x != %02x.\n", pos, c1, c2);
            // we don't exit now because we need to "consume" our whole program.
            returncode = 1;
        }
        putc('.', stderr);
        fflush(stderr);
    }
    readprompt(fd);
    sendcmdp(fd, "FORGET P");
    fprintf(stderr, "Done!\n");
    fclose(fp);
    if (fd > 0) {
        close(fd);
    }
    return returncode;
}

