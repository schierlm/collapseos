( ----- 000 )
Z80 MASTER INDEX

301 Z80 boot code              310 Z80 HAL
320 Z80 assembler
330 AT28 EEPROM                332 SPI relay
335 TMS9918
340 MC6850 driver              345 Zilog SIO driver
350 Sega Master System VDP     355 SMS PAD
360 SMS KBD                    367 SMS SPI relay
368 SMS Ports
370 TI-84+ LCD                 375 TI-84+ Keyboard
380 TRS-80 4P drivers
395 Dan SBC drivers            410 Virgil's workspace
( ----- 001 )
\ Z80 port's Macros and constants. See doc/code/z80.txt
: Z80A 320 328 LOADR 7 LOAD ( Flow words ) ;
: Z80C 302 314 LOADR ;
: TRS804PM 380 LOAD ;
\ see comment at TICKS' definition
\ 7.373MHz target: 737t. outer: 37t inner: 16t
\ tickfactor = (737 - 37) / 16
44 CONSTANT tickfactor
( ----- 002 )
\ Z80 port, core routines
FJR JR, TO L1 $10 OALLOT LSET lblxt ( RST 10 )
  IX INCd, IX INCd, 0 IX+ E LDIXYr, 1 IX+ D LDIXYr,
  HL POP, LDDE(HL), HL INCd, EXDEHL, JP(HL), \ 17 bytes
$28 OALLOT LSET lblcell ( RST 28 )
  HL POP, BC PUSH, HL>BC, FJR JR, TO L2 ( next ) $30 OALLOT
