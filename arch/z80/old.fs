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
44 VALUE tickfactor
( ----- 002 )
\ Z80 port, core routines
FJR JR, TO L1 $10 OALLOT LSET lblxt ( RST 10 )
  IX INC, IX INC, 0 IX+ E LD, 1 IX+ D LD,
  HL POP, LDDE(HL), HL INC, DE HL EX, (HL) JP, \ 17 bytes
$28 OALLOT LSET lblcell ( RST 28 )
  HL POP, BC PUSH, HL>BC, FJR JR, TO L2 ( next ) $30 OALLOT
0 JP, ( RST 30 ) $38 OALLOT
0 JP, ( RST 38 ) $66 OALLOT RETN,
L1 FMARK
  DI, SP PS_ADDR LD, IX RS_ADDR LD, 0 JP, PC 2 - TO lblboot
LSET lblval HL POP, BC PUSH, LDBC(HL), \ to lblnext
LSET lblnext L2 FMARK
DE HL EX, LSET L1 ( EXIT ) LDDE(HL), HL INC, DE HL EX, (HL) JP,
LSET lbldoes HL POP, BC PUSH, HL>BC, BC INC, BC INC, LDHL(HL),
  (HL) JP,
( ----- 003 )
\ Z80 port, EXIT QUIT ABORT BYE RCNT SCNT
CODE EXIT ( put new IP in HL instead of DE for speed )
  L 0 IX+ LD, H 1 IX+ LD, IX DEC, IX DEC, L1 JP,
CODE QUIT LSET L1 ( used in ABORT )
  IX RS_ADDR LD, 0 JP, PC 2 - TO lblmain
CODE ABORT SP PS_ADDR LD, L1 BR JR,
CODE BYE HALT,
CODE RCNT BC PUSH, IX PUSH, HL POP, BC RS_ADDR LD,
  BC SUBHLd, HL>BC, ;CODE
CODE SCNT HL 0 LD, HL SP ADD, BC PUSH, HL>BC, HL PS_ADDR LD,
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
    BC DEC, ( 6t )
    IFZ, ( 12t ) BC POP, ;CODE THEN,
    A tickfactor LD, ( 7t )
    BEGIN, A DEC, ( 4t ) BR CNZ JR, ( 12t )
  BR JR, ( 12t ) ( outer: 37t inner: 16t )
( ----- 005 )
\ Z80 port, PC! PC@ []= [C]? (im1)
CODE PC! HL POP, (C) L OUT, BC POP, ;CODE
CODE PC@ C (C) IN, B 0 LD, ;CODE
CODE []= BC PUSH, EXX, ( protect DE ) BC POP, DE POP, HL POP,
  LSET L1 ( loop )
    A (DE) LD, DE INC, CPI,
    IFNZ, EXX, BC 0 LD, ;CODE THEN,
    CPE L1 JP, ( BC not zero? loop )
  EXX, BC 1 LD, ;CODE
CODE [C]? BCZ, IFZ, BC DEC, HL POP, HL POP, ;CODE THEN,
  BC PUSH, EXX, BC POP, HL POP, DE POP, A E LD, D H LD,
  E L LD, \ HL=a DE=a BC=u A=c
  CPIR, IFZ, DE SUBHLd, HL DEC, ELSE, HL -1 LD, THEN,
  HL PUSH, EXX, BC POP, ;CODE
CODE (im1) IM1, EI, ;CODE
( ----- 006 )
\ Z80 port, /MOD *
CODE * HL POP, DE PUSH, DE HL EX, ( DE * BC -> HL )
  HL 0 LD, A $10 LD, BEGIN,
    HL HL ADD, E RL, D RL,
    IFC, HL BC ADD, THEN,
    A DEC, BR CNZ JR,
  HL>BC, DE POP, ;CODE
\ Divides AC by DE. quotient in AC remainder in HL
CODE /MOD BC>HL, BC POP, DE PUSH, DE HL EX,
  A B LD, B 16 LD, HL 0 LD, BEGIN,
    SCF, C RL, RLA, HL HL ADC, HL DE SBC,
    IFC, HL DE ADD, C DEC, THEN,
  BR DJNZ,
  DE POP, HL PUSH, B A LD, ;CODE
( ----- 007 )
\ Z80 port, FIND
CODE FIND ( sa sl -- w? f ) HL POP,
  HL BC ADD, \ HL points to after last char of s
  'N (n) HL LD, HL SYSVARS $02 ( CURRENT ) + (n) LD, BEGIN,
    HL DEC, A (HL) LD, A $7f AND, ( imm ) A C CP, IFZ,
      HL PUSH, DE PUSH, BC PUSH, DE 'N (n) LD,
      HL DEC, HL DEC, HL DEC, \ Skip prev field
      LSET L1 ( loop )
        DE DEC, A (DE) LD, CPD, IFZ, TO L2 ( break! )
      CPE L1 JP, ( BC not zero? loop ) L2 FMARK
      BC POP, DE POP, HL POP, THEN,
    IFZ, ( match ) HL INC, HL PUSH, BC 1 LD, ;CODE THEN,
    \ no match, go to prev and continue
    HL DEC, A (HL) LD, HL DEC, L (HL) LD, H A LD,
    A L OR, IFZ, ( end of dict ) BC 0 LD, ;CODE THEN,
  BR JR,
( ----- 008 )
\ Z80 port, (b) (n) (br) (?br) (next)
CODE (b) ( -- c ) BC PUSH, A (DE) LD, A>BC, DE INC, ;CODE
CODE (n) ( -- n ) BC PUSH,
  DE HL EX, LDBC(HL), HL INC, DE HL EX, ;CODE
CODE (br) LSET L1 ( used in ?br and next )
  A (DE) LD, ( sign extend A into HL )
  L A LD, A A ADD, ( sign in carry ) A A SBC, ( FF if neg )
  H A LD, HL DE ADD, ( HL --> new IP ) DE HL EX, ;CODE
