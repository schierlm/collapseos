v_init:
call 3435 ; ZXS_CLS
ld a, 2
call 5633 ; ZXS_STRM, current stream = 1, main screen
ret

; the ZX Spectrum BASIC firmware puts the character in A into the current output stream by RST 16

v_putc:
; save all
push hl
push bc
push de
push af
push ix
push iy
ld iy, IYBAS ; restore IY for BASIC
; main
push af ; char
push bc ; curflag
push de ; coords
ld a, 22 ; AT_CTRL, screen position, 22x32
rst 16
pop de
ld a, d
push de
rst 16
pop de
ld a, e
rst 16
pop bc
xor a
cp c
jp z, .char
ld a, 18 ; FLASH_CTRL
rst 16
xor a
inc a ; on
rst 16
pop af
rst 16
ld a, 18 ; FLASH_CTRL
rst 16
xor a ; off
rst 16
jp .rest
.char:
pop af
rst 16
; restore and return
.rest:
pop iy
pop ix
pop af
pop de
pop bc
pop hl
ret
