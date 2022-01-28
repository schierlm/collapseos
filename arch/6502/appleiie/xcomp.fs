1 CONSTS $5f80 SYSVARS
120 LOAD ARCHM 6502A 6502M XCOMPL 6502H 
\ BIN( of $6000 is temporary until we don't need to run BASIC
\ besides it any more.
$6000 TO BIN(
\ Very little empty space in ZP!
$06 TO IPL $08 TO 'N $1a TO INDJ
XCOMPH 6502C COREL
2 CONSTS IPL IPL IPH IPH \ messy workaround...
6502H ASMH
350 LOAD \ Apple IIe drivers
CODE INIT ;CODE
XWRAP
