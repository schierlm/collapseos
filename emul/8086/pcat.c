#include <stdint.h>
#include <stdio.h>
#include <unistd.h>
#include <curses.h>
#include "cpu.h"

#define WCOLS 80
#define WLINES 25
#define SEC_PER_TRK 0x24
#define HEADCNT 2

extern uint8_t byteregtable[8];
extern union _bytewordregs_ regs;
extern uint16_t segregs[4];
extern INTHOOK INTHOOKS[0x100];
extern uint8_t zf;

static FILE *fp;
WINDOW *bw, *dw, *w;
int lastkey = ERR;

static void int10() {
    uint8_t cmd = regs.byteregs[regah];
    uint8_t al = regs.byteregs[regal];
    uint16_t dx = regs.wordregs[regdx];
    switch (cmd) {
        case 0x02: // at-xy
            wmove(w, dx>>8, dx&0xff);
            break;
        case 0x0e: // emit
        if (al >= 0x20 || al == '\n') {
            wechochar(w, al);
        } else if (al == 0x08) {
            int y, x; getyx(w, y, x);
            wmove(w, y, x-1);
        }
        break;
    }
}

static void int13() {
    uint8_t cmd = regs.byteregs[regah];
    uint8_t al = regs.byteregs[regal];
    uint8_t cl = regs.byteregs[regcl];
    uint8_t ch = regs.byteregs[regch];
    uint8_t dh = regs.byteregs[regdh];
    uint16_t bx = regs.wordregs[regbx];
    switch (cmd) {
        case 0x02: // read disk sector(s)
        case 0x03: // write disk sector(s)
        // CL = sector number (1-based), AL = sector count
        // DH = head number, CH = track number
        // ES:BX = dest addr
        fseek(fp, (((ch*HEADCNT*SEC_PER_TRK)+(dh*SEC_PER_TRK))+cl-1)*512, SEEK_SET);
        for (int i=0; i<(al*512); i++) {
            if (cmd == 0x03) { // write
                fputc(getmem8(segregs[reges], bx+i), fp);
            } else { // read
                putmem8(segregs[reges], bx+i, fgetc(fp));
            }
        }
        break;

        case 0x08: // poll sectors per track / per head
        // we just report a lot of them
        regs.wordregs[regcx] = SEC_PER_TRK;
        regs.byteregs[regdh] = HEADCNT-1;
        break;
    }
}

static void int16() {
    uint8_t cmd = regs.byteregs[regah];
    switch (cmd) {
        case 0x00: // get keystroke
            while (lastkey == ERR) {
                lastkey = wgetch(w);
            }
            regs.byteregs[regal] = lastkey;
            lastkey = ERR;
            break;
        case 0x01: // check for keystroke
            if (lastkey == ERR) {
                lastkey = wgetch(w);
            }
            zf = lastkey == ERR;
            regs.byteregs[regal] = lastkey;
            break;
    }
}

static void usage()
{
    fprintf(stderr, "Usage: ./pcat /path/to/fd\n");
}

int main(int argc, char *argv[])
{
    if (argc < 2) {
        usage();
        return 1;
    }
    INTHOOKS[0x10] = int10;
    INTHOOKS[0x13] = int13;
    INTHOOKS[0x16] = int16;
    reset86(0x7c00);
    // initialize memory
    fp = fopen(argv[1], "r");
    if (!fp) {
        fprintf(stderr, "Can't open %s\n", argv[1]);
        return 1;
    }
    // read MBR into RAM
    for (int i=0; i<512; i++) {
        int c = getc(fp);
        if (c != EOF) {
            write86(0x7c00+i, c);
        }
    }
    uint16_t magic = readw86(0x7dfe);
    if (magic != 0xaa55) {
        fprintf(stderr, "Invalid MBR magic %x\n", magic);
        return 1;
    }
    initscr(); cbreak(); noecho(); nl(); clear();
    // border window
    bw = newwin(WLINES+2, WCOLS+2, 0, 0);
    // TODO: I can't get this to work properly...
    nodelay(bw, TRUE);
    wborder(bw, 0, 0, 0, 0, 0, 0, 0, 0);
    wrefresh(bw);
    // debug panel
    dw = newwin(1, 30, LINES-1, COLS-30);
    w = newwin(WLINES, WCOLS, 1, 1);
    scrollok(w, 1);
    while (exec86(100));
    nocbreak(); echo(); delwin(w); delwin(bw); delwin(dw); endwin();
    printf("\nDone!\n");
    fclose(fp);
    return 0;
}
