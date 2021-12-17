( ----- 000 )
CVM MASTER INDEX

301 CVM boot code
304 CVM HAL
309 Common drivers
310 Grid drivers
( ----- 001 )
0 VALUE JROFF 1 VALUE JROPLEN
: CVMC 302 305 LOADR ;
: CVMH 306 LOAD ;
( ----- 002 )
HERE TO ORG
$0c ALLOT0
( END OF STABLE ABI )
0 TO lblnext 0 TO lblcell ( same as next for CVM )
2 TO lbldoes 1 TO lblxt 3 TO lblval
( ----- 003 )
CODE FIND 49 C, ;CODE
CODE []= 50 C, ;CODE
CODE PC! 51 C, ;CODE
CODE PC@ 52 C, ;CODE
CODE * 53 C, ;CODE
CODE /MOD 54 C, ;CODE
CODE QUIT 55 C,
CODE ABORT 56 C,
CODE RCNT 57 C, ;CODE
CODE SCNT 58 C, ;CODE
CODE BYE 59 C,
CODE (br) 30 C, ;CODE
CODE (?br) 8 C, ;CODE
CODE (next) 9 C, ;CODE
( ----- 004 )
CODE C@ 63 C, ;CODE
CODE @ 64 C, ;CODE
CODE ! 65 C, ;CODE
CODE C! 66 C, ;CODE
CODE AND 39 C, ;CODE
CODE OR 40 C, ;CODE
CODE XOR 41 C, ;CODE
CODE 1+ 34 C, ;CODE
CODE 1- 35 C, ;CODE
CODE + 71 C, ;CODE
CODE - 72 C, ;CODE
CODE >> 67 C, ;CODE
CODE << 68 C, ;CODE
CODE >>8 69 C, ;CODE
CODE <<8 70 C, ;CODE
( ----- 005 )
CODE R@ 60 C, ;CODE
CODE R~ 26 C, ;CODE
CODE R> 61 C, ;CODE
CODE >R 62 C, ;CODE
CODE DUP 0 C, ;CODE
CODE DROP 1 C, ;CODE
CODE SWAP 4 C, ;CODE
CODE OVER 5 C, ;CODE
CODE ROT 6 C, ;CODE
CODE ROT> 7 C, ;CODE
CODE TICKS ;CODE
( ----- 006 )
\ HAL layer for CVM
: >JMP, 21 C, ;
: JMPi, 11 C, L, ;
: CALLi, 10 C, L, ;
: JRi, 12 C, C, ;
: ?JRi, 13 C, C, ;
: Z? 14 C, ; : C? 15 C, ; : ^? 16 C, ;
: @Z, 18 C, ; : Z>!, 19 C, ; : C>!, 20 C, ;
: i>, 2 C, L, ;
: (i)>, 3 C, L, ;
: >(i), 22 C, L, ;
: (i)+, 23 C, L, ;
: (i)-, 24 C, L, ;
: >IP, 28 C, ;
: IP>, 29 C, ;
: IP+, 31 C, ;
( ----- 009 )
\ Common drivers
: (key?) 0 PC@ 1 ;
: (emit) 0 PC! ;
: _ ( n blk( -- ) SWAP ( blk( n )
  ( n ) L|M 3 PC! 3 PC! ( blkid )
  ( blk( ) L|M 3 PC! 3 PC! ( dest ) ;
: (blk@) 1 3 PC! ( read ) _ ;
: (blk!) 2 3 PC! ( write ) _ ;
: TX> 8 PC! ; : RX<? 8 PC@ DUP IF 8 PC@ SWAP THEN ;
( ----- 010 )
\ Grid drivers
: COLS $03 PC@ ; : LINES $04 PC@ ;
: CURSOR! ( new old -- )
    DROP COLS /MOD 6 PC! ( y ) 5 PC! ( x ) ;
: CELL! ( c pos -- ) 0 CURSOR! 0 PC! ;
: NEWLN ( ln -- ln ) 1+ DUP LINES = IF 1- 0 7 PC! THEN ;
