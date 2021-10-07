( ----- 000 )
AVR MASTER INDEX

301 AVR macros                 302 AVR assembler
320 SMS PS/2 controller

( ----- 001 )
: AVRA ASML 302 312 LOADR ;
: ATMEGA328P 315 LOAD ;
( ----- 002 )
\ AVR assembler. See doc/asm/avr.txt.
\ We divide by 2 because each PC represents a word.
: PC HERE ORG - >> ;
: _oor ." arg out of range: " .X SPC> ." PC " PC .X NL> ABORT ;
: _r8c DUP 7 > IF _oor THEN ;
: _r32c DUP 31 > IF _oor THEN ;
: _r16+c _r32c DUP 16 < IF _oor THEN ;
: _r64c DUP 63 > IF _oor THEN ;
: _r256c DUP 255 > IF _oor THEN ;
: _Rdp ( op rd -- op', place Rd ) <<4 OR ;
( ----- 003 )
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
( ----- 004 )
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
( ----- 005 )
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
( ----- 006 )
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
( ----- 007 )
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
( ----- 008 )
( a -- k12, absolute addr a, relative to PC in a k12 addr )
: _r7ffc DUP $7ff > IF _oor THEN ;
: _raddr12
    PC - DUP 0< IF $800 + _r7ffc $800 OR ELSE _r7ffc THEN ;
: RJMP _raddr12 $c000 OR ;
: RCALL _raddr12 $d000 OR ;
: RJMP, RJMP L, ; : RCALL, RCALL L, ;
( ----- 009 )
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
( ----- 010 )
9 CONSTS $1c X $08 Y 0 Z
         $1d X+ $19 Y+ $11 Z+
         $1e -X $1a -Y $12 -Z
: _ldst ( Rd XYZ op ) SWAP DUP $10 AND <<8 SWAP $f AND
    OR OR ( Rd op' ) SWAP _Rdp L, ;
: LD, $8000 _ldst ; : ST, SWAP $8200 _ldst ;
( ----- 011 )
\ LBL! L1 .. L1 ' RJMP LBL,
: LBL! ( -- ) PC TO ;
: LBL, ( opw pc -- ) 1- SWAP EXECUTE L, ;
: SKIP, PC 0 L, ;
: TO, ( opw pc )
  \ warning: pc is a PC offset, not a mem addr!
  << ORG + PC 1- HERE ( opw addr tgt hbkp )
  ROT 'HERE ! ( opw tgt hbkp )
  SWAP ROT EXECUTE HERE ! ( hbkp ) 'HERE ! ;
\ FLBL, L1 .. ' RJMP L1 TO,
: FLBL, LBL! 0 L, ;
: BEGIN, PC ; : AGAIN?, ( pc op ) SWAP LBL, ;
: AGAIN, ['] RJMP AGAIN?, ;
: IF, ['] BREQ SKIP, ; : THEN, TO, ;
( ----- 012 )
\ Constant common to all AVR models
38 CONSTS 0 R0 1 R1 2 R2 3 R3 4 R4 5 R5 6 R6 7 R7 8 R8 9 R9
  10 R10 11 R11 12 R12 13 R13 14 R14 15 R15 16 R16 17 R17
  18 R18 19 R19 20 R20 21 R21 22 R22 23 R23 24 R24 25 R25
  26 R26 27 R27 28 R28 29 R29 30 R30 31 R31
  26 XL 27 XH 28 YL 29 YH 30 ZL 31 ZH
( ----- 015 )
( ATmega328P definitions ) 87 CONSTS
$c6 UDR0 $c4 UBRR0L $c5 UBRR0H $c2 UCSR0C $c1 UCSR0B $c0 UCSR0A
$bd TWAMR $bc TWCR $bb TWDR $ba TWAR $b9 TWSR $b8 TWBR $b6 ASSR
$b4 OCR2B $b3 OCR2A $b2 TCNT2 $b1 TCCR2B $b0 TCCR2A $8a OCR1BL
$8b OCR1BH $88 OCR1AL $89 OCR1AH $86 ICR1L $87 ICR1H $84 TCNT1L
$85 TCNT1H $82 TCCR1C $81 TCCR1B $80 TCCR1A $7f DIDR1 $7e DIDR0
$7c ADMUX $7b ADCSRB $7a ADCSRA $79 ADCH $78 ADCL $70 TIMSK2
$6f TIMSK1 $6e TIMSK0 $6c PCMSK1 $6d PCMSK2 $6b PCMSK0 $69 EICRA
$68 PCICR $66 OSCCAL $64 PRR $61 CLKPR $60 WDTCSR $3f SREG
$3d SPL $3e SPH $37 SPMCSR $35 MCUCR $34 MCUSR $33 SMCR $30 ACSR
$2e SPDR $2d SPSR $2c SPCR $2b GPIOR2 $2a GPIOR1 $28 OCR0B
$27 OCR0A $26 TCNT0 $25 TCCR0B $24 TCCR0A $23 GTCCR $22 EEARH
$21 EEARL $20 EEDR $1f EECR $1e GPIOR0 $1d EIMSK $1c EIFR
$1b PCIFR $17 TIFR2 $16 TIFR1 $15 TIFR0 $0b PORTD $0a DDRD
$09 PIND $08 PORTC $07 DDRC $06 PINC $05 PORTB $04 DDRB $03 PINB
( ----- 020 )
SMS PS/2 controller (doc/hw/z80/sms)

To assemble, load the AVR assembler with AVRA, then
"324 342 LOADR".

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
( ----- 021 )
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
( ----- 022 )
R19: Register used for parity computations and tmp value in
     some other places
R20: data being sent to the '164
Y: pointer to the memory location where the next scan code from
   ps/2 will be written.
Z: pointer to the next scan code to push to the 595
( ----- 024 )
18 CONSTS $0060 SRAM_START $015f RAMEND $3d SPL $3e SPH
          $11 GPIOR0 $35 MCUCR $33 TCCR0B $3b GIMSK
          $38 TIFR $32 TCNT0 $16 PINB $17 DDRB $18 PORTB
          2 CLK 1 DATA 3 CP 0 LQ 4 LR
$100 100 - CONSTANT TIMER_INITVAL
\ We need a lot of labels in this program...
5 VALUES L4 L5 L6 L7 L8
( ----- 025 )
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
( ----- 026 )
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
( ----- 027 )
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
( ----- 028 )
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
( ----- 029 )
\ step 3: stop bit
R18 CLR, \ happens in all cases
\ DATA has to be set
R19 TST, \ was DATA set?
' BREQ L1 LBL, \ loop, not set? error, don't push to buf
\ push r17 to the buffer
Y+ R17 ST,
FLBL, L8 \ RCALL checkBoundsY
' RJMP L1 LBL, \ loop
( ----- 030 )
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
( ----- 031 )
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
( ----- 032 )
' BREQ L7 TO, \ processbits2
\ step 2 - parity bit
R1 R19 MOV,
R19 R17 MOV,
FLBL, L5 \ RCALL checkParity
R1 R16 CP,
FLBL, L6 \ BRNE processBitError, r1 != r16? wrong parity
R18 INC,
' RJMP L1 LBL, \ loop
( ----- 033 )
' BRNE L6 TO, \ processBitError
R18 CLR,
R19 $fe LDI,
FLBL, L6 \ RCALL sendToPS2
' RJMP L1 LBL, \ loop

' RJMP L4 TO, \ processbitReset
R18 CLR,
FLBL, L4 \ RCALL resetTimer
' RJMP L1 LBL, \ loop
( ----- 034 )
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
( ----- 035 )
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
( ----- 036 )
' RCALL L2 TO, ' RCALL L4 TO, LBL! L2 \ resetTimer
R16 TIMER_INITVAL LDI,
TCNT0 R16 OUT,
R16 $02 ( TOV0 ) LDI,
TIFR R16 OUT,
RET,
( ----- 037 )
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
( ----- 038 )
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
( ----- 039 )
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
( ----- 040 )
\ We're finished! Enable INT0, reset timer, everything back to
\ normal!
' RCALL L2 LBL, \ resetTimer
CLT, \ also, make sure T isn't mistakely set.
SEI,
RET,
( ----- 041 )
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
( ----- 042 )
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
