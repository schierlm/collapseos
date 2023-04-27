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
: Z80A 5 LOAD ( wordtbl ) 320 329 LOADR 7 LOAD ( Flow words ) ;
: Z80C 302 314 LOADR ;
: TRS804PM 380 LOAD ;
\ see comment at TICKS' definition
\ 7.373MHz target: 737t. outer: 37t inner: 16t
\ tickfactor = (737 - 37) / 16
44 VALUE tickfactor
0 VALUE L4 \ we need a 4th temp label in core routines
( ----- 002 )
\ Z80 port, core routines
FJR jr, TO L1 $10 OALLOT LSET lblxt ( RST 10 )
  IX inc, IX inc, 0 ix+) E ld, 1 ix+) D ld,
  HL pop, ldDE(HL), HL inc, DE HL ex, jp(HL), \ 17 bytes
$28 OALLOT LSET lblcell ( RST 28 )
  HL pop, BC push, HL>BC, FJR jr, TO L2 ( next ) $30 OALLOT
LSET lblval ( RST 30 ) A SYSVARS $18 + m) ld, A A or,
  FJR CZ jrc, TO L3 ( read ) FJR jr, TO L4 ( write ) \ 8 bytes
0 jp, ( RST 38 ) $66 OALLOT retn,
L1 FMARK
 di, SP PS_ADDR i) ld, IX RS_ADDR i) ld, 0 jp, PC 2 - TO lblboot
L3 FMARK ( val read ) HL pop, BC push, ldBC(HL), \ to lblnext
LSET lblnext L2 FMARK
DE HL ex, LSET L1 ( EXIT ) ldDE(HL), HL inc, DE HL ex, jp(HL),
L4 FMARK ( val write ) clrA, SYSVARS $18 + m) A ld, HL pop,
  (HL) C ld, HL inc, (HL) B ld, BC pop, lblnext BR jr,
( ----- 003 )
\ Z80 port, lbldoes EXIT QUIT ABORT BYE RCNT SCNT
LSET lbldoes HL pop, BC push, HL>BC, BC inc, BC inc, ldHL(HL),
  jp(HL),
CODE EXIT \ put new IP in HL instead of DE for speed
  L 0 ix+) ld, H 1 ix+) ld, IX dec, IX dec, L1 jp,
CODE QUIT LSET L1 \ used in ABORT
  IX RS_ADDR i) ld, 0 jp, PC 2 - TO lblmain
CODE ABORT SP PS_ADDR i) ld, L1 BR jr,
CODE BYE halt,
CODE RCNT BC push, IX push, HL pop, BC RS_ADDR i) ld,
  BC subHL, HL>BC, ;CODE
CODE SCNT HL 0 i) ld, HL SP add, BC push, HL>BC,
  HL PS_ADDR i) ld, BC subHL, HL>BC, ;CODE
( ----- 004 )
\ Z80 port, TICKS
\ The word below is designed to wait the proper 100us per tick
\ at 500kHz when tickfactor is 1. If the CPU runs faster,
\ tickfactor has to be adjusted accordingly. "t" in comments
\ below means "T-cycle", which at 500kHz is worth 2us.
CODE TICKS
  \ we pre-dec to compensate for initialization
  BEGIN,
    BC dec, ( 6t )
    IFZ, ( 12t ) BC pop, ;CODE THEN,
    A tickfactor i) ld, ( 7t )
    BEGIN, A dec, ( 4t ) BR CNZ jrc, ( 12t )
  BR jr, ( 12t ) ( outer: 37t inner: 16t )
( ----- 005 )
\ Z80 port, PC! PC@ []= [C]? (im1)
CODE PC! HL pop, (C) L out, BC pop, ;CODE
CODE PC@ C (C) in, B 0 i) ld, ;CODE
CODE []= BC push, exx, ( protect DE ) BC pop, DE pop, HL pop,
  LSET L1 ( loop )
    A (DE) ld, DE inc, cpi,
    IFNZ, exx, BC 0 i) ld, ;CODE THEN,
    L1 CPE jpc, ( BC not zero? loop )
  exx, BC 1 i) ld, ;CODE
CODE [C]? BCZ, IFZ, BC dec, HL pop, HL pop, ;CODE THEN,
  BC push, exx, BC pop, HL pop, DE pop, A E ld, D H ld,
  E L ld, \ HL=a DE=a BC=u A=c
  cpir, IFZ, DE subHL, HL dec, ELSE, HL -1 i) ld, THEN,
  HL push, exx, BC pop, ;CODE
CODE (im1) im1, ei, ;CODE
( ----- 006 )
\ Z80 port, /MOD *
CODE * HL pop, DE push, DE HL ex, ( DE * BC -> HL )
  HL 0 i) ld, A $10 i) ld, BEGIN,
    HL HL add, E rl, D rl,
    IFC, HL BC add, THEN,
    A dec, BR CNZ jrc,
  HL>BC, DE pop, ;CODE
\ Divides AC by DE. quotient in AC remainder in HL
CODE /MOD BC>HL, BC pop, DE push, DE HL ex,
  A B ld, B 16 i) ld, HL 0 i) ld, BEGIN,
    scf, C rl, rla, HL HL adc, HL DE sbc,
    IFC, HL DE add, C dec, THEN,
  BR djnz,
  DE pop, HL push, B A ld, ;CODE
( ----- 007 )
\ Z80 port, FIND
CODE FIND ( sa sl -- w? f ) HL pop,
  HL BC add, \ HL points to after last char of s
  'N m) HL ld, HL SYSVARS $02 ( CURRENT ) + m) ld, BEGIN,
    HL dec, A (HL) ld, A $7f i) and, ( imm ) A C cp, IFZ,
      HL push, DE push, BC push, DE 'N m) ld,
      HL dec, HL dec, HL dec, \ Skip prev field
      LSET L1 ( loop )
        DE dec, A (DE) ld, cpd, IFZ, TO L2 ( break! )
      L1 CPE jpc, ( BC not zero? loop ) L2 FMARK
      BC pop, DE pop, HL pop, THEN,
    IFZ, ( match ) HL inc, HL push, BC 1 i) ld, ;CODE THEN,
    \ no match, go to prev and continue
    HL dec, A (HL) ld, HL dec, L (HL) ld, H A ld,
    A L or, IFZ, ( end of dict ) BC 0 i) ld, ;CODE THEN,
  BR jr,
