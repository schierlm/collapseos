3 VALUES RS_ADDR $f300 PS_ADDR $f3fa HERESTART 0
RS_ADDR $90 - VALUE SYSVARS
SYSVARS $80 + VALUE DRVMEM
DRVMEM VALUE KBD_MEM
DRVMEM 3 + VALUE GRID_MEM
DRVMEM 6 + VALUE FDMEM
DRVMEM 7 + VALUE UNDERCUR \ char under cursor
120 LOAD \ nC,
Z80A XCOMPL Z80M
360 LOAD \ TRS-80 4P decl
XCOMPH Z80C COREL
361 368 LOADR \ trs80 low
 X' FD@ ALIAS (blk@)
 X' FD! ALIAS (blk!)
BLKSUB GRIDSUB
369 LOAD \ trs80 high
: INIT GRID$ KBD$ BLK$ FD$ ;
XWRAP
