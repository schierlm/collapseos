$0040 VALUE SYSVARS \ $00-$3f range are for tmp values
SYSVARS $0c + VALUE IP
ASML
560 562 LOADR \ UXN assembler
XCOMPL XCOMPH
HERE TO ORG $100 TO BIN( uxn[
FWR8 L1 JMP ( -> main )
[ $0c ALLOT0 ] ( STABLE ABI )
BSET lblnext
  IP lit16, LDA2k DUP2 $0002 ADD2 ( IP* ip* ip+2* )
  ROT2 STA2 LDA2 ( wordref* ) \ continue to exec
BSET L2 ( exec ) ( w* )
  DUP2 $0001 ADD2 SWP2 LDA ( w+1* wtype )
  FJR, JCN \ native
    ( w+1* wtype ) JMP2 THEN, ( w+1* ) \ XT
  IP lit16, LDA2k STH2 ( w+1* IP* )
  OVR2 $0002 ADD2 SWP2 STA2 ( w+1* )
  LDA2 BWR L2 ( exec ) JMP
FSET8 L1 ( <- main )
  [ BIN( $04 ( BOOT ) + lit16, ] LDA2 BWR L2 ( exec ) JMP
CODE EXIT STH2r IP lit8, STZ2 ;CODE
CODE foo 'f' $18 DEO 'o' $18 DEO 'o' $18 DEO ;CODE
CODE BYE BRK ;CODE ]uxn
: BOOT foo BYE ;
XCURRENT ORG $04 ( stable ABI BOOT ) + T!
