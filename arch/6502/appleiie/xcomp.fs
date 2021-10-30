120 LOAD ARCHM 6502A XCOMPL 6502H
\ BIN( of $6000 is temporary until we don't need to run BASIC
\ besides it any more. To avoid conflict, do "HIMEM 24575"
$6000 TO BIN(
\ Very little empty space in ZP!
$06 TO IPL $08 TO AL $1a TO INDJ
: dump 0 HERE ORG - >R 0 >A BEGIN ( sum )
  HERE R@ - C@ TUCK + SWAP .x SPC> A+ A> 16 = IF
    ." C: " . 0 NL> 0 >A THEN NEXT DROP ;
XCOMPH 6502C
dump
