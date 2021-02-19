( WIP: only works if you comment out the LOADR+ in B353 )
50 LOAD ( 6809 assembler )
0x8000 CONSTANT PS_ADDR 0x7f00 CONSTANT RS_ADDR
0x7e00 CONSTANT SYSVARS
0xc000 BIN( !
262 LOAD  ( xcomp )
270 LOAD  ( xcomp overrides )
470 476 LOADR ( boot.6809 )
353 354 LOADR
477 LOAD
ORG @ |M 2 PC! 2 PC!
HERE |M 2 PC! 2 PC!