( ----- 008 )
\ Z80 port, (b) (n) (br) (?br) (next)
CODE (b) ( -- c ) BC push, A (DE) ld, A>BC, DE inc, ;CODE
CODE (n) ( -- n ) BC push,
  DE HL ex, ldBC(HL), HL inc, DE HL ex, ;CODE
CODE (br) LSET L1 ( used in ?br and next )
  A (DE) ld, ( sign extend A into HL )
  L A ld, A A add, ( sign in carry ) A A sbc, ( FF if neg )
  H A ld, HL DE add, ( HL --> new IP ) DE HL ex, ;CODE
CODE (?br) BCZ, BC pop, L1 BR CZ jrc, DE inc, ;CODE
CODE (next)
  0 ix+) dec, IFNZ,
    A $ff i) ld, A 0 ix+) cp, IFZ, 1 ix+) dec, THEN,
    L1 BR jr, THEN,
  A A xor, A 1 ix+) cp, L1 BR CNZ jrc,
  IX dec, IX dec, DE inc, ;CODE
( ----- 009 )
\ Z80 port, >R I C@ @ C! ! 1+ 1- + -
CODE >R IX inc, IX inc, 0 ix+) C ld, 1 ix+) B ld, BC pop, ;CODE
CODE R@ BC push, C 0 ix+) ld, B 1 ix+) ld, ;CODE
CODE R~ IX dec, IX dec, ;CODE
CODE R> BC push, C 0 ix+) ld, B 1 ix+) ld,
  IX dec, IX dec, ;CODE
CODE C@ A (BC) ld, A>BC, ;CODE
CODE @ BC>HL, ldBC(HL), ;CODE
CODE C! BC>HL, BC pop, (HL) C ld, BC pop, ;CODE
CODE ! BC>HL, BC pop,
  (HL) C ld, HL inc, (HL) B ld, BC pop, ;CODE
CODE 1+ BC inc, ;CODE
CODE 1- BC dec, ;CODE
CODE + HL pop, HL BC add, HL>BC, ;CODE
CODE - HL pop, BC subHL, HL>BC, ;CODE
( ----- 010 )
\ Z80 port, AND OR XOR >> << >>8 <<8
CODE AND HL pop,
  A C ld, A L and, C A ld, A B ld, A H and, B A ld, ;CODE
CODE OR HL pop,
  A C ld, A L or, C A ld, A B ld, A H or, B A ld, ;CODE
CODE XOR HL pop,
  A C ld, A L xor, C A ld, A B ld, A H xor, B A ld, ;CODE
CODE NOT BCZ, BC 0 i) ld, IFZ, C inc, THEN, ;CODE
CODE >> B srl, C rr, ;CODE
CODE << C sla, B rl, ;CODE
CODE >>8 C B ld, B 0 i) ld, ;CODE
CODE <<8 B C ld, C 0 i) ld, ;CODE
( ----- 011 )
\ Z80 port, ROT ROT> DUP DROP SWAP OVER EXECUTE
CODE ROT ( a b c -- b c a ) ( BC=c )
  HL pop, ( b ) (SP) HL ex, ( a<>b ) BC push, ( c ) HL>BC, ;CODE
CODE ROT> ( a b c -- c a b ) ( BC=c )
  BC>HL, BC pop, ( b ) (SP) HL ex, ( a<>c ) HL push, ;CODE
CODE DUP ( a -- a a ) LSET L1 BC push, ;CODE
CODE ?DUP BCZ, L1 BR CNZ jrc, ;CODE
CODE DROP ( a -- ) BC pop, ;CODE
CODE SWAP ( a b -- b a ) HL pop, BC push, HL>BC, ;CODE
CODE OVER ( a b -- a b a )
  HL pop, HL push, BC push, HL>BC, ;CODE
CODE EXECUTE BC>HL, BC pop, jp(HL),
( ----- 012 )
\ Z80 port, JMPi! CALLi!
CODE JMPi! ( pc a -- len ) BC>HL, BC pop,
  A $c3 i) ld, LSET L1 (HL) A ld, HL inc,
  (HL) C ld, HL inc, (HL) B ld, BC 3 i) ld, ;CODE
CODE CALLi! ( pc a -- len ) BC>HL, BC pop,
  A B ld, A A or, IFZ, A C ld, A $c7 i) and, IFZ, \ RST
   A C ld, A $c7 i) or, (HL) A ld, BC 1 i) ld, ;CODE THEN, THEN,
  ( not RST ) A $cd i) ld, L1 BR jr,
( ----- 013 )
\ Z80 port speedups
CODE TUCK ( a b -- b a b ) HL pop, BC push, HL push, ;CODE
CODE NIP ( a b -- b ) HL pop, ;CODE
CODE +! ( n a -- ) BC>HL, ldBC(HL), HL dec, (SP) HL ex,
  HL BC add, HL>BC, HL pop, (HL) C ld, HL inc, (HL) B ld,
  BC pop, ;CODE
CODE A> BC push, IY push, BC pop, ;CODE
CODE >A BC push, IY pop, BC pop, ;CODE
CODE A>R IY push, HL pop,
  IX inc, IX inc, 0 ix+) L ld, 1 ix+) H ld, ;CODE
CODE R>A L 0 ix+) ld, H 1 ix+) ld, IX dec, IX dec,
  HL push, IY pop, ;CODE
CODE A+ IY inc, ;CODE
CODE A- IY dec, ;CODE
CODE AC@ BC push, C 0 iy+) ld, B 0 i) ld, ;CODE
CODE AC! 0 iy+) C ld, BC pop, ;CODE
( ----- 014 )
\ Z80 port speedups
CODE MOVE ( src dst u -- ) HL pop, DE HL ex, (SP) HL ex,
  BCZ, IFNZ, ldir, THEN, DE pop, BC pop, ;CODE
