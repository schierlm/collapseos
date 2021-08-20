\ This is xcomp code that is common to both serial and grid
\ binaries.
2 VALUES PS_ADDR $fffa RS_ADDR $ff00
RS_ADDR $90 - VALUE SYSVARS
SYSVARS $80 + VALUE GRID_MEM
ASML
0 VALUE JROFF 1 VALUE JROPLEN
530 535 LOADR \ HAL layer for CVM
XCOMPL HALC XCOMPH

HERE TO ORG
$15 ALLOT0
( END OF STABLE ABI )
0 TO lblnext 1 TO lblexec
CODE FIND 2 JMPi,
CODE []= 3 JMPi,
CODE PC! POPp, OUTwp, DROPp, ;CODE
CODE PC@ POPp, INw, PUSHp, ;CODE
COREL
530 535 LOADR \ HAL layer for CVM
HALC
: (key?) 0 PC@ 1 ;
: _ ( n blk( -- ) SWAP ( blk( n )
  ( n ) L|M 3 PC! 3 PC! ( blkid )
  ( blk( ) L|M 3 PC! 3 PC! ( dest ) ;
: (blk@) 1 3 PC! ( read ) _ ;
: (blk!) 2 3 PC! ( write ) _ ;
BLKSUB
( fork between grid and serial begins here )
