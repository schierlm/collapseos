\ TRS-80 Color Computer 2
6809A
3 VALUES PS_ADDR $8000 RS_ADDR $7f00 HERESTART $0600
RS_ADDR $90 - VALUE SYSVARS
SYSVARS $80 + VALUE GRID_MEM
$c000 TO BIN(
XCOMPL ARCHM
6809H HALC XCOMPH 6809C COREL 6809H HALC
310 312 LOADR ( drivers )
GRIDSUB
: INIT GRID$ ;
XWRAP
