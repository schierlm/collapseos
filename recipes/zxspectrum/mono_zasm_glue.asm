; zasm
;
; Reads input from specified blkdev ID, assemble the binary in two passes and
; spit the result in another specified blkdev ID.
;
; We don't buffer the whole source in memory, so we need our input blkdev to
; support Seek so we can read the file a second time. So, for input, we need
; GetB and Seek.
;
; For output, we only need PutB. Output doesn't start until the second pass.
;
; The goal of the second pass is to assign values to all symbols so that we
; can have forward references (instructions referencing a label that happens
; later).
;
; Labels and constants are both treated the same way, that is, they can be
; forward-referenced in instructions. ".equ" directives, however, are evaluated
; during the first pass so forward references are not allowed.
;
; *** Requirements ***
; strncmp
; upcase
; findchar
; blkSel
; blkSet
; fsFindFN
; fsOpen
; fsGetB
; _blkGetB
; _blkPutB
; _blkSeek
; _blkTell
; printstr
; printcrlf

.inc "user.h"
.org 0x85e0
.equ USER_RAMSTART USER_CODE

; *** Overridable consts ***
; NOTE: These limits below are designed to be *just* enough for zasm to assemble
; itself. Considering that this app is Collapse OS' biggest app, it's safe to
; assume that it will be enough for many many use cases. If you need to compile
; apps with lots of big symbols, you'll need to adjust these.
; With these default settings, zasm runs with less than 0x1800 bytes of RAM!

; Maximum number of symbols we can have in the global and consts registry
.equ	ZASM_REG_MAXCNT		0xff

; Maximum number of symbols we can have in the local registry
.equ	ZASM_LREG_MAXCNT	0x20

; Size of the symbol name buffer size. This is a pool. There is no maximum name
; length for a single symbol, just a maximum size for the whole pool.
; Global labels and consts have the same buf size
.equ	ZASM_REG_BUFSZ		0x700

; Size of the names buffer for the local context registry
.equ	ZASM_LREG_BUFSZ		0x100

; ******

.inc "err.h"
.inc "ascii.h"
.inc "blkdev.h"
.inc "fs.h"
jp	zasmMain

.inc "core.asm"
.inc "zasm/const.asm"
.inc "lib/util.asm"
.inc "lib/ari.asm"
.inc "lib/parse.asm"
.inc "zasm/util.asm"
.equ	IO_RAMSTART	USER_RAMSTART
.inc "zasm/io.asm"
.equ	TOK_RAMSTART	IO_RAMEND
.inc "zasm/tok.asm"
.equ	INS_RAMSTART	TOK_RAMEND
.inc "zasm/instr.asm"
.equ	DIREC_RAMSTART	INS_RAMEND
.inc "zasm/directive.asm"
.inc "zasm/parse.asm"
.equ	EXPR_PARSE	parseNumberOrSymbol
.inc "lib/expr.asm"
.equ	SYM_RAMSTART	DIREC_RAMEND
.inc "zasm/symbol.asm"
.equ	ZASM_RAMSTART	SYM_RAMEND
.inc "zasm/main.asm"
USER_RAMSTART:
