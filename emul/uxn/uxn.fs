( ----- 000 )
\ UXN assembler
1 TO BIGEND?
CREATE ops ," BRKINCPOPDUPNIPSWPOVRROTEQUNEQGTHLTHJMPJCNJSRSTH"
           ," LDZSTZLDRSTRLDASTADEIDEOADDSUBMULDIVANDORAEORSFT"
CREATE flags ," 2rk"
: findop ( sa sl -- op? f ) \ find op "sa sl" in ops
  DUP 3 < IF 2DROP 0 EXIT THEN $20 0 DO ( sa sl )
    OVER I 3 * ops + 3 []= IF 2DROP I 1 UNLOOP EXIT THEN
  LOOP 2DROP 0 ;
: findf ( c -- mask )
  $20 flags 3 RANGE DO ( c m ) OVER I C@ = IF
    NIP UNLOOP EXIT THEN << LOOP (wnf) ;
: flags! ( sa sl op -- op ) \ apply 2KR flags to op
  SWAP 3 - ?DUP IF ( sa op sl-3 )
    ROT 3 + SWAP RANGE DO ( op ) I C@ findf OR LOOP
  ELSE NIP THEN ;
( ----- 001 )
: lit8, ( n -- ) $80 C, C, ; : lit16, $a0 C, T, ;
: lit, ( sl n -- )
  TUCK $100 < SWAP 4 < AND ( n f ) IF lit8, ELSE lit16, THEN ;
: uxn<, ( -- f ) \ read and parse one uxn upcode
  WORD OVER C@ ']' = IF 2DROP 0 EXIT THEN
  2DUP PARSE IF ROT DROP lit, 1 EXIT THEN
  2DUP findop IF
    flags! C, 1 ELSE
    FIND IF EXECUTE 1 ELSE (wnf) THEN THEN ;
: uxn[ BEGIN uxn<, NOT UNTIL ; IMMEDIATE
( ----- 002 )
\ uxn HAL, Stack
: DROPp, $24 C, ( NIP2 ) ;
: DUPp, $26 C, ( OVR2 ) $25 C, ( SWP2 ) ;
: PUSHp, $23 C, ( DUP2 ) ; : POPp, $22 C, ( POP2 ) ;
: w>p, DROPp, PUSHp, ; : p>w, POPp, PUSHp, ;
: POPf, POPp, $25 C, ( SWP2 ) ;
: PUSHf, $25 C, ( SWP2 ) $26 C, ( OVR2 ) ;
: POPr, POPp, $6f C, ( STH2r ) ;
: PUSHr, PUSHp, $2f C, ( STH2 ) ;
: SWAPwp, $25 C, ( SWP2 ) ;
: SWAPwf, SWAPwp, $27 C, ( ROT2 ) ;
( ----- 003 )
\ uxn HAL, Jump, Flags
: JMPw, PUSHp, $2c C, ;
: JMPi, lit16, $2c ( JMP2 ) C, ;
: JRi, lit8, $0c C, ( JMP ) ;
: ?JRi, lit8, $0d C, ( JCN ) ;
: Z? $12 lit8, $10 C, ( LDZ ) ;
: C? $13 lit8, $10 C, ( LDZ ) ;
: ^? 1 lit8, $1e C, ( EOR ) ;
: Z>w, POPp, Z? 0 lit8, $05 C, ( SWP ) ;
: C>w, POPp, C? 0 lit8, $05 C, ( SWP ) ;
: w>Z, PUSHp, 0 lit16, $28 C, ( EQU2 )
  $12 lit8, $11 C, ( STZ ) ;
: p>Z, $25 C, ( SWP2 ) w>Z, $25 C, ( SWP2 ) ;
( ----- 004 )
\ uxn HAL, Transfer
: i>w, POPp, lit16, ;
: C@w, $14 C, ( LDA ) 0 lit8, $05 C, ( SWP ) ;
: @w, $34 C, ( LDA2 ) ;
: C!wp, $95 C, ( STAk ) ;
: !wp, $b5 C, ( STA2k ) ;
SYSVARS $0c + VALUE IP
: IP>w, POPp, IP lit8, $30 C, ( LDZ2 ) ;
: w>IP, PUSHp, IP lit8, $31 C, ( STZ2 ) ;
: IP+, IP lit8, $30 C, ( LDZ2 ) $21 C, ( INC2 )
  IP lit8, $31 C, ( STZ2 ) ;
: IP+w, IP lit8, $30 C, ( LDZ2 ) $b8 C, ( ADD2k )
  IP lit8, $31 C, ( STZ2 ) POPp, ;
( ----- 005 )
\ uxn HAL, Arithmetic
: INCw, $21 C, ( INC2 ) ; : DECw, 1 lit16, $39 C, ( SUB2 ) ;
: INCp, SWAPwp, INCw, SWAPwp, ; : DECp, SWAPwp, DECw, SWAPwp, ;
: +pw, $26 C, ( OVR2 ) $38 C, ( ADD2 ) ;
: -wp, $26 C, ( OVR2 ) $39 C, ( SUB2 ) ;
: >>w, $01 lit8, $3f C, ( SFT2 ) ;
: <<w, $10 lit8, $3f C, ( SFT2 ) ;
: >>8w, $02 C, ( POP ) 0 lit8, $05 C, ( SWP ) ;
: <<8w, $04 C, ( NIP ) 0 lit8, ;
: CMPpw, $a8 C, ( EQU2k ) $12 lit8, $11 C, ( STZ )
         $ab C, ( LTH2k ) $13 lit8, $11 C, ( STZ ) ;
: SEXw, $04 C, ( NIP ) $ff lit8, $06 C, ( OVR ) $80 lit8,
  $1c C, ( AND ) 1 lit8, $0d C, ( JCN ) $01 C, ( INC )
  $05 C, ( SWP ) ;
( ----- 006 )
\ uxn HAL, Arithmetic
: ANDwp, $26 C, ( OVR2 ) $3c C, ( AND2 ) ;
: ORwp, $26 C, ( OVR2 ) $3d C, ( ORA2 ) ;
: XORwp, $26 C, ( OVR2 ) $3e C, ( EOR2 ) ;
: XORwi, lit16, $3e C, ( EOR2 ) ;
