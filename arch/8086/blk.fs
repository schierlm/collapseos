( ----- 000 )
: C>!, BX 0 MOVxI, BL 0 ADCi,
8086 MASTER INDEX

301 8086 boot code             306 8086 HAL
311 8086 assembler             320 8086 drivers
( ----- 001 )
\ 8086 macros
: 8086A 5 LOAD ( wordtbl ) 311 318 LOADR 7 LOAD ( Flow ) ;
: 8086C 302 309 LOADR ;
( ----- 002 )
\ 8086 boot code. PS=SP, RS=BP, IP=DX, TOS=BX
FJR JRi, TO L1 ( main ) \ 03=boot driveno
10 ALLOT0 \ End of Stable ABI
L1 FMARK ( main ) DX POPx, ( boot drive no ) $03 DL MOVmr,
  SP PS_ADDR MOVxI, BP RS_ADDR MOVxI,
  DI $04 ( BOOT ) MOVxm, DI JMPr,
LSET lblval DI POPx, BX PUSHx, BX [DI] x[] MOV[], \ to next
LSET lblnext DI DX MOVxx, ( <-- IP ) DX INCx, DX INCx,
  DI [DI] x[] MOV[], DI JMPr,
LSET lblcell AX POPx, BX PUSHx, BX AX MOVxx, lblnext BR JRi,
LSET lblxt BP INCx, BP INCx, [BP] 0 DX []+x MOV[], ( pushRS )
  DX POPx, lblnext BR JRi,
LSET lbldoes DI POPx, BX PUSHx, BX DI MOVxx,  BX INCx, BX INCx,
  DI [DI] x[] MOV[], DI JMPr,
( ----- 003 )
CODE EXIT DX [BP] 0 x[]+ MOV[], BP DECx, BP DECx, ;CODE
CODE []= ( a1 a2 u -- f ) CX BX MOVxx, SI POPx, DI POPx,
  CLD, REPZ, CMPSB, BX 0 MOVxI, IFZ, BX INCx, THEN, ;CODE
CODE [C]? ( c a u -- i ) CX BX MOVxx, DI POPx, AX POPx,
  CLD, REPNZ, SCASB, IFNZ, CX BX MOVxx, THEN,
  BX CX SUBxx, BX DECx, ;CODE
CODE QUIT LSET L1 ( used in ABORT )
  BP RS_ADDR MOVxI, DI $0a ( main ) MOVxm, DI JMPr,
CODE ABORT SP PS_ADDR MOVxI, L1 BR JRi,
CODE BYE HLT, BEGIN, BR JRi,
( ----- 004 )
CODE FIND ( sa sl -- w? f ) CX BX MOVxx, SI POPx,
  DI SYSVARS $2 ( CURRENT ) + MOVxm,
  BEGIN, ( loop )
    AL [DI] -1 r[]+ MOV[], $7f ANDALi, ( strlen )
    CL AL CMPrr, IFZ, ( same len )
      SI PUSHx, DI PUSHx, CX PUSHx, ( --> )
        3 ADDALi, ( header ) AH AH XORrr, DI AX SUBxx,
        CLD, REPZ, CMPSB,
      CX POPx, DI POPx, SI POPx, ( <-- )
      IFZ, DI PUSHx, BX 1 MOVxI, ;CODE THEN,
    THEN,
    DI [x] 3 SUB[]i, DI [DI] x[] MOV[], ( prev ) DI DI ORxx,
  BR JRNZi, ( loop ) BX BX XORxx, ;CODE
( ----- 005 )
CODE * AX POPx, DX PUSHx, ( protect from MUL ) BX MULx, DX POPx,
  BX AX MOVxx, ;CODE
CODE /MOD AX POPx, DX PUSHx, ( protect )
  DX DX XORxx, BX DIVx,
  BX DX MOVxx, DX POPx, ( unprotect )
  BX PUSHx, ( modulo ) BX AX MOVxx, ( division ) ;CODE
CODE RCNT
  BX PUSHx, BX BP MOVxx, AX RS_ADDR MOVxI, BX AX SUBxx, ;CODE
CODE SCNT
  AX PS_ADDR MOVxI, AX SP SUBxx, BX PUSHx, BX AX MOVxx, ;CODE
CODE TICKS ( n=100us ) BX PUSHx,
    SI DX MOVxx, ( protect IP )
    AX POPx, BX 100 MOVxI, BX MULx,
    CX DX MOVxx, ( high ) DX AX MOVxx, ( low )
    AX $8600 MOVxI, ( 86h, WAIT ) $15 INT,
    DX SI MOVxx, ( restore IP ) BX POPx, ;CODE
( ----- 006 )
CODE (n)
  BX PUSHx, DI DX MOVxx, BX [DI] x[] MOV[], 
  DX INCx, DX INCx, ;CODE
