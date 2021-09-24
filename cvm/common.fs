\ This is xcomp code that is common to both serial and grid
\ binaries.
2 VALUES PS_ADDR $fffa RS_ADDR $ff00
RS_ADDR $90 - VALUE SYSVARS
SYSVARS $80 + VALUE GRID_MEM
ARCHM ASML XCOMPL CVMH ASMH XCOMPH
CVMC COREL CVMH ASMH
309 LOAD \ common drivers
BLKSUB
\ fork between grid and serial begins here
