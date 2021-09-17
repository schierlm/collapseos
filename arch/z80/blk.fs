( ----- 000 )
Z80 MASTER INDEX

301 Z80 boot code              310 Z80 HAL
320-329 unused
330 AT28 EEPROM                332 SPI relay
335 TMS9918
340 MC6850 driver              345 Zilog SIO driver
350 Sega Master System VDP     355 SMS PAD
360 SMS KBD                    367 SMS SPI relay
368 SMS Ports
370 TI-84+ LCD                 375 TI-84+ Keyboard
380 TRS-80 4P drivers
395 Dan SBC drivers
( ----- 001 )
\ Z80 port's Macros and constants. See doc/code/z80.txt
: Z80C 302 308 LOADR ; : Z80H 310 314 LOADR ;
: TRS804PM 380 LOAD ;
\ see comment at TICKS' definition
\ 7.373MHz target: 737t. outer: 37t inner: 16t
\ tickfactor = (737 - 37) / 16
44 VALUE tickfactor
: HL>BC, B H LDrr, C L LDrr, ;
: BC>HL, H B LDrr, L C LDrr, ;
: A>BC, C A LDrr, B 0 LDri, ;
: A>HL, L A LDrr, H 0 LDri, ;
1 VALUE JROPLEN -1 VALUE JROFF
( ----- 002 )
\ Z80 port's Code. Load range B281-B299
HERE TO ORG ( STABLE ABI )
FJR JRi, TO L1 ( B282 ) NOP, NOP, ( unused )
NOP, NOP, ( 04, BOOT ) NOP, NOP, ( 06 CURRENT )
NOP, NOP, ( 08, LATEST ) NOP, NOP, ( 0a (main) )
4 ALLOT0 LSET lblxt ( RST 10 )
  IX INCd, IX INCd, 0 IX+ E LDIXYr, 1 IX+ D LDIXYr,
  HL POP, LDDE(HL), HL INCd, EXDEHL, JP(HL), \ 17 bytes
