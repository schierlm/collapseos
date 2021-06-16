/* Common code between all z80 emulators
*/

#include <string.h>
#include "emul.h"

static Machine m;
static ushort traceval = 0;

static uint8_t io_read(int unused, uint16_t addr)
{
    addr &= 0xff;
    IORD fn = m.iord[addr];
    if (fn != NULL) {
        return fn();
    } else {
        fprintf(stderr, "Out of bounds I/O read: %d\n", addr);
        return 0;
    }
}

static void io_write(int unused, uint16_t addr, uint8_t val)
{
    addr &= 0xff;
    IOWR fn = m.iowr[addr];
    if (fn != NULL) {
        fn(val);
    } else {
        fprintf(stderr, "Out of bounds I/O write: %d / %d (0x%x)\n", addr, val, val);
    }
}

uint8_t emul_mem_read(int unused, uint16_t addr)
{
    return m.mem[addr];
}

void emul_mem_write(int unused, uint16_t addr, uint8_t val)
{
    if (addr < m.ramstart) {
        fprintf(stderr, "Writing to ROM (%d)!\n", addr);
        emul_memdump();
        fprintf(stderr, "Press any key to continue...\n");
        while (getchar() > 0x100);
    }
    m.mem[addr] = val;
}

Machine* emul_init(char *binpath, ushort binoffset)
{
    // initialize memory
    memset(m.mem, 0, 0x10000);
    FILE *bfp = fopen(binpath, "r");
    if (!bfp) {
        fprintf(stderr, "Can't open %s\n", binpath);
        return NULL;
    }
    int i = binoffset;
    int c = getc(bfp);
    while (c != EOF) {
#ifdef MAX_ROMSIZE
        if (i >= MAX_ROMSIZE) {
            fprintf(stderr, "ROM image too large.\n");
            fclose(fp);
            return NULL;
        }
#endif
        m.mem[i++] = c;
        c = getc(bfp);
    }
    fclose(bfp);
    m.ramstart = 0;
    m.minsp = 0xffff;
    m.maxix = 0;
    for (int i=0; i<0x100; i++) {
        m.iord[i] = NULL;
        m.iowr[i] = NULL;
    }
    m.pchooks_cnt = 0;
    Z80RESET(&m.cpu);
    m.cpu.memRead = emul_mem_read;
    m.cpu.memWrite = emul_mem_write;
    m.cpu.ioRead = io_read;
    m.cpu.ioWrite = io_write;
    return &m;
}

bool emul_step()
{
    if (!m.cpu.halted) {
        Z80Execute(&m.cpu);
        ushort newsp = m.cpu.R1.wr.SP;
        if (newsp != 0 && newsp < m.minsp) {
            m.minsp = newsp;
        }
        if (m.cpu.R1.wr.IX > m.maxix) {
            m.maxix = m.cpu.R1.wr.IX;
        }
        for (int i=0; i<m.pchooks_cnt; i++) {
            if (m.cpu.PC == m.pchooks[i]) {
                m.pchookfunc(&m);
                break;
            }
        }
        return true;
    } else {
        return false;
    }
}

bool emul_steps(unsigned int steps)
{
    while (steps) {
        if (!emul_step()) {
            return false;
        }
        steps--;
    }
    return true;
}

void emul_loop()
{
    while (emul_step());
}

void emul_trace(ushort addr)
{
    ushort newval = m.mem[addr+1] << 8 | m.mem[addr];
    if (newval != traceval) {
        traceval = newval;
        fprintf(stderr, "trace: %04x PC: %04x\n", traceval, m.cpu.PC);
    }
}

void emul_memdump()
{
    fprintf(stderr, "Dumping memory to memdump. PC %04x\n", m.cpu.PC);
    FILE *fp = fopen("memdump", "w");
    fwrite(m.mem, 0x10000, 1, fp);
    fclose(fp);
}

void emul_debugstr(char *s)
{
    sprintf(s, "SP %04x (%04x) IX %04x (%04x)",
        m.cpu.R1.wr.SP, m.minsp, m.cpu.R1.wr.IX, m.maxix);
}

void emul_printdebug()
{
    fprintf(stderr, "SP/Max: %04x/%04x\n", m.cpu.R1.wr.SP, m.minsp);
    fprintf(stderr, "IX/Max: %04x/%04x\n", m.cpu.R1.wr.IX, m.maxix);
}

byte iord_noop() { return 0; }
void iowr_noop(byte val) {}
