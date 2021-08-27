( ----- 000 )
\ HAL layer for CVM
\ Stack
: DUPp, 0 C, ;
: DROPp, 1 C, ;
: POPp, 2 C, ;
: PUSHp, 3 C, ;
: POPr, 4 C, ;
: PUSHr, 5 C, ;
: SWAPwp, 6 C, ;
: SWAPwf, 7 C, ;
: p>Z, 8 C, ;
( ----- 001 )
\ Transfer
: w>p, 10 C, ;
: p>w, 11 C, ;
: i>w, 12 C, L, ;
: C@w, 13 C, ;
: @w, 14 C, ;
: C!wp, 15 C, ;
: !wp, 16 C, ;
: POPf, 17 C, ;
: PUSHf, 18 C, ;
: OUTwi, 19 C, C, ;
: INwi, 20 C, C, ;
( ----- 002 )
\ Flags
: w>Z, 21 C, ; : Z>w, 22 C, ; : C>w, 23 C, ;
: Z? 61 C, ; : C? 62 C, ; : ^? 63 C, ;
\ Special vars
: w>IP, 24 C, ;
: IP>w, 25 C, ;
: IP+w, 30 C, ;
: IP+, 31 C, ;
( ----- 003 )
\ Jump
: JMPw, 33 C, ;
: JMPi, 34 C, L, ;
: JRi, 35 C, C, ;
: ?JRi, 60 C, C, ;
( ----- 004 )
\ Arithmetic
: INCw, 40 C, ;
: DECw, 41 C, ;
: CMPpw, 42 C, ;
: SEXw, 43 C, ;
: ANDwp, 44 C, ;
: ANDwi, 45 C, L, ;
: ORwp, 46 C, ;
: XORwp, 47 C, ;
: XORwi, 48 C, L, ;
( ----- 005 )
: +pw, 50 C, ;
: INCp, 51 C, ;
: -wp, 52 C, ;
: DECp, 55 C, ;
: >>w, 56 C, ;
: <<w, 57 C, ;
: >>8w, 58 C, ;
: <<8w, 59 C, ;
