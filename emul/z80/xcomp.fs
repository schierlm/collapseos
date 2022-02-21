2 CONSTS $ff00 RS_ADDR $fffa PS_ADDR
RS_ADDR $80 - CONSTANT SYSVARS
SYSVARS $409 - CONSTANT BLK_MEM
ARCHM XCOMP Z80A XCOMPC Z80C COREL 
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
