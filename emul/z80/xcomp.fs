$ff00 VALUE RS_ADDR
$fffa VALUE PS_ADDR
RS_ADDR $80 - VALUE SYSVARS
Z80A XCOMPL Z80M 15 19 LOADR 4 LOAD
XCOMPH Z80C COREL 15 19 LOADR
: _ ( n blk( -- ) SWAP ( blk( n )
  ( n ) 256 /MOD 3 PC! 3 PC! ( blkid )
  ( blk( ) 256 /MOD 3 PC! 3 PC! ( dest ) ;
: (blk@) 1 3 PC! ( read ) _ ;
: (blk!) 2 3 PC! ( write ) _ ;
BLKSUB
: (emit) 0 PC! ;
: (key?) 0 PC@ 1 ;
: COLS 80 ; : LINES 32 ;
: AT-XY 6 PC! ( y ) 5 PC! ( x ) ;
: INIT BLK$ ;
XWRAP
