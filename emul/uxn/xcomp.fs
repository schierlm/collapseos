$0040 VALUE SYSVARS \ $00-$3f range is for tmp values
SYSVARS $0c + VALUE IP
ASML
530 534 LOADR \ UXN assembler + HAL from uxn.fs
\ Macros
: IP@ IP lit8, $30 ( LDZ2 ) C, ;
: IP! IP lit8, $31 ( SDZ2 ) C, ;
: IP+ IP@ $21 ( INC2 ) C, IP! ;
\ xcomp
XCOMPL XCOMPH
HERE TO ORG $100 TO BIN( uxn[
FWR8 L1 JMP ( -> main )
[ $0c ALLOT0 ] ( STABLE ABI )
BSET lblnext
  IP@ INC2k INC2 ( ip* ip+2* )
  IP! LDA2 ( wordref* ) \ continue to exec
BSET lblexec ( exec ) ( w* )
  INC2k SWP2 LDA ( w+1* wtype )
  DUP FJR, JCN \ native
    ( w+1* wtype ) POP JMP2 THEN, ( w+1* wt )
  DUP $02 NEQ FJR, JCN ( CELL ) ;CODE THEN, ( w+1* wt )
  DUP $08 AND 0 EQU FJR, JCN \ VALUE
    $80 AND 0 EQU FJR, JCN ( indirect ) LDA2 THEN,
    LDA2 ;CODE THEN, \ not VALUE
  DUP $04 AND 0 EQU FJR, JCN \ ALIAS
    $80 AND 0 EQU FJR, JCN ( indirect ) LDA2 THEN,
    LDA2 BWR lblexec ( exec ) JMP THEN, \ not ALIAS
  $80 AND 0 EQU FJR, JCN ( DOES )
    LDA2k ( w+1* addr ) SWP2 INC2 INC2 THEN,
  IP@ STH2 INC2k INC2 IP! ( w+1* )
  LDA2 BWR lblexec ( exec ) JMP
FSET8 L1 ( <- main )
  [ BIN( $04 ( BOOT ) + lit16, ] LDA2 BWR lblexec ( exec ) JMP
CODE * MUL2 ;CODE
CODE /MOD
  DIV2k ( a b q ) MUL2k ( a b q b*q ) ROT2 POP2 ( a q b*q )
  ROT2 SWP2 SUB2 ( q r ) SWP2 ;CODE
CODE PC! NIP DEO POP ;CODE
CODE PC@ DEI ;CODE
CODE QUIT
BSET L1 ( used in ABORT ) [ PC ORG $d + T! ] ( Stable ABI )
  0 $03 ( rst ) DEO
  [ BIN( $0a ( main ) + lit16, ] LDA2
  lblexec lit16, ( exec ) JMP2
CODE ABORT 0 $02 ( wst ) DEO BWR L1 ( QUIT ) JMP
CODE EXIT STH2r IP lit8, STZ2 ;CODE
CODE BYE 1 $0e DEO 1 $0f ( halt ) DEO BRK
CODE (b) 0 IP@ INC2k IP! LDA ;CODE
CODE (n) IP@ INC2k INC2 IP! LDA2 ;CODE
CODE (c) IP@ INC2k SWP2 LDA 0 SWP ADD2 IP! JMP2 ;CODE
CODE (br) BSET L1 ( used in ?br and loop )
  IP@ LDAk ( ip* off ) 0 OVR $80 LTH
  FJR, JCN ( neg ) 1 SUB THEN, ( ip* off 0-or-ff )
  SWP ADD2 IP! ;CODE
CODE (?br)
  ORA ( msb|lsb ) 0 EQU BWR L1 JCN IP+ ;CODE
CODE (loop)
  INC2r NEQ2rk STHr BWR L1 JCN ( branch )
  ( exit ) POP2r POP2r IP+ ;CODE
CODE ROT ROT2 ;CODE
CODE ROT> ROT2 ROT2 ;CODE
CODE DUP BSET L1 DUP2 ;CODE
CODE ?DUP ORAk ( msb|lsb ) BWR L1 JCN ;CODE
CODE I STH2kr ;CODE
CODE I' SWP2r STH2kr SWP2r ;CODE
CODE J ROT2r STH2kr ROT2r ROT2r ;CODE
CODE >R STH2 ;CODE
CODE R> STH2r ;CODE
CODE 2>R SWP2 STH2 STH2 ;CODE
CODE 2R> STH2r STH2r SWP2 ;CODE
CODE SCNT 0 $03 DEI ( wst ) ;CODE
CODE RCNT 0 $04 DEI ( rst ) ;CODE
CODE + ADD2 ;CODE
CODE - SUB2 ;CODE
CODE 1+ INC2 ;CODE
CODE 1- $0001 SUB2 ;CODE
CODE << $10 SFT2 ;CODE
CODE >> $01 SFT2 ;CODE
CODE <<8 NIP 0 ;CODE
CODE >>8 POP 0 SWP ;CODE
CODE = EQU2 0 SWP ;CODE
CODE < LTH2 0 SWP ;CODE
CODE (emit) $18 DEO POP ;CODE
]uxn
CODE DROP DROPp, ;CODE
CODE SWAP POPf, PUSHp, ;CODE
CODE OVER POPf, PUSHf, PUSHp, ;CODE
: (emit) $18 PC! ;
: BOOT 'x' 'y' OVER (emit) (emit) BYE ;
XCURRENT ORG $04 ( stable ABI BOOT ) + T!
PC ORG 8 ( LATEST ) + T!
XCURRENT ORG 6 ( CURRENT ) + T!
