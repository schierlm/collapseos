; The include file with tape utilities exposed as commands, for kernel build

; *** A library file for MMAP-TAPE bridge, which can be included with the userspace versions of the routines
; (user routines are below, addressed by the jump table)

; required syscalls:
; strncmp
; fsAlloc
; fsblkTell
; the applications require:
; parseDecimal
; fsFindFN

.inc "user.h"
.org 	0x711f ; 0x7492-0x373

; for CUTSV 1st block
.equ	sv_buffer	TAP_RAMEND
; Parsed arguments
; block, then byte length
.equ	par_blocklen	@+256
; length of the parsed filename
.equ	par_namelen	@+3
; pointer to filename in the args
.equ	par_filename	@+1
.equ	TAPUTIL_RAMEND	@+2

jp tpbinsv
jp tpbinld
jp tphead
jp tpfilsv
jp tpfilld
jp tpcfssv
jp tpcfsld
jp tpcutsv

; t_CFSHead
; t_parseName
; t_parseOver
; t_chainEnd
; t_newFile
; t_save
; t_load
; t_stkStor
; t_stkRet

; CFS header preparation (for CUTSV 1st block, for FILLD newfile)
t_CFSHead:
push hl
push bc
push de

; position in ix
push ix

ld hl, sv_buffer
push hl
pop de
inc de
ld bc, 32
xor a
ld (hl), a
ldir ; clear the header

ld ix, sv_buffer ; for generality
ld a, 'C'
ld (ix+0), a
ld a, 'F'
ld (ix+1), a
ld a, 'S'
ld (ix+2), a
ld hl, par_blocklen ; block length stored by parser
ld a, (hl)
ld (ix+3), a
inc hl ; actual file length stored by parser
ld a, (hl)
ld (ix+4), a
inc hl
ld a, (hl)
ld (ix+5), a
ld bc, 6
ld hl, sv_buffer
add hl, bc
push hl
pop de ; destination for the parsed filename transfer 
ld hl, par_namelen ; filename length stored by the parser
ld c, (hl)
ld hl, (par_filename) ; filename pointer stored by the parser
ldir ; copy filename

pop ix

pop de
pop bc
pop hl
ret

; Parsing of the filename (HL)
t_parseName:
push de
push bc

push hl
push hl
pop de
ld hl, par_filename
ld (hl), e
inc hl
ld (hl), d
pop hl
ld c, 0
.loop:
ld a, (hl)
cp 32
jr z, .end
inc c
inc hl
jr .loop
.end:
push hl
ld hl, par_namelen
ld (hl), c
pop hl
inc hl ; the next argument in HL

pop bc
pop de
ret

; Parsing of the overwrite flag 'o'
t_parseOver:
push de
ld de, .flag
ld a, 1
call strncmp ; Z - equal (overwrite)
pop de
ret
.flag:
.db 'o'

; Search for CFS chain end (for binary/CFS loading)
t_chainEnd:
push hl
ld a, (par_blocklen)
ld hl,.dummy ; placeholder name
call fsAlloc
call fsblkTell ; position in HL
ld bc, MMAP_START
add hl, bc
push hl
pop ix ; chainend in ix

pop hl
ret
.dummy:
.db '@', 0

; New file at the CFS chain end (for binary loading into CFS)
t_newFile:
push de
push hl
push bc

call t_chainEnd
call t_CFSHead
; start of the new file in ix
push ix
pop de
ld hl, sv_buffer
ld bc, 32
ldir ; copy header to CFS
push de
pop ix ; loading position in ix

pop bc
pop hl
pop de
ret

; SAVE-BYTES ROM call
; header/bytes flag set outside
t_save:
;ld ix,addr
;ld de,len
;ld a,head
push iy
ld iy, IYBAS
; one can not call directly, as RST8 is then called upon break
call 0x04c6 ; SA-BYTES+4 to skip SA/LDRET
jr t_ldret

; LOAD-BYTES ROM call
t_load:
;ld ix,addr
;ld de,len
;ld a,head
push iy
ld iy, IYBAS
scf
; one can not call directly, as RST8 is then called upon break
inc d
ex af,af'
dec d
di
ld a, 15
out (254), a
call 1378 ; jump into LD-BYTES
t_ldret:
ld a, (23624) ; restore border
and 0x38
rrca
rrca
rrca
out (254),a
pop iy
ei
ret

; Stack store (CALL)
t_stkStor:
ld (.stkbc), bc
pop bc
ld (.stkret), bc
ld bc, (.stkbc)
push hl
push bc
push de
push ix
push iy
ld bc, (.stkret)
push bc
ret
.stkbc:
.dw 0
.stkret:
.dw 0

; Stack restore (JP)
t_stkRet:
pop iy
pop ix
pop de
pop bc
pop hl
ret

; 2-sec pause between savings
t_pause:
push bc
ld b,100
.loop:
push bc
halt
pop bc
djnz .loop
pop bc
ret

; *** APPLICATIONS ***

tpbinsv:
call t_stkStor
call parseDecimal
jr z, .arglen
ld de, MMAP_LEN
.arglen:
ld ix, MMAP_START
ld a, 255
call t_save
xor a
jp t_stkRet

tpbinld:
call t_stkStor
call parseDecimal
jp nz, t_stkRet
inc hl
call t_parseOver
jr z, .ovlen
push de
call t_chainEnd
pop de
jr .applen
.ovlen:
ld ix, MMAP_START
.applen:
ld a, 255
call t_load
xor a
jp t_stkRet

