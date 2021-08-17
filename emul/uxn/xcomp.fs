$0040 VALUE SYSVARS \ $00-$3f range is for tmp values
SYSVARS $0c + VALUE IP
ASML
560 562 LOADR \ UXN assembler from extras
\ Macros
: IP@ IP lit8, $30 ( LDZ2 ) C, ;
: IP! IP lit8, $31 ( SDZ2 ) C, ;
: IP+ IP@ $3a ( INC2 ) C, IP! ;
\ xcomp
XCOMPL XCOMPH
HERE TO ORG $100 TO BIN( uxn[
FWR8 L1 JMP ( -> main )
[ $0c ALLOT0 ] ( STABLE ABI )
BSET lblnext
  IP@ INC2k INC2 ( ip* ip+2* )
  IP! LDA2 ( wordref* ) \ continue to exec
BSET L2 ( exec ) ( w* )
  INC2k SWP2 LDA ( w+1* wtype )
  FJR, JCN \ native
    ( w+1* wtype ) JMP2 THEN, ( w+1* ) \ XT
  IP@ STH2 INC2k INC2 IP! ( w+1* )
  LDA2 BWR L2 ( exec ) JMP
FSET8 L1 ( <- main )
  [ BIN( $04 ( BOOT ) + lit16, ] LDA2 BWR L2 ( exec ) JMP
CODE QUIT
BSET L1 ( used in ABORT ) [ PC ORG $d + T! ] ( Stable ABI )
  0 $03 ( rst ) DEO
  [ BIN( $0a ( main ) + lit16, ] LDA2 BWR L2 ( exec ) JMP
CODE ABORT 0 $02 ( wst ) DEO BWR L1 ( QUIT ) JMP
CODE EXECUTE BWR L2 ( exec ) JMP
CODE EXIT STH2r IP lit8, STZ2 ;CODE
CODE BYE 1 $0f ( halt ) DEO BRK
CODE (b) 0 IP@ INC2k IP! LDA ;CODE
CODE (n) IP@ INC2k INC2 IP! LDA2 ;CODE
CODE (br) BSET L1 ( used in ?br and loop )
  IP@ LDAk ( ip* off ) 0 OVR $80 LTH
  FJR, JCN ( neg ) DEC THEN, ( ip* off 0-or-ff )
  SWP ADD2 IP! ;CODE
CODE (?br)
  ORA ( msb|lsb ) 0 EQU BWR L1 JCN IP+ ;CODE
CODE (loop)
  INC2r NEQ2rk STHr BWR L1 JCN ( branch )
  ( exit ) POP2r POP2r IP+ ;CODE
CODE 2>R SWP2 STH2 STH2 ;CODE
CODE DUP DUP2 ;CODE
CODE 1+ INC2 ;CODE
CODE 1- DEC2 ;CODE
CODE NOT ORA ( msb|lsb ) 0 EQU 0 SWP ;CODE
CODE foo 'f' $18 DEO 'o' $18 DEO 'o' $18 DEO ;CODE
]uxn
: BOOT 5 0 DO foo LOOP BYE ;
XCURRENT ORG $04 ( stable ABI BOOT ) + T!
