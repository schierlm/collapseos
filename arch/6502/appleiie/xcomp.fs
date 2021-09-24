120 LOAD XCOMPL ARCHM 6502A HALC
\ BIN( of $6000 is temporary until we don't need to run BASIC
\ besides it any more. To avoid conflict, do "HIMEM 24575"
$6000 TO BIN(
: dump 0 HERE ORG - 0 DO ( sum )
  I ORG + C@ TUCK + SWAP .x SPC> I 16 MOD 15 = IF
    ." C: " . 0 NL> THEN LOOP DROP ;
XCOMPH 6502C
dump
