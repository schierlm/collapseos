2 VALUES RS_ADDR $ff00 PS_ADDR $fffa
RS_ADDR $80 - VALUE SYSVARS
8086A XCOMPL 8086M 8086H HALC XCOMPH 8086C COREL
CODE (blk@) BX POPx, AX POPx, 4 INT, ;CODE
CODE (blk!) BX POPx, AX POPx, 5 INT, ;CODE
BLKSUB
CODE (emit) AX POPx, 1 INT, ;CODE
CODE (key?) 2 INT, AH 0 MOVri, AX PUSHx, AX PUSHx, ;CODE
2 VALUES COLS 80 LINES 25
CODE AT-XY ( x y ) BX POPx, AX POPx, 3 INT, ;CODE
: INIT BLK$ ;
XWRAP
