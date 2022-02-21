3 CONSTS $ff00 RS_ADDR $fffa PS_ADDR 0 HERESTART
RS_ADDR $80 - CONSTANT SYSVARS
SYSVARS $409 - CONSTANT BLK_MEM
ARCHM XCOMP Z80A XCOMPC Z80C COREL
CODE (emit)
  A 1 LDri, 1 OUTiA, A C LDrr, 0 OUTiA, BC POP, ;CODE
CODE (key?) ( TODO: make non-blocking )
  BC PUSH, BEGIN, 1 INAi, A INCr, BR JRZ,
  A DECr, PUSHA, C 1 LDri, ;CODE
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
