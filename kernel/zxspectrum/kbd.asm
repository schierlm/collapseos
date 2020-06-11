; the ZX Spectrum BASIC firmware scans the keyboard for ASCII codes on clock interrupts
; this routine just waits for a key and reads its value

k_getc:
;ei
push hl
ld hl, 23611 ; ZXS_FLAGS
res 5, (hl)
.loop:
bit 5, (hl) ; pressed?
jr z, .loop
ld hl, 23560 ; ZXS_LASTK
ld a, (hl)
pop hl
ret
