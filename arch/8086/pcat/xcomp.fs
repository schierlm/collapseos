0xff00 CONSTANT RS_ADDR
0xfffa CONSTANT PS_ADDR
RS_ADDR 0xa0 - CONSTANT SYSVARS
20 LOAD   ( 8086 asm )
400 LOAD ( 8086 decl )
200 205 LOADR ( xcomp )
402 417 LOADR ( 8086 boot code )
210 231 LOADR ( forth core low )
420 424 LOADR ( drivers )
236 239 LOADR ( forth core high )
XWRAP" BLK$ FD$ ' FD@ ' BLK@* **! ' FD! ' BLK!* **! "
