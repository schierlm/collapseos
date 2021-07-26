3 VALUES RS_ADDR $ff00 PS_ADDR $fffa HERESTART 0
RS_ADDR $80 - VALUE SYSVARS
5 LOAD    ( z80 assembler )
280 LOAD  ( boot.z80.decl )
200 205 LOADR ( xcomp )
281 299 LOADR ( boot.z80 )
210 224 LOADR ( core low )
CODE (emit)
    A 1 LDri, 1 OUTiA, HL POP, A L LDrr, 0 OUTiA,
;CODE
CODE (key?) ( TODO: make non-blocking )
    BEGIN, 1 INAi, A INCr, JRZ, AGAIN,
    A DECr, PUSHA, PUSH1,
;CODE
: _sel ( sec -- )
( 32 sectors per track, 512 tracks per disk )
    32 /MOD ( addr sec trk )
    $0a ( seltrk ) 1 PC! 0 PC! 0 0 PC! ( addr sec )
    $0b ( selsec ) 1 PC! 0 PC! ( addr ) ;
: _ ( addr -- )
    ( get 512 bytes )
    $86 ( readsec ) 1 PC!
    512 0 DO 0 PC@ SWAP C!+ LOOP DROP ;
: (blk@) ( blkno blk( -- )
  SWAP << ( 2x ) 2DUP ( a b a b ) _sel _ 1+ _sel 512 + _ ;
: _ ( addr )
    ( write 512 bytes )
    $0c ( writesec ) 1 PC!
    512 0 DO C@+ 0 PC! LOOP DROP ;
: (blk!) ( blkno blk( -- )
  SWAP << ( 2x ) 2DUP ( a b a b ) _sel _ 1+ _sel 512 + _ ;
230 233 LOADR \ BLK subsystem
: FD$ ( select disk 0 )
    $09 ( seldisk ) 1 PC! 0 0 PC! ( sel disk 0 ) ;
: INIT BLK$ FD$ ;
XWRAP INIT
