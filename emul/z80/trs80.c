#include <stdint.h>
#include <stdio.h>
#include <unistd.h>
#include <curses.h>
#include <termios.h>
#include "emul.h"

#define WCOLS 80
#define WLINES 24
#define RAMSTART 0
#define BINSTART 0x3000

WINDOW *bw, *dw, *w;
static FILE *blkfp = NULL;

void debug_panel()
{
    char buf[30];
    emul_debugstr(buf);
    mvwaddnstr(dw, 0, 0, buf, 30);
    wrefresh(dw);
}

static void pchookfunc(Machine *m)
{
    byte val;
    switch (m->cpu.R1.br.A) {
    case 0x01: // @KEY
        debug_panel();
        m->cpu.R1.br.A = wgetch(w);
        break;
    case 0x02: // @DSP
        val = m->cpu.R1.br.C;
        if (val == '\r') {
            val = '\n';
        }
        if (val >= 0x20 || val == '\n') {
            wechochar(w, val);
        } else if (val == 0x08) {
            int y, x; getyx(w, y, x);
            wmove(w, y, x-1);
        }
        break;
    case 0x08: // @KBD TODO: make non-blocking
        debug_panel();
        m->cpu.R1.br.A = wgetch(w);
        m->cpu.R1.br.F |= F_Z;
        break;
    case 0x0f: // @VDCTL
        wmove(w, m->cpu.R1.br.H, m->cpu.R1.br.L);
        break;
    case 0x16: // @EXIT
        m->cpu.halted = true;
        break;
    case 0x28: // @DCSTAT
        // set Z to indicate floppy presence
        if (blkfp != NULL) {
            m->cpu.R1.br.F |= F_Z;
        } else {
            m->cpu.R1.br.F &= ~F_Z;
        }
        break;
    // 0x100b per sector. 10 sector per cylinder, 40 cylinders per floppy.
    // TODO: support swapping floppies. only 1 floppy for now.
    case 0x31: // @RDSEC
    case 0x35: // @WRSEC
        if (blkfp != NULL) {
            fseek(
                blkfp, /* D = cylinder, E = sector */
                ((m->cpu.R1.br.D * 10) + m->cpu.R1.br.E) * 0x100,
                SEEK_SET);
            if (m->cpu.R1.br.A == 0x31) { // @RDSEC
                fread(&m->mem[m->cpu.R1.wr.HL], 0x100, 1, blkfp);
            } else {
                fwrite(&m->mem[m->cpu.R1.wr.HL], 0x100, 1, blkfp);
            }
            m->cpu.R1.br.F |= F_Z;
        }
        break;
    case 0x6a: // @CKBRKC
        m->cpu.R1.br.F |= F_Z;
        break;
    default:
        fprintf(stderr, "Unhandled RST28: %x\n", m->cpu.R1.br.A);
    }
}

static void usage()
{
    fprintf(stderr, "Usage: ./trs80 [-f floppies.img] /path/to/rom\n");
}

int main(int argc, char *argv[])
{
    if (argc < 2) {
        usage();
        return 1;
    }
    int ch;
    while ((ch = getopt(argc, argv, "f:")) != -1) {
        switch (ch) {
            case 'f':
                fprintf(stderr, "Setting up floppies image with %s\n", optarg);
                blkfp = fopen(optarg, "r+");
                if (blkfp == NULL) {
                    fprintf(stderr, "Can't open file\n");
                    return 1;
                }
                break;
        }
    }
    if (optind != argc-1) {
        usage();
        return 1;
    }
    Machine *m = emul_init(argv[optind], BINSTART);
    if (m == NULL) return 1;
    m->ramstart = RAMSTART;
    m->pchookfunc = pchookfunc;
    m->pchooks_cnt = 1;
    m->pchooks[0] = 0x28; // RST 28
    // Place a RET at 0x28 so that it properly returns after pchookfunc().
    m->mem[0x28] = 0xc9; // RET
    m->cpu.PC = BINSTART;
    initscr(); cbreak(); noecho(); nl(); clear();
    // border window
    bw = newwin(WLINES+2, WCOLS+2, 0, 0);
    wborder(bw, 0, 0, 0, 0, 0, 0, 0, 0);
    wrefresh(bw);
    // debug panel
    dw = newwin(1, 30, LINES-1, COLS-30);
    w = newwin(WLINES, WCOLS, 1, 1);
    scrollok(w, 1);
    while (emul_steps(1000)) {
        debug_panel();
    }
    nocbreak(); echo(); delwin(w); delwin(bw); delwin(dw); endwin();
    printf("\nDone!\n");
    emul_printdebug();
    printf("PC: %x\n", m->cpu.PC);
    return 0;
}
