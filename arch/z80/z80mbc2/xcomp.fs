3 CONSTS $ff00 RS_ADDR $fffa PS_ADDR 0 HERESTART
RS_ADDR $80 - VALUE SYSVARS
SYSVARS $409 - VALUE BLK_MEM
ARCHM XCOMP Z80A XCOMPC Z80C COREL
CODE (emit)
  A 1 i) ld, 1 i) A out, A C ld, 0 i) A out, BC pop, ;CODE
CODE (key?) ( TODO: make non-blocking )
  BC push, BEGIN, A 1 i) in, A inc, BR CZ jrc,
  A dec, pushA, C 1 i) ld, ;CODE
: _sel ( sec -- )
( 32 sectors per track, 512 tracks per disk )
    32 /MOD ( addr sec trk )
    $0a ( seltrk ) 1 PC! 0 PC! 0 0 PC! ( addr sec )
    $0b ( selsec ) 1 PC! 0 PC! ( addr ) ;
: _ ( addr -- )
    ( get 512 bytes )
    $86 ( readsec ) 1 PC!
    512 >R BEGIN 0 PC@ SWAP C!+ NEXT DROP ;
: (blk@) ( blkno blk( -- )
  SWAP << ( 2x ) 2DUP ( a b a b ) _sel _ 1+ _sel 512 + _ ;
: _ ( addr )
    ( write 512 bytes )
    $0c ( writesec ) 1 PC!
    512 >R BEGIN C@+ 0 PC! NEXT DROP ;
: (blk!) ( blkno blk( -- )
  SWAP << ( 2x ) 2DUP ( a b a b ) _sel _ 1+ _sel 512 + _ ;
BLKSUB
: FD$ ( select disk 0 )
    $09 ( seldisk ) 1 PC! 0 0 PC! ( sel disk 0 ) ;
: INIT BLK$ FD$ ;
XWRAP
