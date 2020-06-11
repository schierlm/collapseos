; tape blkdev read-only
; to be included as a kernel module.
; In the glue.asm devices list the PutB pointer has to be unsetZ 

; defines:
; tap_buffer = 256-byte tape loading buffer in RAM
; buf_pos = read position in the buffer
; crossing the buffer boundaries require loading or rewinding+loading
; tap_pos = previous read position of the block device,
; then the difference btw current and previous positions, both in EDLH format as throughout the kernel code

tapeblk_init:
; initialized CFS and a placeholder 1-block, 1-byte file in the buffer
; the tape fs is not default, has to be mounted
ld hl, .plh
ld de, tap_buffer
ld bc, 6
ldir
ret
.plh:
.db "CFS",1,1,0,'@'

tapeGetB:
; it gets the new position in DE/HL, has to return value in A
push bc
push ix

; First of all, is the difference between positions negative or positive?
push hl ; store the position
push de
ld ix, tap_pos ; previous

push de ; working copy, higher bytes
ld e, (ix+2) ; lower bytes of previous position
ld d, (ix+3)
scf
ccf
sbc hl,de
ld (ix+6), l
ld (ix+7), h
pop hl
ld e, (ix+0) ; higher bytes
ld d, (ix+1)
sbc hl,de
ld (ix+4), l
ld (ix+5), h
jp nc, .tblk_posdif

.tblk_negdif:
; at this point we have the negative difference
pop de ; restore the current position
pop hl
; store it as 'the previous'
ld (ix+0), e
ld (ix+1), d
ld (ix+2), l
ld (ix+3), h

; let's set the buffer position while we're here
ld a, (buf_pos)
add a, (ix+6) ; l
; the difference bytes are negative, so e.g. add 255 = sub 1
ld (buf_pos), a
; no carry would mean underflow in this case
jp nc, .tblk_rewind
; we now have a chance that the higher bytes are FF (due to lower CY)
xor a
dec a
and (ix+7)
and (ix+4)
and (ix+5)
cp 255
jp z, .tblk_readbyte ; a negative difference within the buffer

; we have to rewind the tape and load back to the current position,
; so it's safe to discard the difference and treat the current position as the positive difference

.tblk_rewind:
; as we will rewind to zero, at least one additional block is to be loaded
xor a
inc h
cp h
jr nz, .tblk_store
inc de
.tblk_store: 
ld (ix+4), e ; diff
ld (ix+5), d
ld (ix+6), l ; diff
ld (ix+7), h

; purple border means 'rewind the tape and press enter'
di
ld a, 3
out (254), a
.tblk_key:
ld a, 191  ; waiting for enter
in a,(254)
rra
jr c, .tblk_key
ei
jr .tblk_skip
; we don't have to set the buffer position, done it already

.tblk_posdif:
; at this point we have the difference and know it is positive
pop de ; restore the current position
pop hl
; store it as 'the previous'
ld (ix+0), e
ld (ix+1), d
ld (ix+2), l
ld (ix+3), h

.tblk_buffer:
; setting the buffer position for the positive difference
ld a, (buf_pos)
add a, (ix+6) ; l
ld (buf_pos), a
jr nc, .tblk_skip

; now we increase the higher difference bytes due to overflow
xor a
inc (ix+7) ; h
cp (ix+7)
jr nz, .tblk_skip
inc (ix+4) ; e
cp (ix+4)
jr nz, .tblk_skip
inc (ix+5) ; d 

.tblk_skip:
; Now, how many tape blocks do we have to load before the target block appears in the buffer?
; it is shown by the 3 higher bytes of the difference
; We've just set them up for the positive difference case.

; For the negative difference case, the L-byte has to be equal to the buf_pos we set earlier

; (ix+7) H, (ix+4) E, (ix+5) D is now the counter for blocks to be loaded
; if it's 0, the block is already at the buffer and we don't have to load anything
xor a
or (ix+7)
or (ix+4)
or (ix+5)
jp z, .tblk_readbyte

; well, let's play the tape
ld a, (ix+5)
ld b,a ; this is the outer cycle
inc b ; as we will djnz
ld a, (ix+7)
ld l, a ; lower byte of the inner counter
ld a, (ix+4)
ld h, a ; higher byte
dec hl ; as we know at this point that at least one block is to be loaded
ld c, 0 ; it's a 16-bit cycle flag used below
xor a
or h
or l
jp z, .tblk_load
ld c, 1 ; hl=nonzero  

.tblk_load:
push bc ; counters
push hl
ld ix, tap_buffer ; we don't need the ix value anymore
ld de, 256
ld a, 255
call t_load
pop hl
pop bc

; counter
xor a
dec hl
or h
or l
jr nz, .tblk_ccheck
ld c, 0 ; on the next cycle, b has to be decremented
jr .tblk_load
.tblk_ccheck:
xor a
or c
jp nz, .tblk_load
inc c ; next 16-bit cycle
djnz .tblk_load

.tblk_readbyte:
ld hl, tap_buffer
ld b, 0
ld a, (buf_pos)
ld c, a
add hl,bc
ld a, (hl) ; here it is!
pop ix
pop bc
cp a
ret

t_load:
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

;end