CODE (?br) BCZ, BC POP, L1 BR CZ JR, DE INC, ;CODE
CODE (next)
  0 IX+ DEC, IFNZ,
    A $ff LD, A 0 IX+ CP, IFZ, 1 IX+ DEC, THEN,
    L1 BR JR, THEN,
  A A XOR, A 1 IX+ CP, L1 BR CNZ JR,
  IX DEC, IX DEC, DE INC, ;CODE
( ----- 009 )
\ Z80 port, >R I C@ @ C! ! 1+ 1- + -
CODE >R IX INC, IX INC, 0 IX+ C LD, 1 IX+ B LD, BC POP, ;CODE
CODE R@ BC PUSH, C 0 IX+ LD, B 1 IX+ LD, ;CODE
CODE R~ IX DEC, IX DEC, ;CODE
CODE R> BC PUSH, C 0 IX+ LD, B 1 IX+ LD,
  IX DEC, IX DEC, ;CODE
CODE C@ A (BC) LD, A>BC, ;CODE
CODE @ BC>HL, LDBC(HL), ;CODE
CODE C! BC>HL, BC POP, (HL) C LD, BC POP, ;CODE
CODE ! BC>HL, BC POP,
  (HL) C LD, HL INC, (HL) B LD, BC POP, ;CODE
CODE 1+ BC INC, ;CODE
CODE 1- BC DEC, ;CODE
CODE + HL POP, HL BC ADD, HL>BC, ;CODE
CODE - HL POP, BC SUBHLd, HL>BC, ;CODE
( ----- 010 )
\ Z80 port, AND OR XOR >> << >>8 <<8
CODE AND HL POP,
  A C LD, A L AND, C A LD, A B LD, A H AND, B A LD, ;CODE
CODE OR HL POP,
  A C LD, A L OR, C A LD, A B LD, A H OR, B A LD, ;CODE
CODE XOR HL POP,
  A C LD, A L XOR, C A LD, A B LD, A H XOR, B A LD, ;CODE
CODE NOT BCZ, BC 0 LD, IFZ, C INC, THEN, ;CODE
CODE >> B SRL, C RR, ;CODE
CODE << C SLA, B RL, ;CODE
CODE >>8 C B LD, B 0 LD, ;CODE
CODE <<8 B C LD, C 0 LD, ;CODE
( ----- 011 )
\ Z80 port, ROT ROT> DUP DROP SWAP OVER EXECUTE
CODE ROT ( a b c -- b c a ) ( BC=c )
  HL POP, ( b ) (SP) HL EX, ( a<>b ) BC PUSH, ( c ) HL>BC, ;CODE
CODE ROT> ( a b c -- c a b ) ( BC=c )
  BC>HL, BC POP, ( b ) (SP) HL EX, ( a<>c ) HL PUSH, ;CODE
CODE DUP ( a -- a a ) LSET L1 BC PUSH, ;CODE
CODE ?DUP BCZ, L1 BR CNZ JR, ;CODE
CODE DROP ( a -- ) BC POP, ;CODE
CODE SWAP ( a b -- b a ) HL POP, BC PUSH, HL>BC, ;CODE
CODE OVER ( a b -- a b a )
  HL POP, HL PUSH, BC PUSH, HL>BC, ;CODE
CODE EXECUTE BC>HL, BC POP, (HL) JP,
( ----- 012 )
\ Z80 port, JMPi! CALLi!
CODE JMPi! ( pc a -- len ) BC>HL, BC POP,
  A $c3 LD, LSET L1 (HL) A LD, HL INC,
  (HL) C LD, HL INC, (HL) B LD, BC 3 LD, ;CODE
CODE CALLi! ( pc a -- len ) BC>HL, BC POP,
  A B LD, A A OR, IFZ, A C LD, A $c7 AND, IFZ, \ RST
    A C LD, A $c7 OR, (HL) A LD, BC 1 LD, ;CODE THEN, THEN,
  ( not RST ) A $cd LD, L1 BR JR,
( ----- 013 )
\ Z80 port speedups
CODE TUCK ( a b -- b a b ) HL POP, BC PUSH, HL PUSH, ;CODE
CODE NIP ( a b -- b ) HL POP, ;CODE
CODE +! ( n a -- ) BC>HL, LDBC(HL), HL DEC, (SP) HL EX,
  HL BC ADD, HL>BC, HL POP, (HL) C LD, HL INC, (HL) B LD,
  BC POP, ;CODE
CODE A> BC PUSH, IY PUSH, BC POP, ;CODE
CODE >A BC PUSH, IY POP, BC POP, ;CODE
CODE A>R IY PUSH, HL POP,
  IX INC, IX INC, 0 IX+ L LD, 1 IX+ H LD, ;CODE
CODE R>A L 0 IX+ LD, H 1 IX+ LD, IX DEC, IX DEC,
  HL PUSH, IY POP, ;CODE
CODE A+ IY INC, ;CODE
CODE A- IY DEC, ;CODE
CODE AC@ BC PUSH, C 0 IY+ LD, B 0 LD, ;CODE
CODE AC! 0 IY+ C LD, BC POP, ;CODE
( ----- 014 )
\ Z80 port speedups
CODE MOVE ( src dst u -- ) HL POP, DE HL EX, (SP) HL EX,
  BCZ, IFNZ, LDIR, THEN, DE POP, BC POP, ;CODE