CODE = HL pop, BC subHL, BC 0 i) ld, IFZ, BC inc, THEN, ;CODE
CODE < HL pop, BC subHL, BC 0 i) ld, IFC, BC inc, THEN, ;CODE
CODE CRC16 ( c n -- c ) BC push, exx, ( protect DE )
  HL pop, ( n ) DE pop, ( c ) A L ld, A D xor, D A ld,
  B 8 i) ld, BEGIN,
    E sla, D rl, IFC, ( msb is set, apply polynomial )
      A D ld, A $10 i) xor, D A ld,
      A E ld, A $21 i) xor, E A ld, THEN,
  BR djnz,
  DE push, exx, ( unprotect DE ) BC pop, ;CODE
( ----- 020 )
\ Z80 Assembler. Operands. See doc/asm. Requires B5
: >>3 >> >> >> ; : <<3 << << << ; : <<4 <<3 << ;
: opreg 7 AND ; : optype >>3 3 AND ;
CREATE nbank $10 ALLOT
0 VALUE nbank>
: nbank@ ( op -- n ) opreg << nbank + @ ;
: nbank! ( n -- idx )
  nbank> TUCK << nbank + ! DUP 1+ opreg TO nbank> ;
28 CONSTS
  $00 B  $01 C  $02 D  $03 E  $04 H  $05 L  $06 (HL)  $07 A
  $08 BC $09 DE $0a HL $0b AF $0b SP
  $20 (BC) $21 (DE) $22 (SP) $23 AF' $24 I $25 R $26 (C)
  $00 CNZ $01 CZ $02 CNC $03 CC $04 CPO $05 CPE $06 CP $07 CM
: i) nbank! $10 OR ;         : m) nbank! $18 OR ;
: ix, $dd C, ; : iy, $fd C, ; : IX ix, HL ; : IY iy, HL ;
: _ <<8 (HL) OR $40 OR ; : ix+) ix, _ ; : iy+) iy, _ ;
( ----- 021 )
\ Z80 Assembler. Checks, asserts, util
: err ABORT" argument error" ;
: # ( f -- ) NOT IF err THEN ;
: HL# HL = # ; : A# A = # ;
: 8b? optype 0 = ; : 16b? optype 1 = ; : ixy+? $40 AND ;
: special? $20 AND ;
: 8b# 8b? # ;
: opexec ( op tbl -- ) SWAP optype WEXEC ;
: opcode, ( opcode -- ) DUP >>8 ?DUP IF C, THEN C, ;
: ?ixy+, ( op -- ) DUP ixy+? IF >>8 C, ELSE DROP THEN ;
( ----- 022 )
\ Z80 Assembler. sub, and, or, xor, cp,
: _reg8, OVER opreg OR opcode, ?ixy+, ;
: _imm, $46 OR opcode, nbank@ C, ;
4 WORDTBL _ ( op code -- )
  'W _reg8, 'W err 'W _imm, 'W err
