0xff00 CONSTANT RS_ADDR
0xfffa CONSTANT PS_ADDR
RS_ADDR 0xa0 - CONSTANT SYSVARS
20 LOAD   ( 8086 asm )
262 LOAD  ( xcomp ) 270 LOAD  ( xcomp overrides )
445 461 LOADR ( 8086 boot code )
353 LOAD  ( xcomp core low )
CODE (emit) AX POPx, 1 INT, ;CODE
CODE (key?) 2 INT, AH 0 MOVri, AX PUSHx, AX PUSHx, ;CODE
: COLS 80 ; : LINES 25 ;
CODE AT-XY ( x y ) BX POPx, AX POPx, 3 INT, ;CODE
CODE _ BX POPx, AX POPx, 4 INT, ;CODE
: EFS@ BLK( _ ;
CODE _ BX POPx, AX POPx, 5 INT, ;CODE
: EFS! BLK( _ ;
( 8086 port doesn't define PC@ and PC!, but test harness uses
  it. Our forth binary uses INT 6 for retcode. )
CODE PC! AX POPx, ( discard ) AX POPx, 6 INT, ;CODE
390 LOAD  ( xcomp core high )
(entry) _ ( Update LATEST ) PC ORG @ 8 + !
," BLK$ "
," ' EFS@ ' BLK@* **! "
," ' EFS! ' BLK!* **! "
EOT,
ORG @ 256 /MOD 2 PC! 2 PC!
H@ 256 /MOD 2 PC! 2 PC!