CODE (b)
  BX PUSHx, DI DX MOVxx, BH BH XORrr, BL [DI] r[] MOV[], 
  DX INCx, ;CODE
CODE (br) LSET L1 ( used in ?br )
  DI DX MOVxx, AL [DI] r[] MOV[], AH AH XORrr, CBW,
  DX AX ADDxx, ;CODE
CODE (?br)
  BX BX ORxx, BX POPx, L1 BR JRZi, DX INCx, ;CODE
CODE (next)
  [BP] 0 [w]+ DEC[], L1 BR JRNZi,
  BP DECx, BP DECx, DX INCx, ;CODE
( ----- 007 )
CODE + AX POPx, BX AX ADDxx, ;CODE
CODE - AX POPx, AX BX SUBxx, BX AX MOVxx, ;CODE
CODE < AX POPx, CX CX XORxx, AX BX CMPxx, IFC, CX INCx, THEN,
  BX CX MOVxx, ;CODE
CODE 1+ BX INCx, ;CODE
CODE 1- BX DECx, ;CODE
CODE AND AX POPx, BX AX ANDxx, ;CODE
CODE OR AX POPx, BX AX ORxx, ;CODE
CODE XOR AX POPx, BX AX XORxx, ;CODE
CODE NOT BX BX ORxx, BX 0 MOVxI, IFZ, BX INCx, THEN, ;CODE
CODE >> BX SHRx1, ;CODE
CODE << BX SHLx1, ;CODE
CODE >>8 BL BH MOVrr, BH BH XORrr, ;CODE
CODE <<8 BH BL MOVrr, BL BL XORrr, ;CODE
( ----- 008 )
CODE R@ BX PUSHx, BX [BP] 0 x[]+ MOV[], ;CODE
CODE R~ BP DECx, BP DECx, ;CODE
CODE R> BX PUSHx, BX [BP] 0 x[]+ MOV[], BP DECx, BP DECx, ;CODE
CODE >R BP INCx, BP INCx, [BP] 0 BX []+x MOV[], BX POPx, ;CODE
CODE ROT ( a b c -- b c a ) ( BX=c ) CX POPx, ( b ) AX POPx, \ a
  CX PUSHx, BX PUSHx, BX AX MOVxx, ;CODE
CODE ROT> ( a b c -- c a b ) CX POPx, AX POPx,
  BX PUSHx, AX PUSHx, BX CX MOVxx, ;CODE
CODE DUP LSET L1 BX PUSHx, ;CODE
CODE ?DUP AX BX MOVxx, AX AX ORxx, L1 BR JRNZi, ;CODE
CODE OVER ( a b -- a b a )
  AX POPx, AX PUSHx, BX PUSHx, BX AX MOVxx, ;CODE
CODE SWAP AX BX MOVxx, BX POPx, AX PUSHx, ;CODE
CODE DROP BX POPx, ;CODE
CODE EXECUTE AX BX MOVxx, BX POPx, AX JMPr,
( ----- 009 )
CODE C@ DI BX MOVxx, BH BH XORrr, BL [DI] r[] MOV[], ;CODE
CODE @ DI BX MOVxx, BX [DI] x[] MOV[], ;CODE
CODE C! DI BX MOVxx, CX POPx, [DI] CL []r MOV[], BX POPx, ;CODE
CODE ! DI BX MOVxx, CX POPx, [DI] CX []x MOV[], BX POPx, ;CODE
CODE JMPi! ( pc a -- len ) DI BX MOVxx, AX POPx, 
  CL $e9 MOVri, LSET L1 [DI] CL []r MOV[],
  CX SYSVARS $4 ( HOME ) + MOVxm, AX CX SUBxx, AX DECx, AX DECx,
  AX DECx, [DI] 1 AX []+x MOV[], BX 3 MOVxI, ;CODE
CODE CALLi! ( pc a -- len ) DI BX MOVxx, AX POPx, 
  CL $e8 MOVri, L1 BR JRi,
( ----- 011 )
\ 8086 assembler. See doc/asm
28 CONSTS 0 AL 1 CL 2 DL 3 BL
          4 AH 5 CH 6 DH 7 BH
          0 AX 1 CX 2 DX 3 BX
          4 SP 5 BP 6 SI 7 DI
          0 ES 1 CS 2 SS 3 DS
          0 [BX+SI] 1 [BX+DI] 2 [BP+SI] 3 [BP+DI]
          4 [SI] 5 [DI] 6 [BP] 7 [BX]