: 8bari, ( A op code -- ) ROT A# OVER _ opexec ;
: op DOER , DOES> ( A op 'code -- ) @ 8bari, ;
$a0 op and,               $b8 op cp,
$b0 op or,                $90 op sub,
$a8 op xor,
( ----- 023 )
\ Z80 Assembler. rl, rr, rlc, rrc, sla, srl, bit, set, res,
4 WORDTBL _ ( op code -- )
'W _reg8, 'W err 'W err 'W err
: op DOER , DOES> ( op 'code ) @ OVER _ opexec ;
$cb10 op rl,   $cb18 op rr,   $cb00 op rlc,  $cb08 op rrc,
$cb20 op sla,  $cb38 op srl,
: op DOER , DOES> ( op b 'code ) @ SWAP <<3 OR OVER _ opexec ;
$cbc0 op set,      $cb80 op res,     $cb40 op bit,
( ----- 024 )
\ Z80 Assembler. inc, dec, add, adc, sbc,
: _reg8<<, @ OVER opreg <<3 OR C, ?ixy+, ;
: _reg16<<, 2 + @ SWAP opreg <<4 OR opcode, ;
: _ixy+<<, C, (HL) SWAP _reg8<<, nbank@ C, ;
4 WORDTBL _ ( op 'codes -- )
  'W _reg8<<, 'W _reg16<<, 'W err 'W err
: op DOER ( 8b ) , ( 16b ) , DOES> ( op 'codes ) OVER _ opexec ;
$03 04 op inc,      $0b 05 op dec,
: op DOER ( 8b ) , ( 16b ) , DOES> ( dst src 'codes -- )
  OVER 16b? IF ROT HL# _reg16<<, ELSE @ 8bari, THEN ;
$09 $80 op add,   $ed4a $88 op adc,   $ed42 $98 op sbc,
( ----- 025 )
\ Z80 Assembler. push, pop, in, out, rst,
4 WORDTBL _ ( op 'codes -- )
'W err 'W _reg16<<, 'W err 'W err
: op DOER 0 , , DOES> ( op 'code -- ) OVER _ opexec ;
$c5 op push,                $c1 op pop,
: _A, ( n in? ) <<3 $d3 OR C, nbank@ C, ;
: _C, ( reg in? ) NOT $ed40 OR SWAP <<3 OR opcode, ;
: _inout, ( op n-or-C in? )
  OVER (C) = IF NIP _C, ELSE ROT DROP _A, THEN ;
: in, 1 _inout, ;  : out, SWAP 0 _inout, ;
: rst, ( n ) $c7 OR C, ;
CREATE _ 9 nC, AF DE (SP) AF' HL HL $08 $eb $e3
: ex, ( op1 op2 -- ) SWAP _ 3 [C]? DUP 0>= #
  3 + _ + DUP C@ ROT = # 3 + C@ C, ;
( ----- 026 )
\ Z80 Assembler. Inherent ops
: op DOER , DOES> @ opcode, ;
$f3 op di,     $fb op ei,     $d9 op exx,    $76 op halt,
$00 op nop,    $37 op scf,    $3f op ccf,    $c9 op ret,
$17 op rla,    $07 op rlca,   $1f op rra,    $0f op rrca,
$eda1 op cpi,  $edb1 op cpir, $eda9 op cpd,  $edb9 op cpdr,
$ed46 op im0,  $ed56 op im1,  $ed5e op im2,  $eda0 op ldi,
$edb0 op ldir, $eda8 op ldd,  $edb8 op lddr, $ed44 op neg,
$ed4d op reti, $ed45 op retn, $eda2 op ini,  $edaa op ind,
$eda3 op outi,
( ----- 027 )
\ Z80 Assembler. ld,
CREATE _s1 $0a , $1a , 0 , 0 , $ed57 , $ed5f , 0 , 0 ,
CREATE _s2 $02 , $12 , 0 , 0 , $ed47 , $ed4f , 0 , 0 ,
: _r8 OVER opreg <<3 OVER opreg OR $40 OR C, OR ?ixy+, ;
: _sp DUP special? IF NIP _s1 ELSE DROP _s2 THEN
      SWAP opreg << + @ opcode, ;
: _n ( dst src -- i mask 16b? )
  nbank@ SWAP DUP 16b? IF opreg <<4 1 ELSE opreg <<3 0 THEN ;
4 WORDTBL _ ( dst src -- ) \ sel on src. dst should be a reg
:W 2DUP OR special? IF _sp ELSE _r8 THEN ;
:W HL# SP = # $f9 C, ;
:W _n IF $01 OR C, L, ELSE $06 OR C, C, THEN ;
:W 2DUP < <<3 ROT> <> _n IF
    DUP $20 = IF $02 ELSE $ed43 THEN OR ROT OR
    ELSE $38 = # SWAP $32 OR THEN opcode, L, ;
: ld, ( dst src -- ) OVER optype OVER optype MAX _ SWAP WEXEC ;
( ----- 028 )
\ Z80 Assembler. Macros
: clrA, A A xor, ;
: subHL, A A or, HL SWAP sbc, ;
: pushA, B 0 i) ld, C A ld, BC push, ;
: HLZ, A H ld, A L or, ;
: DEZ, A D ld, A E or, ;
: BCZ, A B ld, A C or, ;
: ldDE(HL), E (HL) ld, HL inc, D (HL) ld, ;
: ldBC(HL), C (HL) ld, HL inc, B (HL) ld, ;
: ldHL(HL), A (HL) ld, HL inc, H (HL) ld, L A ld, ;
: outHL, A H ld, DUP A out, A L ld, A out, ;
: outDE, A D ld, DUP A out, A E ld, A out, ;
: HL>BC, B H ld, C L ld, ;
: BC>HL, H B ld, L C ld, ;
: A>BC, C A ld, B 0 i) ld, ;
: A>HL, L A ld, H 0 i) ld, ;
( ----- 029 )
\ Z80 Assembler. Jumps, calls and HAL
: cond ( cond opcode -- opcode ) SWAP <<3 OR ;
: br8, ( n opcode -- ) C, C, ;
: jr, $18 br8, ; : djnz, $10 br8, ; : jrc, $20 cond br8, ;
: br16, ( n opcode -- ) C, L, ;
: jp, $c3 br16, ;            : call, $cd br16, ;
: jpc, $c2 cond br16, ;      : callc, $c4 cond br16, ;
: retc, $c0 cond C, ;        : jp(HL), $e9 C, ;
: jp(IX), IX DROP jp(HL), ; : jp(IY), IY DROP jp(HL), ;
ALIAS jp, JMPi, ALIAS jr, JRi,
: JMP(i), m) HL SWAP ld, jp(HL), ;
: CALLi, DUP $38 AND OVER = IF rst, ELSE call, THEN ;
: JRZi, CZ jrc, ; : JRNZi, CNZ jrc, ;
: JRCi, CC jrc, ; : JRNCi, CNC jrc, ;
: i>, BC push, i) BC SWAP ld, ;
: (i)>, BC push, m) BC SWAP ld, ;
( ----- 030 )
CODE AT28C! ( c a -- )
  BC>HL, BC pop,
  (HL) C ld, A C ld, ( orig ) B C ld, ( save )
  C (HL) ld, ( poll ) BEGIN,
    A (HL) ld, ( poll ) A C cp, ( same as old? )
    C A ld, ( save old poll, Z preserved )
  BR CNZ jrc,
\ equal to written? SUB instead of CP to ensure IOERR is NZ
  A B sub, IFNZ, SYSVARS ( IOERR ) m) A ld, THEN, BC pop, ;CODE
: AT28! ( n a -- ) 2DUP AT28C! 1+ SWAP >>8 SWAP AT28C! ;
( ----- 032 )
( SPI relay driver. See doc/hw/z80/spi.txt )
CODE (spix) ( n -- n )
  A C ld,
  SPI_DATA i) A out,
  \ wait until xchg is done
  BEGIN, A SPI_CTL i) in, A 1 i) and, BR CNZ jrc,
  A SPI_DATA i) in,
  C A ld, ;CODE
CODE (spie) ( n -- ) A C ld, SPI_CTL i) A out, BC pop, ;CODE
( ----- 035 )
( Z80 driver for TMS9918. Implements grid protocol. Requires
TMS_CTLPORT, TMS_DATAPORT and ~FNT from the Font compiler at
B520. Patterns are at addr $0000, Names are at $3800.
Load range B315-317 )
CODE _ctl ( a -- sends LSB then MSB )
  A C ld, TMS_CTLPORT i) A out, A B ld, TMS_CTLPORT i) A out,
  BC pop, ;CODE
CODE _data
  A C ld, TMS_DATAPORT i) A out, BC pop, ;CODE
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
    A 6850_CTL i) in, A $02 i) and, ( are we transmitting? )
  BR CZ jrc, ( yes, loop )
  A C ld, 6850_IO i) A out, BC pop, ;CODE
( ----- 041 )
CODE 6850<? BC push,
  clrA, ( 256x ) A $16 i) ( RTS lo ) ld, 6850_CTL i) A out,
  BC 0 i) ld, ( pre-push a failure )
  BEGIN, AF AF' ex, ( preserve cnt )
    A 6850_CTL i) in, A $1 i) and, ( rcv buff full? )
    IFNZ, ( full )
      A 6850_IO i) in, pushA, C 1 i) ld, clrA, ( end loop )
    ELSE, AF AF' ex, ( recall cnt ) A dec, THEN,
  BR CNZ jrc,
  A $56 i) ( RTS hi ) ld, 6850_CTL i) A out, ;CODE
