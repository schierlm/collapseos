( ----- 000 )
6809 MASTER INDEX

301 6809 boot code            311 TRS-80 Color Computer 2
( ----- 001 )
( 6809 declarations )
: 6809C 302 305 LOADR ;
: 6809H 306 309 LOADR ;
1 VALUE JROPLEN -1 VALUE JROFF
( ----- 002 )
( 6809 Boot code. IP=Y, PS=S, RS=U  ) HERE TO ORG
FJR JRi, TO L1 ( main ) $0a ALLOT0
\ end of stable ABI
L1 FMARK ( main ) PS_ADDR # LDS, RS_ADDR # LDU,
BIN( 4 + ( BOOT ) () LDX, X+0 JMP,
LSET lblpush PUSHp,
LSET lblcell LSET lblnext Y++ LDX, X+0 JMP,
LSET lbldoes U++ STY, ( IP->RS ) D Y TFR, lblnext BR JRi,
LSET lblxt U++ STY, ( IP->RS ) PULS, Y lblnext BR JRi,
( ----- 003 )
CODE QUIT LSET L1 ( for ABORT ) RS_ADDR # LDU,
  BIN( $0a + ( main ) () LDX, X+0 JMP,
CODE ABORT PS_ADDR # LDS, L1 BR JRi,
CODE BYE BEGIN, BR JRi,
CODE SCNT PS_ADDR # LDD, 0 <> STS, 0 <> SUBD, PSHS, D ;CODE
CODE RCNT
  RS_ADDR # LDD, 0 <> STD, U D TFR, 0 <> SUBD, PSHS, D ;CODE
( ----- 004 )
CODE /MOD ( a b -- a/b a%b )
  16 # LDA, 0 <> STA, CLRA, CLRB, ( D=running rem ) BEGIN,
    1 # ORCC, 3 S+N ROL, ( a lsb ) 2 S+N ROL, ( a msb )
    ROLB, ROLA, S+0 SUBD,
    FJR BHSi, ( if < ) S+0 ADDD, 3 S+N DEC, ( a lsb ) THEN,
  0 <> DEC, Z? ^? BR ?JRi,
  2 S+N LDX, 2 S+N STD, ( rem ) S+0 STX, ( quotient ) ;CODE
CODE * ( a b -- a*b )
  S+0 ( bm ) LDA, 3 S+N ( al ) LDB, MUL, S+0 ( bm ) STB,
  2 S+N ( am ) LDA, 1 S+N ( bl ) LDB, MUL,
    S+0 ( bm ) ADDB, S+0 STB,
  1 S+N ( al ) LDA, 3 S+N ( bl ) LDB, MUL,
  S++ ADDA, S+0 STD, ;CODE
( ----- 005 )
LSET L1 ( X=s1 Y=s2 B=cnt ) BEGIN,
  X+ LDA, Y+ CMPA, IFNZ, RTS, THEN, DECB, Z? ^? BR ?JRi, RTS,
CODE []= ( a1 a2 u -- f TODO: allow u>$ff )
  0 <> STY, PULS, DXY ( B=u, X=a2, Y=a1 ) L1 () JSR,
  IFZ, 1 # LDD, ELSE, CLRA, CLRB, THEN, PSHS, D 0 <> LDY, ;CODE
CODE FIND ( sa sl -- w? f )
  SYSVARS $02 + ( CURRENT ) () LDX,
  0 <> STY, PULS, D 2 <> STB, BEGIN,
    -X LDB, $7f # ANDB, --X TST, 2 <> CMPB, IFZ,
      3 <> STX, S+0 LDY, NEGB, X+B LEAX, NEGB, L1 () JSR,
      IFZ, ( match ) 0 <> LDY, 3 <> LDD, 3 # ADDD, S+0 STD,
        1 # LDD, PSHS, D ;CODE THEN,
      3 <> LDX, THEN, \ nomatch, X=prev
  X+0 LDX, Z? ^? BR ?JRi, \ not zero, loop
  ( end of dict ) 0 <> LDY, S+0 STX, ( X=0 ) ;CODE
( ----- 006 )
\ 6809 HAL, Stack
: w>p, $ed60 M, ( std 0,S ) ; : p>w, $ec60 M, ( ldd 0,S ) ;
: DUPp, $ae60 M, ( ldx 0,S ) $3410 M, ( pshs x ) ;
: DROPp, $3262 M, ( leas 2,S ) ;
: PUSHp, $3406 M, ( pshs d ) ; : POPp, $3506 M, ( puls d ) ;
: POPf, $3510 M, ( puls x ) POPp, $3410 M, ( pshs x ) ;
: PUSHf, $3510 M, ( puls x ) PUSHp, $3410 M, ( pshs x ) ;
: PUSHr, $edc1 M, ( std u++ ) ; : POPr, $ecc3 M, ( ldd --u ) ;
: SWAPwp, $ae60 M, ( ldx 0,S ) w>p, $1f10 M, ( tfr x,d ) ;
: SWAPwf, $ae62 M, ( ldx 2,S ) $ed62 M, ( std 2,S )
  $1f10 M, ( tfr x,d ) ;
( ----- 007 )
\ 6809 HAL, Jump, flags
SYSVARS $16 + *VALUE ?JROP
: JMPw, $1f01 M, ( tfr d,x ) $6e00 M, ( jmp 0,x ) ;
: JMPi, $7e C, M, ( jmp nn ) ; : CALLi, $bd C, M, ( jsr nn ) ;
: JRi, $20 C, C, ( bra n ) ; : ?JRi, ?JROP C, C, ;
: Z? $27 [*TO] ?JROP ( beq ) ; : C? $25 [*TO] ?JROP ( bcs ) ;
: ^? ?JROP 1 XOR [*TO] ?JROP ( bne/bcc ) ;
\ TODO: add some compile time flag to determine when this code
\ generation is spurious and when its not so that we can avoid
\ emitting it when unnecessary.
: w>Z, $1083 M, 0 M, ( cmpd 0 ) ;
: p>Z, $ae60 M, ( ldx 0,S ) ;
: i>w, $cc C, M, ( ldd nn ) ;
: (i)>w, $fc C, M, ( ldd (nn) ) ;
: C>w, 0 i>w, $c900 M, ( adcb 0 ) ;
: Z>w, Z? 5 ?JRi, 0 i>w, 3 JRi, 1 i>w, ;
( ----- 008 )
\ 6809 HAL, transfer
: C@w, $1f01 M, ( tfr d,x ) $e600 M, ( ldb 0,X )
  $4f C, ( clra ) ;
: @w, $1f01 M, ( tfr d,x ) $ec00 M, ( ldd 0,X ) ;
: C!wp, $1f01 M, ( tfr d,x ) $e661 M, ( ldb 1,S )
  $e700 M, ( stb 0,X ) $1f10 M, ( tfr x,d ) ;
: !wp, $1f01 M, ( tfr d,x ) $ec60 M, ( ldd 0,S )
  $ed00 M, ( std 0,X ) ;
: IP>w, $1f20 M, ( tfr y,d ) ;
: w>IP, $1f02 M, ( tfr d,y ) ;
: IP+, $6da0 M, ( tst ,y+ ) ;
: IP+off, $a620 M, ( lda 0,y ) $31a6 M, ( leay a,y ) ;
( ----- 009 )
\ 6809 HAL, arithmetic
: +wp, $e360 M, ( addd 0,s ) ; : -wp, $a360 M, ( subd 0,s ) ;
: >>w, $4456 M, ( lsra rorb ) ; : <<w, $5849 M, ( lslb rola ) ;
: >>8w, $1f89 M, ( tfr a,b ) $4f C, ( clra ) ;
: <<8w, $1f98 M, ( tfr b,a ) $5f C, ( clrb ) ;
: INCw, $c3 C, 1 M, ( addd 1 ) ;
: DECw, $c3 C, -1 M, ( addd -1 ) ;
: INCp, $6c61 M, ( inc 1,s ) Z? ^? 2 ?JRi, $6c60 M, ( inc0,s ) ;
: DECp, $6d61 M, ( tst 1,s ) Z? ^? 2 ?JRi, $6a60 M, ( dec 0,s )
  $6a61 M, ( dec 1,s ) ;
: CMPwp, $10a3 M, $60 C, ( cmpd 0,s ) ;
: ANDwp, $a460 M, ( anda 0,s ) $e461 M, ( andb 1,s ) ;
: ORwp, $aa60 M, ( ora 0,s ) $ea61 M, ( orb 1,s ) ;
: XORwp, $a860 M, ( eora 0,s ) $e861 M, ( eorb 1,s ) ;
: XORwi, $88 C, DUP >>8 C, ( eora nn ) $c8 C, C, ( eorb nn ) ;
( ----- 010 )
( CoCo2 drivers. Load range: 461-463 )
PC ," @HPX08" CR C, ," AIQY19" 0 C,
   ," BJRZ2:" 0 C,  ," CKS_3;" 0 C,
   ," DLT_4," 0 C,  ," EMU" BS C, ," 5-" 0 C,
   ," FNV_6." 0 C,  ," GOW 7/" 0 C,
   ," @hpx0(" CR C, ," aiqy!)" 0 C,
   ," bjrz" '"' C, '*' C, 0 C, ," cks_#+" 0 C,
   ," dlt_$<" 0 C,  ," emu" BS C, ," %=" 0 C,
   ," fnv_&>" 0 C,  ," gow '?" 0 C,
LSET L1 ( PC ) # LDX, $fe # LDA, BEGIN, ( 8 times )
  $ff02 () STA, ( set col ) $ff00 () LDB, ( read row )
  ( ignore 8th row ) $80 # ORB, $7f # CMPA, IFZ,
    ( ignore shift row ) $40 # ORB, THEN,
  INCB, IFNZ, ( key pressed ) DECB, RTS, THEN,
  ( inc col ) 7 X+N LEAX, 1 # ORCC, ROLA, C? BR ?JRi,
  ( no key ) CLRB, RTS,
( ----- 011 )
CODE (key?) ( -- c? f ) CLRA, CLRB, PSHS, D L1 () JSR,
  IFNZ, ( key! row mask in B col ptr in X )
    ( is shift pressed? ) $7f # LDA, $ff02 () STA,
    $ff00 () LDA, $40 # ANDA, IFZ, ( shift! )
      56 X+N LEAX, THEN,
    BEGIN, X+ LDA, LSRB, C? BR ?JRi,
    ( A = our char ) 1 S+N STA, TSTA, IFNZ, ( valid key )
      1 # LDD, ( f ) PSHS, D ( wait for keyup )
      BEGIN, L1 () JSR, Z? ^? BR ?JRi, THEN,
  THEN, ;CODE
( ----- 012 )
32 VALUE COLS 16 VALUE LINES
: CELL! ( c pos -- )
  SWAP $20 - DUP $5f < IF
    DUP $20 < IF $60 + ELSE DUP $40 < IF $20 + ELSE $40 -
      THEN THEN ( pos glyph )
    SWAP $400 + C! ELSE 2DROP THEN ;
: CURSOR! ( new old -- )
  DROP $400 + DUP C@ $40 XOR SWAP C! ;
