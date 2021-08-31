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
0 TO lblnext
CODE FIND 2 JMPi,
CODE []= 3 JMPi,
CODE PC! 4 JMPi,
CODE PC@ 5 JMPi,
CODE * 6 JMPi,
CODE /MOD 7 JMPi,
CODE QUIT 8 JMPi,
CODE ABORT 9 JMPi,
CODE RCNT 10 JMPi,
CODE SCNT 11 JMPi,
CODE BYE 12 JMPi,
CODE EXECUTE 13 JMPi,
COREL
530 534 LOADR \ HAL layer for CVM
HALC
: (key?) 0 PC@ 1 ;
: _ ( n blk( -- ) SWAP ( blk( n )
  ( n ) L|M 3 PC! 3 PC! ( blkid )
  ( blk( ) L|M 3 PC! 3 PC! ( dest ) ;
: (blk@) 1 3 PC! ( read ) _ ;
: (blk!) 2 3 PC! ( write ) _ ;
BLKSUB
( fork between grid and serial begins here )