( ----- 042 )
ALIAS 6850<? RX<? ALIAS 6850<? (key?)
ALIAS 6850> TX> ALIAS 6850> (emit)
: 6850$ $56 ( RTS high ) [ 6850_CTL LITN ] PC! ;
( ----- 045 )
( Zilog SIO driver. Load range B325-328. Requires:
  SIOA_CTL for ch A control register SIOA_DATA for data
  SIOB_CTL for ch B control register SIOB_DATA for data )
CODE SIOA<? BC push,
  clrA, ( 256x ) BC 0 i) ld, ( pre-push a failure )
  A 5 i) ( PTR5 ) ld, SIOA_CTL i) A out,
  A $68 i) ( RTS low ) ld, SIOA_CTL i) A out,
  BEGIN, AF AF' ex, ( preserve cnt )
    A SIOA_CTL i) in, A $1 i) and, ( rcv buff full? )
    IFNZ, ( full )
      A SIOA_DATA i) in, pushA, C 1 i) ld, clrA, ( end loop )
    ELSE, AF AF' ex, ( recall cnt ) A dec, THEN,
  BR CNZ jrc,
  A 5 i) ( PTR5 ) ld, SIOA_CTL i) A out,
  A $6a i) ( RTS high ) ld, SIOA_CTL i) A out, ;CODE
( ----- 046 )
CODE SIOA>
  BEGIN,
    A SIOA_CTL i) in, A $04 i) and, ( are we transmitting? )
  BR CZ jrc, ( yes, loop )
  A C ld, SIOA_DATA i) A out, BC pop, ;CODE
CREATE _ ( init data ) $18 C, ( CMD3 )
    $24 C, ( CMD2/PTR4 ) $c4 C, ( WR4/64x/1stop/nopar )
    $03 C, ( PTR3 ) $c1 C, ( WR3/RXen/8char )
    $05 C, ( PTR5 ) $6a C, ( WR5/TXen/8char/RTS )
    $21 C, ( CMD2/PTR1 ) 0 C, ( WR1/Rx no INT )
: SIOA$ _ >A 9 >R BEGIN AC@+ [ SIOA_CTL LITN ] PC! NEXT ;
( ----- 047 )
CODE SIOB<? BC push, ( copy/paste of SIOA<? )
  clrA, ( 256x ) BC 0 i) ld, ( pre-push a failure )
  A 5 i) ( PTR5 ) ld, SIOB_CTL i) A out,
  A $68 i) ( RTS low ) ld, SIOB_CTL i) A out,
  BEGIN, AF AF' ex, ( preserve cnt )
    A SIOB_CTL i) in, A $1 i) and, ( rcv buff full? )
    IFNZ, ( full )
      A SIOB_DATA i) in, pushA, C 1 i) ld, clrA, ( end loop )
    ELSE, AF AF' ex, ( recall cnt ) A dec, THEN,
  BR CNZ jrc,
  A 5 i) ( PTR5 ) ld, SIOB_CTL i) A out,
  A $6a i) ( RTS high ) ld, SIOB_CTL i) A out, ;CODE
( ----- 048 )
CODE SIOB>
  BEGIN,
    A SIOB_CTL i) in, A $04 i) and, ( are we transmitting? )
  BR CZ jrc, ( yes, loop )
  A C ld, SIOB_DATA i) A out, BC pop, ;CODE
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
  A CPORT_MEM m) ld, A $f3 i) and, ( TR/TH output )
  B 8 i) ld, BEGIN,
    A $bf i) and, ( TR lo ) C rl,
    IFC, A $40 i) or, ( TR hi ) THEN,
    CPORT_CTL i) A out, ( clic! ) A $80 i) or, ( TH hi )
    CPORT_CTL i) A out, ( clac! )
    AF AF' ex, A CPORT_D1 i) in, ( Up Btn is B6 ) rla, rla,
      L rl, AF AF' ex,
    A $7f i) and, ( TH lo ) CPORT_CTL i) A out, ( cloc! )
  BR djnz, CPORT_MEM m) A ld, C L ld, ;CODE
( ----- 068 )
\ Routines for interacting with SMS controller ports.
\ Requires CPORT_MEM, CPORT_CTL, CPORT_D1 and CPORT_D2 to be
\ defined. CPORT_MEM is a 1 byte buffer for CPORT_CTL. The last
\ 3 consts will usually be $3f, $dc, $dd.
\ mode -- set TR pin on mode a on:
\ 0= output low 1=output high 2=input
CODE _TRA! ( B0 -> B4, B1 -> B0 )
  C rr, rla, rla, rla, rla, B rr, rla,
  A $11 i) and, C A ld, A CPORT_MEM m) ld,
  A $ee i) and, A C or, CPORT_CTL i) A out, CPORT_MEM m) A ld,
  BC pop, ;CODE
CODE _THA! ( B0 -> B5, B1 -> B1 )
  C rr, rla, rla, rla, rla, C rr, rla, rla,
  A $22 i) and, C A ld, A CPORT_MEM m) ld,
  A $dd i) and, A C or, CPORT_CTL i) A out, CPORT_MEM m) A ld,
  BC pop, ;CODE
( ----- 069 )
CODE _TRB! ( B0 -> B6, B1 -> B2 )
  C rr, rla, rla, rla, rla, C rr, rla, rla, rla,
  A $44 i) and, C A ld, A CPORT_MEM m) ld,
  A $bb i) and, A C or, CPORT_CTL i) A out, CPORT_MEM m) A ld,
  BC pop, ;CODE
CODE _THB! ( B0 -> B7, B1 -> B3 )
  C rr, rla, rla, rla, rla, C rr, rla, rla, rla, rla,
  A $88 i) and, C A ld, A CPORT_MEM m) ld,
  A $77 i) and, A C or, CPORT_CTL i) A out, CPORT_MEM m) A ld,
  BC pop, ;CODE
