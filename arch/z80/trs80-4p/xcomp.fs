3 VALUES RS_ADDR 0xff00 PS_ADDR 0xfffa HERESTART 0
RS_ADDR 0x90 - VALUE SYSVARS
SYSVARS 0x80 + VALUE DRVMEM
5 LOAD   ( z80 assembler )
280 LOAD ( boot.z80.decl )
200 205 LOADR ( xcomp )
0x3000 TO BIN(
281 300 LOADR ( boot.z80 )
210 231 LOADR ( forth core low )
360 366 LOADR ( trs80 )
\ TRS-80 wants CR-only newlines
: INIT CR [*TO] NL BLK$ FD$ ;
236 239 LOADR ( forth core high )
XWRAP INIT