tphead:
xor a
inc a
call t_stkStor
call parseDecimal
jp nz, t_stkRet ; ERR no addr
ld ix, sv_buffer
ld a,3
ld (ix+0),3
ld (ix+13),e
ld (ix+14),d
inc hl
call parseDecimal
jp nz, t_stkRet ; ERR no len
ld ix, sv_buffer
ld (ix+11),e
ld (ix+12),d
ld de, 17
xor a
call t_save
xor a
jp t_stkRet

tpfilsv:
xor a
inc a
call t_stkStor
call fsFindFN
jp nz, t_stkRet ; ERR file not found
call fsblkTell ; position in HL
ld bc, MMAP_START
add hl, bc
push hl
pop ix
ld b, (ix+3) ; blocks
;xor a
;or b
;jp z, t_stkRet
ld de, 256
ld hl, 0
.loop:
add hl,de
djnz .loop
ld bc,32
sbc hl,bc
push hl
pop de
push ix
pop hl
add hl,bc
push hl
pop ix
ld a, 255
call t_save
xor a
jp t_stkRet

tpfilld:
call t_stkStor
ld (par_filename), hl
ld b,26
ld c,0
.loop:
ld a,(hl)
cp 32
jr z, .namlen
inc c
inc hl
djnz .loop
.namlen:
ld a, c
ld (par_namelen), a
inc hl
call parseDecimal
jp nz, t_stkRet ; something wrong
ld (par_blocklen+1), de
inc hl
push hl
push de
pop hl
ld de, 256
ld b,1
.div:
sbc hl,de
jr c, .blklen
inc b
jr .div
.blklen:
ld a,b
ld (par_blocklen), a
pop hl
call t_parseOver
jr z, .whole
call t_newFile ; load to ix
jr .load
.whole:
call t_CFSHead ; in sv_buffer
ld de, MMAP_START
ld hl, sv_buffer
ld bc, 32
ldir
push de
pop ix
.load:
ld de, (par_blocklen+1)
ld a,255
call t_load
xor a
jp t_stkRet

tpcfssv:
call t_stkStor
xor a
cp (hl)
jp z, .whole
call fsFindFN
jp nz, .err
call fsblkTell ; position in HL
ld bc, MMAP_START
add hl, bc
push hl
pop ix
ld b, (ix+3) ; blocks
;xor a
;or b
;jp z, t_stkRet
ld de, 256
jr .len 
.whole:
ld de, 256
ld ix, MMAP_START
ld hl, MMAP_LEN
ld b,0
.div:
sbc hl,de
jr c, .len
inc b
jr .div
.len:
; blocklen in B
xor a
or b
jp z, .err
push ix
pop hl
sbc hl,de
push hl
.loop:
pop ix
ld de, 256
add ix, de
ld a, 255
push ix
push bc
call t_save
pop bc
call t_pause
djnz .loop
pop ix
xor a
jp t_stkRet
.err:
xor a
inc a
jp t_stkRet

tpcfsld:
call t_stkStor
call parseDecimal
jr z, .arglen
.deflen:
ld b, 1
jr .flag
.arglen:
xor a
or d
jr nz, .deflen
ld b, e
ld a, b
ld (par_blocklen), a
.flag:
push bc ; cycle counter
inc hl
call t_parseOver
jr z, .whole
call t_chainEnd ; position in ix
jr .load
.whole:
ld ix, MMAP_START
.load:
pop bc
ld de, 256 
; blocklen in B
push ix
pop hl
sbc hl,de
push hl
.loop:
pop ix
ld de, 256
add ix, de
ld a, 255
push ix
push bc
call t_load
pop bc
djnz .loop
pop ix
xor a
jp t_stkRet

tpcutsv:
call t_stkStor
push hl
ld hl, MMAP_START
ld de, .cfslabel
ld a,3
call strncmp
jr nz, .noerr
inc a ; 'CFS'=ERR
pop hl
jp t_stkRet
.cfslabel:
.db "CFS",0
.noerr:
pop hl
cp (hl)
jp z, t_stkRet ; no name provided
ld (par_filename), hl
ld b,26
ld c,0
.loop:
ld a,(hl)
cp 32
jr z, .namlen
inc c
inc hl
djnz .loop
.namlen:
ld a, c
ld (par_namelen), a
inc hl
call parseDecimal
jr z, .arglen
ld de, MMAP_LEN
ld b, 0
jr .deflen
.arglen:
ld b,1
.deflen:
xor a
or d
or e
jp z, t_stkRet ; len=0
ld (par_blocklen+1), de
push de
pop hl
ld de, 256
.div:
sbc hl,de
jr c, .blklen
inc b
jr .div
.blklen:
ld a,b
ld (par_blocklen), a
call t_CFSHead
push bc
ld de, sv_buffer+32
ld hl, MMAP_START
ld bc, 224
ldir
push hl
ld ix, sv_buffer
ld de, 256
ld a, 255
call t_save ; 1st block+meta
pop ix
pop bc
xor a
dec b
jp z, t_stkRet
ld de, 256
push ix
pop hl
sbc hl,de
push hl
.svloop:
pop ix
ld de, 256
add ix, de
ld a, 255
push ix
push bc
call t_save
pop bc
call t_pause
djnz .svloop
pop ix
xor a
jp t_stkRet

;end