CODE _D1@ BC push, A CPORT_D1 i) in, C A ld, B 0 i) ld, ;CODE
CODE _D2@ BC push, A CPORT_D2 i) in, C A ld, B 0 i) ld, ;CODE
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
    A $10 i) ( CMD ) in,
    rla, ( When 7th bit is clr, we can send a new cmd )
  BR CC jrc, ;CODE
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
  di,
    A $ff i) ld,
    KBD_PORT i) A out,
    A C ld,
    KBD_PORT i) A out,
    A KBD_PORT i) in,
  ei,
  C A ld,
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
: fdstat A $f0 i) in, ;
: fdcmd ( i )
  A SWAP i) ld, B $18 i) ld, $f0 i) A out, BEGIN, BR djnz, ;
: fdwait BEGIN, fdstat rrca, BR CC jrc, rlca, ;
: vid+, ( reg -- ) HL VIDMEM i) ld, HL SWAP add, ;
( ----- 081 )
\ TRS-80 4P video driver
24 CONSTANT LINES 80 CONSTANT COLS
CODE CELL! ( c pos -- ) HL pop,
  A L ld, BC vid+, (HL) A ld, BC pop, ;CODE
CODE CELLS! ( a pos u -- ) BC push, exx, BC pop, DE pop,
  DE vid+, DE HL ex, HL pop, BCZ, IFNZ, ldir, THEN, exx, BC pop,
;CODE
CODE CURSOR! ( new old -- ) BC vid+, A (HL) ld, A CURCHAR i) cp,
  IFZ, A UNDERCUR m) ld, (HL) A ld, THEN,
  BC pop, BC vid+, A (HL) ld, UNDERCUR m) A ld, A CURCHAR i) ld,
  (HL) A ld, BC pop, ;CODE
CODE SCROLL ( -- )
  exx, HL VIDMEM 80 + i) ld, DE VIDMEM i) ld, BC 1840 i) ld,
  ldir, H D ld, L E ld, DE inc, A SPC i) ld, (HL) A ld,
  BC 79 i) ld, ldir, exx, ;CODE
: NEWLN ( old -- new ) 1+ DUP LINES = IF 1- SCROLL THEN ;
( ----- 082 )
LSET L2 ( seek, B=trk ) A 21 i) ld, A B cp, A FDMEM m) ld,
  IFC, A $20 i) or, ( WP ) THEN,
  A $80 i) or, $f4 i) A out, \ FD sel
  A B ld, ( trk ) $f3 i) A out, $1c fdcmd ret,
CODE FDRD ( trksec addr -- st ) BC>HL, BC pop,
  L2 call, fdwait A $98 i) and, IFZ, di,
    A C ld, $f2 i) A out, ( sec ) C $f3 i) ld, $84 fdcmd \ read
    BEGIN, BEGIN, fdstat A $b6 i) and, BR CZ jrc, \ DRQ
      A $b4 i) and, IFZ, TO L3 ( error ) ini, BR CNZ jrc, THEN,
  fdwait A $3c i) and, L3 FMARK A>BC, ei, ;CODE
CODE FDWR ( trksec addr -- st ) BC>HL, BC pop,
  L2 call, fdwait A $98 i) and, IFZ, di,
    A C ld, $f2 i) A out, ( sec ) C $f3 i) ld, $a4 fdcmd \ read
    BEGIN, BEGIN, fdstat A $f6 i) and, BR CZ jrc, \ DRQ
      A $f4 i) and, IFZ, TO L3 ( error ) outi, BR CNZ jrc, THEN,
  fdwait A $3c i) and, L3 FMARK A>BC, ei, ;CODE
( ----- 083 )
CODE _dsel ( fdmask -- )
  A C ld, FDMEM m) A ld, A $80 i) or, $f4 i) A out,
  0 fdcmd ( restore ) fdwait BC pop, ;CODE
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
    A $ea i) in, A $40 i) and, IFNZ, ( TX reg empty )
      A $e8 i) in, A $80 i) and, IFZ, ( CTS low )
        A C ld, $eb i) A out, ( send byte ) BC pop, ;CODE
  THEN, THEN, BR jr,
( ----- 086 )
CODE RX<? BC push,
  clrA, ( 256x ) BC 0 i) ld, ( pre-push a failure )
  A $6c i) ( RTS low ) ld, $ea i) A out,
  BEGIN, AF AF' ex, ( preserve cnt )
    A $ea i) in, A $80 i) and, ( rcv buff full? )
    IFNZ, ( full )
      A $eb i) in, A>HL, HL push, C inc, clrA, ( end loop )
    ELSE, AF AF' ex, ( recall cnt ) A dec, THEN,
  BR CNZ jrc,
  A $6d i) ( RTS high ) ld, $ea i) A out, ;CODE
( ----- 087 )
LSET L1 6 nC, '`' 'h' 'p' 'x' '0' '8'
LSET L2 8 nC, $0d 0 $ff 0 0 $08 0 $20
PC XORG $39 + T! ( RST 38 )
AF push, HL push, DE push, BC push,
A $ec i) in, ( RTC INT ack )
A $f440 m) ld, A A or, IFNZ, \ 7th row is special
  HL L2 1- i) ld, BEGIN, HL inc, rra, BR CNC jrc,
  A (HL) ld, ELSE, \ not 7th row
  HL L1 i) ld, DE $f401 i) ld, BC $600 i) ld, BEGIN,
    A (DE) ld, A A or, IFNZ,
      C (HL) ld, BEGIN, C inc, rra, BR CNC jrc,
      C dec, THEN,
    E sla, HL inc, BR djnz,
  A C ld, THEN, \ cont.
( ----- 088 )
\ A=char or zero if no keypress. Now let's debounce
HL KBD_MEM 2 + i) ld, A A or, IFZ, \ no keypress, debounce
  (HL) A ld, ELSE, \ keypress, is it debounced?
  A (HL) cp, IFNZ, \ != debounce buffer
    C A ld, (HL) C ld, A $ff i) cp, IFZ, \ BREAK!
      HL pop, HL pop, HL pop, HL pop, HL pop, ei,
      X' QUIT jp, THEN,
    HL dec, A $f480 m) ld, A 3 i) and, (HL) A ld, HL dec,
    (HL) C ld, THEN, THEN,
