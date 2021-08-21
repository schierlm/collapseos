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
CODE PC! 4 JMPi,
CODE PC@ 5 JMPi,
CODE * 6 JMPi,
CODE /MOD 7 JMPi,
COREL
530 535 LOADR \ HAL layer for CVM
HALC
CODE (key?) 0 INwi, PUSHp, 1 i>w, PUSHp, ;CODE
CODE _ ( n blk( -- )
  POPfp, ( n ) PUSHfp, >>8w, 3 OUTwi, POPfp, 3 OUTwi, ( blkid )
  p>w, ( blk( ) >>8w, 3 OUTwi, POPp, 3 OUTwi, ( dest ) ;CODE
: (blk@) 1 3 PC! ( read ) _ ;
: (blk!) 2 3 PC! ( write ) _ ;
BLKSUB
( fork between grid and serial begins here )
