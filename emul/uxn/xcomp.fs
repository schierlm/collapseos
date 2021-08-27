$0040 VALUE SYSVARS \ $00-$3f range is for tmp values
SYSVARS $0c + VALUE IP
ASML
1 VALUE JROPLEN -2 VALUE JROFF
530 536 LOADR \ UXN assembler + HAL from uxn.fs
\ Macros
: IP@ IP lit8, $30 ( LDZ2 ) C, ;
: IP! IP lit8, $31 ( SDZ2 ) C, ;
: IP+ IP@ $21 ( INC2 ) C, IP! ;
\ xcomp
XCOMPL HALC XCOMPH
HERE TO ORG $100 TO BIN( uxn[
FJR JRi, TO L1
[ $0c ALLOT0 ] ( STABLE ABI )
LSET lblnext
  POP2 IP@ INC2k INC2 ( ip* ip+2* )
  IP! LDA2 ( wordref* ) \ continue to exec
LSET lblexec ( exec ) ( w* )
  INC2k SWP2 LDA ( w+1* wtype )
  DUP FJR ?JRi, \ native
    ( w+1* wtype ) POP JMP2k THEN, ( w+1* wt )
  DUP $02 NEQ FJR ?JRi, ( CELL ) DUP2 ;CODE THEN, ( w+1* wt )
  DUP $08 AND 0 EQU FJR ?JRi, \ VALUE
    $80 AND 0 EQU FJR ?JRi, ( indirect ) LDA2 THEN,
    LDA2 DUP2 ;CODE THEN, \ not VALUE
  DUP $04 AND 0 EQU FJR ?JRi, \ ALIAS
    $80 AND 0 EQU FJR ?JRi, ( indirect ) LDA2 THEN,
    LDA2 lblexec BR JRi, THEN, \ not ALIAS
  $80 AND 0 EQU FJR ?JRi, ( DOES )
    LDA2k ( w+1* addr ) SWP2 INC2 INC2 SWP2 THEN,
  IP@ STH2 INC2k INC2 IP! ( w+1* )
  LDA2 lblexec BR JRi,
L1 FMARK ( <- main )
  [ BIN( $04 ( BOOT ) + lit16, ] LDA2 lblexec BR JRi,
CODE * POP2 MUL2 DUP2 ;CODE
CODE /MOD POP2
  DIV2k ( a b q ) MUL2k ( a b q b*q ) ROT2 POP2 ( a q b*q )
  ROT2 SWP2 SUB2 ( q r ) SWP2 DUP2 ;CODE
CODE PC! ROT2 ROT2 NIP DEO POP ;CODE
CODE PC@ SWP2 DEI SWP2 ;CODE
CODE BYE 1 $0e DEO 1 $0f ( halt ) DEO BRK
CODE SCNT POP2 $02 DEI ( wst ) 0 SWP DUP2 ;CODE
CODE RCNT POP2 0 $03 DEI ( rst ) DUP2 ;CODE
LSET L1 ( a1* a2* u -- f ) \ f=a1==a2 for range u
  BEGIN, DUP 0 NEQ FJR ?JRi, POP POP2 POP2 1 JMP2r THEN,
    STH LDAk STH INC2 SWP2 LDAk STH INC2 SWP2 ( a1* a2* )
    STH2r EQU FJR ?JRi, POPr POP2 POP2 0 JMP2r THEN,
    STHr 1 SUB BR JRi,
CODE []= ( a1 a2 u -- f )
  POP2 NIP L1 BR lit8, JSR 0 SWP DUP2 ;CODE
CODE FIND ( sa sl -- w? f ) \ ZP+0=sl ZP+1=sa
  POP2 0 STZ POP 1 STZ2
  [ SYSVARS $02 ( CURRENT ) + lit16, ] LDA2 BEGIN, ( w* )
    $0001 SUB2 LDAk $7f AND 0 LDZ NEQ FJR ?JRi, \ len match
      ( w-1* ) DUP2 $0002 SUB2 0 LDZ 0 SWP SUB2 ( w-1* sa* )
      1 LDZ2 0 LDZ L1 BR lit8, JSR 1 NEQ FJR ?JRi, \ match
        ( w-1* ) INC2 $0001 DUP2 ;CODE THEN, THEN,
    $0002 SUB2 LDA2 ( prevw* ) DUP2 $0000 NEQ2 BR ?JRi,
  ( no match ) DUP2 ;CODE
;CODE
CODE (key?) POP2 BRK $12 DEI 0 SWP $0001 DUP2 ;CODE
]uxn
CODE QUIT
LSET L1 ( used in ABORT ) PC ORG $d + T! ( Stable ABI )
  uxn[ POP2 0 $03 ( rst ) DEO ]uxn
  BIN( $0a ( main ) + lit16, uxn[ LDA2 ]uxn lblexec JMPi,
CODE ABORT uxn[ 0 $02 ( wst ) DEO $0000 ]uxn L1 BR JRi,
COREL
: (emit) $18 PC! ;
: INIT ;
XWRAP
