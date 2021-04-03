0xff00 CONSTANT RS_ADDR 0xfffa CONSTANT PS_ADDR
RS_ADDR 0xb0 - CONSTANT SYSVARS 0 CONSTANT HERESTART
SYSVARS 0xa0 + CONSTANT DRVMEM
5 LOAD   ( z80 assembler )
280 LOAD ( boot.z80.decl )
200 205 LOADR ( xcomp )
0x3000 BIN( !
281 303 LOADR ( boot.z80 )
210 231 LOADR ( forth core low )
360 366 LOADR ( trs80 )
236 239 LOADR ( forth core high )
( TRS-80 wants CR-only newlines )
XWRAP" CR 0x50 RAM+ C! BLK$ FD$ "
