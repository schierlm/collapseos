2 CONSTS $ff00 RS_ADDR $fffa PS_ADDR
RS_ADDR $80 - CONSTANT SYSVARS
SYSVARS $409 - CONSTANT BLK_MEM
ARCHM XCOMP 8086A XCOMPC 8086C COREL 
CODE (blk@) AX POPx, 4 INT, BX POPx, ;CODE
CODE (blk!) AX POPx, 5 INT, BX POPx, ;CODE
BLKSUB
CODE (emit) AX BX MOVxx, BX POPx, 1 INT, ;CODE
CODE (key?)
  2 INT, AH 0 MOVri, BX PUSHx, BX AX MOVxx, BX PUSHx, ;CODE
2 CONSTS 80 COLS 25 LINES
: INIT BLK$ ;
XWRAP