BC pop, DE pop, HL pop, AF pop, ei, ret,
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
di, A $86 i) ld, $84 i) A out, \ mode 2, 80 chars, page 1
A $81 i) ld, $f4 i) A out, \ DRVSEL DD, drv0
A $40 i) ld, $ec i) A out, \ MODOUT 4MHZ, no EXTIO
HL 0 i) ld, ( dest addr ) clrA, $e4 i) A out, ( no NMI )
A inc, ( trk1 ) BEGIN,
  $f3 i) A out, AF AF' ex, ( save ) $18 ( seek ) fdcmd fdwait
  clrA, $f2 i) A out, C $f3 i) ld, BEGIN,
    $80 ( read sector ) fdcmd ( B=0 )
    BEGIN, fdstat rra, rra, BR CNC jrc, ( DRQ )
    ini, A $c1 i) ld, BEGIN, $f4 i) A out, ini, BR CNZ jrc,
    fdwait A $1c i) ( error mask ) and, IFNZ,
      A SPC i) add, VIDMEM m) A ld, BEGIN, BR jr, THEN,
    A $f2 i) in, A inc, $f2 i) A out, A 18 i) cp, BR CC jrc,
  AF AF' ex, ( restore ) A inc, A 3 i) cp, BR CC jrc, 0 rst,
( ----- 095 )
\ Dan SBC drivers. See doc/hw/z80/dan.txt
\ Macros
: OUTii, ( val port -- ) A ROT i) ld, i) A out, ;
: repeat ( n -- ) >R ' BEGIN ( w ) DUP EXECUTE NEXT DROP ;
( ----- 096 )
\ SPI relay driver
CODE (spix) ( n -- n )
  A C ld,
  SPI_DATA i) A out,
  ( wait until xchg is done )
  nop, nop, nop, nop,
  A SPI_DATA i) in,
  C A ld, ;CODE
CODE (spie) ( n -- )
  $9A CTL8255 OUTii, $3 CTL8255 OUTii,
  A C ld, A 1 i) xor, A 1 i) and, CTL8255 i) A out,
  BC pop, ;CODE
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
CODE (vidclr) ( -- ) BC push,
  $9A CTL8255 OUTii, $3 CTL8255 OUTii, $1 CTL8255 OUTii,
  BC VID_MEM $10 + i) ld, HL VID_WDTH VID_SCN * i) ld,
  BEGIN, clrA, (BC) A ld, BC inc, HL dec, HLZ, BR CNZ jrc,
  BC pop, ;CODE
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
CODE (vidscr) BC push, exx,
 BC VID_SCN 8 - VID_WDTH * i) ld, DE VID_MEM $10 + i) ld,
 HL VID_MEM $10 + VID_WDTH 8 * + i) ld,
 ldir, HL VID_WDTH 8 * i) ld,
 BEGIN, clrA, (DE) A ld, DE inc, HL dec, HLZ,
 BR CNZ jrc, exx, BC pop, ;CODE
: NEWLN DUP 1+ VD_LINES @ = IF (vidscr) ELSE 1+ THEN ;
( ----- 101 )
\ Stream video frames, single scan
CODE (vidfr) ( -- ) BC push, exx,
  C SPI_DATA i) ld, DE VID_MEM $04 + m) ld,
  HL VID_MEM 40 + VID_WDTH - i) ld, HL DE add,
  VID_MEM $06 + m) HL ld, DE VID_WDTH 24 - i) ld,
  B VID_SCN i) ld,
  LSET L1 BEGIN,
    6 CTL8255 OUTii, HL DE add, 7 CTL8255 OUTii,
    A B ld, 4 repeat nop, 24 repeat outi,
    B A ld, BR djnz,
  B 0 i) ld, B 0 i) ld, B 0 i) ld, B VID_VBL 1 - i) ld, FJR jr,
  LSET L2 A VID_VBL 1 - i) ld, FJR jr, FMARK FMARK
    A B ld, B 28 i) ld, BEGIN, BR djnz, HL inc, B A ld,
    7 CTL8255 OUTii, 5 repeat nop, 6 CTL8255 OUTii,
  L2 BR djnz,
( ----- 102 )
  A VID_MEM $02 + m) ld, B A ld, A VID_MEM m) ld,
  A B sub, IFNZ,
    VID_MEM m) A ld, B 23 i) ld, HL inc, B 23 i) ld,
    BEGIN, BR djnz,
    HL VID_MEM $06 + m) ld, B VID_SCN i) ld, 7 CTL8255 OUTii,
    5 repeat nop, 6 CTL8255 OUTii, L1 jp,
  THEN, exx, BC pop, ;CODE
( ----- 103 )
\ Stream video frames, double scan
CODE (vidfr) ( -- ) BC push, exx,
  C SPI_DATA i) ld, DE VID_MEM $04 + m) ld,
  HL VID_MEM 40 + VID_WDTH - i) ld, HL DE add,
  VID_MEM $06 + m) HL ld, DE VID_WDTH 24 - i) ld,
  B VID_SCN i) ld,
  LSET L1 BEGIN,
    6 CTL8255 OUTii, HL DE add, 7 CTL8255 OUTii, A B ld,
    DE dec, DE -25 i) ld, 24 repeat outi, AF push, DE inc,
    6 CTL8255 OUTii, HL DE add, 7 CTL8255 OUTii, AF pop,
    DE VID_WDTH 24 - i) ld, 24 repeat outi, B A ld, BR djnz,
  B 0 i) ld, B 0 i) ld, B 0 i) ld, B VID_VBL 1 - i) ld, FJR jr,
  LSET L2 A VID_VBL 1 - i) ld, FJR jr, FMARK FMARK
    A B ld, B 28 i) ld, BEGIN, BR djnz, HL inc, B A ld,
    7 CTL8255 OUTii, 5 repeat nop, 6 CTL8255 OUTii,
  L2 BR djnz,
