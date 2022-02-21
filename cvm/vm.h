#pragma once
/* core types */
#define true 1
#define false 0
typedef unsigned char Bool;
typedef unsigned char byte;
typedef unsigned short word;

/* sync with common.fs */
#define MEMSIZE 0x10000
#define SP_ADDR 0xfffa
#define RS_ADDR 0xff00
#define SYSVARS RS_ADDR-0x90
/* Port for block reads. Each read or write has to be done in 5 IO writes:
 * 1 - r/w. 1 for read, 2 for write.
 * 2 - blkid MSB
 * 3 - blkid LSB
 * 4 - dest addr MSB
 * 5 - dest addr LSB */
#define BLK_PORT 0x03
/* About memory: the C VM is designed to run on modern POSIX environments, but
 * also on 16-bit DOS ones, where 64K of memory is the limit. In these
 * environments, we can't bluntly allocate 64K of VM memory. Instead, we
 * allocate 32K and mirror it. Most of Collapse OS applications don't use more
 * than 32K, so it works fine. */

typedef byte (*IORD) ();
typedef void (*IOWR) (byte data);

typedef struct {
    byte mem[MEMSIZE];
    word SP; /* parameter Stack Pointer */
    word RS; /* Return Stack pointer */
    word IP; /* Interpreter Pointer */
    word PC; /* Program Counter for HAL bytecode interpreter */
/* Array of 0x100 function pointers to IO read and write routines. Leave to
 * NULL when IO port is unhandled. */
    IORD iord[0x100];
    IOWR iowr[0x100];
/* Used for keeping track of max RS and min SP during the lifetime of the
 * program. Useful for debugging. */
    word maxRS;
    word minSP;
    Bool running;
} VM;

VM* VM_init(char *bin_path, char *blkfs_path);
void VM_deinit();
Bool VM_steps(int n);
word VM_PS_pop();
void VM_PS_push(word n);
void VM_memdump();
void VM_debugstr(char *s);
void VM_printdbg();
