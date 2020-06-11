.equ 	IYBAS 		23610 ; user.h
; The ZX Spectrum firmware requires IY value to be equal to 23610
; (in this case, to handle keyboard interrupts and screen correctly).
; an IM2 handler has to be included to manage this.

.equ	RAMEND		0xffff ; user.h

.equ	USER_CODE	41639 ; = BAS_RAMEND+527 in user.h
; when changing the memory layout, it should have a dummy value for a test assembly,
; then changed to the actual value in relation to the new BAS_RAMEND

.org 0x5efd

jp init

.inc "err.h"
.inc "ascii.h"
.inc "blkdev.h"
.inc "fs.h"

.inc "kernel/zxs/jumps.asm"

.inc "core.asm"
.inc "kernel/str.asm"

.inc "kernel/zxs/vid.asm"
.inc "kernel/zxs/kbd.asm"

.equ GRID_COLS 32
.equ GRID_ROWS 21 ; to avoid CRLF outside of the #2 screen
.equ GRID_SETCELL v_putc
.equ GRID_GETC k_getc
.equ GRID_RAMSTART RAMSTART
.inc "kernel/grid.asm"

.equ	BLOCKDEV_RAMSTART	GRID_RAMEND
.equ	BLOCKDEV_COUNT		4
.inc "kernel/blockdev.asm"
; List of devices
.dw	mmapGetB, mmapPutB
.dw	blk1GetB, blk1PutB
.dw	blk2GetB, blk2PutB
.dw	tapeGetB, unsetZ	;read-only

.equ	STDIO_RAMSTART	BLOCKDEV_RAMEND
.equ	STDIO_GETC	gridGetC 
.equ	STDIO_PUTC	gridPutC
.equ	STDIO_SETCUR	gridSetCurH
.inc "kernel/stdio.asm"

.equ	MMAP_START	0xc000 ; 49152
.equ	MMAP_LEN	RAMEND-MMAP_START+1
; 16K, 64 fs blocks for MMAP FS
.inc "kernel/mmap.asm"

.equ	FS_RAMSTART	STDIO_RAMEND
.equ	FS_HANDLE_COUNT	2
.inc "kernel/fs.asm"

; BASIC shell
; RAM space used in different routines for short term processing.
.equ	SCRATCHPAD_SIZE	STDIO_BUFSIZE
.equ	SCRATCHPAD	FS_RAMEND
.inc "lib/util.asm"
.inc "lib/ari.asm"
.inc "lib/parse.asm"
.inc "lib/fmt.asm"
.equ	EXPR_PARSE	parseLiteralOrVar
.inc "lib/expr.asm"
.inc "basic/util.asm"
.inc "basic/parse.asm"
.inc "basic/tok.asm"
.equ	VAR_RAMSTART	SCRATCHPAD+SCRATCHPAD_SIZE
.inc "basic/var.asm"
.equ	BUF_RAMSTART	VAR_RAMEND
.equ	BUF_POOLSIZE	0x800 ; 0x1000 by default, cut to save some RAM
.equ	BUF_POOL	shell_buf ; in contended memory
.equ	BUF_MAXLINES	0x100
.equ	BUF_LINES	BUF_RAMSTART+4
.equ	BUF_RAMEND	@+BUF_MAXLINES*4 ; continue allocating in higher RAM
.inc "basic/buf.asm"
.equ	BFS_RAMSTART	BUF_RAMEND
.inc "basic/fs.asm"
.inc "basic/blk.asm"
.equ	BAS_RAMSTART	BFS_RAMEND
.inc "basic/main.asm"

; BASIC records the SP value, which is glue init value-2; the address of this storage +6 is BAS_RAMEND
; this is the value to be learned from a memory dump for user.h!

.equ 	tap_buffer	BAS_RAMEND
.equ	buf_pos		@+256
.equ	tap_pos		@+1
.equ	TAP_RAMEND	@+8 ; user.h for assembling zxs/tapeutil.bin, BAS_RAMEND+265
.inc "kernel/zxs/tapeblk.asm"

;.equ 	USER_CODE	BAS_RAMEND+527 ; 265 tapeblk + 262 tapeutil.bin below