: <<3 << << << ;
( ----- 012 )
: OP1 DOER C, DOES> C@ C, ;
$c3 OP1 RET,        $fa OP1 CLI,       $fb OP1 STI,
$f4 OP1 HLT,        $fc OP1 CLD,       $fd OP1 STD,
$90 OP1 NOP,        $98 OP1 CBW,
$f3 OP1 REPZ,       $f2 OP1 REPNZ,     $ac OP1 LODSB,
$ad OP1 LODSW,      $a6 OP1 CMPSB,     $a7 OP1 CMPSW,
$a4 OP1 MOVSB,      $a5 OP1 MOVSW,     $ae OP1 SCASB,
$af OP1 SCASW,      $aa OP1 STOSB,     $ab OP1 STOSW,

: OP1r DOER C, DOES> C@ + C, ;
$40 OP1r INCx,      $48 OP1r DECx,
$58 OP1r POPx,      $50 OP1r PUSHx,
( ----- 013 )
: OPr0 ( reg op ) DOER C, C, DOES>
    C@+ C, C@ <<3 OR $c0 OR C, ;
0 $d0 OPr0 ROLr1,   0 $d1 OPr0 ROLx1,  4 $f6 OPr0 MULr,
1 $d0 OPr0 RORr1,   1 $d1 OPr0 RORx1,  4 $f7 OPr0 MULx,
4 $d0 OPr0 SHLr1,   4 $d1 OPr0 SHLx1,  6 $f6 OPr0 DIVr,
5 $d0 OPr0 SHRr1,   5 $d1 OPr0 SHRx1,  6 $f7 OPr0 DIVx,
0 $d2 OPr0 ROLrCL,  0 $d3 OPr0 ROLxCL, 1 $fe OPr0 DECr,
1 $d2 OPr0 RORrCL,  1 $d3 OPr0 RORxCL, 0 $fe OPr0 INCr,
4 $d2 OPr0 SHLrCL,  4 $d3 OPr0 SHLxCL,
5 $d2 OPr0 SHRrCL,  5 $d3 OPr0 SHRxCL,
( ----- 014 )
: OPrr DOER C, DOES> C@ C, <<3 OR $c0 OR C, ;
$31 OPrr XORxx,     $30 OPrr XORrr,
$88 OPrr MOVrr,     $89 OPrr MOVxx,    $28 OPrr SUBrr,
$29 OPrr SUBxx,     $08 OPrr ORrr,     $09 OPrr ORxx,
$38 OPrr CMPrr,     $39 OPrr CMPxx,    $00 OPrr ADDrr,
$01 OPrr ADDxx,     $12 OPrr ADCrr,    $13 OPrr ADCxx,
$20 OPrr ANDrr,     $21 OPrr ANDxx,
( ----- 015 )
4 WORDTBL mods 'W NOOP 'W C, 'W L, 'W NOOP
: modrm ( disp? modrm -- )
  DUP C, DUP $c7 AND 6 = IF DROP $80 THEN 64 / mods SWAP WEXEC ;
: OP[] ( opbase+modrmbase ) DOER , DOES>
  @ L|M ( disp? modrm opoff modrmbase op ) ROT + C, + modrm ;
( -- disp? modrm opoff )
: [b] ( r/m ) 0 ; : [w] ( r/m ) 1 ;
: [m] ( a ) 6 0 ; : [M] [m] 1+ ;
: [r] ( r ) $c0 OR 0 ; : [x] [r] 1+ ;
: [b]+ ( r/m disp8 ) SWAP $40 OR 0 ; : [w]+ [b]+ 1+ ;
: r[] ( r r/m ) SWAP <<3 OR 2 ; : x[] r[] 1+ ;
: []r ( r/m r ) <<3 OR 0 ; : []x []r 1+ ;
: r[]+ ( r r/m disp8 )
    ROT <<3 ROT OR $40 OR 2 ; : x[]+ r[]+ 1+ ;
: []+r ( r/m disp8 r ) <<3 ROT OR $40 OR 0 ; : []+x []+r 1+ ;
( ----- 016 )
$fe00 OP[] INC[],        $fe08 OP[] DEC[],
$fe30 OP[] PUSH[],       $8e00 OP[] POP[],
$8800 OP[] MOV[],        $3800 OP[] CMP[],

: OP[]i ( opbase+modrmbase ) DOER , DOES> SWAP >R ( i )
  SWAP ( opoff ) DUP IF R@ >>8 NOT IF 2 + THEN THEN >R
  @ L|M ( disp? modrm modrmbase op )
  R@ + C, + modrm R> 1 = IF R> L, ELSE R> C, THEN ;
$8000 OP[]i ADD[]i,      $8010 OP[]i ADC[]i,
$8038 OP[]i CMP[]i,      $8028 OP[]i SUB[]i,

