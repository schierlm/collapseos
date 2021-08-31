( ----- 000 )
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
( ----- 001 )
\ Jump, flags
: JMPw, 10 C, ;
: JMPi, 11 C, L, ;
: JRi, 12 C, C, ;
: ?JRi, 13 C, C, ;
: Z? 14 C, ; : C? 15 C, ; : ^? 16 C, ;
: w>Z, 17 C, ; : p>Z, 18 C, ;
: Z>w, 19 C, ; : C>w, 20 C, ;
( ----- 002 )
\ Transfer
: w>p, 21 C, ;
: p>w, 22 C, ;
: i>w, 23 C, L, ;
: C@w, 24 C, ;
: @w, 25 C, ;
: C!wp, 26 C, ;
: !wp, 27 C, ;
: w>IP, 28 C, ;
: IP>w, 29 C, ;
: IP+off, 30 C, ;
: IP+, 31 C, ;
( ----- 003 )
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
( ----- 004 )
\ Arithmetic
: >>w, 43 C, ;
: <<w, 44 C, ;
: >>8w, 45 C, ;
: <<8w, 46 C, ;
