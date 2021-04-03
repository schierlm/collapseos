0xff00 CONSTANT RS_ADDR        0xfffa CONSTANT PS_ADDR
RS_ADDR 0xa0 - CONSTANT SYSVARS
0 CONSTANT HERESTART
5 LOAD    ( z80 assembler )
280 LOAD  ( boot.z80.decl )
200 205 LOADR ( xcomp )
281 303 LOADR ( boot.z80 )
210 231 LOADR ( forth core low )
CODE (emit)
    A 1 LDri, 1 OUTiA, HL POP, A L LDrr, 0 OUTiA,
;CODE
CODE (key?) ( TODO: make non-blocking )
    BEGIN, 1 INAi, A INCr, JRZ, AGAIN,
    A DECr, PUSHA, PUSH1,
;CODE
: _sel ( sec )
( 32 sectors per track, 512 tracks per disk )
    32 /MOD ( addr sec trk )
    0x0a ( seltrk ) 1 PC! 0 PC! 0 0 PC! ( addr sec )
    0x0b ( selsec ) 1 PC! 0 PC! ( addr ) ;
: _ ( addr )
    ( get 512 bytes )
    0x86 ( readsec ) 1 PC!
    512 0 DO 0 PC@ SWAP C!+ LOOP DROP
;
: FD@ 2 * DUP _sel BLK( _ 1+ _sel BLK( 512 + _ ;
: _ ( addr )
    ( write 512 bytes )
    0x0c ( writesec ) 1 PC!
    512 0 DO C@+ 0 PC! LOOP DROP
;
: FD! 2 * DUP _sel BLK( _ 1+ _sel BLK( 512 + _ ;
: FD$ ( select disk 0 )
    0x09 ( seldisk ) 1 PC! 0 0 PC! ( sel disk 0 )
;
236 239 LOADR ( forth core high )
XWRAP" BLK$ FD$ ' FD@ ' BLK@* **! ' FD! ' BLK!* **! "
