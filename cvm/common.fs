\ This is xcomp code that is common to both serial and grid
\ binaries.
2 CONSTS $fffa PS_ADDR $ff00 RS_ADDR
$fe00 VALUE SYSVARS \ synced with vm.c
SYSVARS $409 - VALUE BLK_MEM
SYSVARS $80 + VALUE GRID_MEM
GRID_MEM 2 + VALUE RXTX_MEM
ARCHM XCOMP CVMA XCOMPC CVMC COREL 
309 LOAD \ common drivers
BLKSUB
\ fork between grid and serial begins here