CODE = HL POP, BC SUBHLd, BC 0 LD, IFZ, BC INC, THEN, ;CODE
CODE < HL POP, BC SUBHLd, BC 0 LD, IFC, BC INC, THEN, ;CODE
CODE CRC16 ( c n -- c ) BC PUSH, EXX, ( protect DE )
  HL POP, ( n ) DE POP, ( c ) A L LD, A D XOR, D A LD,
  B 8 LD, BEGIN,
    E SLA, D RL, IFC, ( msb is set, apply polynomial )
      A D LD, A $10 XOR, D A LD,
      A E LD, A $21 XOR, E A LD, THEN,
  BR DJNZ,
  DE PUSH, EXX, ( unprotect DE ) BC POP, ;CODE
( ----- 020 )
\ Z80 Assembler. See doc/asm
\ tgt/arg/cond/ixy+: -1 = unset. flags: b0=wide b1=n
7 VALUES opcode tgt arg cond ival flags ixy+
: arg$ -1 [TO] tgt -1 [TO] arg -1 [TO] cond 0 [TO] flags
  -1 [TO] ixy+ 0 [TO] opcode ; arg$
: argerr arg$ ABORT" argument error" ;
: assert ( f -- ) NOT IF argerr THEN ;
: tgt? tgt 0>= ; : arg? arg 0>= ; : cond? cond 0>= ;
: >reg tgt? IF arg? NOT assert [TO] arg ELSE [TO] tgt THEN ;
: r DOER C, DOES> C@ >reg ;
7 r A 0 r B 1 r C 2 r D 3 r E 4 r H 5 r L 6 r (HL)
8 r (C) 9 r (n) 10 r (BC) 11 r (DE) 12 r I 13 r R
: c DOER C, DOES> C@ [TO] cond ;
0 c CNZ 1 c CZ 2 c CNC 3 c CC 4 c CPO 5 c CPE 6 c CP 7 c CM
( ----- 021 )
: flag! ( mask -- ) flags OR [TO] flags ;
: wide! 1 flag! ; : n! ( n -- ) [TO] ival 2 flag! ;
: d DOER C, DOES> C@ >reg wide! ;
0 d BC 1 d DE 2 d HL 3 d AF 3 d SP 4 d AF' 5 d (SP)
: wide? flags 1 AND ;
: no ( var -- ) 0< assert ; : yes 0>= assert ;
: nowide wide? NOT assert ; : yeswide wide? assert ;
: assert= = assert ;
: HL? 2 = ; : A? 7 = ; : assertHL ( n ) HL? assert yeswide ;
: op! ( n -- ) opcode OR [TO] opcode ;
: <<3 << << << ; : <<4 <<3 << ;
: minmax ( n n -- low high ) 2DUP > IF SWAP THEN ;
: tgt>r tgt yes tgt <<3 op! ; : tgt>d tgt yes tgt <<4 op! ;
: IX $dd C, HL ; : IY $fd C, HL ;
: IX+ [TO] ixy+ $dd C, (HL) ; : IY+ [TO] ixy+ $fd C, (HL) ;
( ----- 022 )
: op, opcode DUP >>8 ?DUP IF C, THEN C,
  ixy+ 0>= IF ixy+ C, THEN
  flags 2 AND ( n ) IF ival wide? IF L, ELSE C, THEN THEN arg$ ;

: _ ( ?i opcode ) \ 8b A inherent tgt. i or r arg
  tgt A? assert op! arg? IF arg op! ELSE $46 op! n! THEN ;
: OP DOER C, DOES> nowide C@ _ op, ;
$a0 OP AND,               $b8 OP CP,
$b0 OP OR,                $90 OP SUB,
$a8 OP XOR,
: OP DOER ( 8b ) C, ( 16b ) , DOES> \ r i or d-with-HL-tgt
  wide? IF arg yes tgt assertHL 1+ @ op! arg <<4 op!
  ELSE C@ _ THEN op, ;
$09 $80 OP ADD,   $ed4a $88 OP ADC,   $ed42 $98 OP SBC,
( ----- 023 )
: OP DOER C, DOES> \ d tgt only
  tgt>d arg no yeswide C@ op! op, ;
$c5 OP PUSH,                $c1 OP POP,
: OP DOER , DOES> \ r tgt no arg
  tgt yes arg no nowide @ op! tgt op! op, ;
$cb10 OP RL,   $cb18 OP RR,   $cb00 OP RLC,  $cb08 OP RRC,
$cb20 OP SLA,  $cb38 OP SRL,
: OP DOER ( r ) C, ( d ) C, DOES> \ r or d tgt no arg
  arg no wide? IF 1+ tgt>d ELSE tgt>r THEN C@ op! op, ;
$03 $04 OP INC,              $0b $05 OP DEC,
( ----- 024 )
: OP DOER , DOES> tgt no arg no @ op! op, ; \ inherent
$f3 OP DI,     $fb OP EI,     $d9 OP EXX,    $76 OP HALT,
$00 OP NOP,    $17 OP RLA,    $07 OP RLCA,
$1f OP RRA,    $0f OP RRCA,   $37 OP SCF,    $3f OP CCF,
$eda1 OP CPI,  $edb1 OP CPIR, $eda9 OP CPD,  $edb9 OP CPDR,
$ed46 OP IM0,  $ed56 OP IM1,  $ed5e OP IM2,  $eda0 OP LDI,
$edb0 OP LDIR, $eda8 OP LDD,  $edb8 OP LDDR, $ed44 OP NEG,
$ed4d OP RETI, $ed45 OP RETN, $eda2 OP INI,  $edaa OP IND,
$eda3 OP OUTI,
: _ ( off op opcond -- ) tgt no arg no
  cond? IF op! DROP cond <<3 op! ELSE DROP op! THEN op, ;
: JR, n! $18 $20 _ ; : RET, $c9 $c0 _ ; : DJNZ, n! $10 $10 _ ;
: CALL, n! wide! $cd $c4 _ ;
: JP, tgt? IF $e9 op! op, ELSE n! wide! $c3 $c2 _ THEN ;
( ----- 025 )
: OP DOER C, DOES> ( i ) \ r tgt i arg
  nowide tgt yes arg no C@ op! <<3 op! $cb00 op! tgt op! op, ;
