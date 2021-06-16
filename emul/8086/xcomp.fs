2 VALUES RS_ADDR 0xff00 PS_ADDR 0xfffa
RS_ADDR 0x80 - VALUE SYSVARS
20 LOAD   ( 8086 asm )
400 LOAD ( 8086 decl )
200 205 LOADR ( xcomp )
402 417 LOADR ( 8086 boot code )
210 231 LOADR ( forth core low )
CODE (emit) AX POPx, 1 INT, ;CODE
CODE (key?) 2 INT, AH 0 MOVri, AX PUSHx, AX PUSHx, ;CODE
: COLS 80 ; : LINES 25 ;
CODE AT-XY ( x y ) BX POPx, AX POPx, 3 INT, ;CODE
CODE _ BX POPx, AX POPx, 4 INT, ;CODE
: EFS@ BLK( _ ;
CODE _ BX POPx, AX POPx, 5 INT, ;CODE
: EFS! BLK( _ ;
: INIT BLK$ ['] EFS@ [*TO] BLK@* ['] EFS! [*TO] BLK!* ;
236 239 LOADR ( forth core high )
XWRAP INIT
