( ----- 000 )
MASTER INDEX

002 Common assembler words    005 Z80 assembler
020 8086 assembler            030 AVR assembler
050 6809 assembler            65-99 unused
100 Block editor              115 Memory Editor
120 Useful little words
130-149 unused                150 Remote Shell
160 AVR SPI programmer        165 Sega ROM signer
170-199 unused                200 Cross compilation
206 Core words                230 BLK subsystem
240 Grid subsystem            245 PS/2 keyboard subsystem
250 SD Card subsystem         260 Fonts
300 Arch-specific content
( ----- 002 )
\ Common assembler words, low
3 VALUES ORG 0 BIN( 0 BIGEND? 0
: PC HERE ORG - BIN( + ;
: <<3 << << << ; : <<4 <<3 << ;
8 VALUES L1 0 L2 0 L3 0 lblnext 0 lblcell 0 lbldoes 0
  lblpush 0 lblxt 0
: |T L|M BIGEND? NOT IF SWAP THEN ;
: T! ( n a -- ) SWAP |T ROT C!+ C! ;
: T, ( n -- ) |T C, C, ;
: T@ C@+ SWAP C@ BIGEND? IF SWAP THEN <<8 OR ;
( ----- 005 )
\ Z80 Assembler. See doc/asm.txt
21 VALUES A 7 B 0 C 1 D 2 E 3 H 4 L 5 (HL) 6
          BC 0 DE 1 HL 2 AF 3 SP 3
          CNZ 0 CZ 1 CNC 2 CC 3 CPO 4 CPE 5 CP 6 CM 7
\ As a general rule, IX and IY are equivalent to spitting an
\ extra $dd / $fd and then spit the equivalent of HL
: IX $dd C, HL ; : IY $fd C, HL ;
: IX+ <<8 >>8 $dd C, (HL) ;
: IY+ <<8 >>8 $fd C, (HL) ;
: OPXY DOER , DOES> @ ( xyoff opref ) EXECUTE C, ;
( ----- 006 )
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
( ----- 007 )
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
' CPr, OPXY CP(IXY+),
( ----- 008 )
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
( ----- 009 )
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
$18 OP2i JRi,       $10 OP2i DJNZi,    $38 OP2i JRCi,
$30 OP2i JRNCi,     $28 OP2i JRZi,     $20 OP2i JRNZi,
( ----- 010 )
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
( ----- 011 )
: OP3i DOER C, DOES> C@ ( i op ) C, L, ;
$cd OP3i CALL,                $c3 OP3i JP,
$22 OP3i LD(i)HL,             $2a OP3i LDHL(i),
$32 OP3i LD(i)A,              $3a OP3i LDA(i),

: RST, $c7 OR C, ;
: JP(IX), IX DROP JP(HL), ;
: JP(IY), IY DROP JP(HL), ;
: JPc, SWAP <<3 $c2 OR C, L, ;
: CALLc, SWAP <<3 $c4 OR C, L, ;
( ----- 012 )
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
( ----- 020 )
\ 8086 assembler. See doc/asm.txt. B20-B29
28 VALUES AL 0 CL 1 DL 2 BL 3
          AH 4 CH 5 DH 6 BH 7
          AX 0 CX 1 DX 2 BX 3
          SP 4 BP 5 SI 6 DI 7
          ES 0 CS 1 SS 2 DS 3
          [BX+SI] 0 [BX+DI] 1 [BP+SI] 2 [BP+DI] 3
          [SI] 4 [DI] 5 [BP] 6 [BX] 7
( ----- 021 )
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
( ----- 022 )
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
( ----- 023 )
: OPrr DOER C, DOES> C@ C, <<3 OR $c0 OR C, ;
$31 OPrr XORxx,     $30 OPrr XORrr,
$88 OPrr MOVrr,     $89 OPrr MOVxx,    $28 OPrr SUBrr,
$29 OPrr SUBxx,     $08 OPrr ORrr,     $09 OPrr ORxx,
$38 OPrr CMPrr,     $39 OPrr CMPxx,    $00 OPrr ADDrr,
$01 OPrr ADDxx,     $20 OPrr ANDrr,    $21 OPrr ANDxx,
( ----- 024 )
: OPmodrm ( opbase modrmbase ) DOER C, C, DOES>
    @ L|M ( disp? modrm opoff modrmbase opbase ) ROT + C,
    ( disp? modrm modrmbase ) + DUP C, ( disp? modrm )
    $c0 AND DUP IF ( has disp ) $80 AND IF
        ( disp low+high ) L, ELSE ( low only ) C, THEN
    ELSE ( no disp ) DROP THEN ;
( -- disp? modrm opoff )
: [b] ( r/m ) 0 ; : [w] ( r/m ) 1 ;
: [b]+ ( r/m disp8 ) SWAP $40 OR 0 ; : [w]+ [b]+ 1+ ;
: r[] ( r r/m ) SWAP <<3 OR 2 ; : x[] r[] 1+ ;
: []r ( r/m r ) <<3 OR 0 ; : []x []r 1+ ;
: r[]+ ( r r/m disp8 )
    ROT <<3 ROT OR $40 OR 2 ; : x[]+ r[]+ 1+ ;
: []+r ( r/m disp8 r ) <<3 ROT OR $40 OR 0 ; : []+x []+r 1+ ;
( ----- 025 )
$fe 0 OPmodrm INC[],          $fe $8 OPmodrm DEC[],
$fe $30 OPmodrm PUSH[],       $8e 0 OPmodrm POP[],
$88 0 OPmodrm MOV[],          $38 0 OPmodrm CMP[],

: OPi DOER C, DOES> C@ C, C, ;
$04 OPi ADDALi,     $24 OPi ANDALi,    $2c OPi SUBALi,
$cd OPi INT,        $eb OPi JRi,       $74 OPi JRZi,
$75 OPi JRNZi,      $72 OPi JRCi,      $73 OPi JRNCi,
: OPI DOER C, DOES> C@ C, L, ;
$05 OPI ADDAXI,     $25 OPI ANDAXI,    $2d OPI SUBAXI,
$e9 OPI JMPi,       $e8 OPI CALLi,
( ----- 026 )
: CMPri, $80 C, SWAP $f8 OR C, C, ;
: CMPxI, $81 C, SWAP $f8 OR C, L, ;
: CMPxi, $83 C, SWAP $f8 OR C, C, ;
: MOVri, SWAP $b0 OR C, C, ; : MOVxI, SWAP $b8 OR C, L, ;
: MOVsx, $8e C, SWAP <<3 OR $c0 OR C, ;
: MOVrm, $8a C, SWAP <<3 $6 OR C, L, ;
: MOVxm, $8b C, SWAP <<3 $6 OR C, L, ;
: MOVmr, $88 C, <<3 $6 OR C, L, ;
: MOVmx, $89 C, <<3 $6 OR C, L, ;
: PUSHs, <<3 $06 OR C, ; : POPs, <<3 $07 OR C, ;
: SUBxi, $83 C, SWAP $e8 OR C, C, ;
: ADDxi, $83 C, SWAP $c0 OR C, C, ;
: JMPr, $ff C, 7 AND $e0 OR C, ;
: JMPf, ( seg off ) $ea C, L, L, ;
( ----- 030 )
\ AVR assembler. See doc/asm/avr.txt. B30-B40
\ We divide by 2 because each PC represents a word.
: PC HERE ORG - >> ;
: _oor ." arg out of range: " .X SPC> ." PC " PC .X NL> ABORT ;
: _r8c DUP 7 > IF _oor THEN ;
: _r32c DUP 31 > IF _oor THEN ;
: _r16+c _r32c DUP 16 < IF _oor THEN ;
: _r64c DUP 63 > IF _oor THEN ;
: _r256c DUP 255 > IF _oor THEN ;
: _Rdp ( op rd -- op', place Rd ) <<4 OR ;
: ATMEGA328P 45 LOAD ;
( ----- 031 )
( 0000 000d dddd 0000 )
: OPRd DOER , DOES> @ SWAP _r32c _Rdp L, ;
$9405 OPRd ASR,   $9400 OPRd COM,
$940a OPRd DEC,   $9403 OPRd INC,
$9206 OPRd LAC,   $9205 OPRd LAS,
$9207 OPRd LAT,
$9406 OPRd LSR,   $9401 OPRd NEG,
$900f OPRd POP,   $920f OPRd PUSH,
$9407 OPRd ROR,   $9402 OPRd SWAP,
$9204 OPRd XCH,
( ----- 032 )
( 0000 00rd dddd rrrr )
: OPRdRr DOER C, DOES> C@ ( rd rr op )
    OVER _r32c $10 AND >> >> >> OR ( rd rr op' )
    <<8 OR $ff0f AND ( rd op' )
    SWAP _r32c _Rdp L, ;
$1c OPRdRr ADC,   $0c OPRdRr ADD,    $20 OPRdRr AND,
$14 OPRdRr CP,    $04 OPRdRr CPC,    $10 OPRdRr CPSE,
$24 OPRdRr EOR,   $2c OPRdRr MOV,    $9c OPRdRr MUL,
$28 OPRdRr OR,    $08 OPRdRr SBC,    $18 OPRdRr SUB,

( 0000 0AAd dddd AAAA )
: OPRdA DOER C, DOES> C@ ( rd A op )
    OVER _r64c $30 AND >> >> >> OR ( rd A op' )
    <<8 OR $ff0f AND ( rd op' ) SWAP _r32c _Rdp L, ;
$b0 OPRdA IN,     $b8 OPRdA _ : OUT, SWAP _ ;
( ----- 033 )
( 0000 KKKK dddd KKKK )
: OPRdK DOER C, DOES> C@ ( rd K op )
    OVER _r256c $f0 AND >> >> >> >> OR ( rd K op' )
    ROT _r16+c <<4 ROT $0f AND OR ( op' rdK ) C, C, ;
$70 OPRdK ANDI,   $30 OPRdK CPI,     $e0 OPRdK LDI,
$60 OPRdK ORI,    $40 OPRdK SBCI,    $60 OPRdK SBR,
$50 OPRdK SUBI,

( 0000 0000 AAAA Abbb )
: OPAb DOER C, DOES> C@ ( A b op )
    ROT _r32c <<3 ROT _r8c OR C, C, ;
$98 OPAb CBI,     $9a OPAb SBI,      $99 OPAb SBIC,
$9b OPAb SBIS,
( ----- 034 )
: OPNA DOER , DOES> @ L, ;
$9598 OPNA BREAK, $9488 OPNA CLC,    $94d8 OPNA CLH,
$94f8 OPNA CLI,   $94a8 OPNA CLN,    $94c8 OPNA CLS,
$94e8 OPNA CLT,   $94b8 OPNA CLV,    $9498 OPNA CLZ,
$9419 OPNA EIJMP, $9509 OPNA ICALL,  $9519 OPNA EICALL,
$9409 OPNA IJMP,  $0000 OPNA NOP,    $9508 OPNA RET,
$9518 OPNA RETI,  $9408 OPNA SEC,    $9458 OPNA SEH,
$9478 OPNA SEI,   $9428 OPNA SEN,    $9448 OPNA SES,
$9468 OPNA SET,   $9438 OPNA SEV,    $9418 OPNA SEZ,
$9588 OPNA SLEEP, $95a8 OPNA WDR,
( ----- 035 )
( 0000 0000 0sss 0000 )
: OPb DOER , DOES> @ ( b op )
    SWAP _r8c _Rdp L, ;
$9488 OPb BCLR,   $9408 OPb BSET,

( 0000 000d dddd 0bbb )
: OPRdb DOER , DOES> @ ( rd b op )
    ROT _r32c _Rdp SWAP _r8c OR L, ;
$f800 OPRdb BLD,  $fa00 OPRdb BST,
$fc00 OPRdb SBRC, $fe00 OPRdb SBRS,

( special cases )
: CLR, DUP EOR, ;  : TST, DUP AND, ; : LSL, DUP ADD, ;
( ----- 036 )
( a -- k12, absolute addr a, relative to PC in a k12 addr )
: _r7ffc DUP $7ff > IF _oor THEN ;
: _raddr12
    PC - DUP 0< IF $800 + _r7ffc $800 OR ELSE _r7ffc THEN ;
: RJMP _raddr12 $c000 OR ;
: RCALL _raddr12 $d000 OR ;
: RJMP, RJMP L, ; : RCALL, RCALL L, ;
( ----- 037 )
( a -- k7, absolute addr a, relative to PC in a k7 addr )
: _r3fc DUP $3f > IF _oor THEN ;
: _raddr7
    PC - DUP 0< IF $40 + _r3fc $40 OR ELSE _r3fc THEN ;
: _brbx ( a b op -- a ) OR SWAP _raddr7 <<3 OR ;
: BRBC $f400 _brbx ; : BRBS $f000 _brbx ; : BRCC 0 BRBC ;
: BRCS 0 BRBS ; : BREQ 1 BRBS ; : BRNE 1 BRBC ; : BRGE 4 BRBC ;
: BRHC 5 BRBC ; : BRHS 5 BRBS ; : BRID 7 BRBC ; : BRIE 7 BRBS ;
: BRLO BRCS ; : BRLT 4 BRBS ; : BRMI 2 BRBS ; : BRPL 2 BRBC ;
: BRSH BRCC ; : BRTC 6 BRBC ; : BRTS 6 BRBS ; : BRVC 3 BRBC ;
: BRVS 3 BRBS ;
( ----- 038 )
9 VALUES X  $1c Y  $08 Z  0
         X+ $1d Y+ $19 Z+ $11
         -X $1e -Y $1a -Z $12
: _ldst ( Rd XYZ op ) SWAP DUP $10 AND <<8 SWAP $f AND
    OR OR ( Rd op' ) SWAP _Rdp L, ;
: LD, $8000 _ldst ; : ST, SWAP $8200 _ldst ;
( ----- 039 )
\ LBL! L1 .. L1 ' RJMP LBL,
: LBL! ( -- ) PC TO ;
: LBL, ( opw pc -- ) 1- SWAP EXECUTE L, ;
: SKIP, PC 0 L, ;
: TO, ( opw pc )
  \ warning: pc is a PC offset, not a mem addr!
  << ORG + PC 1- HERE ( opw addr tgt hbkp )
  ROT [*TO] HERE ( opw tgt hbkp )
  SWAP ROT EXECUTE HERE ! ( hbkp ) [*TO] HERE ;
\ FLBL, L1 .. ' RJMP L1 TO,
: FLBL, LBL! 0 L, ;
: BEGIN, PC ; : AGAIN?, ( pc op ) SWAP LBL, ;
: AGAIN, ['] RJMP AGAIN?, ;
: IF, ['] BREQ SKIP, ; : THEN, TO, ;
( ----- 040 )
\ Constant common to all AVR models
38 VALUES R0 0 R1 1 R2 2 R3 3 R4 4 R5 5 R6 6 R7 7 R8 8 R9 9
  R10 10 R11 11 R12 12 R13 13 R14 14 R15 15 R16 16 R17 17
  R18 18 R19 19 R20 20 R21 21 R22 22 R23 23 R24 24 R25 25
  R26 26 R27 27 R28 28 R29 29 R30 30 R31 31
  XL 26 XH 27 YL 28 YH 29 ZL 30 ZH 31
( ----- 045 )
( ATmega328P definitions ) 87 VALUES
UDR0 $c6 UBRR0L $c4 UBRR0H $c5 UCSR0C $c2 UCSR0B $c1 UCSR0A $c0
TWAMR $bd TWCR $bc TWDR $bb TWAR $ba TWSR $b9 TWBR $b8 ASSR $b6
OCR2B $b4 OCR2A $b3 TCNT2 $b2 TCCR2B $b1 TCCR2A $b0 OCR1BL $8a
OCR1BH $8b OCR1AL $88 OCR1AH $89 ICR1L $86 ICR1H $87 TCNT1L $84
TCNT1H $85 TCCR1C $82 TCCR1B $81 TCCR1A $80 DIDR1 $7f DIDR0 $7e
ADMUX $7c ADCSRB $7b ADCSRA $7a ADCH $79 ADCL $78 TIMSK2 $70
TIMSK1 $6f TIMSK0 $6e PCMSK1 $6c PCMSK2 $6d PCMSK0 $6b EICRA $69
PCICR $68 OSCCAL $66 PRR $64 CLKPR $61 WDTCSR $60 SREG $3f
SPL $3d SPH $3e SPMCSR $37 MCUCR $35 MCUSR $34 SMCR $33 ACSR $30
SPDR $2e SPSR $2d SPCR $2c GPIOR2 $2b GPIOR1 $2a OCR0B $28
OCR0A $27 TCNT0 $26 TCCR0B $25 TCCR0A $24 GTCCR $23 EEARH $22
EEARL $21 EEDR $20 EECR $1f GPIOR0 $1e EIMSK $1d EIFR $1c
PCIFR $1b TIFR2 $17 TIFR1 $16 TIFR0 $15 PORTD $0b DDRD $0a
PIND $09 PORTC $08 DDRC $07 PINC $06 PORTB $05 DDRB $04 PINB $03
( ----- 050 )
\ 6809 assembler. See doc/asm.txt. B50-B59
1 TO BIGEND?
\ For TFR/EXG
10 VALUES D 0 X 1 Y 2 U 3 S 4 PCR 5 A 8 B 9 CCR 10 DPR 11
( Addressing modes. output: n3? n2? n1 nc opoff )
: # ( n ) 1 0 ; ( Immediate )
: <> ( n ) 1 $10 ; ( Direct )
: () ( n ) L|M 2 $30 ; ( Extended )
: [] ( n ) L|M $9f 3 $20 ; ( Extended Indirect)
( Offset Indexed. We auto-detect 0, 5-bit, 8-bit, 16-bit )
: _0? ?DUP IF 1 ELSE $84 1 0 THEN ;
: _5? DUP $10 + $1f > IF 1 ELSE $1f AND 1 0 THEN ;
: _8? DUP $80 + $ff > IF 1 ELSE <<8 >>8 $88 2 0 THEN ;
: _16 L|M $89 3 ;
( ----- 051 )
: R+N DOER C, DOES> C@ ( roff ) >R
    _0? IF _5? IF _8? IF _16 THEN THEN THEN
    SWAP R> ( roff ) OR SWAP $20 ;
: R+K DOER C, DOES> C@ 1 $20 ;
: PCR+N ( n ) _8? IF _16 THEN SWAP $8c OR SWAP $20 ;
: [R+N] DOER C, DOES> C@ $10 OR ( roff ) >R
    _0? IF _8? IF _16 THEN THEN SWAP R> OR SWAP $20 ;
: [PCR+N] ( n ) _8? IF _16 THEN SWAP $9c OR SWAP $20 ;
0 R+N X+N   $20 R+N Y+N  $40 R+N U+N   $60 R+N S+N
: X+0 0 X+N ; : Y+0 0 Y+N ; : S+0 0 S+N ; : U+0 0 S+N ;
0 [R+N] [X+N] $20 [R+N] [Y+N]
$40 [R+N] [U+N] $60 [R+N] [S+N]
: [X+0] 0 [X+N] ; : [Y+0] 0 [Y+N] ;
: [S+0] 0 [S+N] ; : [U+0] 0 [U+N] ;
( ----- 052 )
$86 R+K X+A   $85 R+K X+B   $8b R+K X+D
$a6 R+K Y+A   $a5 R+K Y+B   $ab R+K Y+D
$c6 R+K U+A   $c5 R+K U+B   $cb R+K U+D
$e6 R+K S+A   $e5 R+K S+B   $eb R+K S+D
$96 R+K [X+A] $95 R+K [X+B] $9b R+K [X+D]
$b6 R+K [Y+A] $b5 R+K [Y+B] $bb R+K [Y+D]
$d6 R+K [U+A] $d5 R+K [U+B] $db R+K [U+D]
$f6 R+K [S+A] $f5 R+K [S+B] $fb R+K [S+D]
$80 R+K X+  $81 R+K X++  $82 R+K -X  $83 R+K --X
$a0 R+K Y+  $a1 R+K Y++  $a2 R+K -Y  $a3 R+K --Y
$c0 R+K U+  $c1 R+K U++  $c2 R+K -U  $c3 R+K --U
$e0 R+K S+  $e1 R+K S++  $e2 R+K -S  $e3 R+K --S
$91 R+K [X++] $93 R+K [--X] $b1 R+K [Y++] $b3 R+K [--Y]
$d1 R+K [U++] $d3 R+K [--U] $f1 R+K [S++] $f3 R+K [--S]
( ----- 053 )
: ,? DUP $ff > IF M, ELSE C, THEN ;
: ,N ( cnt ) 0 DO C, LOOP ;
: OPINH ( inherent ) DOER , DOES> @ ,? ;
( Targets A or B )
: OP1 DOER , DOES> @ ( n2? n1 nc opoff op ) + ,? ,N ;
( Targets D/X/Y/S/U. Same as OP1, but spit 2b immediate )
: OP2 DOER , DOES> @ OVER + ,? IF ,N ELSE DROP M, THEN ;
( Targets memory only. opoff scheme is different than OP1/2 )
: OPMT DOER , DOES> @
    SWAP $10 - ?DUP IF $50 + + THEN ,? ,N ;
( Targets 2 regs )
: OPRR ( src tgt -- ) DOER C, DOES> C@ C, SWAP <<4 OR C, ;
: OPBR ( op1 -- ) DOER C, DOES> ( off -- ) C@ C, C, ;
: OPLBR ( op? -- ) DOER , DOES> ( off -- ) @ ,? M, ;
( ----- 054 )
$89 OP1 ADCA,        $c9 OP1 ADCB,
$8b OP1 ADDA,        $cb OP1 ADDB,      $c3 OP2 ADDD,
$84 OP1 ANDA,        $c4 OP1 ANDB,      $1c OP1 ANDCC,
$48 OPINH ASLA,      $58 OPINH ASLB,    $08 OPMT ASL,
$47 OPINH ASRA,      $57 OPINH ASRB,    $07 OPMT ASR,
$4f OPINH CLRA,      $5f OPINH CLRB,    $0f OPMT CLR,
$81 OP1 CMPA,        $c1 OP1 CMPB,      $1083 OP2 CMPD,
$118c OP2 CMPS,      $1183 OP2 CMPU,    $8c OP2 CMPX,
$108c OP2 CMPY,
$43 OPINH COMA,      $53 OPINH COMB,    $03 OPMT COM,
$3c OP1 CWAI,        $19 OPINH DAA,
$4a OPINH DECA,      $5a OPINH DECB,    $0a OPMT DEC,
$88 OP1 EORA,        $c8 OP1 EORB,      $1e OPRR EXG,
$4c OPINH INCA,      $5c OPINH INCB,    $0c OPMT INC,
$0e OPMT JMP,        $8d OP1 JSR,
( ----- 055 )
$86 OP1 LDA,         $c6 OP1 LDB,       $cc OP2 LDD,
$10ce OP2 LDS,       $ce OP2 LDU,       $8e OP2 LDX,
$108e OP2 LDY,
$12 OP1 LEAS,        $13 OP1 LEAU,      $10 OP1 LEAX,
$11 OP1 LEAY,
$48 OPINH LSLA,      $58 OPINH LSLB,    $08 OPMT LSL,
$44 OPINH LSRA,      $54 OPINH LSRB,    $04 OPMT LSR,
$3d OPINH MUL,
$40 OPINH NEGA,      $50 OPINH NEGB,    $00 OPMT NEG,
$12 OPINH NOP,
$8a OP1 ORA,         $ca OP1 ORB,       $1a OP1 ORCC,
$49 OPINH ROLA,      $59 OPINH ROLB,    $09 OPMT ROL,
$46 OPINH RORA,      $56 OPINH RORB,    $06 OPMT ROR,
$3b OPINH RTI,       $39 OPINH RTS,
$82 OP1 SBCA,        $c2 OP1 SBCB,
$1d OPINH SEX,
( ----- 056 )
$87 OP1 STA,         $c7 OP1 STB,       $cd OP2 STD,
$10cf OP2 STS,       $cf OP2 STU,       $8f OP2 STX,
$108f OP2 STY,
$80 OP1 SUBA,        $c0 OP1 SUBB,      $83 OP2 SUBD,
$3f OPINH SWI,       $103f OPINH SWI2,  $113f OPINH SWI3,
$13 OPINH SYNC,      $1f OPRR TFR,
$4d OPINH TSTA,      $5d OPINH TSTB,    $0d OPMT TST,

$24 OPBR BCCi,       $1024 OPLBR LBCCi, $25 OPBR BCSi,
$1025 OPLBR LBCSi,   $27 OPBR BEQi,     $1027 OPLBR LBEQi,
$2c OPBR BGEi,       $102c OPLBR LBGEi, $2e OPBR BGTi,
$102e OPLBR LBGTi,   $22 OPBR BHIi,     $1022 OPLBR LBHIi,
$24 OPBR BHSi,       $1024 OPLBR LBHSi, $2f OPBR BLEi,
$102f OPLBR LBLEi,   $25 OPBR BLOi,     $1025 OPLBR LBLOi,
$23 OPBR BLSi,       $1023 OPLBR LBLSi, $2d OPBR BLTi,
$102d OPLBR LBLTi,   $2b OPBR BMIi,     $102b OPLBR LBMIi,
( ----- 057 )
$26 OPBR BNEi,       $1026 OPLBR LBNEi, $2a OPBR BPLi,
$102a OPLBR LBPLi,   $20 OPBR BRAi,     $16 OPLBR LBRAi,
$21 OPBR BRNi,       $1021 OPLBR BRNi,  $8d OPBR BSRi,
$17 OPLBR LBSRi,     $28 OPBR BVCi,     $1028 OPLBR LBVCi,
$29 OPBR BVSi,       $1029 OPLBR LBVSi,

: _ ( r c cref mask -- r c ) ROT> OVER = ( r mask c f )
    IF ROT> OR SWAP ELSE NIP THEN ;
: OPP DOER C, DOES> C@ C, 0 TOWORD BEGIN ( r c )
    '$' $80 _ 'S' $40 _ 'U' $40 _ 'Y' $20 _ 'X' $10 _
    '%' $08 _ 'B' $04 _ 'A' $02 _ 'C' $01 _ 'D' $06 _
    '@' $ff _ DROP IN< DUP WS? UNTIL DROP C, ;
$34 OPP PSHS, $36 OPP PSHU, $35 OPP PULS, $37 OPP PULU,
( ----- 100 )
\ Block editor. see doc/ed.txt. B100-B111
\ Cursor position in buffer. EDPOS/64 is line number
0 VALUE EDPOS
CREATE IBUF LNSZ 1+ ALLOT0 \ counted string, first byte is len
CREATE FBUF LNSZ 1+ ALLOT0
: L BLK> ." Block " DUP . NL> LIST ;
: B BLK> 1- BLK@ L ; : N BLK> 1+ BLK@ L ;
: IBUF+ IBUF 1+ ; : FBUF+ FBUF 1+ ;
: ILEN IBUF C@ ; : FLEN FBUF C@ ;
: EDPOS! [TO] EDPOS ; : EDPOS+! EDPOS + EDPOS! ;
: 'pos ( pos -- a, addr of pos in memory ) BLK( + ;
: 'EDPOS EDPOS 'pos ;
( ----- 101 )
\ Block editor, private helpers
: _lpos ( ln -- a ) LNSZ * 'pos ;
: _pln ( ln -- ) \ print line no ln with pos caret
  DUP _lpos DUP LNLEN RANGE DO ( lno )
    I 'EDPOS = IF '^' EMIT THEN
    I C@ DUP SPC < IF DROP SPC THEN EMIT
  LOOP ( lno ) SPC> 1+ . ;
: _zline ( a -- ) LNSZ SPC FILL ; \ zero-out a line
: _zbuf ( buf -- ) 0 SWAP C!+ _zline ; \ zero-out a buf
: _type ( buf -- ) \ type into buf until CR
  IN< DUP CR = IF 2DROP EXIT THEN OVER _zbuf ( buf c )
  OVER 1+ LNSZ RANGE DO ( buf c )
    I C! ( buf ) DUP C@ 1+ OVER C! ( inc len )
    IN< DUP CR = IF LEAVE THEN LOOP 2DROP ;
( ----- 102 )
\ Block editor, T P U
\ user-facing lines are 1-based
: T 1- DUP LNSZ * EDPOS! _pln ;
: P IBUF _type IBUF+ 'EDPOS LNSZ MOVE BLK!! ;
: _mvln+ ( ln -- move ln 1 line down )
    DUP 14 > IF DROP EXIT THEN
    _lpos DUP LNSZ + LNSZ MOVE ;
: _mvln- ( ln -- move ln 1 line up )
    DUP 14 > IF DROP 15 _lpos _zline
    ELSE 1+ _lpos DUP LNSZ - LNSZ MOVE THEN ;
: _U ( U without P, used in VE )
  15 EDPOS LNSZ / - ?DUP IF 0 DO 14 I - _mvln+ LOOP THEN ;
: U _U P ;
( ----- 103 )
\ Block editor, F i
: _F ( F without _type and _pln. used in VE )
  BLK) 'EDPOS 1+ DO
    I FBUF+ FLEN []= IF I BLK( - EDPOS! LEAVE THEN LOOP ;
: F FBUF _type _F EDPOS LNSZ / _pln ;
: _rbufsz ( size of linebuf to the right of curpos )
  EDPOS LNSZ MOD LNSZ -^ ;
: _i ( i without _pln and _type. used in VE )
  _rbufsz ILEN OVER < IF ( rsize )
    ILEN - ( chars-to-move )
    'EDPOS DUP ILEN + ROT ( a a+ilen ctm ) MOVE- ILEN
  THEN ( len-to-insert )
  IBUF+ 'EDPOS ROT MOVE ( ilen ) BLK!! ;
: i IBUF _type _i EDPOS LNSZ / _pln ;
( ----- 104 )
\ Block editor, X E Y
: icpy ( n -- copy n chars from cursor to IBUF )
  DUP IBUF C! IBUF+ _zline 'EDPOS IBUF+ ( n a buf ) ROT MOVE ;
: _X ( n -- )
  ?DUP NOT IF EXIT THEN
  _rbufsz MIN DUP icpy 'EDPOS 2DUP + ( n a1 a1+n )
  SWAP _rbufsz MOVE ( n )
  \ get to next line - n
  DUP EDPOS $ffc0 AND $40 + -^ 'pos ( n a )
  SWAP SPC FILL BLK!! ;
: X _X EDPOS LNSZ / _pln ;
: _E FLEN _X ;
: E FLEN X ;
: Y FBUF IBUF LNSZ 1+ MOVE ;
( ----- 105 )
\ Visual text editor. VALUEs, large? width pos@ mode! ...
CREATE CMD '%' C, 0 C,
4 VALUES PREVPOS 0 PREVBLK 0 xoff 0 ACC 0
LNSZ 3 + VALUE MAXW
: large? COLS MAXW > ; : col- MAXW COLS MIN -^ ;
: width large? IF LNSZ ELSE COLS THEN ;
: acc@ ACC 1 MAX ; : pos@ ( x y -- ) EDPOS LNSZ /MOD ;
: num ( c -- ) \ c is in range 0-9
  '0' - ACC 10 * + [TO] ACC ;
: mode! ( c -- ) 4 col- CELL! ;
( ----- 106 )
\ VE, rfshln contents selblk pos! xoff? setpos
: _ ( ln -- ) \ refresh line ln
  DUP _lpos xoff + SWAP 3 + COLS * large? IF 3 + THEN
  width CELLS! ;
: rfshln pos@ NIP _ ; \ refresh active line
: contents 16 0 DO I _ LOOP ;
: selblk BLK> [TO] PREVBLK BLK@ contents ;
: pos! ( newpos -- ) EDPOS [TO] PREVPOS
    DUP 0< IF DROP 0 THEN 1023 MIN EDPOS! ;
: xoff? pos@ DROP ( x )
  xoff ?DUP IF < IF 0 [TO] xoff contents THEN ELSE
    width >= IF LNSZ COLS - [TO] xoff contents THEN THEN ;
: setpos ( -- ) pos@ 3 + ( header ) SWAP ( y x ) xoff -
  large? IF 3 + ( gutter ) THEN SWAP AT-XY ;
( ----- 107 )
\ VE, cmv buftype bufprint bufs
: cmv ( n -- , char movement ) acc@ * EDPOS + pos! ;
: buftype ( buf ln -- )
  3 OVER AT-XY KEY DUP SPC < IF 2DROP DROP EXIT THEN ( b ln c )
  SWAP COLS * 3 + 3 col- nspcs ( buf c )
  IN( SWAP LNTYPE DROP BEGIN ( buf a ) KEY LNTYPE UNTIL
  IN( - ( buf len ) SWAP C!+ IN( SWAP LNSZ MOVE IN$ ;
: bufprint ( buf pos -- )
  DUP LNSZ nspcs OVER C@ ROT 1+ ROT> CELLS! ;
: bufs ( -- )
  COLS ( pos ) 'I' OVER CELL! ':' OVER 1+ CELL! ( pos )
  IBUF OVER 3 + bufprint ( pos )
  << 'F' OVER CELL! ':' OVER 1+ CELL! ( pos )
  FBUF SWAP 3 + bufprint ;
( ----- 108 )
\ VE cmds: G [ ] t I F Y X h H L g @ !
: %G ACC selblk ;
: %[ BLK> acc@ - selblk ; : %] BLK> acc@ + selblk ;
: %t PREVBLK selblk ;
: %I 'I' mode! IBUF 1 buftype _i bufs rfshln ;
: %F 'F' mode! FBUF 2 buftype _F bufs setpos ;
: %Y Y bufs ; : %E _E bufs rfshln ;
: %X acc@ _X bufs rfshln ;
: %h -1 cmv ; : %l 1 cmv ; : %k -64 cmv ; : %j 64 cmv ;
: %H EDPOS $3c0 AND pos! ;
: %L EDPOS DUP $3f OR 2DUP = IF 2DROP EXIT THEN SWAP BEGIN
    ( res p ) 1+ DUP 'pos C@ WS? NOT IF NIP DUP 1+ SWAP THEN
    DUP $3f AND $3f = UNTIL DROP pos! ;
: %g ACC 1 MAX 1- 64 * pos! ;
: %@ BLK> BLK( (blk@) 0 [*TO] BLKDTY contents ;
: %! BLK> FLUSH [*TO] BLK> ;
( ----- 109 )
\ VE cmds: w W b B
' NOOP ALIAS C@+-
: C@- ( a -- a-1 c ) DUP C@ SWAP 1- SWAP ;
: go>> ['] C@+ [TO] C@+- ;
: go<< ['] C@- [TO] C@+- ;
: word>> BEGIN C@+- WS? UNTIL ;
: ws>> BEGIN C@+- WS? NOT UNTIL ;
: bpos! BLK( - pos! ;
: %w go>> 'EDPOS acc@ 0 DO word>> ws>> LOOP 1- bpos! ;
: %W go>> 'EDPOS acc@ 0 DO 1+ ws>> word>> LOOP 2 - bpos! ;
: %b go<< 'EDPOS acc@ 0 DO 1- ws>> word>> LOOP 2 + bpos! ;
: %B go<< 'EDPOS acc@ 0 DO word>> ws>> LOOP 1+ bpos! ;
( ----- 110 )
\ VE cmds: f R O o D
: %f EDPOS PREVPOS 2DUP = IF 2DROP EXIT THEN
  2DUP > IF DUP pos! SWAP THEN
  ( p1 p2, p1 < p2 ) OVER - LNSZ MIN ( pos len ) DUP FBUF C!
  FBUF+ _zline SWAP 'pos FBUF+ ( len src dst ) ROT MOVE bufs ;
: %R ( replace mode )
  'R' mode!
  BEGIN setpos KEY DUP BS? IF -1 EDPOS+! DROP 0 THEN
    DUP SPC >= IF
    DUP EMIT 'EDPOS C! 1 EDPOS+! BLK!! 0
  THEN UNTIL ;
: %O _U EDPOS $3c0 AND DUP pos! 'pos _zline BLK!! contents ;
: %o EDPOS $3c0 < IF EDPOS 64 + EDPOS! %O THEN ;
: %D %H LNSZ icpy
    acc@ 0 DO 16 EDPOS LNSZ / DO I _mvln- LOOP LOOP
    BLK!! bufs contents ;
( ----- 111 )
\ VE final: status nums gutter handle VE
: status 0 $20 nspcs 0 0 AT-XY ." BLK" SPC> BLK> . SPC> ACC .
  SPC> pos@ . ',' EMIT . xoff IF '>' EMIT THEN SPC>
  BLKDTY IF '*' EMIT THEN SPC mode! ;
: nums 17 1 DO 0 2 I + AT-XY I . LOOP ;
: gutter large? IF 19 0 DO '|' I COLS * MAXW + CELL! LOOP THEN ;
: handle ( c -- f )
  DUP '0' '9' =><= IF num 0 EXIT THEN
  DUP CMD 1+ C! CMD 2 FIND IF EXECUTE THEN
  0 [TO] ACC 'q' = ;
: VE
  BLK> 0< IF 0 BLK@ THEN
  clrscr 0 [TO] ACC 0 [TO] PREVPOS
  nums bufs contents gutter
  BEGIN xoff? status setpos KEY handle UNTIL 0 19 AT-XY ;
( ----- 115 )
\ Memory Editor. See doc/me.txt. B115-119
CREATE CMD '#' C, 0 C, \ not same prefix as VE
CREATE BUF '$' C, 4 ALLOT \ always hex
\ POS is relative to ADDR
5 VALUES ADDR 0 POS 0 AWIDTH 16 HALT? 0 ASCII? 0
LINES 2 - VALUE AHEIGHT
AHEIGHT AWIDTH * VALUE PAGE
COLS 33 < [IF] 8 TO AWIDTH [THEN]
: _ ( n -- c ) DUP 9 > IF [ 'a' 10 - LITN ] ELSE '0' THEN + ;
: _p ( c -- n ) '0' - DUP 9 > IF $df AND 'A' '0' - - DUP 6 < IF
    10 + ELSE DROP $100 THEN THEN ;
: addr ADDR POS + ;
: hex! ( c pos -- )
  OVER 16 / _ OVER CELL! ( c pos ) 1+ SWAP $f AND _ SWAP CELL! ;
: bottom 0 LINES 1- AT-XY ;
( ----- 116 )
\ Memory Editor, line rfshln contents showpos
: line ( ln -- )
  DUP AWIDTH * ADDR + SWAP 1+ COLS * ( a pos )
  ':' OVER CELL! OVER <<8 >>8 OVER 1+ hex! 4 + ( a pos+4 )
  AWIDTH >> 0 DO ( a pos )
    OVER I << + C@+ ( a pos a+1 c ) ROT TUCK hex! ( a a+1 pos )
    2 + SWAP C@ OVER hex! 3 + ( a pos+5 ) LOOP
  SWAP AWIDTH RANGE DO ( pos )
    I C@ DUP SPC < IF DROP '.' THEN OVER CELL! 1+ LOOP DROP ;
: rfshln POS AWIDTH / line ;
: contents LINES 2 - 0 DO I line LOOP ;
: showpos
  POS AWIDTH /MOD ( r q ) 1+ SWAP ( y r ) ASCII? IF
  AWIDTH >> 5 * + ELSE DUP 1 AND << SWAP >> 5 * + THEN
  4 + ( y x ) SWAP AT-XY ;
( ----- 117 )
\ Memory Editor, addr! pos! status type typep
: addr! $fff0 AND [TO] ADDR contents ;
: pos! DUP 0< IF PAGE + THEN DUP PAGE >= IF PAGE - THEN
  [TO] POS showpos ;
: status 0 COLS nspcs
  0 0 AT-XY ." A: " ADDR .X SPC> ." C: " POS .X SPC> ." S: "
  PSDUMP POS pos! ;
: type ( cnt -- sa sl ) DUP ( cnt ) 0 DO ( cnt )
  KEY DUP SPC < IF DROP I LEAVE ELSE DUP EMIT BUF 1+ I + C! THEN
  LOOP BUF SWAP 1+ ;
: typep ( cnt -- n? f )
  type ( sa sl ) DUP IF PARSE ELSE NIP THEN ;
( ----- 118 )
\ Memory Editor, almost all actions
: #] ADDR PAGE + addr! ; : #[ ADDR PAGE - addr! ;
: #J ADDR $10 + addr! POS $10 - pos! ;
: #K ADDR $10 - addr! POS $10 + pos! ;
: #l POS 1+ pos! ; : #h POS 1- pos! ;
: #j POS AWIDTH + pos! ; : #k POS AWIDTH - pos! ;
: #m addr ; : #@ addr @ ; : #! addr ! contents ;
: #g SCNT IF DUP ADDR - PAGE < IF
  ADDR - pos! ELSE DUP addr! $f AND pos! THEN THEN ;
: #G bottom 4 typep IF #g THEN ;
: #a ASCII? NOT [TO] ASCII? showpos ;
: #f #@ #g ; : #e #m #f ;
: _h SPC> showpos 2 typep ;
: _a showpos KEY DUP SPC < IF DROP 0 ELSE DUP EMIT 1 THEN ;
: #R BEGIN SPC> ASCII? IF _a ELSE _h THEN ( n? f ) IF
    addr C! rfshln #l 0 ELSE 1 THEN UNTIL rfshln ;
( ----- 119 )
\ Memory Editor, #q handle ME
: #q 1 [TO] HALT? ;
: handle ( c -- f )
  CMD 1+ C! CMD 2 FIND IF EXECUTE THEN ;
: ME clrscr contents 0 pos! BEGIN
    status KEY handle HALT? UNTIL bottom ;
( ----- 120 )
\ Useful little words. nC, MIN MAX MOVE-
\ parse the next n words and write them as chars
: nC, ( n -- ) 0 DO RUN1 C, LOOP ;
: MIN ( n n - n ) 2DUP > IF SWAP THEN DROP ;
: MAX ( n n - n ) 2DUP < IF SWAP THEN DROP ;
\ Compute CRC16 over a memory range
: CRC16[] ( a u -- c ) 0 ROT> RANGE DO I C@ CRC16 LOOP ;
: MOVE- ( a1 a2 u -- ) \ MOVE starting from the end
  ?DUP IF 1- TUCK + ( a1 u-1 a2+ ) ROT> TUCK + ( a2+ u-1 a1+ )
    SWAP 1+ ( u ) 0 DO ( a2 a1 )
      DUP C@ ROT> 1- ( c a2 a1- ) ROT> TUCK C! 1- SWAP
  LOOP THEN 2DROP ;
( ----- 121 )
\ Useful little words. MEM>BLK BLK>MEM
\ Copy an area of memory into blocks.
: MEM>BLK ( addr blkno blkcnt )
  ( bcnt ) 0 DO ( a bno )
    DUP I + BLK@ OVER I $400 * + ( a bno a' )
    BLK( $400 MOVE BLK!! LOOP ( a bno ) 2DROP FLUSH ;
\ Copy subsequent blocks in an area of memory
: BLK>MEM ( blkno blkcnt addr )
  ROT> RANGE DO ( a )
    I BLK@ BLK( OVER $400 MOVE $400 + LOOP DROP ;
( ----- 122 )
\ Context. Allows multiple concurrent dictionaries.
\ See doc/usage.txt

0 VALUE saveto \ where to save CURRENT in next switch
: context DOER CURRENT , DOES> ( a -- )
  saveto IF CURRENT [*TO] saveto THEN ( a )
  DUP [TO] saveto ( a )
  @ [*TO] CURRENT ;
( ----- 123 )
\ Grid applications helper words. nspcs clrscr
: nspcs ( pos n ) RANGE DO SPC I CELL! LOOP ;
: clrscr 0 COLS LINES * nspcs ;
( ----- 150 )
( Remote Shell. load range B150-B154 )
: _<< ( print everything available from RX<? )
  BEGIN RX<? IF EMIT ELSE EXIT THEN AGAIN ;
: _<<r ( _<< with retries )
  BEGIN _<< 100 TICKS RX<? IF EMIT ELSE EXIT THEN AGAIN ;
: RX< BEGIN RX<? UNTIL ;
: _<<1r RX< EMIT _<<r ;
: rsh BEGIN
  KEY? IF DUP EOT = IF DROP EXIT ELSE TX> THEN THEN _<< AGAIN ;
: rstype ( sa sl --, like STYPE, but remote )
  ( sl ) 0 DO C@+ TX> _<<r LOOP DROP _<<r CR TX> RX< DROP _<<r ;
: rstypep ( like rstype, but read ok prompt )
    rstype BEGIN RX< WS? NOT UNTIL _<<1r ;
( ----- 151 )
: unpack DUP $f0 OR SWAP $0f OR ;
: out unpack TX> TX> ; : out2 L|M out out ;
: rupload ( loca rema u -- )
  LIT" : in KEY $f0 AND KEY $0f AND OR ;" rstypep
  LIT" : in2 in <<8 in OR ;" rstypep
  \ sig: chk -- chk, a and then u are KEYed in
  LIT" : _ in2 in2 RANGE DO in TUCK + SWAP I C! LOOP ;" rstypep
  DUP ROT ( loca u u rema ) LIT" 0 _" rstype out2 out2
  OVER + SWAP 0 ROT> ( 0 loca+u loca )
  DO '.' EMIT I C@ DUP ROT + SWAP out LOOP
  _<<1r LIT" .X FORGET in" rstypep .X ;
( ----- 152 )
( XMODEM routines )
: _<<s BEGIN RX<? IF DROP ELSE EXIT THEN AGAIN ;
: _rx>mem1 ( addr -- f, Receive single packet, f=eot )
  RX< 1 = NOT IF ( EOT ) $6 ( ACK ) TX> 1 EXIT THEN
  '.' EMIT RX< DROP RX< DROP ( packet num )
  0 ( addr crc ) SWAP 128 RANGE DO ( crc )
    RX< DUP ( crc n n ) I C! ( crc n ) CRC16 LOOP
  RX< <<8 RX< OR ( sender's CRC )
  = IF $6 ( ACK ) ELSE $15 'N' EMIT ( NACK ) THEN TX> 0 ;
: RX>MEM ( addr --, Receive packets into addr until EOT )
  _<<s 'C' TX> BEGIN ( a )
  DUP _rx>mem1 SWAP 128 + SWAP UNTIL DROP ;
: RX>BLK ( -- )
  _<<s 'C' TX> BLK( BEGIN ( a )
  DUP BLK) = IF DROP BLK( BLK! BLK> 1+ [*TO] BLK> THEN
  DUP _rx>mem1 SWAP 128 + SWAP UNTIL 2DROP ;
( ----- 153 )
: _snd128 ( a -- a )
    0 ( a crc ) 128 0 DO ( a crc )
      OVER I + C@ DUP TX> ( a crc n ) CRC16 ( a crc ) LOOP
    L|M TX> TX> ( a ) ;
: _ack? 0 BEGIN DROP RX< DUP 'C' = NOT UNTIL
	DUP $06 ( ACK ) = IF DROP 1
    ELSE $15 = NOT IF ABORT" out of sync" THEN 0 THEN ;
: _waitC
  ." Waiting for C..." BEGIN RX<? IF 'C' = ELSE 0 THEN UNTIL ;
: _mem>tx ( addr pktstart pktend -- ) SWAP DO ( a )
    'P' EMIT I . SPC> $01 ( SOH ) TX>
    I 1+ ( pkt start at 1 ) DUP TX> $ff -^ TX>
    _snd128 _ack? IF 128 + ( a+128 ) ELSE R> 1- >R THEN
  LOOP DROP ;
( ----- 154 )
: MEM>TX ( a u -- Send u bytes to TX )
  _waitC 128 /MOD SWAP IF 1+ THEN ( pktcnt ) 0 SWAP _mem>tx
  $4 ( EOT ) TX> RX< DROP ;
: BLK>TX ( b1 b2 -- )
  _waitC OVER - ( cnt ) 0 DO ( b1 )
    'B' EMIT DUP I + DUP . SPC> BLK@ BLK(
    I 8 * DUP 8 + ( a pktstart pktend ) _mem>tx
  LOOP DROP
  $4 ( EOT ) TX> RX< DROP ;
( ----- 160 )
\ AVR Programmer, B160-B163. doc/avr.txt
\ page size in words, 64 is default on atmega328P
64 VALUE aspfpgsz
0 VALUE aspprevx
: _x ( a -- b ) DUP [TO] aspprevx (spix) ;
: _xc ( a -- b ) DUP (spix) ( a b )
    DUP aspprevx = NOT IF ABORT" AVR err" THEN ( a b )
    SWAP [TO] aspprevx ( b ) ;
: _cmd ( b4 b3 b2 b1 -- r4 ) _xc DROP _xc DROP _xc DROP _x ;
: asprdy ( -- ) BEGIN 0 0 0 $f0 _cmd 1 AND NOT UNTIL ;
: asp$ ( spidevid -- )
    ( RESET pulse ) DUP (spie) 0 (spie) (spie)
    ( wait >20ms ) 220 TICKS
    ( enable prog ) $ac (spix) DROP
    $53 _x DROP 0 _xc DROP 0 _x DROP ;
: asperase 0 0 $80 $ac _cmd asprdy ;
( ----- 161 )
( fuse access. read/write one byte at a time )
: aspfl@ ( -- lfuse ) 0 0 0 $50 _cmd ;
: aspfh@ ( -- hfuse ) 0 0 $08 $58 _cmd ;
: aspfe@ ( -- efuse ) 0 0 $00 $58 _cmd ;
: aspfl! ( lfuse -- ) 0 $a0 $ac _cmd ;
: aspfh! ( hfuse -- ) 0 $a8 $ac _cmd ;
: aspfe! ( efuse -- ) 0 $a4 $ac _cmd ;
( ----- 162 )
: aspfb! ( n a --, write word n to flash buffer addr a )
    SWAP L|M SWAP ( a hi lo ) ROT ( hi lo a )
    DUP ROT ( hi a a lo ) SWAP ( hi a lo a )
    0 $40 ( hi a lo a 0 $40 ) _cmd DROP ( hi a )
    0 $48 _cmd DROP ;
: aspfp! ( page --, write buffer to page )
    0 SWAP aspfpgsz * L|M ( 0 lsb msb )
    $4c _cmd DROP asprdy ;
: aspf@ ( page a -- n, read word from flash )
    SWAP aspfpgsz * OR ( addr ) L|M ( lsb msb )
    2DUP 0 ROT> ( lsb msb 0 lsb msb )
    $20 _cmd ( lsb msb low )
    ROT> 0 ROT> ( low 0 lsb msb ) $28 _cmd <<8 OR ;
( ----- 163 )
: aspe@ ( addr -- byte, read from EEPROM )
    0 SWAP L|M SWAP ( 0 msb lsb )
    $a0 ( 0 msb lsb $a0 ) _cmd ;
: aspe! ( byte addr --, write to EEPROM )
    L|M SWAP ( b msb lsb )
    $c0 ( b msb lsb $c0 ) _cmd DROP asprdy ;
( ----- 165 )
( Sega ROM signer. See doc/sega.txt )
: C!+^ ( a c -- a+1 ) OVER C! 1+ ;
: segasig ( addr size -- )
    $2000 OVER LSHIFT ( a sz bytesz )
    ROT TUCK + $10 - ( sz a end )
    TUCK SWAP 0 ROT> ( sz end sum end a ) DO ( sz end sum )
        I C@ + LOOP ( sz end sum ) SWAP ( sz sum end )
    'T' C!+^ 'M' C!+^ 'R' C!+^ SPC C!+^ 'S' C!+^
    'E' C!+^ 'G' C!+^ 'A' C!+^ 0 C!+^ 0 C!+^
    ( sum's LSB ) OVER C!+^ ( MSB ) SWAP >>8 OVER C! 1+
    ( sz end ) 0 C!+^ 0 C!+^ 0 C!+^ SWAP $4a + SWAP C! ;
( ----- 200 )
\ Cross compilation program. See doc/cross.txt. B200-B205
: XCOMPH 201 205 LOADR ; : ARCHM 301 LOAD ;
: FONTC 262 263 LOADR ;
: COREL 207 224 LOADR ; : COREH 225 229 LOADR ;
: BLKSUB 230 234 LOADR ; : GRIDSUB 240 241 LOADR ;
: PS2SUB 246 248 LOADR ; : HALC 206 LOAD ;
'? HERESTART NOT [IF] 0 VALUE HERESTART [THEN]
0 VALUE XCURRENT \ CURRENT in target system, in target's addr
7 VALUES
  (n)* 0 (b)* 0 2>R* 0 (loop)* 0 (br)* 0 (?br)* 0 EXIT* 0
( ----- 201 )
: _xoff ORG BIN( - ;
: ENTRY
  WORD TUCK MOVE, XCURRENT T, C, HERE _xoff - [TO] XCURRENT ;
: ALIAS ENTRY JMPi, ; : *ALIAS ENTRY (i)>w, JMPw, ;
: VALUE ENTRY i>w, lblpush JMPi, ;
: *VALUE ENTRY (i)>w, lblpush JMPi, ;
: VALUES 0 DO ENTRY RUN1 i>w, lblpush JMPi, LOOP ;
: CREATE ENTRY lblcell CALLi, ;
: W= ( sa sl w -- f ) 2DUP 1- C@ $7f AND = IF ( same len )
    ( sa sl w ) OVER - 3 - ( s+1 len w-3-len ) ROT> []=
    ELSE 2DROP DROP 0 THEN ;
: _xfind ( sa sl -- w? f ) PAD C!+ ! XCURRENT BEGIN ( w )
  _xoff + DUP PAD C@+ SWAP @ SWAP ROT W= IF ( w ) 1 EXIT THEN
  3 - ( prev field ) T@ ?DUP NOT UNTIL 0 ( not found ) ;
: XFIND ( sa sl -- w ) _xfind NOT IF (wnf) THEN _xoff - ;
( ----- 202 )
: '? WORD _xfind DUP IF SWAP _xoff - SWAP THEN ;
: X' '? NOT IF (wnf) THEN ;
: _codecheck ( lbl str -- )
  XCURRENT _xoff + W= IF XCURRENT SWAP VAL! ELSE DROP THEN ;
: CODE ENTRY
  ['] EXIT* LIT" EXIT" _codecheck ['] (b)* LIT" (b)" _codecheck
  ['] (n)* LIT" (n)" _codecheck ['] 2>R* LIT" 2>R" _codecheck
  ['] (loop)* LIT" (loop)" _codecheck
  ['] (br)* LIT" (br)" _codecheck
  ['] (?br)* LIT" (?br)" _codecheck ;
: ;CODE lblnext JMPi, ;
( ----- 203 )
: XWRAP
  COREH HERESTART ?DUP NOT IF PC THEN ORG 8 ( LATEST ) + T!
  XCURRENT ORG 6 ( CURRENT ) + T! ;
: LITN DUP $ff > IF (n)* T, T, ELSE (b)* T, C, THEN ;
: imm? ( w -- f ) 1- C@ $80 AND ;
: X: CODE lblxt CALLi, BEGIN
  WORD LIT" ;" S= IF EXIT* T, EXIT THEN
  CURWORD PARSE IF LITN ELSE CURWORD _xfind IF ( w )
    DUP imm? IF ABORT" immed!" THEN _xoff - T,
  ELSE CURWORD FIND IF ( w )
    DUP imm? IF EXECUTE ELSE (wnf) THEN
    ELSE (wnf) THEN
  THEN ( _xfind ) THEN ( PARSE ) AGAIN ;
( ----- 204 )
: ['] WORD XFIND LITN ; IMMEDIATE
: COMPILE [COMPILE] ['] LIT" ," XFIND T, ; IMMEDIATE
: DO 2>R* T, HERE ; IMMEDIATE
: LOOP (loop)* T, HERE - C, ; IMMEDIATE
: IF (?br)* T, HERE 1 ALLOT ; IMMEDIATE
: ELSE (br)* T, 1 ALLOT [COMPILE] THEN HERE 1- ; IMMEDIATE
: AGAIN (br)* T, HERE - C, ; IMMEDIATE
: UNTIL (?br)* T, HERE - C, ; IMMEDIATE
( ----- 205 )
: [*TO] X' LITN LIT" *VAL!" XFIND T, ; IMMEDIATE
: LIT" (br)* T, HERE 1 ALLOT HERE ," TUCK HERE -^ SWAP
  [COMPILE] THEN SWAP _xoff - LITN LITN ; IMMEDIATE
: [COMPILE] WORD XFIND T, ; IMMEDIATE
: IMMEDIATE XCURRENT _xoff + 1- DUP C@ $80 OR SWAP C! ;
: : [ ' X: , ] ;
( ----- 206 )
\ HAL convenience layer
: BEGIN, PC ;
: LSET BEGIN, TO ;
: BR PC - [ JROFF JROPLEN - LITN ] + _bchk ;
: FJR BEGIN, [ JROPLEN LITN ] + 0 ;
: IFZ, Z? ^? FJR ?JRi, ; : IFNZ, Z? FJR ?JRi, ;
: IFC, C? ^? FJR ?JRi, ; : IFNC, C? FJR ?JRi, ;
: FMARK DUP PC ( l l pc ) -^ [ JROFF LITN ] + ( l off )
  \ warning: l is a PC offset, not a mem addr!
  SWAP ORG + BIN( - ( off addr ) C! ;
: THEN, FMARK ; : ELSE, FJR JRi, SWAP FMARK ;
: ;CODE [ lblnext LITN ] JMPi, ;
: CODE[ ( -- a )
  COMPILE (c) HERE 1 ALLOT INTERPRET ; IMMEDIATE
: ]CODE ( a -- ) ;CODE FMARK 2R> 2DROP ;
( ----- 207 )
\ Core Forth words. See doc/cross.txt.
\ Load range low: B206-B224 high: B226-B229
CODE EXIT POPr, w>IP, ;CODE
CODE EXECUTE POPp, JMPw,
CODE (br) LSET L1 ( used in ?br and loop ) IP+off, ;CODE
CODE (?br) POPp, w>Z, Z? L1 BR ?JRi, IP+, ;CODE
CODE (loop)
  POPr, INCw, PUSHp, POPr, CMPwp,
  IFNZ, PUSHr, POPp, PUSHr, L1 BR JRi, THEN,
  IP+, DROPp, ;CODE
CODE (b) IP>w, C@w, IP+, PUSHp, ;CODE
CODE (n) IP>w, @w, IP+, IP+, PUSHp, ;CODE
CODE (c) IP>w, IP+off, INCw, JMPw,
( ----- 208 )
\ Core words, >R R> 2>R 2R> I DUP ?DUP DROP SWAP OVER ..
CODE >R POPp, PUSHr, ;CODE
CODE R> POPr, PUSHp, ;CODE
CODE 2>R POPf, PUSHr, POPp, PUSHr, ;CODE
CODE 2R> DUPp, POPr, PUSHf, POPr, w>p, ;CODE
CODE I POPr, PUSHp, PUSHr, ;CODE
CODE DUP DUPp, ;CODE
CODE ?DUP p>Z, IFNZ, DUPp, THEN, ;CODE
CODE DROP DROPp, ;CODE
CODE SWAP POPf, PUSHp, ;CODE
CODE OVER POPf, PUSHf, PUSHp, ;CODE
CODE ROT POPp, SWAPwp, SWAPwf, PUSHp, ;CODE
CODE ROT> POPp, SWAPwf, SWAPwp, PUSHp, ;CODE
( ----- 209 )
\ Core words, AND OR XOR NOT + - ! @ C! C@
CODE AND POPp, ANDwp, w>p, ;CODE
CODE OR POPp, ORwp, w>p, ;CODE
CODE XOR POPp, XORwp, w>p, ;CODE
CODE NOT p>Z, Z>w, w>p, ;CODE
CODE + POPp, +wp, w>p, ;CODE
CODE - POPf, -wp, w>p, ;CODE
CODE ! POPp, !wp, DROPp, ;CODE
CODE @ p>w, @w, w>p, ;CODE
CODE C! POPp, C!wp, DROPp, ;CODE
CODE C@ p>w, C@w, w>p, ;CODE
( ----- 210 )
\ Core words, = > < 1+ 1- << >> <<8 >>8
CODE = POPp, CMPwp, Z>w, w>p, ;CODE
CODE > POPp, CMPwp, C>w, w>p, ;CODE
CODE < POPf, CMPwp, C>w, w>p, ;CODE
CODE 1+ INCp, ;CODE
CODE 1- DECp, ;CODE
CODE >> p>w, >>w, w>p, ;CODE
CODE << p>w, <<w, w>p, ;CODE
CODE >>8 p>w, >>8w, w>p, ;CODE
CODE <<8 p>w, <<8w, w>p, ;CODE
( ----- 211 )
\ Core words, NOOP CURRENT HERE PC ORG BIN( SYSVARS IOERR ..
CODE NOOP ;CODE
SYSVARS $02 + *VALUE CURRENT
SYSVARS $04 + *VALUE HERE
SYSVARS $04 + *VALUE PC
SYSVARS $18 + VALUE PAD
0 VALUE ORG
BIN( VALUE BIN( SYSVARS VALUE SYSVARS
SYSVARS *VALUE IOERR
\ size of a line. used for INBUF and BLK. Keep this to $40 or
\ you're gonna have a bad time.
$40 VALUE LNSZ
( ----- 212 )
\ Core words, 0< >= <= =><= 2DUP 2DROP NIP TUCK L|M C@+ ..
: 0< $7fff > ; : >= < NOT ; : <= > NOT ;
: =><= ( n l h -- f ) OVER - ROT> ( h n l ) - >= ;
CODE 2DUP POPf, PUSHf, DUPp, PUSHf, ;CODE
CODE 2DROP DROPp, DROPp, ;CODE
CODE NIP POPf, ;CODE CODE TUCK POPf, DUPp, PUSHf, ;CODE
: L|M DUP <<8 >>8 SWAP >>8 ;
: RSHIFT ?DUP IF 0 DO >> LOOP THEN ;
: LSHIFT ?DUP IF 0 DO << LOOP THEN ; : -^ SWAP - ;
CODE C@+ p>w, INCp, C@w, PUSHp, ;CODE
CODE C!+ POPp, C!wp, INCw, w>p, ;CODE
: LEAVE R> R> DROP I 1- >R >R ;
CODE UNLOOP POPr, POPr, ;CODE
CODE VAL! POPp, INCw, !wp, DROPp, ;CODE
CODE *VAL! POPp, INCw, @w, !wp, DROPp, ;CODE
: / /MOD NIP ; : MOD /MOD DROP ;
( ----- 213 )
\ Core words, ALLOT FILL IMMEDIATE , L, M, MOVE MOVE- MOVE, ..
: RANGE ( a u -- ah al ) OVER + SWAP ;
: ALLOT HERE + [*TO] HERE ;
: FILL ( a u b -- ) ROT> RANGE DO ( b ) DUP I C! LOOP DROP ;
: ALLOT0 ( u -- ) HERE OVER 0 FILL ALLOT ;
: IMMEDIATE CURRENT 1- DUP C@ 128 OR SWAP C! ;
: , HERE ! 2 ALLOT ; : C, HERE C! 1 ALLOT ;
: L, DUP C, >>8 C, ; : M, DUP >>8 C, C, ;
: MOVE ( src dst u -- )
  ?DUP IF RANGE DO ( src ) C@+ ( src+1 b ) I C! LOOP
  ELSE DROP THEN DROP ;
: MOVE, ( a u -- ) HERE OVER ALLOT SWAP MOVE ;
( ----- 214 )
\ Core words, we begin EMITting
SYSVARS $0e + *ALIAS EMIT
: STYPE RANGE DO I C@ EMIT LOOP ;
5 VALUES EOT $04 BS $08 LF $0a CR $0d SPC $20
SYSVARS $0a + *VALUE NL
: SPC> SPC EMIT ;
: NL> NL L|M ?DUP IF EMIT THEN EMIT ;
: STACK? SCNT 0< IF LIT" stack underflow" STYPE ABORT THEN ;
( ----- 215 )
\ Core words, number formatting
: . ( n -- )
  ?DUP NOT IF '0' EMIT EXIT THEN \ 0 is a special case
  DUP 0< IF '-' EMIT -1 * THEN
  $ff SWAP ( stop ) BEGIN 10 /MOD ( d q ) ?DUP NOT UNTIL
  BEGIN '0' + EMIT DUP 9 > UNTIL DROP ;
: _ DUP 9 > IF [ 'a' 10 - LITN ] ELSE '0' THEN + ;
: .x <<8 >>8 16 /MOD ( l h ) _ EMIT _ EMIT ;
: .X L|M .x .x ;
( ----- 216 )
\ Core words, literal parsing
: _ud ( sa sl -- n? f ) \ parse unsigned decimal
  0 ROT> RANGE DO ( r )
    10 * I C@ ( r c ) '0' - DUP 9 > IF
      2DROP 0 UNLOOP EXIT THEN + LOOP ( r ) 1 ;
: _d ( sa sl -- n? f ) \ parse possibly signed decimal
  OVER C@ '-' = IF
    SWAP 1+ SWAP 1- _ud DUP IF SWAP 0 -^ SWAP THEN
    ELSE _ud THEN ;
: _h ( sa sl -- n 1 OR sa sl 0 ) \ parse hex
  OVER C@ '$' = NOT IF 0 EXIT THEN
  2DUP 0 ROT> RANGE 1+ DO ( sa sl r )
    16 * I C@ ( r c ) '0' - DUP 9 > IF
      $df AND [ 'A' '0' - LITN ] - DUP 6 < IF
        10 + ELSE 2DROP 0 UNLOOP EXIT THEN THEN
    ( r n ) + LOOP ( sa sl r ) NIP NIP 1 ;
( ----- 217 )
\ Core words, literal parsing, CRC16
: _c ( sa sl -- n 1 OR sa sl 0 ) \ parse character
  DUP 3 = IF OVER C@ ''' = IF OVER 2 + C@ ''' = IF
    DROP 1+ C@ 1 EXIT THEN THEN THEN 0 ;
: PARSE ( sa sl -- n? f )
  _c ?DUP IF EXIT THEN _h ?DUP NOT IF _d THEN ;
CODE CRC16 ( c n -- c )
  POPp, <<8w, XORwp, w>p, 8 i>w, SWAPwp, BEGIN, ( w=c p=u )
    <<w, IFC, $1021 XORwi, THEN, DECp, p>Z, Z? ^? BR ?JRi,
    w>p, ;CODE
( ----- 218 )
\ Core words, input buffer
SYSVARS $10 + *ALIAS KEY?
: KEY BEGIN KEY? UNTIL ;
SYSVARS $2e + *VALUE IN(
SYSVARS $30 + *VALUE IN> \ current position in IN(
SYSVARS $08 + *ALIAS LN<
: IN) IN( LNSZ + ;
: BS? DUP $7f ( DEL ) = SWAP BS = OR ;
( ----- 219 )
\ Core words, input buffer
\ type c into ptr inside INBUF. f=true if typing should stop
: LNTYPE ( ptr c -- ptr+-1 f )
  DUP BS? IF ( ptr c )
    DROP DUP IN( > IF 1- BS EMIT THEN SPC> BS EMIT 0
  ELSE ( ptr c ) \ non-BS
    DUP SPC < IF DROP DUP IN) OVER - SPC FILL 1 ELSE
      TUCK EMIT C!+ DUP IN) = THEN THEN ;
: RDLN ( -- ) \ Read 1 line in IN(
  LIT"  ok" STYPE NL> IN( [*TO] IN>
  IN( BEGIN KEY LNTYPE UNTIL DROP NL> ;
: IN< ( -- c ) \ Read one character from INBUF
  IN> IN) = IF LN< THEN IN> C@ IN> 1+ [*TO] IN> ;
: IN$ ['] RDLN [*TO] LN<
  [ SYSVARS $40 ( INBUF ) + LITN ] [*TO] IN( IN) [*TO] IN> ;
( ----- 220 )
\ Core words, WORD parsing
: ," BEGIN IN< DUP '"' = IF DROP EXIT THEN C, AGAIN ;
: WS? SPC <= ;
: TOWORD ( -- c ) \ Advance IN> to first non-WS and yield it.
  0 ( dummy ) BEGIN DROP IN< DUP WS? NOT UNTIL ;
: CURWORD ( -- sa sl ) [ SYSVARS $12 + LITN ] C@+ SWAP @ SWAP ;
: WORD ( -- sa sl )
  TOWORD DROP IN> 1- DUP BEGIN ( old a )
    C@+ WS? OVER IN) = OR UNTIL ( old new )
  DUP [*TO] IN> ( old new ) OVER - ( old len )
  IN> 1- C@ WS? IF 1- THEN ( adjust len when not EOL )
  2DUP [ SYSVARS $12 ( CURWORD ) + LITN ] C!+ ! ;
( ----- 221 )
\ Core words, INTERPRET loop
: (wnf) CURWORD STYPE LIT"  word not found" STYPE ABORT ;
: RUN1 \ read next word in stream and interpret it
  WORD PARSE NOT IF
    CURWORD FIND IF EXECUTE STACK? ELSE (wnf) THEN THEN ;
: INTERPRET BEGIN RUN1 AGAIN ;
\ We want to pop the RS until it points to a xt *right after*
\ a reference to INTERPET (yeah, that's pretty hackish!)
: ESCAPE! 0 ( dummy ) BEGIN
  DROP R> DUP 2 - ( xt xt-2 ) @ ['] INTERPRET = UNTIL >R ;
( ----- 222 )
\ Core words, Dictionary
: ENTRY WORD TUCK MOVE, ( len )
  CURRENT , C, \ write prev value and size
  HERE [*TO] CURRENT ;
X' ENTRY ALIAS CODE
: '? WORD FIND DUP IF NIP THEN ;
: ' WORD FIND NOT IF (wnf) THEN ;
: TO ' VAL! ; : *TO ' *VAL! ;
: FORGET
  ' DUP ( w w )
  \ HERE must be at the end of prev's word, that is, at the
  \ beginning of w.
  DUP 1- C@ ( len ) << >> ( rm IMMEDIATE )
  3 + ( fixed header len ) - [*TO] HERE ( w )
  ( get prev addr ) 3 - DUP @ [*TO] CURRENT ;
( ----- 223 )
\ Core words, S=, [IF], _bchk
: S= ( sa1 sl1 sa2 sl2 -- f )
  ROT OVER = IF ( same len, s2 s1 l ) []=
  ELSE DROP 2DROP 0 THEN ;
: [IF]
  IF EXIT THEN LIT" [THEN]" BEGIN 2DUP WORD S= UNTIL 2DROP ;
: [THEN] ;
: _bchk DUP $80 + $ff > IF LIT" br ovfl" STYPE ABORT THEN ;
( ----- 224 )
\ Core words, DUMP, .S
: DUMP ( n a -- )
  SWAP 8 /MOD SWAP IF 1+ THEN 0 DO
    ':' EMIT DUP .x SPC> DUP ( a a )
    4 0 DO C@+ .x C@+ .x SPC> LOOP DROP ( a )
    8 0 DO
      C@+ DUP SPC < IF DROP '.' THEN EMIT LOOP NL>
  LOOP DROP ;
: PSDUMP SCNT NOT IF EXIT THEN
  SCNT PAD ! BEGIN DUP .X SPC> >R SCNT NOT UNTIL
  BEGIN R> SCNT PAD @ = UNTIL ;
: .S ( -- )
  LIT" SP " STYPE SCNT .x SPC> LIT" RS " STYPE RCNT .x SPC>
  LIT" -- " STYPE STACK? PSDUMP ;
( ----- 225 )
\ Core high, CREATE DOER DOES>
: CREATE CODE [ lblcell LITN ] CALLi, ;
: DOER CODE 0 i>w, [ lbldoes LITN ] CALLi, ;
\ Because we pop RS below, we'll exit parent definition
: DOES> R> CURRENT 1+ ! ;
: ALIAS ( addr -- ) ENTRY JMPi, ;
: *ALIAS ( addr -- ) ENTRY (i)>w, JMPw, ;
: VALUE ENTRY i>w, [ lblpush LITN ] JMPi, ;
: *VALUE ENTRY (i)>w, [ lblpush LITN ] JMPi, ;
: VALUES ( n -- )
  0 DO ENTRY RUN1 i>w, [ lblpush LITN ] JMPi, LOOP ;
( ----- 226 )
\ Core high, BOOT
: (main) IN$ INTERPRET BYE ;
XCURRENT ORG $0a ( stable ABI (main) ) + T!
: BOOT
  [ BIN( $06 ( CURRENT ) + LITN ] @ [*TO] CURRENT
  [ BIN( $08 ( LATEST ) + LITN ] @ [*TO] HERE
  ['] (emit) ['] EMIT *VAL! ['] (key?) [*TO] KEY?
  0 [*TO] IOERR $0d0a ( CR/LF ) [*TO] NL
  INIT LIT" Collapse OS" STYPE ABORT ;
XCURRENT ORG $04 ( stable ABI BOOT ) + T!
( ----- 227 )
\ Core high, See bootstrap doc. DO..LOOP, LITN, :
: DO COMPILE 2>R HERE ; IMMEDIATE
: LOOP COMPILE (loop) HERE - _bchk C, ; IMMEDIATE
: LITN DUP >>8 IF COMPILE (n) , ELSE COMPILE (b) C, THEN ;
: : CODE [ lblxt LITN ] CALLi, BEGIN
    WORD LIT" ;" S= IF COMPILE EXIT EXIT THEN
    CURWORD PARSE IF LITN ELSE CURWORD FIND IF
      DUP 1- C@ $80 AND ( imm? ) IF EXECUTE ELSE , THEN
    ELSE (wnf) THEN THEN
  AGAIN ;
( ----- 228 )
\ Core high, IF..ELSE..THEN ( \
: IF ( -- a | a: br cell addr )
  COMPILE (?br) HERE 1 ALLOT ( br cell allot ) ; IMMEDIATE
: THEN ( a -- | a: br cell addr )
  DUP HERE -^ _bchk SWAP ( a-H a ) C! ; IMMEDIATE
: ELSE ( a1 -- a2 | a1: IF cell a2: ELSE cell )
  COMPILE (br) 1 ALLOT [COMPILE] THEN
  HERE 1- ( push a. 1- for allot offset ) ; IMMEDIATE
: ( LIT" )" BEGIN 2DUP WORD S= UNTIL 2DROP ; IMMEDIATE
: \ IN) [*TO] IN> ; IMMEDIATE
: LIT"
  COMPILE (br) HERE 1 ALLOT HERE ," TUCK HERE -^ SWAP
  [COMPILE] THEN SWAP LITN LITN ; IMMEDIATE
( ----- 229 )
\ Core high, .", ABORT", BEGIN..AGAIN..UNTIL, many others.
: ." [COMPILE] LIT" COMPILE STYPE ; IMMEDIATE
: ABORT" [COMPILE] ." COMPILE ABORT ; IMMEDIATE
: BEGIN HERE ; IMMEDIATE
: AGAIN COMPILE (br) HERE - _bchk C, ; IMMEDIATE
: UNTIL COMPILE (?br) HERE - _bchk C, ; IMMEDIATE
: [TO] ' LITN COMPILE VAL! ; IMMEDIATE
: [*TO] ' LITN COMPILE *VAL! ; IMMEDIATE
: [ INTERPRET ; IMMEDIATE
: ] 2R> 2DROP ; \ INTERPRET+RUN1
: COMPILE ' LITN ['] , , ; IMMEDIATE
: [COMPILE] ' , ; IMMEDIATE
: ['] ' LITN ; IMMEDIATE
( ----- 230 )
\ BLK subsystem. See doc/blk.txt. Load range: B230-234
\ Current blk pointer -1 means "invalid"
SYSVARS $38 + *VALUE BLK>
\ Whether buffer is dirty
SYSVARS $3a + *VALUE BLKDTY
SYSVARS 1024 - VALUE BLK(
SYSVARS VALUE BLK)
: BLK$ 0 [*TO] BLKDTY -1 [*TO] BLK> ;
( ----- 231 )
: BLK! ( -- ) BLK> BLK( (blk!) 0 [*TO] BLKDTY ;
: FLUSH BLKDTY IF BLK! THEN -1 [*TO] BLK> ;
: BLK@ ( n -- )
  DUP BLK> = IF DROP EXIT THEN
  FLUSH DUP [*TO] BLK> BLK( (blk@) ;
: BLK!! 1 [*TO] BLKDTY ;
: WIPE BLK( 1024 SPC FILL BLK!! ;
: COPY ( src dst -- ) FLUSH SWAP BLK@ [*TO] BLK> BLK! ;
( ----- 232 )
: LNLEN ( a -- len ) \ len based on last visible char in line
  -1 ( res ) LNSZ 0 DO ( a res )
    OVER I + C@ SPC > IF DROP I THEN LOOP 1+ NIP ;
: EMITLN ( a -- ) \ emit LNSZ chars from a or stop at CR
  DUP LNLEN ?DUP IF RANGE DO I C@ EMIT LOOP ELSE DROP THEN NL> ;
: LIST ( n -- ) \ print contents of BLK n
  BLK@ 16 0 DO
    I 1+ DUP 10 < IF SPC> THEN . SPC>
    LNSZ I * BLK( + EMITLN LOOP ;
: INDEX ( b1 b2 -- ) \ print first line of blocks b1 through b2
  1+ SWAP DO I DUP . SPC> BLK@ BLK( EMITLN LOOP ;
( ----- 233 )
: _ ( -- ) \ set IN( to next line in block
  IN) BLK) = IF ESCAPE! THEN
  IN) [*TO] IN( IN( [*TO] IN> ;
: LOAD
  IN> >R ['] _ [*TO] LN< BLK@ BLK( [*TO] IN( IN( [*TO] IN>
  INTERPRET IN$ R> [*TO] IN> ;
: LOADR 1+ SWAP DO I DUP . SPC> LOAD LOOP ;
( ----- 234 )
\ Application loader, to include in boot binary
: ED 120 LOAD 100 104 LOADR ;
: VE ED 123 LOAD 105 111 LOADR ;
: ME 123 LOAD 115 119 LOADR ;
: ASML 2 LOAD ;
: Z80A ASML 5 12 LOADR ;
: 8086A ASML 20 26 LOADR ;
: AVRA ASML 30 40 LOADR ;
: 6809A ASML 50 59 LOADR ;
: RSH 150 154 LOADR ;
: AVRP 160 163 LOADR ;
: XCOMPL 200 LOAD ;
( ----- 240 )
\ Grid subsystem. See doc/grid.txt. Load range: B240-B241
GRID_MEM *VALUE XYPOS
'? CURSOR! NOT [IF] : CURSOR! 2DROP ; [THEN]
: XYPOS! COLS LINES * MOD DUP XYPOS CURSOR! [*TO] XYPOS ;
: AT-XY ( x y -- ) COLS * + XYPOS! ;
'? NEWLN NOT [IF]
: NEWLN ( oldln -- newln )
  1+ LINES MOD DUP COLS * COLS RANGE DO SPC I CELL! LOOP ;
[THEN]
'? CELLS! NOT [IF]
: CELLS! ( a pos u -- )
  ?DUP IF RANGE DO ( a ) C@+ I CELL! LOOP
    ELSE DROP THEN DROP ; [THEN]
( ----- 241 )
: _lf XYPOS COLS / NEWLN COLS * XYPOS! ;
: _bs SPC XYPOS TUCK CELL! ( pos ) 1- XYPOS! ;
: (emit)
    DUP BS? IF DROP _bs EXIT THEN
    DUP CR = IF DROP SPC XYPOS CELL! _lf EXIT THEN
    DUP SPC < IF DROP EXIT THEN
    XYPOS CELL!
    XYPOS 1+ DUP COLS MOD IF XYPOS! ELSE DROP _lf THEN ;
: GRID$ 0 [*TO] XYPOS ;
( ----- 245 )
PS/2 keyboard subsystem

Provides (key?) from a driver providing the PS/2 protocol. That
is, for a driver taking care of providing all key codes emanat-
ing from a PS/2 keyboard, this subsystem takes care of mapping
those keystrokes to ASCII characters. This code is designed to
be cross-compiled and loaded with drivers.

Requires PS2_MEM to be defined.

Load range: 246-249
( ----- 246 )
: PS2_SHIFT [ PS2_MEM LITN ] ; : PS2$ 0 PS2_SHIFT C! ;
\ A list of the values associated with the $80 possible scan
\ codes of the set 2 of the PS/2 keyboard specs. 0 means no
\ value. That value is a character that can be read in (key?)
\ No make code in the PS/2 set 2 reaches $80.
\ TODO: I don't know why, but the key 2 is sent as $1f by 2 of
\ my keyboards. Is it a timing problem on the ATtiny?
CREATE PS2_CODES $80 nC,
0   0   0   0   0   0   0   0 0 0   0   0   0   9   '`' 0
0   0   0   0   0   'q' '1' 0 0 0   'z' 's' 'a' 'w' '2' '2'
0   'c' 'x' 'd' 'e' '4' '3' 0 0 32  'v' 'f' 't' 'r' '5' 0
0   'n' 'b' 'h' 'g' 'y' '6' 0 0 0   'm' 'j' 'u' '7' '8' 0
0   ',' 'k' 'i' 'o' '0' '9' 0 0 '.' '/' 'l' ';' 'p' '-' 0
0   0   ''' 0   '[' '=' 0   0 0 0   13  ']' 0   '\' 0   0
0   0   0   0   0   0   8   0 0 '1' 0   '4' '7' 0   0   0
'0' '.' '2' '5' '6' '8' 27  0 0 0   '3' 0   0   '9' 0   0
( ----- 247 )
( Same values, but shifted ) $80 nC,
0   0   0   0   0   0   0   0 0 0   0   0   0   9   '~' 0
0   0   0   0   0   'Q' '!' 0 0 0   'Z' 'S' 'A' 'W' '@' '@'
0   'C' 'X' 'D' 'E' '$' '#' 0 0 32  'V' 'F' 'T' 'R' '%' 0
0   'N' 'B' 'H' 'G' 'Y' '^' 0 0 0   'M' 'J' 'U' '&' '*' 0
0   '<' 'K' 'I' 'O' ')' '(' 0 0 '>' '?' 'L' ':' 'P' '_' 0
0   0   '"' 0   '{' '+' 0   0 0 0   13  '}' 0   '|' 0   0
0   0   0   0   0   0   8   0 0 0   0   0   0   0   0   0
0   0   0   0   0   0   27  0 0 0   0   0   0   0   0   0
( ----- 248 )
: _shift? ( kc -- f ) DUP $12 = SWAP $59 = OR ;
: (key?) ( -- c? f )
    (ps2kc) DUP NOT IF EXIT THEN ( kc )
    DUP $e0 ( extended ) = IF ( ignore ) DROP 0 EXIT THEN
    DUP $f0 ( break ) = IF DROP ( )
        ( get next kc and see if it's a shift )
        BEGIN (ps2kc) ?DUP UNTIL ( kc )
        _shift? IF ( drop shift ) 0 PS2_SHIFT C! THEN
        ( whether we had a shift or not, we return the next )
        0 EXIT THEN
    DUP $7f > IF DROP 0 EXIT THEN
    DUP _shift? IF DROP 1 PS2_SHIFT C! 0 EXIT THEN
    ( ah, finally, we have a gentle run-of-the-mill KC )
    PS2_CODES PS2_SHIFT C@ IF $80 + THEN + C@ ( c, maybe 0 )
    ?DUP ( c? f ) ;
( ----- 250 )
\ SD Card subsystem Load range: B250-B258
SDC_MEM *VALUE SDC_SDHC
: _idle ( -- n ) $ff (spix) ;

( spix $ff until the response is something else than $ff
  for a maximum of 20 times. Returns $ff if no response. )
: _wait ( -- n )
    0 ( dummy ) 20 0 DO
        DROP _idle DUP $ff = NOT IF LEAVE THEN LOOP ;

( adjust block for LBA for SD/SDHC )
: _badj ( arg1 arg2 -- arg1 arg2 )
  SDC_SDHC IF 0 SWAP ELSE DUP 128 / SWAP <<8 << THEN ;
( ----- 251 )
( The opposite of sdcWaitResp: we wait until response is $ff.
  After a successful read or write operation, the card will be
  busy for a while. We need to give it time before interacting
  with it again. Technically, we could continue processing on
  our side while the card it busy, and maybe we will one day,
  but at the moment, I'm having random write errors if I don't
  do this right after a write, so I prefer to stay cautious
  for now. )
: _ready ( -- ) BEGIN _idle $ff = UNTIL ;
( ----- 252 )
( Computes n into crc c with polynomial $09
  Note that the result is "left aligned", that is, that 8th
  bit to the "right" is insignificant (will be stop bit). )
: _crc7 ( c n -- c )
  XOR 8 0 DO ( c )
    << ( c<<1 ) DUP >>8 IF
      ( MSB was set, apply polynomial )
      <<8 >>8
      $12 XOR ( $09 << 1, we apply CRC on high bits )
    THEN
  LOOP ;
( send-and-crc7 )
: _s+crc ( n c -- c ) SWAP DUP (spix) DROP _crc7 ;
( ----- 253 )
( cmd arg1 arg2 -- resp )
( Sends a command to the SD card, along with arguments and
  specified CRC fields. (CRC is only needed in initial commands
  though). This does *not* handle CS. You have to
  select/deselect the card outside this routine. )
: _cmd
    _wait DROP ROT    ( a1 a2 cmd )
    0 _s+crc          ( a1 a2 crc )
    ROT L|M ROT       ( a2 h l crc )
    _s+crc _s+crc     ( a2 crc )
    SWAP L|M ROT      ( h l crc )
    _s+crc _s+crc     ( crc )
    1 OR              ( ensure stop bit )
    (spix) DROP       ( send CRC )
    _wait  ( wait for a valid response... ) ;
( ----- 254 )
( cmd arg1 arg2 -- r )
( Send a command that expects a R1 response, handling CS. )
: SDCMDR1 [ SDC_DEVID LITN ] (spie) _cmd 0 (spie) ;

( cmd arg1 arg2 -- r arg1 arg2 )
( Send a command that expects a R7 response, handling CS. A R7
  is a R1 followed by 4 bytes. arg1 contains bytes 0:1, arg2
  has 2:3 )
: SDCMDR7
    [ SDC_DEVID LITN ] (spie)
    _cmd                 ( r )
    _idle <<8 _idle +  ( r arg1 )
    _idle <<8 _idle +  ( r arg1 arg2 )
    0 (spie) ;
: _rdsdhc ( -- ) $7A ( CMD58 ) 0 0 SDCMDR7 DROP $4000
  AND [*TO] SDC_SDHC DROP ;
( ----- 255 )
: _err 0 (spie) LIT" SDerr" STYPE ABORT ;

( Tight definition ahead, pre-comment.

  Initialize a SD card. This should be called at least 1ms
  after the powering up of the card. We begin by waking up the
  SD card. After power up, a SD card has to receive at least
  74 dummy clocks with CS and DI high. We send 80.
  Then send cmd0 for a maximum of 10 times, success is when
  we get $01. Then comes the CMD8. We send it with a $01aa
  argument and expect a $01aa argument back, along with a
  $01 R1 response. After that, we need to repeatedly run
  CMD55+CMD41 ($40000000) until the card goes out of idle
  mode, that is, when it stops sending us $01 response and
  send us $00 instead. Any other response means that
  initialization failed. )
( ----- 256 )
: SDC$
    10 0 DO _idle DROP LOOP
    0 ( dummy ) 10 0 DO  ( r )
        DROP $40 0 0 SDCMDR1  ( CMD0 )
        1 = DUP IF LEAVE THEN
    LOOP NOT IF _err THEN
    $48 0 $1aa ( CMD8 ) SDCMDR7 ( r arg1 arg2 )
    ( expected 1 0 $1aa )
    $1aa = ROT ( arg1 f r ) 1 = AND SWAP ( f&f arg1 )
    NOT ( 0 expected ) AND ( f&f&f ) NOT IF _err THEN
    BEGIN
        $77 0 0 SDCMDR1  ( CMD55 )
        1 = NOT IF _err THEN
        $69 $4000 0 SDCMDR1  ( CMD41 )
        DUP 1 > IF _err THEN
    NOT UNTIL _rdsdhc ; ( out of idle mode, success! )
( ----- 257 )
: _ ( dstaddr blkno -- )
  [ SDC_DEVID LITN ] (spie)
  $51 ( CMD17 ) SWAP _badj ( a cmd arg1 arg2 ) _cmd IF _err THEN
  _wait $fe = NOT IF _err THEN
  0 SWAP ( crc1 a ) 512 RANGE DO ( crc1 )
    _idle ( crc1 b ) DUP I C! ( crc1 b ) CRC16 LOOP ( crc1 )
    _idle <<8 _idle + ( crc1 crc2 )
    _wait DROP 0 (spie) = NOT IF _err THEN ;
: SDC@ ( blkno blk( -- )
  SWAP << ( 2x ) 2DUP ( a b a b ) _
  ( a b ) 1+ SWAP 512 + SWAP _ ;
( ----- 258 )
: _ ( srcaddr blkno -- )
  [ SDC_DEVID LITN ] (spie)
  $58 ( CMD24 ) SWAP _badj ( a cmd arg1 arg2 ) _cmd IF _err THEN
  _idle DROP $fe (spix) DROP 0 SWAP ( crc a )
  512 RANGE DO ( crc )
    I C@ ( crc b ) DUP (spix) DROP CRC16 LOOP ( crc )
    DUP >>8 ( crc msb ) (spix) DROP (spix) DROP
    _wait DROP _ready 0 (spie) ;
: SDC! ( blkno blk( -- )
  SWAP << ( 2x ) 2DUP ( a b a b ) _
  ( a b ) 1+ SWAP 512 + SWAP _ ;
( ----- 260 )
Fonts

Fonts are kept in "source" form in the following blocks and
then compiled to binary bitmasks by the following code. In
source form, fonts are a simple sequence of '.' and 'X'. '.'
means empty, 'X' means filled. Glyphs are entered one after the
other, starting at $21 and ending at $7e. To be space
efficient in blocks, we align glyphs horizontally in the blocks
to fit as many character as we can. For example, a 5x7 font
would mean that we would have 12x2 glyphs per block.

261 Font compiler              265 3x5 font
267 5x7 font                   271 7x7 font
( ----- 261 )
\ Converts "dot-X" fonts to binary "glyph rows". One byte for
\ each row. In a 5x7 font, each glyph thus use 7 bytes.
\ Resulting bytes are aligned to the left of the byte.
\ Therefore, for a 5-bit wide char, "X.X.X" translates to
\ 10101000. Left-aligned bytes are easier to work with when
\ compositing glyphs.
( ----- 262 )
2 VALUES _w 0 _h 0
: _g ( given a top-left of dot-X in BLK(, spit H bin lines )
  _h 0 DO 0 _w 0 DO ( a r )
    << OVER I + C@ 'X' = IF 1+ THEN
  LOOP 8 _w - LSHIFT C, 64 + LOOP DROP ;
: _l ( a u -- a, spit a line of u glyphs )
  ( u ) 0 DO ( a ) DUP I _w * + _g LOOP ;
( ----- 263 )
: CPFNT3x5 3 [TO] _w 5 [TO] _h
    _h ALLOT0 ( space char )
    265 BLK@ BLK( 21 _l 320 + 21 _l 320 + 21 _l DROP ( 63 )
    266 BLK@ BLK( 21 _l 320 + 10 _l DROP ( 94! ) ;
: CPFNT5x7 5 [TO] _w 7 [TO] _h
    _h ALLOT0 ( space char )
    270 267 DO I BLK@ BLK( 12 _l 448 + 12 _l DROP LOOP ( 72 )
    270 BLK@ BLK( 12 _l 448 + 10 _l DROP ( 94! ) ;
: CPFNT7x7 7 [TO] _w 7 [TO] _h
    _h ALLOT0 ( space char )
    276 271 DO I BLK@ BLK( 9 _l 448 + 9 _l DROP LOOP ( 90 )
    276 BLK@ BLK( 4 _l DROP ( 94! ) ;
( ----- 265 )
.X.X.XX.X.XXX...X..X...XX...X...............X.X..X.XX.XX.X.XXXX
.X.X.XXXXXX...XX.X.X..X..X.XXX.X............XX.XXX...X..XX.XX..
.X........XX.X..X.....X..X..X.XXX...XXX....X.X.X.X..X.XX.XXXXX.
......XXXXX.X..X.X....X..X.X.X.X..X.......X..X.X.X.X....X..X..X
.X....X.X.X...X.XX.....XX........X......X.X...X.XXXXXXXX...XXX.
.XXXXXXXXXXX........X...X..XX..X..X.XX..XXXX.XXXXXX.XXX.XXXXXXX
X....XX.XX.X.X..X..X.XXX.X...XXXXX.XX.XX..X.XX..X..X..X.X.X...X
XXX.X.XXXXXX......X.......X.X.XXXXXXXX.X..X.XXX.XX.X.XXXX.X...X
X.XX..X.X..X.X..X..X.XXX.X....X..X.XX.XX..X.XX..X..X.XX.X.X...X
XXXX..XXXXX....X....X...X...X..XXX.XXX..XXXX.XXXX...XXX.XXXXXX.
X.XX..X.XXX.XXXXX.XXXXX..XXXXXX.XX.XX.XX.XX.XXXXXXXX..XXX.X....
XX.X..XXXX.XX.XX.XX.XX.XX...X.X.XX.XX.XX.XX.X..XX..X....XX.X...
X..X..XXXX.XX.XXX.X.XXX..X..X.X.XX.XXXX.X..X..X.X...X...X......
XX.X..X.XX.XX.XX..XXXX.X..X.X.X.XX.XXXXX.X.X.X..X....X..X......
X.XXXXX.XX.XXXXX...XXX.XXX..X.XXX.X.X.XX.X.X.XXXXXX..XXXX...XXX
!"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[\]^_
( ----- 266 )
X.....X.......X....XX...X...X...XX..XX.......................X.
.X.XX.X...XX..X.X.X...X.X........X.X.X.X.XXX..X.XX..XX.XX.XXXXX
.....XXX.X...XXX.XXX.X.XXX..X...XXX..X.XXXX.XX.XX.XX.XX..XX..X.
...XXXX.XX..X.XXX.X...XXX.X.X...XX.X.X.X.XX.XX.XXX..XXX....X.X.
...XXXXX..XX.XX.XXX..XX.X.X.X.XX.X.X.XXX.XX.X.X.X....XX..XX..XX
...................XX.X.XX.....................................
X.XX.XX.XX.XX.XXXX.X..X..X..XX
X.XX.XX.X.X..X..XXX...X...XXX.
X.XX.XXXX.X..X.XX..X..X..X....
XXX.X.X.XX.X.X.XXX.XX.X.XX....
`abcdefghijklmnopqrstuvwxyz{|}~
( ----- 267 )
..X...X.X........X..............X....X....X.................
..X...X.X..X.X..XXXXX...X.XX....X...X......X.X.X.X..X.......
..X.......XXXXXX.......X.X..X......X........X.XXX...X.......
..X........X.X..XXX...X...XX.......X........XXXXXXXXXXX.....
..........XXXXX....X.X....XX.X.....X........X.XXX...X.......
..X........X.X.XXXX.X...XX..X.......X......X.X.X.X..X.....X.
..X..............X.......XXX.X.......X....X..............X..
................XXX...XX..XXX..XXX...XX.XXXXX.XXX.XXXXX.XXX.
..............XX...X.X.X.X...XX...X.X.X.X....X........XX...X
.............X.X..XX...X.....X....XX..X.XXXX.X........XX...X
XXXXX.......X..X.X.X...X....X...XX.XXXXX....XXXXX....X..XXX.
...........X...XX..X...X...X......X...X.....XX...X..X..X...X
......XX..X....X...X...X..X...X...X...X.X...XX...X.X...X...X
......XX........XXX..XXXXXXXXX.XXX....X..XXX..XXX.X.....XXX.
!"#$%&'()*+,-./012345678
( ----- 268 )
.XXX...............X.....X.....XXX..XXX..XXX.XXXX..XXX.XXXX.
X...X..X....X....XX.......XX..X...XX...XX...XX...XX...XX...X
X...X..X....X...XX..XXXXX..XX.....XX..XXX...XX...XX....X...X
.XXX...........X.............X...X.X..XXXXXXXXXXX.X....X...X
....X..X....X...XX..XXXXX..XX...X..X....X...XX...XX....X...X
....X..X...X.....XX.......XX.......X...XX...XX...XX...XX...X
.XXX...............X.....X......X...XXX.X...XXXXX..XXX.XXXX.
XXXXXXXXXX.XXX.X...X.XXX....XXX..X.X....X...XX...X.XXX.XXXX.
X....X....X...XX...X..X......XX.X..X....XX.XXXX..XX...XX...X
X....X....X....X...X..X......XXX...X....X.X.XXX..XX...XX...X
XXXX.XXXX.X..XXXXXXX..X......XX....X....X...XX.X.XX...XXXXX.
X....X....X...XX...X..X......XXX...X....X...XX..XXX...XX....
X....X....X...XX...X..X..X...XX.X..X....X...XX..XXX...XX....
XXXXXX.....XXX.X...X.XXX..XXX.X..X.XXXXXX...XX...X.XXX.X....
9:;<=>?@ABCDEFGHIJKLMNOP
( ----- 269 )
.XXX.XXXX..XXX.XXXXXX...XX...XX...XX...XX...XXXXXXXXX.......
X...XX...XX...X..X..X...XX...XX...XX...XX...XX...XX....X....
X...XX...XX......X..X...XX...XX...X.X.X..X.X....X.X.....X...
X...XXXXX..XXX...X..X...XX...XX...X..X....X....X..X......X..
X.X.XX.X......X..X..X...XX...XX.X.X.X.X...X...X...X.......X.
X..XXX..X.X...X..X..X...X.X.X.X.X.XX...X..X..X...XX........X
.XXXXX...X.XXX...X...XXX...X...X.X.X...X..X..XXXXXXXX.......
..XXX..X.........X..........................................
....X.X.X.........X.........................................
....XX...X...........XXX.X.....XXX.....X.XXX..XX....XXXX....
....X...................XX....X...X....XX...XX..X..X..XX....
....X................XXXXXXX..X......XXXXXXXXX......XXXXXX..
....X...............X...XX..X.X...X.X..XX....XXX......XX..X.
..XXX.....XXXXX......XXXXXXX...XXX...XXX.XXXXX......XX.X..X.
QRSTUVWXYZ[\]^_`abcdefgh
( ----- 270 )
............................................................
............................................................
..X......XX..X..XX...X.X.XXX...XXX.XXX....XXXX.XX..XXX..X...
..........X.X....X..X.X.XX..X.X...XX..X..X..XXX...X....XXX..
..X......XXX.....X..X...XX...XX...XXXX....XXXX.....XXX..X...
..X...X..XX.X....X..X...XX...XX...XX........XX........X.X...
..X....XX.X..X...XX.X...XX...X.XXX.X........XX.....XXX...XX.
................................XX...X...XX.......
...............................X.....X.....X......
X...XX...XX...XX...XX...XXXXXX.X.....X.....X..X.X.
X...XX...XX...X.X.X..X.X....X.X......X......XX.X..
X...XX...XX...X..X....X....X...X.....X.....X......
X...X.X.X.X.X.X.X.X..X....X....X.....X.....X......
.XXX...X...X.X.X...XX....XXXXX..XX...X...XX.......
ijklmnopqrstuvwxyz{|}~
( ----- 271 )
..XX....XX.XX..XX.XX....XX..XX......XXX......XX.....XX...XX....
..XX....XX.XX..XX.XX..XXXXXXXX..XX.XX.XX....XX.....XX.....XX...
..XX....XX.XX.XXXXXXXXX.X......XX..XX.XX...XX.....XX.......XX..
..XX...........XX.XX..XXXXX...XX....XXX...........XX.......XX..
..XX..........XXXXXXX...X.XX.XX....XX.XX.X........XX.......XX..
...............XX.XX.XXXXXX.XX..XX.XX..XX..........XX.....XX...
..XX...........XX.XX...XX.......XX..XXX.XX..........XX...XX....
...........................................XXXX....XX....XXXX..
..XX.....XX............................XX.XX..XX..XXX...XX..XX.
XXXXXX...XX...........................XX..XX.XXX...XX.......XX.
.XXXX..XXXXXX........XXXXXX..........XX...XXXXXX...XX......XX..
XXXXXX...XX.........................XX....XXX.XX...XX.....XX...
..XX.....XX.....XX............XX...XX.....XX..XX...XX....XX....
...............XX.............XX...........XXXX..XXXXXX.XXXXXX.
!"#$%&'()*+,-./012
( ----- 272 )
.XXXX.....XX..XXXXXX...XXX..XXXXXX..XXXX...XXXX................
XX..XX...XXX..XX......XX........XX.XX..XX.XX..XX...............
....XX..XXXX..XXXXX..XX........XX..XX..XX.XX..XX...XX.....XX...
..XXX..XX.XX......XX.XXXXX....XX....XXXX...XXXXX...XX.....XX...
....XX.XXXXXX.....XX.XX..XX..XX....XX..XX.....XX...............
XX..XX....XX..XX..XX.XX..XX..XX....XX..XX....XX....XX.....XX...
.XXXX.....XX...XXXX...XXXX...XX.....XXXX...XXX.....XX....XX....
...XX.........XX......XXXX...XXXX...XXXX..XXXXX...XXXX..XXXX...
..XX...........XX....XX..XX.XX..XX.XX..XX.XX..XX.XX..XX.XX.XX..
.XX....XXXXXX...XX......XX..XX.XXX.XX..XX.XX..XX.XX.....XX..XX.
XX...............XX....XX...XX.X.X.XXXXXX.XXXXX..XX.....XX..XX.
.XX....XXXXXX...XX.....XX...XX.XXX.XX..XX.XX..XX.XX.....XX..XX.
..XX...........XX...........XX.....XX..XX.XX..XX.XX..XX.XX.XX..
...XX.........XX.......XX....XXXX..XX..XX.XXXXX...XXXX..XXXX...
3456789:;<=>?@ABCD
( ----- 273 )
XXXXXX.XXXXXX..XXXX..XX..XX.XXXXXX..XXXXX.XX..XX.XX.....XX...XX
XX.....XX.....XX..XX.XX..XX...XX......XX..XX.XX..XX.....XXX.XXX
XX.....XX.....XX.....XX..XX...XX......XX..XXXX...XX.....XXXXXXX
XXXXX..XXXXX..XX.XXX.XXXXXX...XX......XX..XXX....XX.....XX.X.XX
XX.....XX.....XX..XX.XX..XX...XX......XX..XXXX...XX.....XX.X.XX
XX.....XX.....XX..XX.XX..XX...XX...XX.XX..XX.XX..XX.....XX...XX
XXXXXX.XX......XXXX..XX..XX.XXXXXX..XXX...XX..XX.XXXXXX.XX...XX
XX..XX..XXXX..XXXXX...XXXX..XXXXX...XXXX..XXXXXX.XX..XX.XX..XX.
XX..XX.XX..XX.XX..XX.XX..XX.XX..XX.XX..XX...XX...XX..XX.XX..XX.
XXX.XX.XX..XX.XX..XX.XX..XX.XX..XX.XX.......XX...XX..XX.XX..XX.
XXXXXX.XX..XX.XXXXX..XX..XX.XXXXX...XXXX....XX...XX..XX.XX..XX.
XX.XXX.XX..XX.XX.....XX.X.X.XX.XX......XX...XX...XX..XX.XX..XX.
XX..XX.XX..XX.XX.....XX.XX..XX..XX.XX..XX...XX...XX..XX..XXXX..
XX..XX..XXXX..XX......XX.XX.XX..XX..XXXX....XX....XXXX....XX...
EFGHIJKLMNOPQRSTUVWXYZ[\]^_
( ----- 274 )
XX...XXXX..XX.XX..XX.XXXXXX.XXXXX.........XXXXX....XX..........
XX...XXXX..XX.XX..XX.....XX.XX.....XX........XX...XXXX.........
XX.X.XX.XXXX..XX..XX....XX..XX......XX.......XX..XX..XX........
XX.X.XX..XX....XXXX....XX...XX.......XX......XX..X....X........
XXXXXXX.XXXX....XX....XX....XX........XX.....XX................
XXX.XXXXX..XX...XX...XX.....XX.........XX....XX................
XX...XXXX..XX...XX...XXXXXX.XXXXX.........XXXXX.........XXXXXXX
.XX...........XX................XX..........XXX.........XX.....
..XX..........XX................XX.........XX.....XXXX..XX.....
...XX...XXXX..XXXXX...XXXX...XXXXX..XXXX...XX....XX..XX.XXXXX..
...........XX.XX..XX.XX..XX.XX..XX.XX..XX.XXXXX..XX..XX.XX..XX.
........XXXXX.XX..XX.XX.....XX..XX.XXXXXX..XX.....XXXXX.XX..XX.
.......XX..XX.XX..XX.XX..XX.XX..XX.XX......XX........XX.XX..XX.
........XXXXX.XXXXX...XXXX...XXXXX..XXXX...XX.....XXX...XX..XX.
WXYZ[\]^_`abcdefghijklmnopqrstuvwxyz{|}~
( ----- 275 )
..XX.....XX...XX......XXX......................................
..............XX.......XX......................................
.XXX....XXX...XX..XX...XX....XX.XX.XXXXX...XXXX..XXXXX...XXXXX.
..XX.....XX...XX.XX....XX...XXXXXXXXX..XX.XX..XX.XX..XX.XX..XX.
..XX.....XX...XXXX.....XX...XX.X.XXXX..XX.XX..XX.XX..XX.XX..XX.
..XX.....XX...XX.XX....XX...XX.X.XXXX..XX.XX..XX.XXXXX...XXXXX.
.XXXX..XX.....XX..XX..XXXX..XX...XXXX..XX..XXXX..XX.........XX.
...............XX..............................................
...............XX..............................................
XX.XX...XXXXX.XXXXX..XX..XX.XX..XX.XX...XXXX..XX.XX..XX.XXXXXX.
XXX.XX.XX......XX....XX..XX.XX..XX.XX.X.XX.XXXX..XX..XX....XX..
XX......XXXX...XX....XX..XX.XX..XX.XX.X.XX..XX...XX..XX...XX...
XX.........XX..XX....XX..XX..XXXX..XXXXXXX.XXXX...XXXXX..XX....
XX.....XXXXX....XXX...XXXXX...XX....XX.XX.XX..XX.....XX.XXXXXX.
ijklmnopqrstuvwxyz{|}~
( ----- 276 )
...XX....XX...XX......XX...X
..XX.....XX....XX....XX.X.XX
..XX.....XX....XX....X...XX.
XXX......XX.....XXX.........
..XX.....XX....XX...........
..XX.....XX....XX...........
...XX....XX...XX............
{|}~
( ----- 299 )
