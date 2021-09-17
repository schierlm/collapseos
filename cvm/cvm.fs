( ----- 000 )
CVM MASTER INDEX

301 CVM boot code
304 CVM HAL
309 Common drivers
310 Grid drivers
( ----- 001 )
0 VALUE JROFF 1 VALUE JROPLEN
: CVMC 302 303 LOADR ;
: CVMH 304 308 LOADR ;
( ----- 002 )
HERE TO ORG
$0c ALLOT0
( END OF STABLE ABI )
0 TO lblnext 0 TO lblcell ( same as next for CVM )
2 TO lbldoes 3 TO lblpush 1 TO lblxt
( ----- 003 )
CODE FIND 4 JMPi,
CODE []= 5 JMPi,
CODE PC! 6 JMPi,
CODE PC@ 7 JMPi,
CODE * 8 JMPi,
CODE /MOD 9 JMPi,
CODE QUIT 10 JMPi,
CODE ABORT 11 JMPi,
CODE RCNT 12 JMPi,
CODE SCNT 13 JMPi,
CODE BYE 14 JMPi,
( ----- 004 )
\ HAL layer for CVM
\ Stack
: DUPp, 0 C, ;
: DROPp, 1 C, ;
: POPp, 2 C, ;
: PUSHp, 3 C, ;
: POPr, 4 C, ;
: PUSHr, 5 C, ;
: POPf, 6 C, ;
: PUSHf, 7 C, ;
: SWAPwp, 8 C, ;
: SWAPwf, 9 C, ;
( ----- 005 )
\ Jump, flags
: JMPw, 10 C, ;
: JMPi, 11 C, L, ;
: CALLi, 47 C, L, ;
: JRi, 12 C, C, ;
: ?JRi, 13 C, C, ;
: Z? 14 C, ; : C? 15 C, ; : ^? 16 C, ;
: w>Z, 17 C, ; : p>Z, 18 C, ;
: Z>w, 19 C, ; : C>w, 20 C, ;
( ----- 006 )
\ Transfer
: w>p, 21 C, ;
: p>w, 22 C, ;
: i>w, 23 C, L, ;
: (i)>w, 48 C, L, ;
: C@w, 24 C, ;
: @w, 25 C, ;
: C!wp, 26 C, ;
: !wp, 27 C, ;
: w>IP, 28 C, ;
: IP>w, 29 C, ;
: IP+off, 30 C, ;
: IP+, 31 C, ;
( ----- 007 )
\ Arithmetic
: INCw, 32 C, ;
: DECw, 33 C, ;
: INCp, 34 C, ;
: DECp, 35 C, ;
: +wp, 36 C, ;
: -wp, 37 C, ;
: CMPwp, 38 C, ;
: ANDwp, 39 C, ;
: ORwp, 40 C, ;
: XORwp, 41 C, ;
: XORwi, 42 C, L, ;
( ----- 008 )
\ Arithmetic
: >>w, 43 C, ;
: <<w, 44 C, ;
: >>8w, 45 C, ;
: <<8w, 46 C, ;
( ----- 009 )
\ Common drivers
: (key?) 0 PC@ 1 ;
: (emit) 0 PC! ;
: _ ( n blk( -- ) SWAP ( blk( n )
  ( n ) L|M 3 PC! 3 PC! ( blkid )
  ( blk( ) L|M 3 PC! 3 PC! ( dest ) ;
: (blk@) 1 3 PC! ( read ) _ ;
: (blk!) 2 3 PC! ( write ) _ ;
( ----- 010 )
\ Grid drivers
: COLS $03 PC@ ; : LINES $04 PC@ ;
: CURSOR! ( new old -- )
    DROP COLS /MOD 6 PC! ( y ) 5 PC! ( x ) ;
: CELL! ( c pos -- ) 0 CURSOR! 0 PC! ;
: NEWLN ( ln -- ln ) 1+ DUP LINES = IF 1- 0 7 PC! THEN ;
