2 CONSTS $ff00 RS_ADDR $fffa PS_ADDR
RS_ADDR $90 - CONSTANT SYSVARS
SYSVARS $80 + CONSTANT DRV_ADDR
DRV_ADDR 2 + CONSTANT GRID_MEM
ARCHM XCOMP 8086A 8086H
XCOMPC 8086C COREL 8086H ASMH
320 324 LOADR ( drivers )
ALIAS FD@ (blk@)
ALIAS FD! (blk!)
BLKSUB GRIDSUB
: INIT GRID$ BLK$ FD$ ;
XWRAP