0 JP, ( RST 30 ) $38 OALLOT
0 JP, ( RST 38 ) $66 OALLOT RETN,
L1 FMARK
  DI, SP PS_ADDR LDdi, IX RS_ADDR LDdi, 
  BIN( $04 ( BOOT ) + LDHL(i), JP(HL),
LSET lblval HL POP, BC PUSH, LDBC(HL), \ to lblnext
LSET lblnext L2 FMARK
  EXDEHL, LSET L1 ( EXIT ) LDDE(HL), HL INCd, EXDEHL, JP(HL),
LSET lbldoes HL POP, BC PUSH, HL>BC, BC INCd, BC INCd, LDHL(HL),
  JP(HL),
( ----- 003 )
\ Z80 port, EXIT QUIT ABORT BYE RCNT SCNT
CODE EXIT ( put new IP in HL instead of DE for speed )
  L 0 IX+ LDrIXY, H 1 IX+ LDrIXY, IX DECd, IX DECd, L1 JP,
CODE QUIT LSET L1 ( used in ABORT )
IX RS_ADDR LDdi, BIN( $0a ( main ) + LDHL(i), JP(HL),
CODE ABORT SP PS_ADDR LDdi, L1 BR JR,
CODE BYE HALT,
CODE RCNT BC PUSH, IX PUSH, HL POP, BC RS_ADDR LDdi,
  BC SUBHLd, HL>BC, ;CODE
CODE SCNT HL 0 LDdi, SP ADDHLd, BC PUSH, HL>BC, HL PS_ADDR LDdi,
  BC SUBHLd, HL>BC, ;CODE
( ----- 004 )
\ Z80 port, TICKS
\ The word below is designed to wait the proper 100us per tick
\ at 500kHz when tickfactor is 1. If the CPU runs faster,
\ tickfactor has to be adjusted accordingly. "t" in comments
\ below means "T-cycle", which at 500kHz is worth 2us.
CODE TICKS
  \ we pre-dec to compensate for initialization
  BEGIN,
    BC DECd, ( 6t )
    IFZ, ( 12t ) BC POP, ;CODE THEN,
    A tickfactor LDri, ( 7t )
    BEGIN, A DECr, ( 4t ) BR JRNZ, ( 12t )
  BR JR, ( 12t ) ( outer: 37t inner: 16t )
( ----- 005 )
\ Z80 port, PC! PC@ []= [C]? (im1)
CODE PC! HL POP, L OUT(C)r, BC POP, ;CODE
CODE PC@ C INr(C), B 0 LDri, ;CODE
CODE []= BC PUSH, EXX, ( protect DE ) BC POP, DE POP, HL POP,
  LSET L1 ( loop )
    LDA(DE), DE INCd, CPI,
    IFNZ, EXX, BC 0 LDdi, ;CODE THEN,
    CPE L1 JPc, ( BC not zero? loop )
  EXX, BC 1 LDdi, ;CODE
CODE [C]? BCZ, IFZ, BC DECd, HL POP, HL POP, ;CODE THEN,
  BC PUSH, EXX, BC POP, HL POP, DE POP, A E LDrr, D H LDrr,
  E L LDrr, \ HL=a DE=a BC=u A=c 
  CPIR, IFZ, DE SUBHLd, HL DECd, ELSE, HL -1 LDdi, THEN,
  HL PUSH, EXX, BC POP, ;CODE
CODE (im1) IM1, EI, ;CODE
( ----- 006 )
\ Z80 port, /MOD *
CODE * HL POP, DE PUSH, EXDEHL, ( DE * BC -> HL )
  HL 0 LDdi, A $10 LDri, BEGIN,
    HL ADDHLd, E RL, D RL,
    IFC, BC ADDHLd, THEN,
    A DECr, BR JRNZ,
  HL>BC, DE POP, ;CODE
\ Divides AC by DE. quotient in AC remainder in HL
CODE /MOD BC>HL, BC POP, DE PUSH, EXDEHL,
  A B LDrr, B 16 LDri, HL 0 LDdi, BEGIN,
    SCF, C RL, RLA, HL ADCHLd, DE SBCHLd,
    IFC, DE ADDHLd, C DECr, THEN,
  BR DJNZ,
  DE POP, HL PUSH, B A LDrr, ;CODE
( ----- 007 )
\ Z80 port, FIND
CODE FIND ( sa sl -- w? f ) HL POP, 
  BC ADDHLd, \ HL points to after last char of s
  'N HL LD(i)d, HL SYSVARS $02 ( CURRENT ) + LDd(i), BEGIN,
    HL DECd, A (HL) LDrr, $7f ANDi, ( imm ) C CPr, IFZ,
      HL PUSH, DE PUSH, BC PUSH, DE 'N LDd(i),
      HL DECd, HL DECd, HL DECd, \ Skip prev field
      LSET L1 ( loop )
        DE DECd, LDA(DE), CPD, IFZ, TO L2 ( break! )
      CPE L1 JPc, ( BC not zero? loop ) L2 FMARK
      BC POP, DE POP, HL POP, THEN,
    IFZ, ( match ) HL INCd, HL PUSH, BC 1 LDdi, ;CODE THEN,
    \ no match, go to prev and continue
    HL DECd, A (HL) LDrr, HL DECd, L (HL) LDrr, H A LDrr,
    L ORr, IFZ, ( end of dict ) BC 0 LDdi, ;CODE THEN,
  BR JR,
( ----- 008 )
\ Z80 port, (b) (n) (br) (?br) (next)
CODE (b) ( -- c ) BC PUSH, LDA(DE), A>BC, DE INCd, ;CODE
CODE (n) ( -- n ) BC PUSH,
  EXDEHL, LDBC(HL), HL INCd, EXDEHL, ;CODE
CODE (br) LSET L1 ( used in ?br and next )
  LDA(DE), ( sign extend A into HL )
  L A LDrr, A ADDr, ( sign in carry ) A SBCr, ( FF if neg )
  H A LDrr, DE ADDHLd, ( HL --> new IP ) EXDEHL, ;CODE
CODE (?br) BCZ, BC POP, L1 BR JRZ, DE INCd, ;CODE
CODE (next)
  0 IX+ DEC(IXY+), IFNZ,
    A $ff LDri, 0 IX+ CP(IXY+), IFZ, 1 IX+ DEC(IXY+), THEN,
    L1 BR JR, THEN,
  A XORr, 1 IX+ CP(IXY+), L1 BR JRNZ,
  IX DECd, IX DECd, DE INCd, ;CODE
( ----- 009 )
\ Z80 port, >R I C@ @ C! ! 1+ 1- + -
CODE >R IX INCd, IX INCd, 0 IX+ C LDIXYr, 1 IX+ B LDIXYr,
  BC POP, ;CODE
CODE R@ BC PUSH, C 0 IX+ LDrIXY, B 1 IX+ LDrIXY, ;CODE
CODE R~ IX DECd, IX DECd, ;CODE
CODE R> BC PUSH, C 0 IX+ LDrIXY, B 1 IX+ LDrIXY,
  IX DECd, IX DECd, ;CODE
CODE C@ LDA(BC), A>BC, ;CODE
CODE @ BC>HL, LDBC(HL), ;CODE
CODE C! BC>HL, BC POP, (HL) C LDrr, BC POP, ;CODE
CODE ! BC>HL, BC POP,
  (HL) C LDrr, HL INCd, (HL) B LDrr, BC POP, ;CODE
CODE 1+ BC INCd, ;CODE
CODE 1- BC DECd, ;CODE
CODE + HL POP, BC ADDHLd, HL>BC, ;CODE
CODE - HL POP, BC SUBHLd, HL>BC, ;CODE
( ----- 010 )
\ Z80 port, AND OR XOR >> << >>8 <<8
CODE AND HL POP,
  A C LDrr, L ANDr, C A LDrr,
  A B LDrr, H ANDr, B A LDrr, ;CODE
CODE OR HL POP,
  A C LDrr, L ORr, C A LDrr,
  A B LDrr, H ORr, B A LDrr, ;CODE
CODE XOR HL POP,
  A C LDrr, L XORr, C A LDrr,
  A B LDrr, H XORr, B A LDrr, ;CODE
CODE NOT BCZ, BC 0 LDdi, IFZ, C INCr, THEN, ;CODE
CODE >> B SRL, C RR, ;CODE
CODE << C SLA, B RL, ;CODE
CODE >>8 C B LDrr, B 0 LDri, ;CODE
CODE <<8 B C LDrr, C 0 LDri, ;CODE
( ----- 011 )
\ Z80 port, ROT ROT> DUP DROP SWAP OVER EXECUTE
CODE ROT ( a b c -- b c a ) ( BC=c )
  HL POP, ( b ) EX(SP)HL, ( a<>b ) BC PUSH, ( c ) HL>BC, ;CODE
CODE ROT> ( a b c -- c a b ) ( BC=c )
  BC>HL, BC POP, ( b ) EX(SP)HL, ( a<>c ) HL PUSH, ;CODE
CODE DUP ( a -- a a ) LSET L1 BC PUSH, ;CODE
CODE ?DUP BCZ, L1 BR JRNZ, ;CODE
CODE DROP ( a -- ) BC POP, ;CODE
CODE SWAP ( a b -- b a ) HL POP, BC PUSH, HL>BC, ;CODE
CODE OVER ( a b -- a b a )
  HL POP, HL PUSH, BC PUSH, HL>BC, ;CODE
CODE EXECUTE BC>HL, BC POP, JP(HL),
( ----- 012 )
\ Z80 port, JMPi! CALLi! i>!
CODE JMPi! ( pc a -- len ) BC>HL, BC POP,
  A $c3 LDri, LSET L1 (HL) A LDrr, HL INCd,
  (HL) C LDrr, HL INCd, (HL) B LDrr, BC 3 LDdi, ;CODE
CODE CALLi! ( pc a -- len ) BC>HL, BC POP,
  A B LDrr, A ORr, IFZ, A C LDrr, $c7 ANDi, IFZ, \ RST
    A C LDrr, $c7 ORi, (HL) A LDrr, BC 1 LDdi, ;CODE THEN, THEN,
  ( not RST ) A $cd LDri, L1 BR JR,
( ----- 013 )
\ Z80 port speedups
CODE TUCK ( a b -- b a b ) HL POP, BC PUSH, HL PUSH, ;CODE
CODE NIP ( a b -- b ) HL POP, ;CODE
CODE +! ( n a -- ) BC>HL, LDBC(HL), HL DECd, EX(SP)HL,
  BC ADDHLd, HL>BC, HL POP, (HL) C LDrr, HL INCd, (HL) B LDrr,
  BC POP, ;CODE
CODE A> BC PUSH, IY PUSH, BC POP, ;CODE
CODE >A BC PUSH, IY POP, BC POP, ;CODE
CODE A>R IY PUSH, HL POP,
  IX INCd, IX INCd, 0 IX+ L LDIXYr, 1 IX+ H LDIXYr, ;CODE
CODE R>A L 0 IX+ LDrIXY, H 1 IX+ LDrIXY, IX DECd, IX DECd,
  HL PUSH, IY POP, ;CODE
CODE A+ IY INCd, ;CODE
CODE A- IY DECd, ;CODE
CODE AC@ BC PUSH, C 0 IY+ LDrIXY, B 0 LDri, ;CODE
CODE AC! 0 IY+ C LDIXYr, BC POP, ;CODE
( ----- 014 )
\ Z80 port speedups
CODE MOVE ( src dst u -- ) HL POP, EXDEHL, EX(SP)HL,
  BCZ, IFNZ, LDIR, THEN, DE POP, BC POP, ;CODE
CODE = HL POP, BC SUBHLd, BC 0 LDdi, IFZ, BC INCd, THEN, ;CODE
CODE < HL POP, BC SUBHLd, BC 0 LDdi, IFC, BC INCd, THEN, ;CODE
CODE CRC16 ( c n -- c ) BC PUSH, EXX, ( protect DE )
  HL POP, ( n ) DE POP, ( c ) A L LDrr, D XORr, D A LDrr,
  B 8 LDri, BEGIN,
    E SLA, D RL, IFC, ( msb is set, apply polynomial )
      A D LDrr, $10 XORi, D A LDrr,
      A E LDrr, $21 XORi, E A LDrr, THEN,
  BR DJNZ, 
  DE PUSH, EXX, ( unprotect DE ) BC POP, ;CODE
( ----- 020 )
\ Z80 Assembler. See doc/asm.txt
21 CONSTS 7 A 0 B 1 C 2 D 3 E 4 H 5 L 6 (HL)
          0 BC 1 DE 2 HL 3 AF 3 SP
          0 CNZ 1 CZ 2 CNC 3 CC 4 CPO 5 CPE 6 CP 7 CM
: <<3 << << << ; : <<4 <<3 << ;
\ As a general rule, IX and IY are equivalent to spitting an
\ extra $dd / $fd and then spit the equivalent of HL
: IX $dd C, HL ; : IY $fd C, HL ;
: IX+ <<8 >>8 $dd C, (HL) ;
: IY+ <<8 >>8 $fd C, (HL) ;
: OPXY DOER , DOES> @ ( xyoff opref ) EXECUTE C, ;
( ----- 021 )
: OP1 DOER C, DOES> C@ C, ;
$f3 OP1 DI,                   $fb OP1 EI,
$eb OP1 EXDEHL,               $d9 OP1 EXX,
$08 OP1 EXAFAF',              $e3 OP1 EX(SP)HL,
$76 OP1 HALT,                 $e9 OP1 JP(HL),
$12 OP1 LD(DE)A,              $1a OP1 LDA(DE),
$02 OP1 LD(BC)A,              $0a OP1 LDA(BC),
$00 OP1 NOP,                  $c9 OP1 RET,
$17 OP1 RLA,                  $07 OP1 RLCA,
$1f OP1 RRA,                  $0f OP1 RRCA,
$37 OP1 SCF,
( ----- 022 )
: OP1r DOER C, DOES> C@ ( r op ) SWAP <<3 OR C, ;
$04 OP1r INCr,                $05 OP1r DECr,
' INCr, OPXY INC(IXY+),        ' DECr, OPXY DEC(IXY+),
\ OP1r also works for conditions
$c0 OP1r RETc,

: OP1r0 DOER C, DOES> C@ ( r op ) OR C, ;
$80 OP1r0 ADDr,               $88 OP1r0 ADCr,
$a0 OP1r0 ANDr,               $b8 OP1r0 CPr,
$b0 OP1r0 ORr,                $90 OP1r0 SUBr,
$98 OP1r0 SBCr,               $a8 OP1r0 XORr,
' ADDr, OPXY ADD(IXY+),       ' ADCr, OPXY ADC(IXY+),
' CPr, OPXY CP(IXY+),         ' ORr, OPXY OR(IXY+),
' ANDr, OPXY AND(IXY+),       ' XORr, OPXY XOR(IXY+),
' SUBr, OPXY SUB(IXY+),       ' SBCr, OPXY SBC(IXY+),
( ----- 023 )
: OP1d DOER C, DOES> C@ ( d op ) SWAP <<4 OR C, ;
$c5 OP1d PUSH,                $c1 OP1d POP,
$03 OP1d INCd,                $0b OP1d DECd,
$09 OP1d ADDHLd,
: ADDIXd, IX DROP ADDHLd, ;  : ADDIXIX, HL ADDIXd, ;
: ADDIYd, IY DROP ADDHLd, ;  : ADDIYIY, HL ADDIYd, ;

: LDrr, ( rd rr ) SWAP <<3 OR $40 OR C, ;
' LDrr, OPXY LDIXYr,
: LDrIXY, ( rd ixy+- HL ) ROT SWAP LDIXYr, ;
: LDri, ( r i ) SWAP <<3 $06 OR C, C, ;
: LDdi, ( d n ) SWAP <<4 $01 OR C, L, ;
: LDd(i), ( d i ) $ed C, SWAP <<4 $4b OR C, L, ;
: LD(i)d, ( i d ) $ed C, <<4 $43 OR C, L, ;
( ----- 024 )
: OPED DOER C, DOES> $ed C, C@ C, ;
$a1 OPED CPI,       $b1 OPED CPIR,     $a9 OPED CPD,
$b9 OPED CPDR,      $46 OPED IM0,      $56 OPED IM1,
$5e OPED IM2,       $a0 OPED LDI,      $b0 OPED LDIR,
$a8 OPED LDD,       $b8 OPED LDDR,     $44 OPED NEG,
$4d OPED RETI,      $45 OPED RETN,     $a2 OPED INI,
$aa OPED IND,       $a3 OPED OUTI,

: OP2i DOER C, DOES> C@ ( i op ) C, C, ;
$d3 OP2i OUTiA,     $db OP2i INAi,
$c6 OP2i ADDi,      $ce OP2i ADCi,
$e6 OP2i ANDi,      $f6 OP2i ORi,      $d6 OP2i SUBi,
$ee OP2i XORi,      $fe OP2i CPi,
$18 OP2i JR,        $10 OP2i DJNZ,     $38 OP2i JRC,
$30 OP2i JRNC,      $28 OP2i JRZ,      $20 OP2i JRNZ,
( ----- 025 )
: OP2br DOER C, DOES>
    $cb C, C@ ( b r op ) ROT <<3 OR OR C, ;
$c0 OP2br SET,      $80 OP2br RES,     $40 OP2br BIT,
\ bitwise rotation ops have a similar sig
: OProt DOER C, DOES> $cb C, C@ ( r op ) OR C, ;
$10 OProt RL,       $00 OProt RLC,     $18 OProt RR,
$08 OProt RRC,      $20 OProt SLA,     $38 OProt SRL,

\ cell contains both bytes. MSB is spit as-is, LSB is ORed
\ with r.
: OP2r DOER , DOES> @ L|M ( r lsb msb ) C, SWAP <<3 OR C, ;
$ed41 OP2r OUT(C)r, $ed40 OP2r INr(C),

: OP2d DOER C, DOES> $ed C, C@ ( d op ) SWAP <<4 OR C, ;
$4a OP2d ADCHLd,    $42 OP2d SBCHLd,
( ----- 026 )
: OP3i DOER C, DOES> C@ ( i op ) C, L, ;
$cd OP3i CALL,                $c3 OP3i JP,
$22 OP3i LD(i)HL,             $2a OP3i LDHL(i),
$32 OP3i LD(i)A,              $3a OP3i LDA(i),

: RST, $c7 OR C, ;
: JP(IX), IX DROP JP(HL), ;
: JP(IY), IY DROP JP(HL), ;
: JPc, SWAP <<3 $c2 OR C, L, ;
: CALLc, SWAP <<3 $c4 OR C, L, ;
( ----- 027 )
\ Macros
: SUBHLd, A ORr, SBCHLd, ; \ clear carry + SBC
: PUSHA, B 0 LDri, C A LDrr, BC PUSH, ;
: HLZ, A H LDrr, L ORr, ;
: DEZ, A D LDrr, E ORr, ;
: BCZ, A B LDrr, C ORr, ;
: LDDE(HL), E (HL) LDrr, HL INCd, D (HL) LDrr, ;
: LDBC(HL), C (HL) LDrr, HL INCd, B (HL) LDrr, ;
: LDHL(HL), A (HL) LDrr, HL INCd, H (HL) LDrr, L A LDrr, ;
: OUTHL, DUP A H LDrr, OUTiA, A L LDrr, OUTiA, ;
: OUTDE, DUP A D LDrr, OUTiA, A E LDrr, OUTiA, ;
: HL>BC, B H LDrr, C L LDrr, ;
: BC>HL, H B LDrr, L C LDrr, ;
: A>BC, C A LDrr, B 0 LDri, ;
: A>HL, L A LDrr, H 0 LDri, ;
( ----- 028 )
\ Z80 HAL
ALIAS JP, JMPi, ALIAS JR, JRi,
: JMP(i), LDHL(i), JP(HL), ;
: CALLi, DUP $38 AND OVER = IF RST, ELSE CALL, THEN ;
ALIAS JRZ, JRZi, ALIAS JRNZ, JRNZi,
ALIAS JRC, JRCi, ALIAS JRNC, JRNCi,
: i>, BC PUSH, BC SWAP LDdi, ;
: (i)>, BC PUSH, BC SWAP LDd(i), ;
( ----- 030 )
CODE AT28C! ( c a -- )
  BC>HL, BC POP,
  (HL) C LDrr, A C LDrr, ( orig ) B C LDrr, ( save )
  C (HL) LDrr, ( poll ) BEGIN,
    A (HL) LDrr, ( poll ) C CPr, ( same as old? )
    C A LDrr, ( save old poll, Z preserved )
  BR JRNZ,
\ equal to written? SUB instead of CP to ensure IOERR is NZ
  B SUBr, IFNZ, SYSVARS ( IOERR ) LD(i)A, THEN, BC POP, ;CODE
: AT28! ( n a -- ) 2DUP AT28C! 1+ SWAP >>8 SWAP AT28C! ;
( ----- 032 )
( SPI relay driver. See doc/hw/z80/spi.txt )
CODE (spix) ( n -- n )
  A C LDrr,
  SPI_DATA OUTiA,
  ( wait until xchg is done )
  BEGIN, SPI_CTL INAi, 1 ANDi, BR JRNZ,
  SPI_DATA INAi,
  C A LDrr, ;CODE
CODE (spie) ( n -- )
  A C LDrr, SPI_CTL OUTiA, BC POP, ;CODE
( ----- 035 )
( Z80 driver for TMS9918. Implements grid protocol. Requires
TMS_CTLPORT, TMS_DATAPORT and ~FNT from the Font compiler at
B520. Patterns are at addr $0000, Names are at $3800.
Load range B315-317 )
CODE _ctl ( a -- sends LSB then MSB )
  A C LDrr, TMS_CTLPORT OUTiA, A B LDrr, TMS_CTLPORT OUTiA,
  BC POP, ;CODE
CODE _data
  A C LDrr, TMS_DATAPORT OUTiA, BC POP, ;CODE
( ----- 036 )
: _zero ( x -- send 0 _data x times )
  ( x ) >R BEGIN 0 _data NEXT ;
( Each row in ~FNT is a row of the glyph and there is 7 of
them.  We insert a blank one at the end of those 7. )
: _sfont ( a -- a+7, Send font to TMS )
  7 >R BEGIN C@+ _data NEXT ( blank row ) 0 _data ;
: _sfont^ ( a -- a+7, Send inverted font to TMS )
  7 >R BEGIN C@+ $ff XOR _data NEXT ( blank row ) $ff _data ;
: CELL! ( c pos )
    $7800 OR _ctl ( tilenum )
    SPC - ( glyph ) $5f MOD _data ;
( ----- 037 )
: CURSOR! ( new old -- )
    DUP $3800 OR _ctl [ TMS_DATAPORT LITN ] PC@
    $7f AND ( new old glyph ) SWAP $7800 OR _ctl _data
    DUP $3800 OR _ctl [ TMS_DATAPORT LITN ] PC@
    $80 OR ( new glyph ) SWAP $7800 OR _ctl _data ;
: COLS 40 ; : LINES 24 ;
: TMS$
    $8100 _ctl ( blank screen )
    $7800 _ctl COLS LINES * _zero
    $4000 _ctl $5f >R ~FNT BEGIN _sfont NEXT DROP
    $4400 _ctl $5f >R ~FNT BEGIN _sfont^ NEXT DROP
    $820e _ctl ( name table $3800 )
    $8400 _ctl ( pattern table $0000 )
    $87f0 _ctl ( colors 0 and 1 )
    $8000 _ctl $81d0 _ctl ( text mode, display on ) ;
( ----- 040 )
( MC6850 Driver. Load range B320-B322. Requires:
  6850_CTL for control register
  6850_IO for data register.
  CTL numbers used: $16 = no interrupt, 8bit words, 1 stop bit
  64x divide. $56 = RTS high )
CODE 6850>
  BEGIN,
    6850_CTL INAi, $02 ANDi, ( are we transmitting? )
  BR JRZ, ( yes, loop )
  A C LDrr, 6850_IO OUTiA, BC POP, ;CODE
( ----- 041 )
CODE 6850<? BC PUSH,
  A XORr, ( 256x ) A $16 ( RTS lo ) LDri, 6850_CTL OUTiA,
  BC 0 LDdi, ( pre-push a failure )
  BEGIN, EXAFAF', ( preserve cnt )
    6850_CTL INAi, $1 ANDi, ( rcv buff full? )
    IFNZ, ( full )
      6850_IO INAi, PUSHA, C 1 LDri, A XORr, ( end loop )
    ELSE, EXAFAF', ( recall cnt ) A DECr, THEN,
  BR JRNZ,
  A $56 ( RTS hi ) LDri, 6850_CTL OUTiA, ;CODE
( ----- 042 )
ALIAS 6850<? RX<? ALIAS 6850<? (key?)
ALIAS 6850> TX> ALIAS 6850> (emit)
: 6850$ $56 ( RTS high ) [ 6850_CTL LITN ] PC! ;
( ----- 045 )
( Zilog SIO driver. Load range B325-328. Requires:
  SIOA_CTL for ch A control register SIOA_DATA for data
  SIOB_CTL for ch B control register SIOB_DATA for data )
CODE SIOA<? BC PUSH,
  A XORr, ( 256x ) BC 0 LDdi, ( pre-push a failure )
  A 5 ( PTR5 ) LDri, SIOA_CTL OUTiA,
  A $68 ( RTS low ) LDri, SIOA_CTL OUTiA,
  BEGIN, EXAFAF', ( preserve cnt )
    SIOA_CTL INAi, $1 ANDi, ( rcv buff full? )
    IFNZ, ( full )
      SIOA_DATA INAi, PUSHA, C 1 LDri, A XORr, ( end loop )
    ELSE, EXAFAF', ( recall cnt ) A DECr, THEN,
  BR JRNZ,
  A 5 ( PTR5 ) LDri, SIOA_CTL OUTiA,
  A $6a ( RTS high ) LDri, SIOA_CTL OUTiA, ;CODE
( ----- 046 )
CODE SIOA>
  BEGIN,
    SIOA_CTL INAi, $04 ANDi, ( are we transmitting? )
  BR JRZ, ( yes, loop )
  A C LDrr, SIOA_DATA OUTiA, BC POP, ;CODE
CREATE _ ( init data ) $18 C, ( CMD3 )
    $24 C, ( CMD2/PTR4 ) $c4 C, ( WR4/64x/1stop/nopar )
    $03 C, ( PTR3 ) $c1 C, ( WR3/RXen/8char )
    $05 C, ( PTR5 ) $6a C, ( WR5/TXen/8char/RTS )
    $21 C, ( CMD2/PTR1 ) 0 C, ( WR1/Rx no INT )
: SIOA$ _ >A 9 >R BEGIN AC@+ [ SIOA_CTL LITN ] PC! NEXT ;
( ----- 047 )
CODE SIOB<? BC PUSH, ( copy/paste of SIOA<? )
  A XORr, ( 256x ) BC 0 LDdi, ( pre-push a failure )
  A 5 ( PTR5 ) LDri, SIOB_CTL OUTiA,
  A $68 ( RTS low ) LDri, SIOB_CTL OUTiA,
  BEGIN, EXAFAF', ( preserve cnt )
    SIOB_CTL INAi, $1 ANDi, ( rcv buff full? )
    IFNZ, ( full )
      SIOB_DATA INAi, PUSHA, C 1 LDri, A XORr, ( end loop )
    ELSE, EXAFAF', ( recall cnt ) A DECr, THEN,
  BR JRNZ,
  A 5 ( PTR5 ) LDri, SIOB_CTL OUTiA,
  A $6a ( RTS high ) LDri, SIOB_CTL OUTiA, ;CODE
( ----- 048 )
CODE SIOB>
  BEGIN,
    SIOB_CTL INAi, $04 ANDi, ( are we transmitting? )
  BR JRZ, ( yes, loop )
  A C LDrr, SIOB_DATA OUTiA, BC POP, ;CODE
: SIOB$ _ >A 9 >R BEGIN AC@+ [ SIOB_CTL LITN ] PC! NEXT ;
( ----- 050 )
\ VDP Driver. see doc/hw/sms/vdp.txt. Load range B330-B332.
CREATE _idat
$04 C, $80 C, \ Bit 2: Select mode 4
$00 C, $81 C,
$0f C, $82 C, \ Name table: $3800, *B0 must be 1*
$ff C, $85 C, \ Sprite table: $3f00
$ff C, $86 C, \ sprite use tiles from $2000
$ff C, $87 C, \ Border uses palette $f
$00 C, $88 C, \ BG X scroll
$00 C, $89 C, \ BG Y scroll
$ff C, $8a C, \ Line counter (why have this?)
( ----- 051 )
: _sfont ( a -- a+7, Send font to VDP )
  7 >R BEGIN C@+ _data 3 _zero NEXT ( blank row ) 4 _zero ;
: CELL! ( c pos )
  2 * $7800 OR _ctl ( c )
  $20 - ( glyph ) $5f MOD _data ;
( ----- 052 )
: CURSOR! ( new old -- )
  ( unset palette bit in old tile )
  2 * 1+ $7800 OR _ctl 0 _data
  ( set palette bit for at specified pos )
  2 * 1+ $7800 OR _ctl $8 _data ;
: VDP$
  9 >R _idat BEGIN DUP @ _ctl 1+ 1+ NEXT DROP
  ( blank screen ) $7800 _ctl COLS LINES * 2 * _zero
  ( palettes )
  $c000 _ctl
  ( BG ) 1 _zero $3f _data 14 _zero
  ( sprite, inverted colors ) $3f _data 15 _zero
  $4000 _ctl $5f >R ~FNT BEGIN _sfont NEXT DROP
  ( bit 6, enable display, bit 7, ?? ) $81c0 _ctl ;
: COLS 32 ; : LINES 24 ;
( ----- 055 )
( SMS pad driver. See doc/hw/z80/sms/pad.txt.
  Load range: 335-338 )
: _prevstat [ PAD_MEM LITN ] ;
: _sel [ PAD_MEM 1+ LITN ] ;
: _next [ PAD_MEM 2 + LITN ] ;
: _sel+! ( n -- ) _sel C@ + _sel C! ;
: _status ( -- n, see doc )
  1 _THA! ( output, high/unselected )
  _D1@ $3f AND ( low 6 bits are good )
( Start and A are returned when TH is selected, in bits 5 and
  4. Well get them, left-shift them and integrate them to B. )
  0 _THA! ( output, low/selected )
  _D1@ $30 AND << << OR ;
( ----- 056 )
: _chk ( c --, check _sel range )
  _sel C@ DUP $7f > IF $20 _sel C! THEN
  $20 < IF $7f _sel C! THEN ;
CREATE _ '0' C, ':' C, 'A' C, '[' C, 'a' C, $ff C,
: _nxtcls
  _sel @ >R _ BEGIN ( a R:c ) C@+ R@ > UNTIL ( a R:c ) R~
  1- C@ _sel ! ;
( ----- 057 )
: _updsel ( -- f, has an action button been pressed? )
  _status _prevstat C@ OVER = IF DROP 0 EXIT THEN
  DUP _prevstat C! ( changed, update ) ( s )
  $01 ( UP ) OVER AND NOT IF 1 _sel+! THEN
  $02 ( DOWN ) OVER AND NOT IF -1 _sel+! THEN
  $04 ( LEFT ) OVER AND NOT IF -5 _sel+! THEN
  $08 ( RIGHT ) OVER AND NOT IF 5 _sel+! THEN
  $10 ( BUTB ) OVER AND NOT IF _nxtcls THEN
  ( update sel in VDP )
  _chk _sel C@ XYPOS CELL!
  ( return whether any of the high 3 bits is low )
  $e0 AND $e0 < ;
( ----- 058 )
: (key?) ( -- c? f )
  _next C@ IF _next C@ 0 _next C! 1 EXIT THEN
  _updsel IF
    _prevstat C@
    $20 ( BUTC ) OVER AND NOT IF DROP _sel C@ 1 EXIT THEN
    $40 ( BUTA ) AND NOT IF $8 ( BS ) 1 EXIT THEN
    ( If not BUTC or BUTA, it has to be START )
    $d _next C! _sel C@ 1
    ELSE 0 ( f ) THEN ;
: PAD$ $ff _prevstat C! 'a' _sel C! 0 _next C! ;
( ----- 060 )
( kbd - implement (ps2kc) for SMS PS/2 adapter )
: (ps2kcA) ( for port A )
( Before reading a character, we must first verify that there
is something to read. When the adapter is finished filling its
'164 up, it resets the latch, which output's is connected to
TL. When the '164 is full, TL is low. Port A TL is bit 4 )
  _D1@ $10 AND IF 0 EXIT ( nothing ) THEN
  0 _THA! ( Port A TH output, low )
  _D1@ ( bit 3:0 go in 3:0 ) $0f AND ( n )
  1 _THA! ( Port A TH output, high )
  _D1@ ( bit 3:0 go in 7:4 ) $0f AND << << << << OR ( n )
  2 _THA! ( TH input ) ;
( ----- 061 )
: (ps2kcB) ( for port B )
  ( Port B TL is bit 2 )
  _D2@ $04 AND IF 0 EXIT ( nothing ) THEN
  0 _THB! ( Port B TH output, low )
  _D1@ ( bit 7:6 go in 1:0 ) >> >> >> >> >> >> ( n )
  _D2@ ( bit 1:0 go in 3:2 ) $03 AND << << OR ( n )
  1 _THB! ( Port B TH output, high )
  _D1@ ( bit 7:6 go in 5:4 ) $c0 AND >> >> OR ( n )
  _D2@ ( bit 1:0 go in 7:6 ) $03 AND <<8 >> >> OR ( n )
  2 _THB! ( TH input ) ;
( ----- 067 )
: (spie) DROP ; ( always enabled )
CODE (spix) ( x -- x, for port B )
  ( TR = DATA TH = CLK )
  CPORT_MEM LDA(i), $f3 ANDi, ( TR/TH output )
  B 8 LDri, BEGIN,
    $bf ANDi, ( TR lo ) C RL,
    IFC, $40 ORi, ( TR hi ) THEN,
    CPORT_CTL OUTiA, ( clic! ) $80 ORi, ( TH hi )
    CPORT_CTL OUTiA, ( clac! )
    EXAFAF', CPORT_D1 INAi, ( Up Btn is B6 ) RLA, RLA,
      L RL, EXAFAF',
    $7f ANDi, ( TH lo ) CPORT_CTL OUTiA, ( cloc! )
  BR DJNZ, CPORT_MEM LD(i)A,
  C L LDrr, ;CODE
( ----- 068 )
\ Routines for interacting with SMS controller ports.
\ Requires CPORT_MEM, CPORT_CTL, CPORT_D1 and CPORT_D2 to be
\ defined. CPORT_MEM is a 1 byte buffer for CPORT_CTL. The last
\ 3 consts will usually be $3f, $dc, $dd.
\ mode -- set TR pin on mode a on:
\ 0= output low 1=output high 2=input
CODE _TRA! ( B0 -> B4, B1 -> B0 )
  C RR, RLA, RLA, RLA, RLA, B RR, RLA,
  $11 ANDi, C A LDrr, CPORT_MEM LDA(i),
  $ee ANDi, C ORr, CPORT_CTL OUTiA, CPORT_MEM LD(i)A,
  BC POP, ;CODE
CODE _THA! ( B0 -> B5, B1 -> B1 )
  C RR, RLA, RLA, RLA, RLA, C RR, RLA, RLA,
  $22 ANDi, C A LDrr, CPORT_MEM LDA(i),
  $dd ANDi, C ORr, CPORT_CTL OUTiA, CPORT_MEM LD(i)A,
  BC POP, ;CODE
( ----- 069 )
CODE _TRB! ( B0 -> B6, B1 -> B2 )
  C RR, RLA, RLA, RLA, RLA, C RR, RLA, RLA, RLA,
  $44 ANDi, C A LDrr, CPORT_MEM LDA(i),
  $bb ANDi, C ORr, CPORT_CTL OUTiA, CPORT_MEM LD(i)A,
  BC POP, ;CODE
CODE _THB! ( B0 -> B7, B1 -> B3 )
  C RR, RLA, RLA, RLA, RLA, C RR, RLA, RLA, RLA, RLA,
  $88 ANDi, C A LDrr, CPORT_MEM LDA(i),
  $77 ANDi, C ORr, CPORT_CTL OUTiA, CPORT_MEM LD(i)A,
  BC POP, ;CODE
CODE _D1@ BC PUSH, CPORT_D1 INAi, C A LDrr, B 0 LDri, ;CODE
CODE _D2@ BC PUSH, CPORT_D2 INAi, C A LDrr, B 0 LDri, ;CODE
( ----- 070 )
( TI-84+ LCD driver. See doc/hw/z80/ti84/lcd.txt
  Load range: 350-353 )
: _mem+ [ LCD_MEM LITN ] @ + ;
: FNTW 3 ; : FNTH 5 ;
: COLS 96 FNTW 1+ / ; : LINES 64 FNTH 1+ / ;
( Wait until the lcd is ready to receive a command. It's a bit
  weird to implement a waiting routine in asm, but the forth
  version is a bit heavy and we don't want to wait longer than
  we have to. )
CODE _wait
  BEGIN,
    $10 ( CMD ) INAi,
    RLA, ( When 7th bit is clr, we can send a new cmd )
  BR JRC, ;CODE
( ----- 071 )
: LCD_BUF 0 _mem+ ;
: _cmd $10 ( CMD ) PC! _wait ;
: _data! $11 ( DATA ) PC! _wait ;
: _data@ $11 ( DATA ) PC@ _wait ;
: LCDOFF $02 ( CMD_DISABLE ) _cmd ;
: LCDON $03 ( CMD_ENABLE ) _cmd ;
: _yinc $07 _cmd ; : _xinc $05 _cmd ;
: _zoff! ( off -- ) $40 + _cmd ;
: _col! ( col -- ) $20 + _cmd ;
: _row! ( row -- ) $80 + _cmd ;
: LCD$
  HERE [ LCD_MEM LITN ] ! FNTH 2 * ALLOT
  LCDON $01 ( 8-bit mode ) _cmd FNTH 1+ _zoff!  ;
( ----- 072 )
: _clrrows ( n u -- Clears u rows starting at n )
  >R _row! BEGIN
    _yinc 0 _col! 11 >R BEGIN 0 _data! NEXT
    _xinc 0 _data! NEXT ;
: NEWLN ( oldln -- newln )
  1+ DUP 1+ FNTH 1+ * _zoff! ( ln )
  DUP FNTH 1+ * FNTH 1+ _clrrows ( newln ) ;
: LCDCLR 0 64 _clrrows ;
( ----- 073 )
: _atrow! ( pos -- ) COLS / FNTH 1+ * _row! ;
: _tocol ( pos -- col off ) COLS MOD FNTW 1+ * 8 /MOD ;
: CELL! ( c pos -- )
  DUP _atrow! DUP _tocol _col! ROT ( pos coff c )
  $20 - FNTH * ~FNT + ( pos coff a )
  _xinc _data@ DROP
  A> >R LCD_BUF >A FNTH >R BEGIN ( pos coff a )
    OVER 8 -^ SWAP C@+ ( pos coff 8-coff a+1 c ) ROT LSHIFT
    _data@ <<8 OR ( pos coff a+1 c )
    DUP A> FNTH + C! >>8 AC!+
  NEXT 2DROP ( pos )
  DUP _atrow!
  LCD_BUF >A FNTH >R BEGIN AC@+ _data! NEXT
  DUP _atrow! _tocol NIP 1+ _col!
  FNTH >R BEGIN AC@+ _data! NEXT R> >A ;
( ----- 075 )
\ Requires KBD_MEM, KBD_PORT and nC, from B120.
\ Load range: 355-359

\ gm -- pm, get pressed keys mask for group mask gm
CODE _get
  DI,
    A $ff LDri,
    KBD_PORT OUTiA,
    A C LDrr,
    KBD_PORT OUTiA,
    KBD_PORT INAi,
  EI,
  C A LDrr,
;CODE
( ----- 076 )
\ wait until all keys are de-pressed. To avoid repeat keys, we
\ require 64 subsequent polls to indicate all depressed keys.
\ all keys are considered depressed when the 0 group returns
\ $ff.
: _wait 64 BEGIN 0 _get $ff = NOT IF DROP 64 THEN
    1- DUP NOT UNTIL DROP ;
\ digits table. each row represents a group. 0 means unsupported
\ no group 7 because it has no key. $80 = alpha, $81 = 2nd
CREATE _dtbl 7 8 * nC,
  0   0   0   0   0   0    0 0
  $d  '+' '-' '*' '/' '^'  0 0
  0   '3' '6' '9' ')' 0    0 0
  '.' '2' '5' '8' '(' 0    0 0
  '0' '1' '4' '7' ',' 0    0 0
  0   0   0   0   0   0    0 $80
  0   0   0   0   0   $81  0 $7f
( ----- 077 )
\ alpha table. same as _dtbl, for when we're in alpha mode.
CREATE _atbl 7 8 * nC,
  0   0   0   0   0   0   0   0
  $d  '"' 'W' 'R' 'M' 'H' 0   0
  '?' 0   'V' 'Q' 'L' 'G' 0   0
  ':' 'Z' 'U' 'P' 'K' 'F' 'C' 0
  32 'Y'  'T' 'O' 'J' 'E' 'B' 0
  0  'X'  'S' 'N' 'I' 'D' 'A' $80
  0   0   0   0   0    $81 0  $7f
: _@ [ KBD_MEM LITN ] C@ ; : _! [ KBD_MEM LITN ] C! ;
: _2nd@ _@ 1 AND ; : _2nd! _@ $fe AND + _! ;
: _alpha@ _@ 2 AND ; : _alpha! 2 * _@ $fd AND + _! ;
: _alock@ _@ 4 AND ; : _alock^ _@ 4 XOR _! ;
( ----- 078 )
: _gti ( -- tindex, that it, index in _dtbl or _atbl )
  7 >R 0 BEGIN ( gid )
    1 OVER LSHIFT $ff -^ ( gid dmask ) _get
    DUP $ff = IF DROP 1+ ELSE R~ 1 >R THEN
  NEXT ( gid dmask )
  _wait $ff XOR ( dpos ) 0 ( dindex )
  BEGIN 1+ 2DUP RSHIFT NOT UNTIL 1-
  ( gid dpos dindex ) NIP
  ( gid dindex ) SWAP 8 * + ;
( ----- 079 )
: (key?) ( -- c? f )
    0 _get $ff = IF ( no key pressed ) 0 EXIT THEN
    _alpha@ _alock@ IF NOT THEN IF _atbl ELSE _dtbl THEN
    _gti + C@ ( c )
    DUP $80 = IF _2nd@ IF _alock^ ELSE 1 _alpha! THEN THEN
    DUP $81 = _2nd!
    DUP 1 $7f =><= IF ( we have something )
    ( lower? ) _2nd@ IF DUP 'A' 'Z' =><= IF $20 OR THEN THEN
        0 _2nd! 0 _alpha! 1 ( c f )
    ELSE ( nothing ) DROP 0 THEN ;
: KBD$ 0 [ KBD_MEM LITN ] C! ;
( ----- 080 )
\ TRS-80 drivers declarations and macros
: TRS804PL 381 389 LOADR ; : TRS804PH 390 LOAD ;
$f800 CONSTANT VIDMEM $bf CONSTANT CURCHAR
: fdstat $f0 INAi, ;
: fdcmd A SWAP LDri, B $18 LDri,
  $f0 OUTiA, BEGIN, BR DJNZ, ;
: fdwait BEGIN, fdstat RRCA, BR JRC, RLCA, ;
: vid+, ( reg -- ) HL VIDMEM LDdi, ADDHLd, ;
( ----- 081 )
\ TRS-80 4P video driver
24 CONSTANT LINES 80 CONSTANT COLS
CODE CELL! ( c pos -- ) HL POP,
  A L LDrr, BC vid+, (HL) A LDrr, BC POP, ;CODE
CODE CELLS! ( a pos u -- ) BC PUSH, EXX, BC POP, DE POP,
  DE vid+, EXDEHL, HL POP, BCZ, IFNZ, LDIR, THEN, EXX, BC POP,
;CODE
CODE CURSOR! ( new old -- ) BC vid+, A (HL) LDrr, CURCHAR CPi,
  IFZ, UNDERCUR LDA(i), (HL) A LDrr, THEN,
  BC POP, BC vid+, A (HL) LDrr, UNDERCUR LD(i)A, A CURCHAR LDri,
  (HL) A LDrr, BC POP, ;CODE
CODE SCROLL ( -- )
  EXX, HL VIDMEM 80 + LDdi, DE VIDMEM LDdi, BC 1840 LDdi, LDIR,
  H D LDrr, L E LDrr, DE INCd, A SPC LDri, (HL) A LDrr,
  BC 79 LDdi, LDIR, EXX, ;CODE
: NEWLN ( old -- new ) 1+ DUP LINES = IF 1- SCROLL THEN ;
( ----- 082 )
LSET L2 ( seek, B=trk )
  A 21 LDri, B CPr, FDMEM LDA(i), IFC, $20 ORi, ( WP ) THEN,
  $80 ORi, $f4 OUTiA, \ FD sel
  A B LDrr, ( trk ) $f3 OUTiA, $1c fdcmd RET,
CODE FDRD ( trksec addr -- st ) BC>HL, BC POP,
  L2 CALL, fdwait $98 ANDi, IFZ, DI,
    A C LDrr, $f2 OUTiA, ( sec ) C $f3 LDri, $84 fdcmd ( read )
    BEGIN, BEGIN, fdstat $b6 ANDi, BR JRZ, \ DRQ
      $b4 ANDi, IFZ, TO L3 ( error ) INI, BR JRNZ, THEN,
  fdwait $3c ANDi, L3 FMARK A>BC, EI, ;CODE
CODE FDWR ( trksec addr -- st ) BC>HL, BC POP,
  L2 CALL, fdwait $98 ANDi, IFZ, DI,
    A C LDrr, $f2 OUTiA, ( sec ) C $f3 LDri, $a4 fdcmd ( read )
    BEGIN, BEGIN, fdstat $f6 ANDi, BR JRZ, \ DRQ
      $f4 ANDi, IFZ, TO L3 ( error ) OUTI, BR JRNZ, THEN,
  fdwait $3c ANDi, L3 FMARK A>BC, EI, ;CODE
( ----- 083 )
CODE _dsel ( fdmask -- )
  A C LDrr, FDMEM LD(i)A, $80 ORi, $f4 OUTiA,
  0 fdcmd ( restore ) fdwait BC POP, ;CODE
: FDSEL ( drvno -- ) 1 SWAP LSHIFT [ FDMEM LITN ] C@ OVER = NOT
  IF _dsel ELSE DROP THEN ;
FDMEM 1+ DUP CONSTANT 'FDOP *ALIAS FDOP
FDMEM 3 + CONSTANT FDOFFS \ 4b, 2 for each drive
: _err LIT" FDerr " STYPE .X ABORT ;
: _trksec ( sec -- trksec )
\ 4 256b sectors per block, 18 sec per trk, 40 trk max
  18 /MOD ( sec trk ) DUP 39 > IF $ffff _err THEN <<8 + ;
: _in? ( blk off -- f ) - 180 < ;
: _dadj ( blk -- blk )
  FDOFFS @ 2DUP _in? IF 0 FDSEL - EXIT THEN DROP ( blk )
  FDOFFS 1+ 1+ @ 2DUP _in? IF 1 FDSEL - EXIT THEN DROP ( blk )
  . SPC> LIT" is out of disk range" STYPE ABORT ;
( ----- 084 )
: FD@! ( blk blk( -- )
  A> >R SWAP _dadj << << ( blk*4=sec ) >A 4 >R BEGIN ( dest )
    A> A+ _trksec OVER ( dest trksec dest )
    FDOP ( dest ) ?DUP IF _err THEN $100 +
  NEXT DROP R> >A ;
: FD@ ['] FDRD 'FDOP ! FD@! ;
: FD! ['] FDWR 'FDOP ! FD@! ;
( ----- 085 )
: CL$ ( baudcode -- )
  $02 $e8 PC! ( UART RST ) DUP 16 * OR $e9 PC! ( bauds )
  $6d $ea PC! ( word8 no parity no-RTS ) ;
CODE TX> BEGIN,
    $ea INAi, $40 ANDi, IFNZ, ( TX reg empty )
      $e8 INAi, $80 ANDi, IFZ, ( CTS low )
        A C LDrr, $eb OUTiA, ( send byte ) BC POP, ;CODE
  THEN, THEN, BR JR,
( ----- 086 )
CODE RX<? BC PUSH,
  A XORr, ( 256x ) BC 0 LDdi, ( pre-push a failure )
  A $6c ( RTS low ) LDri, $ea OUTiA,
  BEGIN, EXAFAF', ( preserve cnt )
    $ea INAi, $80 ANDi, ( rcv buff full? )
    IFNZ, ( full )
      $eb INAi, A>HL, HL PUSH, C INCr, A XORr, ( end loop )
    ELSE, EXAFAF', ( recall cnt ) A DECr, THEN,
  BR JRNZ,
  A $6d ( RTS high ) LDri, $ea OUTiA, ;CODE
( ----- 087 )
LSET L1 6 nC, '`' 'h' 'p' 'x' '0' '8'
LSET L2 8 nC, $0d 0 $ff 0 0 $08 0 $20
PC XORG $39 + T! ( RST 38 )
AF PUSH, HL PUSH, DE PUSH, BC PUSH,
$ec INAi, ( RTC INT ack )
$f440 LDA(i), A ORr, IFNZ, \ 7th row is special
  HL L2 1- LDdi, BEGIN, HL INCd, RRA, BR JRNC,
  A (HL) LDrr, ELSE, \ not 7th row
  HL L1 LDdi, DE $f401 LDdi, BC $600 LDdi, BEGIN,
    LDA(DE), A ORr, IFNZ,
      C (HL) LDrr, BEGIN, C INCr, RRA, BR JRNC,
      C DECr, THEN,
    E SLA, HL INCd, BR DJNZ,
  A C LDrr, THEN, \ cont.
( ----- 088 )
\ A=char or zero if no keypress. Now let's debounce
HL KBD_MEM 2 + LDdi, A ORr, IFZ, \ no keypress, debounce
  (HL) A LDrr, ELSE, \ keypress, is it debounced?
  (HL) CPr, IFNZ, \ != debounce buffer
    C A LDrr, (HL) C LDrr, $ff CPi, IFZ, \ BREAK!
      HL POP, HL POP, HL POP, HL POP, HL POP, EI,
      X' QUIT JP, THEN,
    HL DECd, $f480 LDA(i), 3 ANDi, (HL) A LDrr, HL DECd,
    (HL) C LDrr, THEN, THEN,
BC POP, DE POP, HL POP, AF POP, EI, RET,
( ----- 089 )
KBD_MEM CONSTANT KBDBUF \ LSB=char MSB=shift
: KBD$ 0 KBDBUF ! $04 $e0 PC! ( enable RTC INT ) (im1) ;
: (key?) KBDBUF @ DUP <<8 >>8 NOT IF DROP 0 EXIT THEN
  0 KBDBUF ! L|M ( char flags )
  OVER '<' '`' =><= IF 1 XOR THEN \ invert shift
  TUCK 1 AND IF \ lshift  ( flags char )
    DUP '@' < IF $ef ELSE $df THEN AND THEN
  SWAP 2 AND IF \ rshift ( char )
    DUP '1' < IF $2f ELSE $4a THEN + THEN
  1 ( success ) ;
( ----- 090 )
: FD0 FLUSH 0 FDSEL ;
: FD1 FLUSH 1 FDSEL ;
:~  [ FDMEM LITN ] C@ 1- << FDOFFS + ! ;
: D1 0 ~ ; : D2 200 ~ ; : D3 300 ~ ; : D4 400 ~ ; : D5 500 ~ ;
: ND $8000 ~ ;
: FD$ FDOFFS 4 $80 FILL ( no disk ) 1 FDSEL ;
( ----- 091 )
\ TRS-80 4P bootloader. Loads sectors 2-17 to addr 0.
DI, A $86 LDri, $84 OUTiA, \ mode 2, 80 chars, page 1
A $81 LDri, $f4 OUTiA, \ DRVSEL DD, drv0
A $40 LDri, $ec OUTiA, \ MODOUT 4MHZ, no EXTIO
HL 0 LDdi, ( dest addr ) A XORr, $e4 OUTiA, ( no NMI )
A INCr, ( trk1 ) BEGIN,
  $f3 OUTiA, EXAFAF', ( save ) $18 ( seek ) fdcmd fdwait
  A XORr, $f2 OUTiA, C $f3 LDri, BEGIN,
    $80 ( read sector ) fdcmd ( B=0 )
    BEGIN, fdstat RRA, RRA, BR JRNC, ( DRQ )
    INI, A $c1 LDri, BEGIN, $f4 OUTiA, INI, BR JRNZ,
    fdwait $1c ( error mask ) ANDi, IFNZ,
      SPC ADDi, VIDMEM LD(i)A, BEGIN, BR JR, THEN,
    $f2 INAi, A INCr, $f2 OUTiA, 18 CPi, BR JRC,
  EXAFAF', ( restore ) A INCr, 3 CPi, BR JRC, 0 RST,
( ----- 095 )
\ Dan SBC drivers. See doc/hw/z80/dan.txt
\ Macros
: OUTii, ( val port -- ) A ROT LDri, OUTiA, ;
: repeat ( n -- ) >R ' BEGIN ( w ) DUP EXECUTE NEXT DROP ;
( ----- 096 )
\ SPI relay driver
CODE (spix) ( n -- n )
  A C LDrr,
  SPI_DATA OUTiA,
  ( wait until xchg is done )
  NOP, NOP, NOP, NOP,
  SPI_DATA INAi,
  C A LDrr, ;CODE
CODE (spie) ( n -- )
  $9A CTL8255 OUTii, $3 CTL8255 OUTii,
  A C LDrr, 1 XORi, 1 ANDi, CTL8255 OUTiA, BC POP, ;CODE
( ----- 097 )
\ software framebuffer subsystem
VID_MEM CONSTANT VD_DECFR
VID_MEM $02 + CONSTANT VD_DECTL
VID_MEM $04 + CONSTANT VD_CURCL
VID_MEM $06 + CONSTANT VD_FRMST
VID_MEM $08 + CONSTANT VD_COLS
VID_MEM $0A + CONSTANT VD_LINES
VID_MEM $0C + CONSTANT VD_FRB
VID_MEM $0E + CONSTANT VD_OFS
\ Clear Framebuffer
CODE (vidclr) ( -- ) BC PUSH,
  $9A CTL8255 OUTii, $3 CTL8255 OUTii, $1 CTL8255 OUTii,
  BC VID_MEM $10 + LDdi, HL VID_WDTH VID_SCN * LDdi,
  BEGIN, A XORr, LD(BC)A, BC INCd, HL DECd, HLZ, BR JRNZ,
  BC POP, ;CODE
( ----- 098 )
: VID_OFS
  [ VID_WDTH 8 * LITN ] * + VD_FRB @ + VD_OFS ! (vidclr) ;
: VID$ ( -- )
  1 VD_DECFR ! 0 VD_DECTL ! 0 VD_CURCL ! 0
  VD_FRMST ! [ VID_WDTH 1 - LITN ] VD_COLS !
  [ VID_LN 1 - LITN ] VD_LINES !
  [ VID_MEM $10 + LITN ] VD_FRB ! 1 4 VID_OFS ;
( ----- 099 )
: COLS VD_COLS @ ;
: LINES VD_LINES @ ;
: VID_LOC VD_COLS @ /MOD
  [ VID_WDTH 8 * LITN ] * VD_OFS @ + ;
: CELL! VID_LOC + SWAP SPC - DUP 96 < IF
  DUP DUP << + << + ~FNT + 7 >R BEGIN
  2DUP C@ >> SWAP C! 1+ SWAP
  [ VID_WDTH LITN ] + SWAP NEXT
  DROP 0 SWAP C! ELSE 2DROP THEN ;
( ----- 100 )
: VID_LCR VID_LOC SWAP DUP
  DUP 12 < IF DROP 0 ELSE 12 -
  DUP [ VID_WDTH 24 - LITN ] > IF DROP [ VID_WDTH 24 - LITN ]
  THEN THEN VD_CURCL ! ;
: CURSOR! 0 SWAP VID_LOC + [ VID_WDTH 7 * LITN ] + C!
  255 SWAP VID_LCR + [ VID_WDTH 7 * LITN ] + C! ;
CODE (vidscr) BC PUSH, EXX,
 BC VID_SCN 8 - VID_WDTH * LDdi, DE VID_MEM $10 + LDdi,
 HL VID_MEM $10 + VID_WDTH 8 * + LDdi,
 LDIR,  HL VID_WDTH 8 * LDdi,
 BEGIN, A XORr, LD(DE)A, DE INCd, HL DECd, HLZ,
 BR JRNZ, EXX, BC POP, ;CODE
: NEWLN DUP 1+ VD_LINES @ = IF (vidscr) ELSE 1+ THEN ;
( ----- 101 )
\ Stream video frames, single scan
CODE (vidfr) ( -- ) BC PUSH, EXX,
  C SPI_DATA LDri, DE VID_MEM $04 + LDd(i),
  HL VID_MEM 40 + VID_WDTH - LDdi, DE ADDHLd,
  VID_MEM $06 + LD(i)HL, DE VID_WDTH 24 - LDdi,
  B VID_SCN LDri,
  LSET L1 BEGIN,
    6 CTL8255 OUTii, DE ADDHLd, 7 CTL8255 OUTii,
    A B LDrr, 4 repeat NOP, 24 repeat OUTI,
    B A LDrr, BR DJNZ,
  B 0 LDri, B 0 LDri, B 0 LDri, B VID_VBL 1 - LDri, FJR JR,
  LSET L2 A VID_VBL 1 - LDri, FJR JR, FMARK FMARK
    A B LDrr, B 28 LDri, BEGIN, BR DJNZ, HL INCd, B A LDrr,
    7 CTL8255 OUTii, 5 repeat NOP, 6 CTL8255 OUTii,
  L2 BR DJNZ,
( ----- 102 )
  VID_MEM $02 + LDA(i), B A LDrr, VID_MEM LDA(i),
  B SUBr, IFNZ,
    VID_MEM LD(i)A, B 23 LDri, HL INCd, B 23 LDri,
    BEGIN, BR DJNZ,
    VID_MEM $06 + LDHL(i), B VID_SCN LDri, 7 CTL8255 OUTii,
    5 repeat NOP, 6 CTL8255 OUTii, L1 JMPi,
  THEN, EXX, BC POP, ;CODE
( ----- 103 )
\ Stream video frames, double scan
CODE (vidfr) ( -- ) BC PUSH, EXX,
  C SPI_DATA LDri, DE VID_MEM $04 + LDd(i),
  HL VID_MEM 40 + VID_WDTH - LDdi, DE ADDHLd,
  VID_MEM $06 + LD(i)HL, DE VID_WDTH 24 - LDdi, B VID_SCN LDri,
  LSET L1 BEGIN,
    6 CTL8255 OUTii, DE ADDHLd, 7 CTL8255 OUTii, A B LDrr,
    DE DECd, DE -25 LDdi, 24 repeat OUTI,
    AF PUSH, DE INCd, 6 CTL8255 OUTii, DE ADDHLd,
    7 CTL8255 OUTii, AF POP, DE VID_WDTH 24 - LDdi,
    24 repeat OUTI, B A LDrr, BR DJNZ,
  B 0 LDri, B 0 LDri, B 0 LDri, B VID_VBL 1 - LDri, FJR JR,
  LSET L2 A VID_VBL 1 - LDri, FJR JR, FMARK FMARK
    A B LDrr, B 28 LDri, BEGIN, BR DJNZ, HL INCd, B A LDrr,
    7 CTL8255 OUTii, 5 repeat NOP, 6 CTL8255 OUTii,
  L2 BR DJNZ,
( ----- 104 )
  VID_MEM $02 + LDA(i), B A LDrr, VID_MEM LDA(i), B SUBr, IFNZ,
    VID_MEM LD(i)A, B 23 LDri, HL INCd, B 23 LDri,
    BEGIN, BR DJNZ, VID_MEM $06 + LDHL(i), B VID_SCN LDri,
    7 CTL8255 OUTii, 5 repeat NOP, 6 CTL8255 OUTii, L1 JMPi,
  THEN, EXX, BC POP, ;CODE
( ----- 105 )
\ PS2 keyboard driver subsystem
PSK_MEM CONSTANT PSK_STAT
PSK_MEM $02 + CONSTANT PSK_CC
PSK_MEM $04 + CONSTANT PSK_BUFI
PSK_MEM $06 + CONSTANT PSK_BUFO
PSK_MEM $08 + CONSTANT PSK_BUF
PC XORG $39 + T! ( RST 38 )
DI, AF PUSH, $10 SIOA_CTL OUTii, SIOA_CTL INAi,
4 A BIT, IFZ, AF POP, EI, RETI, THEN, ( I1 - T1 )
PSK_MEM LDA(i), A ORr,
IFZ, PTC8255 INAi, 7 A BIT,         ( I1 - )
IFZ, A 1 LDri, PSK_MEM LD(i)A, THEN,  ( I2 - T2 )
( ----- 106 )
AF POP, EI, RETI, THEN,             ( - T1 )
$9 CPi, FJR JRNZ, TO L3
HL PUSH, PSK_MEM $02 + LDHL(i), H 8 LDri, A XORr,
BEGIN, L RRC, 0 ADCi, H DECr, BR JRNZ,
H A LDrr, PTC8255 INAi, A H LDrr, 0 ADCi, $1 ANDi,
FJR JRZ, TO L1 A XORr, VID_MEM LD(i)A, VID_MEM $02 + LD(i)A,
PSK_MEM $04 + LDA(i), L A LDrr, PSK_MEM $06 + LDA(i),
A INCr, PS2_BMSK ANDi, L CPr, FJR JRZ, TO L1
PSK_MEM $06 + LD(i)A, L A LDrr,
A PSK_MEM $08 + <<8 >>8 LDri, L ADDr, L A LDrr,
A PSK_MEM $08 + >>8 LDri, 0 ADCi,
( ----- 107 )
H A LDrr, PSK_MEM $02 + LDA(i), (HL) A LDrr,
L1 FMARK A XORr, PSK_MEM LD(i)A, HL POP, AF POP, EI, RETI,
L3 FMARK PTC8255 INAi, RLCA, PSK_MEM $02 + LDA(i),
RRA, PSK_MEM $02 + LD(i)A,
PSK_MEM LDA(i), A INCr, PSK_MEM LD(i)A,
AF POP, EI, RETI,
( ----- 108 )
CODE (pskset)
  DI, $11 SIOA_CTL OUTii, $19 SIOA_CTL OUTii, IM1, EI, ;CODE
: PSK< ( -- n )
  PSK_BUFI @ PSK_BUFO @ = IF 0 ELSE PSK_BUFI @
  1+ [ PS2_BMSK LITN ] AND DUP PSK_BUF + C@
  SWAP PSK_BUFI ! THEN ;
: PSKV< ( -- n )
  PSK_BUFI @ PSK_BUFO @ = IF
  BEGIN 1 VD_DECFR ! (vidfr)
  PSK_BUFI @ PSK_BUFO @ = NOT UNTIL THEN
  PSK_BUFI @ 1+ [ PS2_BMSK LITN ] AND DUP
  PSK_BUF + C@ SWAP PSK_BUFI ! ;
: PSK$ ( -- )
  0 PSK_BUFO ! 0 PSK_BUFI ! 0 PSK_STAT ! (pskset) ;
( ----- 109 )
: (ps2kc) 0 BEGIN DROP PSKV<
  DUP 5 = IF 0 VD_CURCL ! DROP 0 THEN
  DUP 6 = IF VD_CURCL @ 4 < IF 0 ELSE VD_CURCL @ 4 - THEN
    VD_CURCL ! DROP 0 THEN
  DUP 4 = IF VD_CURCL @ [ VID_WDTH 28 - LITN ] > IF
    [ VID_WDTH 24 - LITN ] ELSE VD_CURCL @ 4 + THEN
    VD_CURCL ! DROP 0 THEN DUP UNTIL ;
( ----- 110 )
\ playing with FDC 179x's READ ADDRESS cmd.
\  needs B380 macros and B382's L2
\ read 26 ID fields and write their 26*6 bytes to a
CODE FDADDR ( trk a -- st ) \ st=status byte w/ error-only mask
  DE PUSH, BC>HL, A $81 LDri, $f4 OUTiA, fdwait
  DI, D 26 LDri, BEGIN, $c4 fdcmd BC $06f3 LDdi,
    BEGIN, BEGIN, fdstat $b6 ANDi, BR JRZ, \ DRQ
      $b4 ANDi, IFZ, TO L3 ( error ) INI, BR JRNZ,
    fdwait D DECr, BR JRNZ,
  ( A from fdwait ) $3c ANDi, L3 FMARK EI, A>BC, DE POP, ;CODE

CODE FDSEEK ( trk -- st )
  A 21 LDri, C CPr, A $81 LDri, IFC, $20 ORi, ( WP ) THEN,
  $f4 OUTiA, A B LDrr, ( trk ) $f3 OUTiA, $18 fdcmd
  fdwait $98 ANDi, C A LDrr, B 0 LDri, ;CODE
( ----- 111 )
: INIR, $edb2 M, ;
CODE FDTRK@ ( a -- st ) \ st=status byte w/ error-only mask
  BC>HL, A $81 LDri, $f4 OUTiA, fdwait
\   DI, $e4 fdcmd C $f3 LDri,
\   BEGIN, fdstat 2 ANDi, BR JRZ, \ DRQ
\   INIR, INIR, INIR, INIR, INIR,  fdstat EI, A>BC, ;CODE
\   LSET L1 INI,
\   LSET L2 fdstat RRA, RRA, L1 BR JRC, ( DRQ! )
\     RLA, L2 BR JRC,
\   RLA, $3c ANDi, EI, A>BC, ;CODE
( ----- 112 )
: INIR, $edb2 M, ;
CODE FDTRK@ ( a -- st ) \ st=status byte w/ error-only mask
  BC>HL, A $81 LDri, $f4 OUTiA, fdwait
  DI, $e4 fdcmd C $f3 LDri,
  BEGIN, fdstat 2 ANDi, BR JRZ, \ DRQ
  INIR, INIR, INIR, INIR, INIR, INIR, INIR, INIR, INIR,

    \ fdstat RRA, BR JRC,
  fdstat EI, A>BC, ;CODE
\   INIR, INIR, INIR, INIR, INIR,  fdstat EI, A>BC, ;CODE
\   LSET L1 INI,
\   LSET L2 fdstat RRA, RRA, L1 BR JRC, ( DRQ! )
\     RLA, L2 BR JRC,
\   RLA, $3c ANDi, EI, A>BC, ;CODE
( ----- 113 )
\ xcomp for my TRS80 4P.
\ Requires ARCHM, Z80A and D2 and D3 loaded in drives
3 CONSTS $f300 RS_ADDR $f3fa PS_ADDR 0 HERESTART
RS_ADDR $90 - CONSTANT SYSVARS
SYSVARS $80 + CONSTANT DRVMEM
DRVMEM CONSTANT KBD_MEM
DRVMEM 3 + CONSTANT GRID_MEM
DRVMEM 6 + CONSTANT FDMEM
DRVMEM 13 + CONSTANT UNDERCUR
DRVMEM 14 + CONSTANT RXTX_MEM
: comp1 XCOMPL Z80H TRS804PM 414 LOAD
  ." type comp2" ;
( ----- 114 )
\ xcomp for my TRS80 4P, part 2
: comp2 XCOMPH Z80C COREL Z80H
  ." Load D3 and D1 and type comp3" ;
: comp3 ASMH TRS804PL 415 LOAD
  ." Load D3 and D2 and type comp4" ;
: comp4 BLKSUB GRIDSUB TRS804PH 416 LOAD
  ." Finish the whole thing with XWRAP" ;
( ----- 115 )
\ xcomp for my TRS-80 4P, part 3
ALIAS FD@ (blk@)
ALIAS FD! (blk!)
( ----- 116 )
\ xcomp for my TRS80 4P, part 4
: INIT GRID$ KBD$ BLK$ FD$ ;