( ----- 104 )
  A VID_MEM $02 + m) ld, B A ld, A VID_MEM m) ld, A B sub,
  IFNZ,
    VID_MEM m) A ld, B 23 i) ld, HL inc, B 23 i) ld,
    BEGIN, BR djnz, HL VID_MEM $06 + m) ld, B VID_SCN i) ld,
    7 CTL8255 OUTii, 5 repeat nop, 6 CTL8255 OUTii, L1 jp,
  THEN, exx, BC pop, ;CODE
( ----- 105 )
\ PS2 keyboard driver subsystem
PSK_MEM CONSTANT PSK_STAT
PSK_MEM $02 + CONSTANT PSK_CC
PSK_MEM $04 + CONSTANT PSK_BUFI
PSK_MEM $06 + CONSTANT PSK_BUFO
PSK_MEM $08 + CONSTANT PSK_BUF
PC XORG $39 + T! ( RST 38 )
di, AF push, $10 SIOA_CTL OUTii, A SIOA_CTL i) in,
A 4 bit, IFZ, AF pop, ei, reti, THEN,  ( I1 - T1 )
A PSK_MEM m) ld, A A or,
IFZ, A PTC8255 i) in, A 7 bit,         ( I1 - )
IFZ, A 1 i) ld, PSK_MEM m) A ld, THEN, ( I2 - T2 )
( ----- 106 )
AF pop, ei, reti, THEN,             ( - T1 )
A $9 i) cp, FJR CNZ jrc, TO L3
HL push, HL PSK_MEM $02 + m) ld, H 8 i) ld, clrA,
BEGIN, L rrc, A 0 i) adc, H dec, BR CNZ jrc,
H A ld, A PTC8255 i) in, A H ld, A 0 i) adc, A $1 i) and,
FJR CZ jrc, TO L1 clrA, VID_MEM m) A ld, VID_MEM $02 + m) A ld,
A PSK_MEM $04 + m) ld, L A ld, A PSK_MEM $06 + m) ld,
A inc, A PS2_BMSK i) and, A L cp, FJR CZ jrc, TO L1
PSK_MEM $06 + m) A ld, L A ld,
A PSK_MEM $08 + <<8 >>8 i) ld, A L add, L A ld,
A PSK_MEM $08 + >>8 i) ld, A 0 i) adc,
( ----- 107 )
H A ld, A PSK_MEM $02 + m) ld, (HL) A ld,
L1 FMARK clrA, PSK_MEM m) A ld, HL pop, AF pop, ei, reti,
L3 FMARK A PTC8255 i) in, rlca, A PSK_MEM $02 + m) ld,
rra, PSK_MEM $02 + m) A ld,
A PSK_MEM m) ld, A inc, PSK_MEM m) A ld,
AF pop, ei, reti,
( ----- 108 )
CODE (pskset)
  di, $11 SIOA_CTL OUTii, $19 SIOA_CTL OUTii, im1, ei, ;CODE
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
    BEGIN, BEGIN, fdstat $b6 ANDi, Z? BR ?JRi, \ DRQ
      $b4 ANDi, IFZ, TO L3 ( error ) INI, Z? ^? BR ?JRi,
    fdwait D DECr, Z? ^? BR ?JRi,
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
\   BEGIN, fdstat 2 ANDi, Z? BR ?JRi, \ DRQ
\   INIR, INIR, INIR, INIR, INIR,  fdstat EI, A>BC, ;CODE
\   LSET L1 INI,
\   LSET L2 fdstat RRA, RRA, C? L1 BR ?JRi, ( DRQ! )
\     RLA, C? L2 BR ?JRi,
\   RLA, $3c ANDi, EI, A>BC, ;CODE
( ----- 112 )
: INIR, $edb2 M, ;
CODE FDTRK@ ( a -- st ) \ st=status byte w/ error-only mask
  BC>HL, A $81 LDri, $f4 OUTiA, fdwait
  DI, $e4 fdcmd C $f3 LDri,
  BEGIN, fdstat 2 ANDi, Z? BR ?JRi, \ DRQ
  INIR, INIR, INIR, INIR, INIR, INIR, INIR, INIR, INIR,

    \ fdstat RRA, C? BR ?JRi,
  fdstat EI, A>BC, ;CODE
\   INIR, INIR, INIR, INIR, INIR,  fdstat EI, A>BC, ;CODE
\   LSET L1 INI,
\   LSET L2 fdstat RRA, RRA, C? L1 BR ?JRi, ( DRQ! )
\     RLA, C? L2 BR ?JRi,
\   RLA, $3c ANDi, EI, A>BC, ;CODE
( ----- 113 )
\ xcomp for my TRS80 4P.
3 CONSTS $f300 RS_ADDR $f3fa PS_ADDR 0 HERESTART
RS_ADDR $90 - VALUE SYSVARS
SYSVARS $80 + VALUE DRVMEM
SYSVARS $409 - VALUE BLK_MEM
DRVMEM VALUE KBD_MEM
DRVMEM 3 + VALUE GRID_MEM
DRVMEM 6 + VALUE FDMEM
DRVMEM 9 + VALUE MSPAN_MEM
DRVMEM 10 + VALUE UNDERCUR
DRVMEM 11 + VALUE RXTX_MEM
\ ARCHM XCOMP Z80A TRS804PM
\ XCOMPC Z80C COREL TRS804P
( ----- 114 )
ALIAS FD@ (ms@) ALIAS FD! (ms!)
CREATE (msdsks) 100 C, 100 C, 100 C, 180 C, 180 C, 0 C,
\ MSPANSUB BLKSUB GRIDSUB RXTXSUB
( ----- 115 )
: INIT GRID$ KBD$ BLK$ MSPAN$ FD$ $e CL$ ;
\ XWRAP
( ----- 116 )
\ trying out new TO semantics
CREATE to? 0 C,
PC ( lblval ) HL to? LD, (HL) 0 BIT, IFZ, ( read )
  HL POP, BC PUSH, LDBC(HL), ;CODE THEN, ( write )
  (HL) 0 RES, HL POP, (HL) C LD, HL INC, (HL) B LD, BC POP,
  ;CODE
CODE to A 1 LD, (to?) A LD, ;CODE
CODE fooval ( lblval ) CALL, $1234 ,