$c0 OP SET,      $80 OP RES,     $40 OP BIT,

: _(C) ( tgt mask -- ) $ed00 op! $40 OR op! <<3 op! op, ;
: _A ( n op -- ) arg no op! n! tgt A? assert op, ;
: IN, nowide arg 8 = IF tgt 0 _(C) ELSE $db _A THEN ;
: OUT, nowide tgt 8 = IF arg 1 _(C) ELSE $d3 _A THEN ;

CREATE _ $34 C, ( AF AF' ) $12 C, ( DE HL ) $52 C, ( (SP) HL )
         $08 C,            $eb C,           $e3 C,
: EX, tgt yes arg yes yeswide tgt <<4 arg OR _ 3 [C]?
  DUP 0>= assert 3 + _ + C@ op! op, ;
: RST, ( n ) $c7 OR op! op, ;
( ----- 026 )
: ld8i n! $06 op! tgt <<3 op! ;
\ (nn) (BC) (DE) I R
CREATE _ $3a , $0a , $1a , $ed57 , $ed5f ,
( inv )  $32 , $02 , $12 , $ed47 , $ed4f ,
: ld8r tgt arg minmax DUP 9 < IF ( rr )
    2DROP $40 op! tgt>r arg op! EXIT THEN
  SWAP A? assert DUP 9 = IF ( (nn) ) SWAP n! wide! THEN
  tgt 9 >= IF ( (nn),A ) 5 + THEN 9 - << _ + @ op! ;
: ld16i n! $01 op! tgt>d ;
: ld16r tgt 3 = IF ( SP ) arg assertHL $f9 op! EXIT THEN n!
  tgt 9 = IF ( (n) ) arg HL? IF $22 ELSE $ed43 arg <<4 OR THEN
  ELSE arg 9 assert= tgt HL? IF $2a ELSE $ed4b tgt <<4 OR THEN
  THEN op! ;
CREATE _ ' ld8i , ' ld8r , ' ld16i , ' ld16r ,
: LD, tgt yes wide? << arg? + << _ + @ EXECUTE op, ;
( ----- 027 )
\ Macros
: CLRA, A A XOR, ;
: SUBHLd, tgt arg$ A A OR, HL [TO] arg SBC, ;
: PUSHA, B 0 LD, C A LD, BC PUSH, ;
: HLZ, A H LD, A L OR, ;
: DEZ, A D LD, A E OR, ;
: BCZ, A B LD, A C OR, ;
: LDDE(HL), E (HL) LD, HL INC, D (HL) LD, ;
: LDBC(HL), C (HL) LD, HL INC, B (HL) LD, ;
: LDHL(HL), A (HL) LD, HL INC, H (HL) LD, L A LD, ;
: OUTHL, A H LD, DUP A OUT, A L LD, A OUT, ;
: OUTDE, A D LD, DUP A OUT, A E LD, A OUT, ;
: HL>BC, B H LD, C L LD, ;
: BC>HL, H B LD, L C LD, ;
: A>BC, C A LD, B 0 LD, ;
: A>HL, L A LD, H 0 LD, ;
( ----- 028 )
\ Z80 HAL
ALIAS JP, JMPi, ALIAS JR, JRi,
: JMP(i), HL (n) LD, (HL) JP, ;
: CALLi, DUP $38 AND OVER = IF RST, ELSE CALL, THEN ;
: JRZi, CZ JR, ; : JRNZi, CNZ JR, ;
: JRCi, CC JR, ; : JRNCi, CNC JR, ;
: i>, BC PUSH, BC ( i ) LD, ;
: (i)>, BC PUSH, BC (n) LD, ;
( ----- 030 )
CODE AT28C! ( c a -- )
  BC>HL, BC POP,
  (HL) C LD, A C LD, ( orig ) B C LD, ( save )
  C (HL) LD, ( poll ) BEGIN,
    A (HL) LD, ( poll ) A C CP, ( same as old? )
    C A LD, ( save old poll, Z preserved )
  BR CNZ JR,
\ equal to written? SUB instead of CP to ensure IOERR is NZ
  A B SUB, IFNZ, SYSVARS ( IOERR ) (n) A LD, THEN, BC POP, ;CODE
: AT28! ( n a -- ) 2DUP AT28C! 1+ SWAP >>8 SWAP AT28C! ;
( ----- 032 )
( SPI relay driver. See doc/hw/z80/spi.txt )
CODE (spix) ( n -- n )
  A C LD,
  SPI_DATA A OUT,
  \ wait until xchg is done
  BEGIN, A SPI_CTL IN, A 1 AND, BR CNZ JR,
  A SPI_DATA IN,
  C A LD, ;CODE
CODE (spie) ( n -- ) A C LD, SPI_CTL A OUT, BC POP, ;CODE
( ----- 035 )
( Z80 driver for TMS9918. Implements grid protocol. Requires
TMS_CTLPORT, TMS_DATAPORT and ~FNT from the Font compiler at
B520. Patterns are at addr $0000, Names are at $3800.
Load range B315-317 )
CODE _ctl ( a -- sends LSB then MSB )
  A C LD, TMS_CTLPORT A OUT, A B LD, TMS_CTLPORT A OUT,
  BC POP, ;CODE
CODE _data
  A C LD, TMS_DATAPORT A OUT, BC POP, ;CODE
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
    A 6850_CTL IN, A $02 AND, ( are we transmitting? )
  BR CZ JR, ( yes, loop )
  A C LD, 6850_IO A OUT, BC POP, ;CODE
( ----- 041 )
CODE 6850<? BC PUSH,
  CLRA, ( 256x ) A $16 ( RTS lo ) LD, 6850_CTL A OUT,
  BC 0 LD, ( pre-push a failure )
  BEGIN, AF AF' EX, ( preserve cnt )
    A 6850_CTL IN, A $1 AND, ( rcv buff full? )
    IFNZ, ( full )
      A 6850_IO IN, PUSHA, C 1 LD, CLRA, ( end loop )
    ELSE, AF AF' EX, ( recall cnt ) A DEC, THEN,
  BR CNZ JR,
  A $56 ( RTS hi ) LD, 6850_CTL A OUT, ;CODE
( ----- 042 )
ALIAS 6850<? RX<? ALIAS 6850<? (key?)
ALIAS 6850> TX> ALIAS 6850> (emit)
: 6850$ $56 ( RTS high ) [ 6850_CTL LITN ] PC! ;
( ----- 045 )
( Zilog SIO driver. Load range B325-328. Requires:
  SIOA_CTL for ch A control register SIOA_DATA for data
  SIOB_CTL for ch B control register SIOB_DATA for data )
CODE SIOA<? BC PUSH,
  CLRA, ( 256x ) BC 0 LD, ( pre-push a failure )
  A 5 ( PTR5 ) LD, SIOA_CTL A OUT,
  A $68 ( RTS low ) LD, SIOA_CTL A OUT,
  BEGIN, AF AF' EX, ( preserve cnt )
    A SIOA_CTL IN, A $1 AND, ( rcv buff full? )
    IFNZ, ( full )
      A SIOA_DATA IN, PUSHA, C 1 LD, CLRA, ( end loop )
    ELSE, AF AF' EX, ( recall cnt ) A DEC, THEN,
  BR CNZ JR,
  A 5 ( PTR5 ) LD, SIOA_CTL A OUT,
  A $6a ( RTS high ) LD, SIOA_CTL A OUT, ;CODE
( ----- 046 )
CODE SIOA>
  BEGIN,
    A SIOA_CTL IN, A $04 AND, ( are we transmitting? )
  BR CZ JR, ( yes, loop )
  A C LD, SIOA_DATA A OUT, BC POP, ;CODE
CREATE _ ( init data ) $18 C, ( CMD3 )
    $24 C, ( CMD2/PTR4 ) $c4 C, ( WR4/64x/1stop/nopar )
    $03 C, ( PTR3 ) $c1 C, ( WR3/RXen/8char )
    $05 C, ( PTR5 ) $6a C, ( WR5/TXen/8char/RTS )
    $21 C, ( CMD2/PTR1 ) 0 C, ( WR1/Rx no INT )
: SIOA$ _ >A 9 >R BEGIN AC@+ [ SIOA_CTL LITN ] PC! NEXT ;
( ----- 047 )
CODE SIOB<? BC PUSH, ( copy/paste of SIOA<? )
  CLRA, ( 256x ) BC 0 LD, ( pre-push a failure )
  A 5 ( PTR5 ) LD, SIOB_CTL A OUT,
  A $68 ( RTS low ) LD, SIOB_CTL A OUT,
  BEGIN, AF AF' EX, ( preserve cnt )
    A SIOB_CTL IN, A $1 AND, ( rcv buff full? )
    IFNZ, ( full )
      A SIOB_DATA IN, PUSHA, C 1 LD, CLRA, ( end loop )
    ELSE, AF AF' EX, ( recall cnt ) A DEC, THEN,
  BR CNZ JR,
  A 5 ( PTR5 ) LD, SIOB_CTL A OUT,
  A $6a ( RTS high ) LD, SIOB_CTL A OUT, ;CODE
( ----- 048 )
CODE SIOB>
  BEGIN,
    A SIOB_CTL IN, A $04 AND, ( are we transmitting? )
  BR CZ JR, ( yes, loop )
  A C LD, SIOB_DATA A OUT, BC POP, ;CODE
: SIOB$ _ >A 9 >R BEGIN AC@+ [ SIOB_CTL LITN ] PC! NEXT ;
( ----- 050 )
\ VDP Driver. see doc/hw/sms/vdp. Load range B330-B332.
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
\ VDP driver
: _sfont ( a -- a+7, Send font to VDP )
  7 >R BEGIN C@+ _data 3 _zero NEXT ( blank row ) 4 _zero ;
: CELL! ( c pos )
  2 * $7800 OR _ctl ( c )
  $20 - ( glyph ) $5f MOD _data ;
( ----- 052 )
\ VDP driver
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
\ SMS pad driver. See doc/hw/z80/sms/pad. Load range: 355-358
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
  \ TR = DATA TH = CLK
  A CPORT_MEM (n) LD, A $f3 AND, ( TR/TH output )
  B 8 LD, BEGIN,
    A $bf AND, ( TR lo ) C RL,
    IFC, A $40 OR, ( TR hi ) THEN,
    CPORT_CTL A OUT, ( clic! ) A $80 OR, ( TH hi )
    CPORT_CTL A OUT, ( clac! )
    AF AF' EX, A CPORT_D1 IN, ( Up Btn is B6 ) RLA, RLA,
      L RL, AF AF' EX,
    A $7f AND, ( TH lo ) CPORT_CTL A OUT, ( cloc! )
  BR DJNZ, CPORT_MEM (n) A LD, C L LD, ;CODE
( ----- 068 )
\ Routines for interacting with SMS controller ports.
\ Requires CPORT_MEM, CPORT_CTL, CPORT_D1 and CPORT_D2 to be
\ defined. CPORT_MEM is a 1 byte buffer for CPORT_CTL. The last
\ 3 consts will usually be $3f, $dc, $dd.
\ mode -- set TR pin on mode a on:
\ 0= output low 1=output high 2=input
CODE _TRA! ( B0 -> B4, B1 -> B0 )
  C RR, RLA, RLA, RLA, RLA, B RR, RLA,
  A $11 AND, C A LD, A CPORT_MEM (n) LD,
  A $ee AND, A C OR, CPORT_CTL A OUT, CPORT_MEM (n) A LD,
  BC POP, ;CODE
CODE _THA! ( B0 -> B5, B1 -> B1 )
  C RR, RLA, RLA, RLA, RLA, C RR, RLA, RLA,
  A $22 AND, C A LD, A CPORT_MEM (n) LD,
  A $dd AND, A C OR, CPORT_CTL A OUT, CPORT_MEM (n) A LD,
  BC POP, ;CODE
( ----- 069 )
CODE _TRB! ( B0 -> B6, B1 -> B2 )
  C RR, RLA, RLA, RLA, RLA, C RR, RLA, RLA, RLA,
  A $44 AND, C A LD, A CPORT_MEM (n) LD,
  A $bb AND, A C OR, CPORT_CTL A OUT, CPORT_MEM (n) A LD,
  BC POP, ;CODE
CODE _THB! ( B0 -> B7, B1 -> B3 )
  C RR, RLA, RLA, RLA, RLA, C RR, RLA, RLA, RLA, RLA,
  A $88 AND, C A LD, A CPORT_MEM (n) LD,
  A $77 AND, A C OR, CPORT_CTL A OUT, CPORT_MEM (n) A LD,
  BC POP, ;CODE
CODE _D1@ BC PUSH, A CPORT_D1 IN, C A LD, B 0 LD, ;CODE
CODE _D2@ BC PUSH, A CPORT_D2 IN, C A LD, B 0 LD, ;CODE
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
    A $10 ( CMD ) IN,
    RLA, ( When 7th bit is clr, we can send a new cmd )
  BR CC JR, ;CODE
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
    A $ff LD,
    KBD_PORT A OUT,
    A C LD,
    KBD_PORT A OUT,
    A KBD_PORT IN,
  EI,
  C A LD,
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
\ FDMEM 3b: FDSEL 1b FDOP 2b
: TRS804P 381 389 LOADR ;
$f800 VALUE VIDMEM $bf VALUE CURCHAR
0 VALUE lblflush
: fdstat A $f0 IN, ;
: fdcmd ( i ) A LD, B $18 LD, A $f0 OUT, BEGIN, BR DJNZ, ;
: fdwait BEGIN, fdstat RRCA, BR CC JR, RLCA, ;
: vid+, ( tgt:reg -- )
  tgt arg$ HL VIDMEM LD, HL [TO] arg ADD, ;
( ----- 081 )
\ TRS-80 4P video driver
24 CONSTANT LINES 80 CONSTANT COLS
CODE CELL! ( c pos -- ) HL POP,
  A L LD, BC vid+, (HL) A LD, BC POP, ;CODE
CODE CELLS! ( a pos u -- ) BC PUSH, EXX, BC POP, DE POP,
  DE vid+, DE HL EX, HL POP, BCZ, IFNZ, LDIR, THEN, EXX, BC POP,
;CODE
CODE CURSOR! ( new old -- ) BC vid+, A (HL) LD, A CURCHAR CP,
  IFZ, A UNDERCUR (n) LD, (HL) A LD, THEN,
  BC POP, BC vid+, A (HL) LD, UNDERCUR (n) A LD, A CURCHAR LD,
  (HL) A LD, BC POP, ;CODE
CODE SCROLL ( -- )
  EXX, HL VIDMEM 80 + LD, DE VIDMEM LD, BC 1840 LD, LDIR,
  H D LD, L E LD, DE INC, A SPC LD, (HL) A LD,
  BC 79 LD, LDIR, EXX, ;CODE
: NEWLN ( old -- new ) 1+ DUP LINES = IF 1- SCROLL THEN ;
( ----- 082 )
LSET L2 ( seek, B=trk )
  A 21 LD, A B CP, A FDMEM (n) LD, IFC, A $20 OR, ( WP ) THEN,
  A $80 OR, $f4 A OUT, \ FD sel
  A B LD, ( trk ) $f3 A OUT, $1c fdcmd RET,
CODE FDRD ( trksec addr -- st ) BC>HL, BC POP,
  L2 CALL, fdwait A $98 AND, IFZ, DI,
    A C LD, $f2 A OUT, ( sec ) C $f3 LD, $84 fdcmd ( read )
    BEGIN, BEGIN, fdstat A $b6 AND, BR CZ JR, \ DRQ
      A $b4 AND, IFZ, TO L3 ( error ) INI, BR CNZ JR, THEN,
  fdwait A $3c AND, L3 FMARK A>BC, EI, ;CODE
CODE FDWR ( trksec addr -- st ) BC>HL, BC POP,
  L2 CALL, fdwait A $98 AND, IFZ, DI,
    A C LD, $f2 A OUT, ( sec ) C $f3 LD, $a4 fdcmd ( read )
    BEGIN, BEGIN, fdstat A $f6 AND, BR CZ JR, \ DRQ
      A $f4 AND, IFZ, TO L3 ( error ) OUTI, BR CNZ JR, THEN,
  fdwait A $3c AND, L3 FMARK A>BC, EI, ;CODE
( ----- 083 )
CODE _dsel ( fdmask -- )
  A C LD, FDMEM (n) A LD, A $80 OR, $f4 A OUT,
  0 fdcmd ( restore ) fdwait BC POP, ;CODE
: DRVSEL ( drv -- ) 1 SWAP LSHIFT [ FDMEM LITN ] C@ OVER = NOT
  IF _dsel ELSE DROP THEN ;
: FD$ 1 DRVSEL ;
FDMEM 1+ DUP CONSTANT 'FDOP *ALIAS FDOP
: _err S" FDerr " STYPE .X ABORT ;
: _trksec ( sec -- trksec )
\ 4 256b sectors per block, 18 sec per trk, 40 trk max
  18 /MOD ( sec trk ) DUP 39 > IF $ffff _err THEN <<8 + ;
( ----- 084 )
: FD@! ( blk blk( -- )
  A> >R SWAP << << ( blk*4=sec ) >A 4 >R BEGIN ( dest )
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
    A $ea IN, A $40 AND, IFNZ, ( TX reg empty )
      A $e8 IN, A $80 AND, IFZ, ( CTS low )
        A C LD, $eb A OUT, ( send byte ) BC POP, ;CODE
  THEN, THEN, BR JR,
( ----- 086 )
CODE RX<? BC PUSH,
  CLRA, ( 256x ) BC 0 LD, ( pre-push a failure )
  A $6c ( RTS low ) LD, $ea A OUT,
  BEGIN, AF AF' EX, ( preserve cnt )
    A $ea IN, A $80 AND, ( rcv buff full? )
    IFNZ, ( full )
      A $eb IN, A>HL, HL PUSH, C INC, CLRA, ( end loop )
    ELSE, AF AF' EX, ( recall cnt ) A DEC, THEN,
  BR CNZ JR,
  A $6d ( RTS high ) LD, $ea A OUT, ;CODE
( ----- 087 )
LSET L1 6 nC, '`' 'h' 'p' 'x' '0' '8'
LSET L2 8 nC, $0d 0 $ff 0 0 $08 0 $20
PC XORG $39 + T! ( RST 38 )
AF PUSH, HL PUSH, DE PUSH, BC PUSH,
A $ec IN, ( RTC INT ack )
A $f440 (n) LD, A A OR, IFNZ, \ 7th row is special
  HL L2 1- LD, BEGIN, HL INC, RRA, BR CNC JR,
  A (HL) LD, ELSE, \ not 7th row
  HL L1 LD, DE $f401 LD, BC $600 LD, BEGIN,
    A (DE) LD, A A OR, IFNZ,
      C (HL) LD, BEGIN, C INC, RRA, BR CNC JR,
      C DEC, THEN,
    E SLA, HL INC, BR DJNZ,
  A C LD, THEN, \ cont.
( ----- 088 )
\ A=char or zero if no keypress. Now let's debounce
HL KBD_MEM 2 + LD, A A OR, IFZ, \ no keypress, debounce
  (HL) A LD, ELSE, \ keypress, is it debounced?
  A (HL) CP, IFNZ, \ != debounce buffer
    C A LD, (HL) C LD, A $ff CP, IFZ, \ BREAK!
      HL POP, HL POP, HL POP, HL POP, HL POP, EI,
      X' QUIT JP, THEN,
    HL DEC, A $f480 (n) LD, A 3 AND, (HL) A LD, HL DEC,
    (HL) C LD, THEN, THEN,
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
( ----- 091 )
\ TRS-80 4P bootloader. Loads sectors 2-17 to addr 0.
DI, A $86 LD, $84 A OUT, \ mode 2, 80 chars, page 1
A $81 LD, $f4 A OUT, \ DRVSEL DD, drv0
A $40 LD, $ec A OUT, \ MODOUT 4MHZ, no EXTIO
HL 0 LD, ( dest addr ) CLRA, $e4 A OUT, ( no NMI )
A INC, ( trk1 ) BEGIN,
  $f3 A OUT, AF AF' EX, ( save ) $18 ( seek ) fdcmd fdwait
  CLRA, $f2 A OUT, C $f3 LD, BEGIN,
    $80 ( read sector ) fdcmd ( B=0 )
    BEGIN, fdstat RRA, RRA, BR CNC JR, ( DRQ )
    INI, A $c1 LD, BEGIN, $f4 A OUT, INI, BR CNZ JR,
    fdwait A $1c ( error mask ) AND, IFNZ,
      A SPC ADD, VIDMEM (n) A LD, BEGIN, BR JR, THEN,
    A $f2 IN, A INC, $f2 A OUT, A 18 CP, BR CC JR,
  AF AF' EX, ( restore ) A INC, A 3 CP, BR CC JR, 0 RST,
( ----- 095 )
\ Dan SBC drivers. See doc/hw/z80/dan.txt
\ Macros
: OUTii, ( val port -- ) A SWAP LD, A OUT, ;
: repeat ( n -- ) >R ' BEGIN ( w ) DUP EXECUTE NEXT DROP ;
( ----- 096 )
\ SPI relay driver
CODE (spix) ( n -- n )
  A C LD,
  SPI_DATA A OUT,
  ( wait until xchg is done )
  NOP, NOP, NOP, NOP,
  A SPI_DATA IN,
  C A LD, ;CODE
CODE (spie) ( n -- )
  $9A CTL8255 OUTii, $3 CTL8255 OUTii,
  A C LD, A 1 XOR, A 1 AND, CTL8255 A OUT, BC POP, ;CODE
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
  BC VID_MEM $10 + LD, HL VID_WDTH VID_SCN * LD,
  BEGIN, CLRA, (BC) A LD, BC INC, HL DEC, HLZ, BR CNZ JR,
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
 BC VID_SCN 8 - VID_WDTH * LD, DE VID_MEM $10 + LD,
 HL VID_MEM $10 + VID_WDTH 8 * + LD,
 LDIR,  HL VID_WDTH 8 * LD,
 BEGIN, CLRA, (DE) A LD, DE INC, HL DEC, HLZ,
 BR CNZ JR, EXX, BC POP, ;CODE
: NEWLN DUP 1+ VD_LINES @ = IF (vidscr) ELSE 1+ THEN ;
( ----- 101 )
\ Stream video frames, single scan
CODE (vidfr) ( -- ) BC PUSH, EXX,
  C SPI_DATA LD, DE VID_MEM $04 + (n) LD,
  HL VID_MEM 40 + VID_WDTH - LD, HL DE ADD,
  VID_MEM $06 + (n) HL LD, DE VID_WDTH 24 - LD,
  B VID_SCN LD,
  LSET L1 BEGIN,
    6 CTL8255 OUTii, HL DE ADD, 7 CTL8255 OUTii,
    A B LD, 4 repeat NOP, 24 repeat OUTI,
    B A LD, BR DJNZ,
  B 0 LD, B 0 LD, B 0 LD, B VID_VBL 1 - LD, FJR JR,
  LSET L2 A VID_VBL 1 - LD, FJR JR, FMARK FMARK
    A B LD, B 28 LD, BEGIN, BR DJNZ, HL INC, B A LD,
    7 CTL8255 OUTii, 5 repeat NOP, 6 CTL8255 OUTii,
  L2 BR DJNZ,
( ----- 102 )
  A VID_MEM $02 + (n) LD, B A LD, A VID_MEM (n) LD,
  A B SUB, IFNZ,
    VID_MEM (n) A LD, B 23 LD, HL INC, B 23 LD,
    BEGIN, BR DJNZ,
    HL VID_MEM $06 + (n) LD, B VID_SCN LD, 7 CTL8255 OUTii,
    5 repeat NOP, 6 CTL8255 OUTii, L1 JP,
  THEN, EXX, BC POP, ;CODE
( ----- 103 )
\ Stream video frames, double scan
CODE (vidfr) ( -- ) BC PUSH, EXX,
  C SPI_DATA LD, DE VID_MEM $04 + (n) LD,
  HL VID_MEM 40 + VID_WDTH - LD, HL DE ADD,
  VID_MEM $06 + (n) HL LD, DE VID_WDTH 24 - LD, B VID_SCN LD,
  LSET L1 BEGIN,
    6 CTL8255 OUTii, HL DE ADD, 7 CTL8255 OUTii, A B LD,
    DE DEC, DE -25 LD, 24 repeat OUTI,
    AF PUSH, DE INC, 6 CTL8255 OUTii, HL DE ADD,
    7 CTL8255 OUTii, AF POP, DE VID_WDTH 24 - LD,
    24 repeat OUTI, B A LD, BR DJNZ,
  B 0 LD, B 0 LD, B 0 LD, B VID_VBL 1 - LD, FJR JR,
  LSET L2 A VID_VBL 1 - LD, FJR JR, FMARK FMARK
    A B LD, B 28 LD, BEGIN, BR DJNZ, HL INC, B A LD,
    7 CTL8255 OUTii, 5 repeat NOP, 6 CTL8255 OUTii,
  L2 BR DJNZ,
( ----- 104 )
  A VID_MEM $02 + (n) LD, B A LD, A VID_MEM (n) LD, A B SUB,
  IFNZ,
    VID_MEM (n) A LD, B 23 LD, HL INC, B 23 LD,
    BEGIN, BR DJNZ, HL VID_MEM $06 + (n) LD, B VID_SCN LD,
    7 CTL8255 OUTii, 5 repeat NOP, 6 CTL8255 OUTii, L1 JP,
  THEN, EXX, BC POP, ;CODE
( ----- 105 )
\ PS2 keyboard driver subsystem
PSK_MEM CONSTANT PSK_STAT
PSK_MEM $02 + CONSTANT PSK_CC
PSK_MEM $04 + CONSTANT PSK_BUFI
PSK_MEM $06 + CONSTANT PSK_BUFO
PSK_MEM $08 + CONSTANT PSK_BUF
PC XORG $39 + T! ( RST 38 )
DI, AF PUSH, $10 SIOA_CTL OUTii, A SIOA_CTL IN,
4 A BIT, IFZ, AF POP, EI, RETI, THEN, ( I1 - T1 )
A PSK_MEM (n) LD, A A OR,
IFZ, A PTC8255 IN, 7 A BIT,           ( I1 - )
IFZ, A 1 LD, PSK_MEM (n) A LD, THEN,  ( I2 - T2 )
( ----- 106 )
AF POP, EI, RETI, THEN,             ( - T1 )
A $9 CP, FJR CNZ JR, TO L3
HL PUSH, HL PSK_MEM $02 + (n) LD, H 8 LD, CLRA,
BEGIN, L RRC, A 0 ADC, H DEC, BR CNZ JR,
H A LD, A PTC8255 IN, A H LD, A 0 ADC, A $1 AND,
FJR CZ JR, TO L1 CLRA, VID_MEM (n) A LD, VID_MEM $02 + (n) A LD,
A PSK_MEM $04 + (n) LD, L A LD, A PSK_MEM $06 + (n) LD,
A INC, A PS2_BMSK AND, A L CP, FJR CZ JR, TO L1
PSK_MEM $06 + (n) A LD, L A LD,
A PSK_MEM $08 + <<8 >>8 LD, A L ADD, L A LD,
A PSK_MEM $08 + >>8 LD, A 0 ADC,
( ----- 107 )
H A LD, A PSK_MEM $02 + (n) LD, (HL) A LD,
L1 FMARK CLRA, PSK_MEM (n) A LD, HL POP, AF POP, EI, RETI,
L3 FMARK A PTC8255 IN, RLCA, A PSK_MEM $02 + (n) LD,
RRA, PSK_MEM $02 + (n) A LD,
A PSK_MEM (n) LD, A INC, PSK_MEM (n) A LD,
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
RS_ADDR $90 - VALUE SYSVARS
SYSVARS $80 + VALUE DRVMEM
DRVMEM VALUE KBD_MEM
DRVMEM 3 + VALUE GRID_MEM
DRVMEM 6 + VALUE FDMEM
DRVMEM 13 + VALUE UNDERCUR
DRVMEM 14 + VALUE RXTX_MEM
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