.equ	ZBCOUNT		USER_CODE+14
tpztell:
; fetches the zasm internal counter (IO_OUT_BLK) at ZASM_RAMSTART+14 
; it's called through 'addr ztell: s=a: usr s: print h'
ld hl, (ZBCOUNT)
xor a
ret

basFindCmdExtra:
	ld	hl, basBLKCmds
	call	basFindCmd
	ret	z
	ld	hl, basFSCmds
	call	basFindCmd
	ret	z
	ld	hl, .mycmds
	call	basFindCmd
	ret	z
	jp	basPgmHook
.mycmds:
.db "binsv", 0
.dw tapeutil
.db "binld", 0
.dw tapeutil+3
.db "head", 0
.dw tapeutil+6
.db "filsv", 0
.dw tapeutil+9
.db "filld", 0
.dw tapeutil+12
.db "cfssv", 0
.dw tapeutil+15
.db "cfsld", 0
.dw tapeutil+18
.db "cutsv", 0
.dw tapeutil+21
.db "ztell", 0
.dw tpztell
.db "ed", 0
.dw edrun
.db "zasm", 0
.dw zasmrun
.db 0xff 

init:
di
ld sp, 0x8000
; if precise timings are needed,
; the stack should be moved to non-contended memory
call int_init
call v_init
call tapeblk_init

; init a FS in mmap
; possibly not needed in the final build
	ld	hl, MMAP_START
	ld	a, 'C'
	ld	(hl), a
	inc	hl
	ld	a, 'F'
	ld	(hl), a
	inc	hl
	ld	a, 'S'
	ld	(hl), a

call	gridInit

call	fsInit
	xor	a
	ld	de, BLOCKDEV_SEL
	call	blkSel
	call	fsOn

call	basInit
ld	hl, basFindCmdExtra
ld	(BAS_FINDHOOK), hl

ei
jp	basStart

; *** blkdev 1: file handle 0 ***

blk1GetB:
	ld	ix, FS_HANDLES
	jp	fsGetB

blk1PutB:
	ld	ix, FS_HANDLES
	jp	fsPutB

; *** blkdev 2: file handle 1 ***

blk2GetB:
	ld	ix, FS_HANDLES+FS_HANDLE_SIZE
	jp	fsGetB

blk2PutB:
	ld	ix, FS_HANDLES+FS_HANDLE_SIZE
	jp	fsPutB

int_init:
di
ld a,0x80
ld i,a
im 2
ret ; does not enable interrupts yet

tapeutil:
.bin "zxs/tapeutil.bin"
; all tape utilities in one block with library
; t_load ROM call routine is duplicated in zxs/tapeblk.asm in case those utilities are moved to userspace

; 0x7492 is free space, +2K=7c92, the rest 0x36e=878b for stack space
shell_buf:
; basic shell BUF_POOL points here

.fill 0x8000-$
; The ZX Spectrum hardware noises the CPU data bus so that any value can appear on interrupt instead of 0xFF.
; A 257-byte table is thus required to hold the INT handler address for IM2 mode.

; interrupt table = 128 words + 1 byte
.dw 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181
.dw 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181
.dw 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181
.dw 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181
.dw 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181
.dw 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181
.dw 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181
.dw 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181
.dw 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181
.dw 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181
.dw 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181
.dw 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181
.dw 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181
.dw 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181
.dw 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181
.dw 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181, 0x8181
.db 0x81

; the handler and the table are to reside in the non-contended memory, i.e. in 0x8000-0xC000
; (or rather in 0x8080-0xBFBF)

.fill 0x8181-$

; *** interrupt handler
; other possible int routines here, e.g. RS232 or debug calls
push iy
ld iy, IYBAS
rst 56
pop iy
ei
ret

;0x818c
edrun:
.bin "zxs/ed.bin" ;1108 = 0x0454

;0x85e0 = 0x818c+0x454
zasmrun:
.bin "zxs/zasm.bin" ;4881 = 0x1311

RAMSTART: ; 0x98f1 = 0x85e0+0x1311 (39153)
; bin length (14836)
 
 