: OPI DOER C, DOES> C@ C, L, ;
$05 OPI ADDAXI,     $15 OPI ADCALI,    $25 OPI ANDAXI,
$2d OPI SUBAXI,     $a1 OPI MOVAXm,    $a3 OPI MOVmAX,
( ----- 017 )
: OPi DOER C, DOES> C@ C, C, ;
$04 OPi ADDALi,     $14 OPi ADCALi,    $24 OPi ANDALi,
$2c OPi SUBALi,     $cd OPi INT,
$eb OPi JRi,        $74 OPi JRZi,
$75 OPi JRNZi,      $72 OPi JRCi,      $73 OPi JRNCi,
$a0 OPi MOVALm,     $a2 OPi MOVmAL,
: MOVri, SWAP $b0 OR C, C, ; : MOVxI, SWAP $b8 OR C, L, ;
: MOVsx, $8e C, SWAP <<3 OR $c0 OR C, ;
: MOVrm, $8a C, SWAP <<3 $6 OR C, L, ;
: MOVxm, $8b C, SWAP <<3 $6 OR C, L, ;
: MOVmr, $88 C, <<3 $6 OR C, L, ;
: MOVmx, $89 C, <<3 $6 OR C, L, ;
: PUSHs, <<3 $06 OR C, ; : POPs, <<3 $07 OR C, ;
: JMPr, $ff C, 7 AND $e0 OR C, ;
: JMPf, ( seg off ) $ea C, L, L, ;
( ----- 018 )
: JMPi, $e9 C, ( jmp near ) PC - 2 - L, ;
: CALLi, $e8 C, ( jmp near ) PC - 2 - L, ;
: i>, BX PUSHx, BX SWAP MOVxI, ;
: JMP(i), MOVAXm, AX JMPr, ;
: (i)>, BX PUSHx, BX SWAP MOVxm, ;
( ----- 020 )
( PC/AT drivers. Load range: 320-326 )
CODE (key?)
  BX PUSHx, BX BX XORxx, AH 1 MOVri, $16 INT, IFNZ,
    AH AH XORrr, $16 INT, AH AH XORrr, BX INCx, AX PUSHx, THEN,
;CODE
( ----- 021 )
CODE 13H08H ( driveno -- cx dx )
  DX PUSHx, ( protect ) DX BX MOVxx, AX $800 MOVxI,
  ES PUSHs, DI DI XORxx, ES DI MOVsx,
  $13 INT, BX DX MOVxx, ES POPs, DX POPx, ( unprotect )
  CX PUSHx, ;CODE
CODE 13H ( ax bx cx dx -- ax bx cx dx )
  SI BX MOVxx, ( DX ) CX POPx, BX POPx, AX POPx,
  DX PUSHx, ( protect ) DX SI MOVxx, DI DI XORxx,
  $13 INT, SI DX MOVxx, DX POPx, ( unprotect )
  AX PUSHx, BX PUSHx, CX PUSHx, BX SI MOVxx, ;CODE
( ----- 022 )
DRV_ADDR CONSTANT FDSPT
DRV_ADDR 1+ CONSTANT FDHEADS
:~ ( AX BX sec )
  ( AH=read sectors, AL=1 sector, BX=dest,
    CH=trackno CL=secno DH=head DL=drive )
  FDSPT C@ /MOD ( AX BX sec trk )
  FDHEADS C@ /MOD ( AX BX sec head trk )
  <<8 ROT OR 1+ ( AX BX head CX )
  SWAP <<8 $03 C@ ( boot drive ) OR ( AX BX CX DX )
  13H 2DROP 2DROP ;
( ----- 023 )
\ Sectors are 512b, so blk numbers are all x2. We add 16 to
\ this because blkfs starts at sector 16.
: FD@ ( blkno blk( -- )
  SWAP << ( 2* ) 16 + 2DUP ( a b a b )
  $0201 ROT> ( a b c a b ) ~ ( a b )
  1+ SWAP $200 + SWAP $0201 ROT> ( c a b ) ~ ;
: FD! ( blkno blk( -- )
  SWAP << ( 2* ) 16 + 2DUP ( a b a b )
  $0301 ROT> ( a b c a b ) ~ ( a b )
  1+ SWAP $200 + SWAP $0301 ROT> ( c a b ) ~ ;
: FD$
\ get number of sectors per track with command 08H.
  $03 ( boot drive ) C@ 13H08H
  >>8 1+ FDHEADS C!
  $3f AND FDSPT C! ;
( ----- 024 )
2 CONSTS 80 COLS 25 LINES
CODE CURSOR! ( new old ) AX POPx, ( new ) DX PUSHx, ( protect )
  BX 80 MOVxI, DX DX XORxx, BX DIVx, ( col in DL, row in AL )
  DH AL MOVrr, AH 2 MOVri,
  $10 INT, DX POPx, ( unprotect ) BX POPx, ;CODE
CODE _ ( c -- ) \ char out
  AL BL MOVrr, BX POPx, AH $0e MOVri, $10 INT, ;CODE
: CELL! ( c pos -- ) 0 CURSOR! _ ;
: NEWLN ( old -- new ) 1+ DUP LINES = IF 1- CR ~ LF ~ THEN ;
