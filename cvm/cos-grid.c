/* this program requires POSIX */
#include <stdio.h>
#include <curses.h>
#include <termios.h>
#include "vm.h"

#ifndef BLKFS_PATH
#define BLKFS_PATH "blkfs"
#endif
#ifndef FBIN_PATH
#define FBIN_PATH "grid.bin"
#endif
#define WCOLS 80
#define WLINES 32
#define STDIO_PORT 0x00
#define COLS_PORT 0x03
#define LINES_PORT 0x04
#define SETX_PORT 0x05
#define SETY_PORT 0x06
#define SCROLL_PORT 0x07

WINDOW *bw, *dw, *w;

void debug_panel()
{
    char buf[30];
    VM_debugstr(buf);
    mvwaddnstr(dw, 0, 0, buf, 30);
    wrefresh(dw);
}

static byte iord_stdio()
{
    int c;
    debug_panel();
    c = wgetch(w);
    return (byte)c;
}

static void iowr_stdio(byte val)
{
    if (val < ' ') val = '~';
    wechochar(w, val);
}

static byte iord_cols() { return WCOLS; }
static byte iord_lines() { return WLINES; }

static void iowr_setx(byte val)
{
    int y, x; getyx(w, y, x);
    wmove(w, y, val);
}

static void iowr_sety(byte val)
{
    int y, x; getyx(w, y, x);
    wmove(w, val, x);
}

static void iowr_scroll(byte val)
{
    scrollok(w, 1);
    wmove(w, WLINES-1, WCOLS-1);
    wprintw(w, "\n");
    scrollok(w, 0);
}

int main(int argc, char *argv[])
{
    VM *vm = VM_init(FBIN_PATH, BLKFS_PATH);
    if (!vm) {
        return 1;
    }
    vm->iord[STDIO_PORT] = iord_stdio;
    vm->iowr[STDIO_PORT] = iowr_stdio;
    vm->iord[COLS_PORT] = iord_cols;
    vm->iord[LINES_PORT] = iord_lines;
    vm->iowr[SETX_PORT] = iowr_setx;
    vm->iowr[SETY_PORT] = iowr_sety;
    vm->iowr[SCROLL_PORT] = iowr_scroll;
    initscr(); cbreak(); noecho(); nl(); clear();
    /* border window */
    bw = newwin(WLINES+2, WCOLS+2, 0, 0);
    wborder(bw, 0, 0, 0, 0, 0, 0, 0, 0);
    wrefresh(bw);
    /* debug panel */
    dw = newwin(1, 30, LINES-1, COLS-30);
    w = newwin(WLINES, WCOLS, 1, 1);
    scrollok(w, 0);
    while (VM_steps(1000)) {
        debug_panel();
    }
    nocbreak(); echo(); delwin(w); delwin(bw); delwin(dw); endwin();
    printf("\nDone!\n");
    fprintf(stderr, "Done!\n");
    VM_printdbg();
    VM_deinit();
    return 0;
}
