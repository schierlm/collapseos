0xff00 CONSTANT RS_ADDR
0xfffa CONSTANT PS_ADDR
RS_ADDR 0xa0 - CONSTANT SYSVARS
0 CONSTANT HERESTART
5 LOAD  ( z80 assembler )
280 LOAD  ( boot.z80.decl )
200 205 LOADR  ( xcomp )
281 300 LOADR ( boot.z80 )
210 231 LOADR  ( forth core low )
: (emit) 0 PC! ;
: (key?) 0 PC@ 1 ;
: EFS@
    1 3 PC! ( read )
    256 /MOD 3 PC! 3 PC! ( blkid )
    BLK( 256 /MOD 3 PC! 3 PC! ( dest )
;
: EFS!
    2 3 PC! ( write )
    256 /MOD 3 PC! 3 PC! ( blkid )
    BLK( 256 /MOD 3 PC! 3 PC! ( dest )
;
: COLS 80 ; : LINES 32 ;
: AT-XY 6 PC! ( y ) 5 PC! ( x ) ;

236 239 LOADR ( forth core high )
XWRAP" BLK$ ' EFS@ ' BLK@* **! ' EFS! ' BLK!* **! "