7 ALLOT0
0 JP, ( RST 28 ) 5 ALLOT0
0 JP, ( RST 30 ) 5 ALLOT0
0 JP, ( RST 38 )
( ----- 003 )
L1 FMARK ( B281 )
  SP PS_ADDR LDdi, IX RS_ADDR LDdi,
  BIN( $04 ( BOOT ) + LDHL(i), JP(HL),
LSET lbldoes IX INCd, IX INCd, 0 IX+ E LDIXYr, 1 IX+ D LDIXYr,
  EXDEHL, \ continue to lblcell
LSET lblcell HL POP, \ continue to lblpush
LSET lblpush PUSHp, \ continue to lblnext
LSET lblnext EXDEHL, LDDE(HL), HL INCd, EXDEHL, JP(HL),
( ----- 004 )
( The word below is designed to wait the proper 100us per tick
  at 500kHz when tickfactor is 1. If the CPU runs faster,
  tickfactor has to be adjusted accordingly. "t" in comments
  below means "T-cycle", which at 500kHz is worth 2us. )
CODE TICKS
  ( we pre-dec to compensate for initialization )
  BEGIN,
    BC DECd, ( 6t )
    IFZ, ( 12t ) BC POP, ;CODE THEN,
    A tickfactor LDri, ( 7t )
    BEGIN, A DECr, ( 4t ) Z? ^? BR ?JRi, ( 12t )
  BR JRi, ( 12t ) ( outer: 37t inner: 16t )
( ----- 005 )
CODE PC! HL POP, L OUT(C)r, BC POP, ;CODE
CODE PC@ C INr(C), B 0 LDri, ;CODE
CODE []= BC PUSH, EXX, ( protect DE ) BC POP, DE POP, HL POP,
  LSET L1 ( loop )
    LDA(DE), DE INCd, CPI,
    IFNZ, EXX, BC 0 LDdi, ;CODE THEN,
    CPE L1 JPc, ( BC not zero? loop )
  EXX, BC 1 LDdi, ;CODE
CODE (im1) IM1, EI, ;CODE
CODE * HL POP, DE PUSH, EXDEHL, ( DE * BC -> HL )
  HL 0 LDdi, A $10 LDri, BEGIN,
    HL ADDHLd, E RL, D RL,
    IFC, BC ADDHLd, THEN,
    A DECr, Z? ^? BR ?JRi,
  HL>BC, DE POP, ;CODE
( ----- 006 )
\ Divides AC by DE. quotient in AC remainder in HL
CODE /MOD BC>HL, BC POP, DE PUSH, EXDEHL,
  A B LDrr, B 16 LDri, HL 0 LDdi, BEGIN,
    SCF, C RL, RLA,
    HL ADCHLd, DE SBCHLd,
    IFC, DE ADDHLd, C DECr, THEN,
  BR DJNZi,
  DE POP, HL PUSH, B A LDrr, ;CODE
CODE QUIT LSET L1 ( used in ABORT )
IX RS_ADDR LDdi, BIN( $0a ( main ) + LDHL(i), JP(HL),
CODE ABORT SP PS_ADDR LDdi, L1 BR JRi,
CODE BYE HALT,
CODE RCNT RS_ADDR i>w, PUSHp, IX PUSH, HL POP, -wp, w>p, ;CODE
CODE SCNT 0 i>w, SP ADDHLd, PUSHp, PS_ADDR i>w, -wp, w>p, ;CODE
( ----- 007 )
CODE FIND ( sa sl -- w? f ) BC PUSH, EXX, BC POP, HL POP,
  BC ADDHLd, HL DECd, \ HL points to the last char of s
  DE SYSVARS $02 ( CURRENT ) + LDd(i),
  BEGIN, \ main loop
    DE DECd, LDA(DE), $7f ANDi, ( IMMEDIATE ) C CPr, IFZ,
      HL PUSH, DE PUSH, BC PUSH,
      DE DECd, DE DECd, \ Skip prev field
      LSET L1 ( loop )
        DE DECd, LDA(DE), CPD, IFZ, TO L2 ( break! )
      CPE L1 JPc, ( BC not zero? loop ) L2 FMARK
      BC POP, DE POP, HL POP, THEN,
( ----- 008 )
\ At this point, Z is set if we have a match.
    IFZ, ( match ) DE INCd, DE PUSH, EXX, BC 1 LDdi, ;CODE THEN,
\ no match, go to prev and continue
\ we read prev field backwards
    DE DECd, LDA(DE), EXAFAF', DE DECd, LDA(DE),
    E A LDrr, EXAFAF', D A LDrr,
    E ORr, IFZ, \ DE=0, end of dict
      EXX, BC 0 LDdi, ;CODE THEN,
  BR JRi, \ main loop
( ----- 010 )
\ Z80 HAL, Stack
: w>p, $444d M, ; \ ld b,h; ld c,l
: p>w, $6069 M, ; \ ld h,b; ld l,c
: DROPp, $c1 C, ( pop bc ) ; : POPp, p>w, DROPp, ;
: DUPp, $c5 C, ( push bc ) ; : PUSHp, DUPp, w>p, ;
: POPf, $e1 C, ( pop hl ) ; : PUSHf, $e5 C, ( push hl ) ;
: POPr, $dd6e M, $00 C, ( ld l,ix+0 )
        $dd66 M, $01 C, ( ld h,ix+1 )
        $dd2b M, ( dec ix ) $dd2b M, ( dec ix ) ;
: PUSHr, $dd23 M, ( inc ix ) $dd23 M, ( inc ix )
         $dd75 M, $00 C, ( ld l,ix+0 )
         $dd74 M, $01 C, ( ld h,ix+1 ) ;
\ ld a,h; h,b; b,a; a,l; l,c; c,a
: SWAPwp, $7c60 M, $477d M, $694f M, ;
: SWAPwf, $e3 C, ; \ ex (sp),hl
( ----- 011 )
\ Z80 HAL, Jumps, Transfer
SYSVARS $16 + *VALUE ?JROP
: JMPw, $e9 C, ; \ jp (hl)
: JMPi, $c3 C, L, ;
: CALLi, DUP $38 AND OVER = IF
  ( RST ) $c7 OR C, ELSE $cd C, L, THEN ;
: JRi, $18 C, ( JR ) C, ;
: ?JRi, ?JROP C, C, ;
: INCw, $23 C, ( inc hl ) ; : DECw, $2b C, ( dec hl ) ;
: INCp, $03 C, ( inc bc ) ; : DECp, $0b C, ( dec bc ) ;
: i>w, $21 C, L, ; \ ld hl,nn
: (i)>w, $2a C, L, ; \ ld hl,(nn)
: C@w, $6e C, $2600 M, ; \ ld l,(hl); ld h,0
: @w, $7e23 M, $666f M, ; \ ld a,(hl); inc hl; h,(hl); l,a
: C!wp, $71 C, ; \ ld (hl),c
: !wp, C!wp, INCw, $70 C, ; \ ld (hl),b
( ----- 012 )
\ Z80 HAL, Transfer
: w>Z, $7db4 M, ; \ ld a,l; or h
: p>Z, $78b1 M, ; \ ld a,c; or b
: Z? $28 [*TO] ?JROP ; : C? $38 [*TO] ?JROP ;
: ^? ?JROP 8 XOR [*TO] ?JROP ;
: C>w, 0 i>w, $ed6a M, ; \ adc hl,hl
: Z>w, 0 i>w, Z? ^? 1 ?JRi, INCw, ;
: IP>w, $626b M, ; \ ld h,d; ld l,e
: w>IP, $545d M, ; \ ld d,h; ld e,l
: IP+, $13 C, ; \ inc de
: IP+off, $1a C, ( ld a,(de) ) $cb7f M, ( bit 7,a )
  Z? 1 ?JRi, $15 C, ( dec d ) $83 C, ( add e )
  C? ^? 1 ?JRi, $14 C, ( inc d ) $5f C, ( ld e,a ) ;
( ----- 013 )
\ Z80 HAL, Arithmetic
: +wp, $09 C, ; \ add hl,bc
: -wp, $b7 C, $ed42 M, ; \ or a; sbc hl,bc
: >>w, $cb3c M, $cb1d M, ; \ srl h; rr l
: <<w, $29 C, ; \ add hl,hl
: >>8w, $6c C, $2600 M, ; \ ld l,h; ld h,0
: <<8w, $65 C, $2e00 M, ; \ ld h,l; ld l,0
( ----- 014 )
\ Z80 HAL, Arithmetic
: CMPwp, $7cb8 M, Z? ^? 2 ?JRi, $7db9 M, ; \ a,h; cpb; a,l; cpc
: ANDwp, $7ca0 M, $677d M, $a16f M, ; \ a,h;and b;h,a;a,l;and c
: ORwp, $7cb0 M, $677d M, $b16f M, ; \ a,h;or b;h,a;a,l;or c;l,a
: XORwp, $7ca8 M, $677d M, $a96f M, ; \ a,h;xor b;h,a;a,l;xor c
: XORwi, $7cee M, DUP >>8 C, $677d M, $ee C, C, $6f C, ;
( ----- 030 )
CODE AT28C! ( c a -- )
  BC>HL, BC POP,
  (HL) C LDrr, A C LDrr, ( orig ) B C LDrr, ( save )
  C (HL) LDrr, ( poll ) BEGIN,
    A (HL) LDrr, ( poll ) C CPr, ( same as old? )
    C A LDrr, ( save old poll, Z preserved )
  Z? ^? BR ?JRi,
\ equal to written? SUB instead of CP to ensure IOERR is NZ
  B SUBr, IFNZ, SYSVARS ( IOERR ) LD(i)A, THEN, BC POP, ;CODE
: AT28! ( n a -- ) 2DUP AT28C! 1+ SWAP >>8 SWAP AT28C! ;
( ----- 032 )
( SPI relay driver. See doc/hw/z80/spi.txt )
CODE (spix) ( n -- n )
  A C LDrr,
  SPI_DATA OUTiA,
  ( wait until xchg is done )
  BEGIN, SPI_CTL INAi, 1 ANDi, Z? ^? BR ?JRi,
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
    ( x ) 0 DO 0 _data LOOP ;
( Each row in ~FNT is a row of the glyph and there is 7 of
them.  We insert a blank one at the end of those 7. )
: _sfont ( a -- Send font to TMS )
    7 0 DO C@+ _data LOOP DROP
    ( blank row ) 0 _data ;
: _sfont^ ( a -- Send inverted font to TMS )
    7 0 DO C@+ $ff XOR _data LOOP DROP
    ( blank row ) $ff _data ;
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
    $4000 _ctl $5f 0 DO ~FNT I 7 * + _sfont LOOP
    $4400 _ctl $5f 0 DO ~FNT I 7 * + _sfont^ LOOP
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
  Z? BR ?JRi, ( yes, loop )
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
  Z? ^? BR ?JRi,
  A $56 ( RTS hi ) LDri, 6850_CTL OUTiA, ;CODE
( ----- 042 )
X' 6850<? ALIAS RX<? X' 6850<? ALIAS (key?)
X' 6850> ALIAS TX> X' 6850> ALIAS (emit)
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
  Z? ^? BR ?JRi,
  A 5 ( PTR5 ) LDri, SIOA_CTL OUTiA,
  A $6a ( RTS high ) LDri, SIOA_CTL OUTiA, ;CODE
( ----- 046 )
CODE SIOA>
  BEGIN,
    SIOA_CTL INAi, $04 ANDi, ( are we transmitting? )
  Z? BR ?JRi, ( yes, loop )
  A C LDrr, SIOA_DATA OUTiA, BC POP, ;CODE
CREATE _ ( init data ) $18 C, ( CMD3 )
    $24 C, ( CMD2/PTR4 ) $c4 C, ( WR4/64x/1stop/nopar )
    $03 C, ( PTR3 ) $c1 C, ( WR3/RXen/8char )
    $05 C, ( PTR5 ) $6a C, ( WR5/TXen/8char/RTS )
    $21 C, ( CMD2/PTR1 ) 0 C, ( WR1/Rx no INT )
: SIOA$ _ 9 RANGE DO I C@ [ SIOA_CTL LITN ] PC! LOOP ;
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
  Z? ^? BR ?JRi,
  A 5 ( PTR5 ) LDri, SIOB_CTL OUTiA,
  A $6a ( RTS high ) LDri, SIOB_CTL OUTiA, ;CODE
( ----- 048 )
CODE SIOB>
  BEGIN,
    SIOB_CTL INAi, $04 ANDi, ( are we transmitting? )
  Z? BR ?JRi, ( yes, loop )
  A C LDrr, SIOB_DATA OUTiA, BC POP, ;CODE
: SIOB$ _ 9 RANGE DO I C@ [ SIOB_CTL LITN ] PC! LOOP ;
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
: _sfont ( a -- Send font to VDP )
  7 RANGE DO I C@ _data 3 _zero LOOP ( blank row ) 4 _zero ;
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
  9 0 DO _idat I 2 * + @ _ctl LOOP
  ( blank screen ) $7800 _ctl COLS LINES * 2 * _zero
  ( palettes )
  $c000 _ctl
  ( BG ) 1 _zero $3f _data 14 _zero
  ( sprite, inverted colors ) $3f _data 15 _zero
  $4000 _ctl $5f 0 DO ~FNT I 7 * + _sfont LOOP
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
  _sel @ >R _ BEGIN ( a R:c ) C@+ I > UNTIL ( a R:c ) R> DROP
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
  BR DJNZi, CPORT_MEM LD(i)A,
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
  C? BR ?JRi, ;CODE
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
  SWAP _row! ( u ) 0 DO
    _yinc 0 _col!
    11 0 DO 0 _data! LOOP
    _xinc 0 _data! LOOP ;
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
  FNTH 0 DO ( pos coff a )
    OVER 8 -^ SWAP C@+ ( pos coff 8-coff a+1 c ) ROT LSHIFT
    _data@ <<8 OR
    LCD_BUF I + 2DUP FNTH + C!
    SWAP >>8 SWAP C!
  LOOP 2DROP
  DUP _atrow!
  FNTH 0 DO LCD_BUF I + C@ _data! LOOP
  DUP _atrow! _tocol NIP 1+ _col!
  FNTH 0 DO LCD_BUF FNTH + I + C@ _data! LOOP ;
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
    7 0 DO
        1 I LSHIFT $ff -^ ( group dmask ) _get
        DUP $ff = IF DROP ELSE I ( dmask gid ) LEAVE THEN
    LOOP _wait
    SWAP ( gid dmask )
    $ff XOR ( dpos ) 0 ( dindex )
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
: TRS804PL 381 388 LOADR ; : TRS804PH 389 LOAD ;
$f800 VALUE VIDMEM $bf VALUE CURCHAR
: fdstat $f0 INAi, ;
: fdcmd A SWAP LDri, B $18 LDri,
  $f0 OUTiA, BEGIN, BR DJNZi, ;
: fdwait BEGIN, fdstat RRCA, C? BR ?JRi, RLCA, ;
: vid+, ( reg -- ) HL VIDMEM LDdi, ADDHLd, ;
( ----- 081 )
\ TRS-80 4P video driver
24 VALUE LINES 80 VALUE COLS
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
    BEGIN, BEGIN, fdstat $b6 ANDi, Z? BR ?JRi, \ DRQ
      $b4 ANDi, IFZ, TO L3 ( error ) INI, Z? ^? BR ?JRi, THEN,
  fdwait $3c ANDi, L3 FMARK A>BC, EI, ;CODE
CODE FDWR ( trksec addr -- st ) BC>HL, BC POP,
  L2 CALL, fdwait $98 ANDi, IFZ, DI,
    A C LDrr, $f2 OUTiA, ( sec ) C $f3 LDri, $a4 fdcmd ( read )
    BEGIN, BEGIN, fdstat $f6 ANDi, Z? BR ?JRi, \ DRQ
      $f4 ANDi, IFZ, TO L3 ( error ) OUTI, Z? ^? BR ?JRi, THEN,
  fdwait $3c ANDi, L3 FMARK A>BC, EI, ;CODE
( ----- 083 )
FDMEM 1+ *ALIAS FDOP
: _err LIT" FDerr " STYPE .X ABORT ;
: _trksec ( sec -- trksec )
\ 4 256b sectors per block, 18 sec per trk, 40 trk max
  18 /MOD ( sec trk ) DUP 39 > IF $ffff _err THEN <<8 + ;
: FD@! ( blk blk( -- )
  SWAP << << ( blk( blk*4=sec ) 4 RANGE DO ( dest )
    I _trksec OVER ( dest trksec dest )
    FDOP ( dest ) ?DUP IF _err THEN $100 +
  LOOP DROP ;
: FD@ ['] FDRD [*TO] FDOP FD@! ;
: FD! ['] FDWR [*TO] FDOP FD@! ;
CODE FDSEL ( fdmask -- )
  A C LDrr, BC POP, FDMEM LD(i)A, $80 ORi, $f4 OUTiA,
  0 fdcmd ( restore ) fdwait ;CODE
: FD$ 2 FDSEL ;
( ----- 084 )
: CL$ ( baudcode -- )
  $02 $e8 PC! ( UART RST )
  DUP << << << << OR $e9 PC! ( bauds )
  $6d $ea PC! ( word8 no parity no-RTS ) ;
CODE TX> BEGIN,
    $ea INAi, $40 ANDi, IFNZ, ( TX reg empty )
      $e8 INAi, $80 ANDi, IFZ, ( CTS low )
        A C LDrr, $eb OUTiA, ( send byte ) BC POP, ;CODE
  THEN, THEN, BR JRi,
( ----- 085 )
CODE RX<? BC PUSH,
  A XORr, ( 256x ) BC 0 LDdi, ( pre-push a failure )
  A $6c ( RTS low ) LDri, $ea OUTiA,
  BEGIN, EXAFAF', ( preserve cnt )
    $ea INAi, $80 ANDi, ( rcv buff full? )
    IFNZ, ( full )
      $eb INAi, A>HL, HL PUSH, C INCr, A XORr, ( end loop )
    ELSE, EXAFAF', ( recall cnt ) A DECr, THEN,
  Z? ^? BR ?JRi,
  A $6d ( RTS high ) LDri, $ea OUTiA, ;CODE
( ----- 086 )
LSET L1 6 nC, '`' 'h' 'p' 'x' '0' '8'
LSET L2 8 nC, $0d 0 $ff 0 0 $08 0 $20
PC ORG $39 + T! ( RST 38 )
AF PUSH, HL PUSH, DE PUSH, BC PUSH,
$ec INAi, ( RTC INT ack )
$f440 LDA(i), A ORr, IFNZ, \ 7th row is special
  HL L2 1- LDdi, BEGIN, HL INCd, RRA, C? ^? BR ?JRi,
  A (HL) LDrr, ELSE, \ not 7th row
  HL L1 LDdi, DE $f401 LDdi, BC $600 LDdi, BEGIN,
    LDA(DE), A ORr, IFNZ,
      C (HL) LDrr, BEGIN, C INCr, RRA, C? ^? BR ?JRi,
      C DECr, THEN,
    E SLA, HL INCd, BR DJNZi,
  A C LDrr, THEN, \ cont.
( ----- 087 )
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
( ----- 088 )
KBD_MEM *VALUE KBDBUF \ LSB=char MSB=shift
: KBD$ 0 [*TO] KBDBUF $04 $e0 PC! ( enable RTC INT ) (im1) ;
: (key?) KBDBUF DUP <<8 >>8 NOT IF DROP 0 EXIT THEN
  0 [*TO] KBDBUF L|M ( char flags )
  OVER '<' '`' =><= IF 1 XOR THEN \ invert shift
  TUCK 1 AND IF \ lshift  ( flags char )
    DUP '@' < IF $ef ELSE $df THEN AND THEN
  SWAP 2 AND IF \ rshift ( char )
    DUP '1' < IF $2f ELSE $4a THEN + THEN
  1 ( success ) ;
( ----- 089 )
: FD0 FLUSH 1 FDSEL ;
: FD1 FLUSH 2 FDSEL ;
( ----- 090 )
\ TRS-80 4P bootloader. Loads sectors 2-17 to addr 0.
HERE TO ORG
DI, A $86 LDri, $84 OUTiA, \ mode 2, 80 chars, page 1
A $81 LDri, $f4 OUTiA, \ DRVSEL DD, drv0
A $40 LDri, $ec OUTiA, \ MODOUT 4MHZ, no EXTIO
HL 0 LDdi, ( dest addr ) A XORr, $e4 OUTiA, ( no NMI )
A INCr, ( trk1 ) BEGIN,
  $f3 OUTiA, EXAFAF', ( save ) $18 ( seek ) fdcmd fdwait
  A XORr, $f2 OUTiA, C $f3 LDri, BEGIN,
    $80 ( read sector ) fdcmd ( B=0 )
    BEGIN, fdstat RRA, RRA, C? ^? BR ?JRi, ( DRQ )
    INI, A $c1 LDri, BEGIN, $f4 OUTiA, INI, Z? ^? BR ?JRi,
    fdwait $1c ( error mask ) ANDi, IFNZ,
      SPC ADDi, VIDMEM LD(i)A, BEGIN, BR JRi, THEN,
    $f2 INAi, A INCr, $f2 OUTiA, 18 CPi, C? BR ?JRi,
  EXAFAF', ( restore ) A INCr, 3 CPi, C? BR ?JRi, 0 RST,
( ----- 095 )
\ Dan SBC drivers. See doc/hw/z80/dan.txt
\ Macros
: OUTii, ( val port -- ) A ROT LDri, OUTiA, ;
: repeat ( n -- ) ' SWAP 0 DO ( w ) DUP EXECUTE LOOP DROP ;
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
  $92 CTL8255 OUTii, $3 CTL8255 OUTii,
  A C LDrr, 1 XORi, 1 ANDi, CTL8255 OUTiA, BC POP, ;CODE
( ----- 097 )
\ software framebuffer subsystem
VID_MEM *VALUE VD_DECFR
VID_MEM $02 + *VALUE VD_DECTL
VID_MEM $04 + *VALUE VD_CURCL
VID_MEM $06 + *VALUE VD_FRMST
VID_MEM $08 + *VALUE VD_COLS
VID_MEM $0A + *VALUE VD_LINES
VID_MEM $0C + *VALUE VD_FRB
VID_MEM $0E + *VALUE VD_OFS
\ Clear Framebuffer
CODE (vidclr) ( -- ) BC PUSH,
  $92 CTL8255 OUTii, $3 CTL8255 OUTii, $1 CTL8255 OUTii,
  BC VID_MEM $10 + LDdi, HL VID_WDTH VID_SCN * LDdi,
  BEGIN, A XORr, LD(BC)A, BC INCd, HL DECd, HLZ, Z? ^? BR ?JRi,
  BC POP, ;CODE
( ----- 098 )
: VID_OFS
  [ VID_WDTH 8 * LITN ] * + VD_FRB + [*TO] VD_OFS (vidclr) ;
: VID$ ( -- )
  1 [*TO] VD_DECFR 0 [*TO] VD_DECTL 0 [*TO] VD_CURCL 0
  [*TO] VD_FRMST [ VID_WDTH 1 - LITN ] [*TO] VD_COLS
  [ VID_LN 1 - LITN ] [*TO] VD_LINES
  [ VID_MEM $10 + LITN ] [*TO] VD_FRB 1 4 VID_OFS ;
( ----- 099 )
: COLS VD_COLS ;
: LINES VD_LINES ;
: VID_LOC VD_COLS /MOD
  [ VID_WDTH 8 * LITN ] * VD_OFS + ;
: CELL! VID_LOC + SWAP SPC - DUP 96 < IF
  DUP DUP << + << + ~FNT + 7 0 DO
  2DUP C@ >> SWAP C! 1+ SWAP
  [ VID_WDTH LITN ] + SWAP LOOP
  DROP 0 SWAP C! ELSE 2DROP THEN ;
( ----- 100 )
: VID_LCR VID_LOC SWAP DUP
  DUP 12 < IF DROP 0 ELSE 12 -
  DUP [ VID_WDTH 24 - LITN ] > IF DROP [ VID_WDTH 24 - LITN ]
  THEN THEN [*TO] VD_CURCL ;
: CURSOR! 0 SWAP VID_LOC + [ VID_WDTH 7 * LITN ] + C!
  255 SWAP VID_LCR + [ VID_WDTH 7 * LITN ] + C! ;
CODE (vidscr) BC PUSH, EXX,
 BC VID_SCN 8 - VID_WDTH * LDdi,
 DE VID_MEM $10 + LDdi,
 HL VID_MEM $10 + VID_WDTH 8 * + LDdi,
 LDIR, EXX, BC POP, ;CODE
: NEWLN DUP 1+ VD_LINES = IF (vidscr) ELSE 1+ THEN ;
( ----- 101 )
\ Stream video frames, single scan
CODE (vidfr) ( -- ) BC PUSH, EXX,
  C SPI_DATA LDri, DE VID_MEM $04 + LDd(i),
  HL VID_MEM 40 + VID_WDTH - LDdi, DE ADDHLd,
  VID_MEM $06 + LD(i)HL, DE VID_WDTH 24 - LDdi,
  B VID_SCN LDri,
  LSET L1 BEGIN,
    14 CTL8255 OUTii, DE ADDHLd, 15 CTL8255 OUTii,
    A B LDrr, 4 repeat NOP, 24 repeat OUTI,
    B A LDrr, BR DJNZi,
  B 0 LDri, B 0 LDri, B 0 LDri, B VID_VBL 1 - LDri, FJR JRi,
  LSET L2 A VID_VBL 1 - LDri, FJR JRi, FMARK FMARK
    A B LDrr, B 28 LDri, BEGIN, BR DJNZi, HL INCd, B A LDrr,
    15 CTL8255 OUTii, 5 repeat NOP, 14 CTL8255 OUTii,
  L2 BR DJNZi,
( ----- 102 )
  VID_MEM $02 + LDA(i), B A LDrr, VID_MEM LDA(i),
  B SUBr, IFNZ,
    VID_MEM LD(i)A, B 23 LDri, HL INCd, B 23 LDri,
    BEGIN, BR DJNZi,
    VID_MEM $06 + LDHL(i), B VID_SCN LDri, 15 CTL8255 OUTii,
    5 repeat NOP, 14 CTL8255 OUTii, L1 JMPi,
  THEN, EXX, BC POP, ;CODE
( ----- 103 )
\ Stream video frames, double scan
CODE (vidfr) ( -- ) BC PUSH, EXX,
  C SPI_DATA LDri, DE VID_MEM $04 + LDd(i),
  HL VID_MEM 40 + VID_WDTH - LDdi, DE ADDHLd,
  VID_MEM $06 + LD(i)HL, DE VID_WDTH 24 - LDdi, B VID_SCN LDri,
  LSET L1 BEGIN,
    14 CTL8255 OUTii, DE ADDHLd, 15 CTL8255 OUTii, A B LDrr,
    DE DECd, DE -25 LDdi, 24 repeat OUTI,
    AF PUSH, DE INCd, 14 CTL8255 OUTii, DE ADDHLd,
    15 CTL8255 OUTii, AF POP, DE VID_WDTH 24 - LDdi,
    24 repeat OUTI, B A LDrr, BR DJNZi,
  B 0 LDri, B 0 LDri, B 0 LDri, B VID_VBL 1 - LDri, FJR JRi,
  LSET L2 A VID_VBL 1 - LDri, FJR JRi, FMARK FMARK
    A B LDrr, B 28 LDri, BEGIN, BR DJNZi, HL INCd, B A LDrr,
    15 CTL8255 OUTii, 5 repeat NOP, 14 CTL8255 OUTii,
  L2 BR DJNZi,
( ----- 104 )
  VID_MEM $02 + LDA(i), B A LDrr, VID_MEM LDA(i), B SUBr, IFNZ,
    VID_MEM LD(i)A, B 23 LDri, HL INCd, B 23 LDri,
    BEGIN, BR DJNZi, VID_MEM $06 + LDHL(i), B VID_SCN LDri,
    15 CTL8255 OUTii, 5 repeat NOP, 14 CTL8255 OUTii, L1 JMPi,
  THEN, EXX, BC POP, ;CODE
( ----- 105 )
\ PS2 keyboard driver subsystem
PSK_MEM *VALUE PSK_STAT
PSK_MEM $02 + *VALUE PSK_CC
PSK_MEM $04 + *VALUE PSK_BUFI
PSK_MEM $06 + *VALUE PSK_BUFO
PSK_MEM $08 + VALUE PSK_BUF
PC ORG $39 + T! ( RST 38 )
DI, AF PUSH, $10 SIOA_CTL OUTii, SIOA_CTL INAi,
4 A BIT, IFZ, AF POP, EI, RETI, THEN, ( I1 - T1 )
PSK_MEM LDA(i), A ORr,
IFZ, PTB8255 INAi, 0 A BIT,         ( I1 - )
IFZ, A 1 LDri, PSK_MEM LD(i)A, THEN,  ( I2 - T2 )
( ----- 106 )
AF POP, EI, RETI, THEN,             ( - T1 )
$9 CPi, FJR JRNZi, TO L3
HL PUSH, PSK_MEM $02 + LDHL(i), H 8 LDri, A XORr,
BEGIN, L RRC, 0 ADCi, H DECr, BR JRNZi,
H A LDrr, PTB8255 INAi, H ADDr, $1 ANDi, FJR JRZi, TO L1
A XORr, VID_MEM LD(i)A, VID_MEM $02 + LD(i)A,
PSK_MEM $04 + LDA(i), L A LDrr, PSK_MEM $06 + LDA(i),
A INCr, PS2_BMSK ANDi, L CPr, FJR JRZi, TO L1
PSK_MEM $06 + LD(i)A, L A LDrr,
A PSK_MEM $08 + <<8 >>8 LDri, L ADDr, L A LDrr,
A PSK_MEM $08 + >>8 LDri, 0 ADCi,
( ----- 107 )
H A LDrr, PSK_MEM $02 + LDA(i), (HL) A LDrr,
L1 FMARK A XORr, PSK_MEM LD(i)A, HL POP, AF POP, EI, RETI,
L3 FMARK PTB8255 INAi, RRCA, PSK_MEM $02 + LDA(i),
RRA, PSK_MEM $02 + LD(i)A,
PSK_MEM LDA(i), A INCr, PSK_MEM LD(i)A,
AF POP, EI, RETI,
( ----- 108 )
CODE (pskset)
  DI, $11 SIOA_CTL OUTii, $19 SIOA_CTL OUTii, IM1, EI, ;CODE
: PSK< ( -- n )
  PSK_BUFI PSK_BUFO = IF 0 ELSE PSK_BUFI
  1+ [ PS2_BMSK LITN ] AND DUP PSK_BUF + C@
  SWAP [*TO] PSK_BUFI THEN ;
: PSKV< ( -- n )
  PSK_BUFI PSK_BUFO = IF
  BEGIN 1 [*TO] VD_DECFR (vidfr)
  PSK_BUFI PSK_BUFO = NOT UNTIL THEN
  PSK_BUFI 1+ [ PS2_BMSK LITN ] AND DUP
  PSK_BUF + C@ SWAP [*TO] PSK_BUFI ;
: PSK$ ( -- )
  0 [*TO] PSK_BUFO 0 [*TO] PSK_BUFI
  0 [*TO] PSK_STAT (pskset) ;
( ----- 109 )
: (ps2kc) 0 BEGIN DROP PSKV<
  DUP 5 = IF 0 [*TO] VD_CURCL DROP 0 THEN
  DUP 6 = IF VD_CURCL 4 < IF 0 ELSE VD_CURCL 4 - THEN
    [*TO] VD_CURCL DROP 0 THEN
  DUP 4 = IF VD_CURCL [ VID_WDTH 28 - LITN ] > IF
    [ VID_WDTH 24 - LITN ] ELSE VD_CURCL 4 + THEN
    [*TO] VD_CURCL DROP 0 THEN DUP UNTIL ;
( ----- 110 )
SMS PS/2 controller (doc/hw/z80/sms)

To assemble, load the AVR assembler with AVRA, then
"114 132 LOADR".

Receives keystrokes from PS/2 keyboard and send them to the
'164. On the PS/2 side, it works the same way as the controller
in the rc2014/ps2 recipe.  However, in this case, what we have
on the other side isn't a z80 bus, it's the one of the two
controller ports of the SMS through a DB9 connector.

The PS/2 related code is copied from rc2014/ps2 without much
change. The only differences are that it pushes its data to a
'164 instead of a '595 and that it synchronizes with the SMS
with a SR latch, so we don't need PCINT. We can also afford to
run at 1MHz instead of 8.                                  cont.
( ----- 111 )
Register Usage

GPIOR0 flags:
0 - when set, indicates that the DATA pin was high when we
    received a bit through INT0. When we receive a bit, we set
    flag T to indicate it.

R16: tmp stuff
R17: recv buffer. Whenever we receive a bit, we push it in
     there.
R18: recv step:
     - 0: idle
     - 1: receiving data
     - 2: awaiting parity bit
     - 3: awaiting stop bit                                cont.
( ----- 112 )
R19: Register used for parity computations and tmp value in
     some other places
R20: data being sent to the '164
Y: pointer to the memory location where the next scan code from
   ps/2 will be written.
Z: pointer to the next scan code to push to the 595
( ----- 114 )
18 VALUES SRAM_START $0060 RAMEND $015f SPL $3d SPH $3e
          GPIOR0 $11 MCUCR $35 TCCR0B $33 GIMSK $3b
          TIFR $38 TCNT0 $32 PINB $16 DDRB $17 PORTB $18
          CLK 2 DATA 1 CP 3 LQ 0 LR 4
$100 100 - VALUE TIMER_INITVAL
\ We need a lot of labels in this program...
5 VALUES L4 0 L5 0 L6 0 L7 0 L8 0
( ----- 115 )
HERE TO ORG
FLBL, L1 \ main
FLBL, L2 \ hdlINT0
\ Read DATA and set GPIOR0/0 if high. Then, set flag T.
\ no SREG fiddling because no SREG-modifying instruction
' RJMP L2 TO, \ hdlINT0
PINB DATA SBIC,
GPIOR0 0 SBI,
SET,
RETI,
( ----- 116 )
' RJMP L1 TO, \ main
R16 RAMEND <<8 >>8 LDI, SPL R16 OUT,
R16 RAMEND >>8 LDI, SPH R16 OUT,
R18 CLR, GPIOR0 R18 OUT, \ init variables
R16 $02 ( ISC01 ) LDI, MCUCR R16 OUT, \ INT0, falling edge
R16 $40 ( INT0 ) LDI, GIMSK R16 OUT, \ Enable INT0
YH CLR, YL SRAM_START LDI, \ Setup buffer
ZH CLR, ZL SRAM_START LDI,
\ Setup timer. We use the timer to clear up "processbit"
\ registers after 100us without a clock. This allows us to start
\ the next frame in a fresh state. at 1MHZ, no prescaling is
\ necessary. Each TCNT0 tick is already 1us long.
R16 $01 ( CS00 ) LDI, \ no prescaler
TCCR0B R16 OUT,
DDRB CP SBI, PORTB LR CBI, DDRB LR SBI, SEI,
( ----- 117 )
LBL! L1 \ loop
FLBL, L2 \ BRTS processbit. flag T set? we have a bit to process
YL ZL CP, \ if YL == ZL, buf is empty
FLBL, L3 \ BRNE sendTo164. YL != ZL? buf has data
\ nothing to do. Before looping, let's check if our
\ communication timer overflowed.
R16 TIFR IN,
R16 1 ( TOV0 ) SBRC,
FLBL, L4 \ RJMP processbitReset, timer0 overflow? reset
\ Nothing to do for real.
' RJMP L1 LBL, \ loop
( ----- 118 )
\ Process the data bit received in INT0 handler.
' BRTS L2 TO, \ processbit
R19 GPIOR0 IN, \ backup GPIOR0 before we reset T
R19 $1 ANDI, \ only keep the first flag
GPIOR0 0 CBI,
CLT, \ ready to receive another bit
\ We've received a bit. reset timer
FLBL, L2 \ RCALL resetTimer
\ Which step are we at?
R18 TST, FLBL, L5 \ BREQ processbits0
R18 1 CPI, FLBL, L6 \ BREQ processbits1
R18 2 CPI, FLBL, L7 \ BREQ processbits2
( ----- 119 )
\ step 3: stop bit
R18 CLR, \ happens in all cases
\ DATA has to be set
R19 TST, \ was DATA set?
' BREQ L1 LBL, \ loop, not set? error, don't push to buf
\ push r17 to the buffer
Y+ R17 ST,
FLBL, L8 \ RCALL checkBoundsY
' RJMP L1 LBL, \ loop
( ----- 120 )
' BREQ L5 TO, \ processbits0
\ step 0 - start bit
\ DATA has to be cleared
R19 TST, \ was DATA set?
' BRNE L1 LBL, \ loop. set? error. no need to do anything. keep
               \ r18 as-is.
\ DATA is cleared. prepare r17 and r18 for step 1
R18 INC,
R17 $80 LDI,
' RJMP L1 LBL, \ loop
( ----- 121 )
' BREQ L6 TO, \ processbits1
\ step 1 - receive bit
\ We're about to rotate the carry flag into r17. Let's set it
\ first depending on whether DATA is set.
CLC,
R19 0 SBRC, \ skip if DATA is cleared
SEC,
\ Carry flag is set
R17 ROR,
\ Good. now, are we finished rotating? If carry flag is set,
\ it means that we've rotated in 8 bits.
' BRCC L1 LBL, \ loop
\ We're finished, go to step 2
R18 INC,
' RJMP L1 LBL, \ loop
( ----- 122 )
' BREQ L7 TO, \ processbits2
\ step 2 - parity bit
R1 R19 MOV,
R19 R17 MOV,
FLBL, L5 \ RCALL checkParity
R1 R16 CP,
FLBL, L6 \ BRNE processBitError, r1 != r16? wrong parity
R18 INC,
' RJMP L1 LBL, \ loop
( ----- 123 )
' BRNE L6 TO, \ processBitError
R18 CLR,
R19 $fe LDI,
FLBL, L6 \ RCALL sendToPS2
' RJMP L1 LBL, \ loop

' RJMP L4 TO, \ processbitReset
R18 CLR,
FLBL, L4 \ RCALL resetTimer
' RJMP L1 LBL, \ loop
( ----- 124 )
' BRNE L3 TO, \ sendTo164
\ Send the value of r20 to the '164
PINB LQ SBIS, \ LQ is set? we can send the next byte
' RJMP L1 LBL, \ loop, even if we have something in the
               \ buffer, we can't: the SMS hasn't read our
               \ previous buffer yet.
\ We disable any interrupt handling during this routine.
\ Whatever it is, it has no meaning to us at this point in time
\ and processing it might mess things up.
CLI,
DDRB DATA SBI,
R20 Z+ LD,
FLBL, L3 \ RCALL checkBoundsZ
R16 R8 LDI,
( ----- 125 )
BEGIN,
    PORTB DATA CBI,
    R20 7 SBRC, \ if leftmost bit isn't cleared, set DATA high
    PORTB DATA SBI,
    \ toggle CP
    PORTB CP CBI, R20 LSL, PORTB CP SBI,
    R16 DEC,
' BRNE AGAIN?, \ not zero yet? loop
\ release PS/2
DDRB DATA CBI,
SEI,
\ Reset the latch to indicate that the next number is ready
PORTB LR SBI,
PORTB LR CBI,
' RJMP L1 LBL, \ loop
( ----- 126 )
' RCALL L2 TO, ' RCALL L4 TO, LBL! L2 \ resetTimer
R16 TIMER_INITVAL LDI,
TCNT0 R16 OUT,
R16 $02 ( TOV0 ) LDI,
TIFR R16 OUT,
RET,
( ----- 127 )
' RCALL L6 TO, \ sendToPS2
\ Send the value of r19 to the PS/2 keyboard
CLI,
\ First, indicate our request to send by holding both Clock low
\ for 100us, then pull Data low lines low for 100us.
PORTB CLK CBI,
DDRB CLK SBI,
' RCALL L2 LBL, \ resetTimer
\ Wait until the timer overflows
BEGIN, R16 TIFR IN, R16 1 ( TOV0 ) SBRS, AGAIN,
\ Good, 100us passed.
\ Pull Data low, that's our start bit.
PORTB DATA CBI,
DDRB DATA SBI,
( ----- 128 )
\ Now, let's release the clock. At the next raising edge, we'll
\ be expected to have set up our first bit (LSB). We set up
\ when CLK is low.
DDRB CLK CBI, \ Should be starting high now.
R16 8 LDI, \ We will do the next loop 8 times
R1 R19 MOV, \ Let's remember initial r19 for parity
BEGIN,
    BEGIN, PINB CLK SBIC, AGAIN, \ Wait for CLK to go low
    PORTB DATA CBI, \ set up DATA
    R19 0 SBRC, \ skip if LSB is clear
    PORTB DATA SBI,
    R19 LSR,
	\ Wait for CLK to go high
    BEGIN, PINB CLK SBIS, AGAIN,
    16 DEC,
' BRNE AGAIN?, \ not zero? loop
( ----- 129 )
\ Data was sent, CLK is high. Let's send parity
R19 R1 MOV, \ recall saved value
FLBL, L6 \ RCALL checkParity
BEGIN, PINB CLK SBIC, AGAIN, \ Wait for CLK to go low
\ set parity bit
PORTB DATA CBI,
R16 0 SBRC, \ parity bit in r16
PORTB DATA SBI,
BEGIN, PINB CLK SBIS, AGAIN, \ Wait for CLK to go high
BEGIN, PINB CLK SBIC, AGAIN, \ Wait for CLK to go low
\ We can now release the DATA line
DDRB DATA CBI,
\ Wait for DATA to go low, that's our ACK
BEGIN, PINB DATA SBIC, AGAIN,
BEGIN, PINB CLK SBIC, AGAIN, \ Wait for CLK to go low
( ----- 130 )
\ We're finished! Enable INT0, reset timer, everything back to
\ normal!
' RCALL L2 LBL, \ resetTimer
CLT, \ also, make sure T isn't mistakely set.
SEI,
RET,
( ----- 131 )
' RCALL L8 TO, \ checkBoundsY
\ Check that Y is within bounds, reset to SRAM_START if not.
YL TST,
IF, RET, ( not zero, nothing to do ) THEN,
\ YL is zero. Reset Z
YH CLR, YL SRAM_START <<8 >>8 LDI,
RET,
' RCALL L3 TO, \ checkBoundsZ
\ Check that Z is within bounds, reset to SRAM_START if not.
ZL TST,
IF, RET, ( not zero, nothing to do ) THEN,
\ ZL is zero. Reset Z
ZH CLR, ZL SRAM_START <<8 >>8 LDI,
RET,
( ----- 132 )
' RCALL L5 TO, ' RCALL L6 TO, \ checkParity
\ Counts the number of 1s in r19 and set r16 to 1 if there's an
\ even number of 1s, 0 if they're odd.
R16 1 LDI,
BEGIN,
    R19 LSR,
    ' BRCC SKIP, R16 INC, ( carry set? we had a 1 ) TO,
    R19 TST, \ is r19 zero yet?
' BRNE AGAIN?, \ no? loop
R16 $1 ANDI,
RET,
