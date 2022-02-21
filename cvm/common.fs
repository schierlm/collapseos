\ This is xcomp code that is common to both serial and grid
\ binaries.
2 CONSTS $fffa PS_ADDR $ff00 RS_ADDR
RS_ADDR $90 - CONSTANT SYSVARS
SYSVARS $409 - CONSTANT BLK_MEM
SYSVARS $80 + CONSTANT GRID_MEM
GRID_MEM 2 + CONSTANT RXTX_MEM
ARCHM XCOMP CVMA XCOMPC CVMC COREL 
309 LOAD \ common drivers
BLKSUB
\ fork between grid and serial begins here
