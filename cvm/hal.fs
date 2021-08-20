( ----- 000 )
\ HAL layer for CVM
\ Stack
: DROPp, 1 C, ;
: POPp, 2 C, ;
: PUSHp, 3 C, ;
: POPr, 4 C, ;
: PUSHr, 5 C, ;
: SWAPwp, 6 C, ;
: OVERwp, 7 C, ;
: ROTwp, 8 C, ;
: ROT>wp, 9 C, ;
( ----- 001 )
\ Transfer
: w>p, 10 C, ;
: p>w, 11 C, ;
: i>w, 12 C, L, ;
: C@w, 13 C, ;
: @w, 14 C, ;
: C!wp, 15 C, ;
: !wp, 16 C, ;
: OUTwp, 19 C, ; \ TODO make port hardcoded
: INw, 20 C, ;
( ----- 002 )
\ Flags
: Z?w, 21 C, ;
: Z>w, 22 C, ;
: C>w, 23 C, ;
\ Special vars
: w>IP, 24 C, ;
: IP>w, 25 C, ;
: w>RSP, 26 C, ;
: RSP>w, 27 C, ;
: w>PSP, 28 C, ;
: PSP>w, 29 C, ;
: IP+w, 30 C, ;
: IP+, 31 C, ;
: HALT, 32 C, ;
( ----- 003 )
\ Jump
: JMPw, 33 C, ;
: JMPi, 34 C, L, ;
: JRi, 35 C, C, ;
: JRZi, 36 C, C, ;
: JRNZi, 37 C, C, ;
: JRCi, 38 C, C, ;
: JRNCi, 39 C, C, ;
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
: NOTw, 49 C, ;
( ----- 005 )
: +pw 50 C, ;
: -wp, 52 C, ;
: *pw, 53 C, ;
: /MODpw, 54 C, ;
: CMP[], 55 C, ;
: >>w, 56 C, ;
: <<w, 57 C, ;
: >>8w, 58 C, ;
: <<8w, 59 C, ;
