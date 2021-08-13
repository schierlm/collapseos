( ----- 000 )
MASTER INDEX

002 Common assembler words    005 Z80 assembler
020 8086 assembler            030 AVR assembler
050 6809 assembler            60-99 unused
100 Block editor              115 Memory Editor
120 Useful little words
130-149 unused                150 Remote Shell
160 AVR SPI programmer        165 Sega ROM signer
170-199 unused                200 Cross compilation
210 Core words                230 BLK subsystem
240 Grid subsystem            245 PS/2 keyboard subsystem
250 SD Card subsystem         260 Fonts
280 Z80 boot code             310 Z80 drivers
400 8086 boot code            420 8086 drivers             cont.
( ----- 001 )
MASTER INDEX (cont.)

450 6809 boot code            460 6809 drivers
500 AVR firmware
( ----- 002 )
\ Common assembler words, low
3 VALUES ORG 0 BIN( 0 BIGEND? 0
: PC HERE ORG - BIN( + ;
: <<3 << << << ; : <<4 <<3 << ;
4 VALUES L1 0 L2 0 L3 0 lblnext 0
: |T L|M BIGEND? NOT IF SWAP THEN ;
: T! ( n a -- ) SWAP |T ROT C!+ C! ;
: T, ( n -- ) |T C, C, ;
: T@ C@+ SWAP C@ BIGEND? IF SWAP THEN <<8 OR ;
: lblnext? lblnext ?DUP NOT IF ABORT" no lblnext!" THEN ;
\ BIN( is in increments of $100 and EXIT is always within $100.
: LIVETGT ['] EXIT >>8 <<8 DUP [TO] BIN( DUP [TO] ORG
  $f + @ [TO] lblnext ;
( ----- 003 )
\ Common assembler words, high
: CODE ENTRY 0 ( native ) C, ;
: CODE[ ( -- a )
  COMPILE (c) HERE 1 ALLOT INTERPRET ; IMMEDIATE
: ]CODE ( a -- ) ;CODE DUP HERE -^ 1- _bchk SWAP C! 2R> 2DROP ;
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
: OPXY CREATE , DOES> @ ( xyoff opref ) EXECUTE C, ;
( ----- 006 )
: OP1 CREATE C, DOES> C@ C, ;
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
\ Relative jumps are a bit special. They're supposed to take
\ an argument, but they don't take it so they can work with
\ the label system. Therefore, relative jumps are an OP1 but
\ when you use them, you're expected to write the offset
\ afterwards yourself.
$18 OP1 JR,                   $10 OP1 DJNZ,
$38 OP1 JRC,                  $30 OP1 JRNC,
$28 OP1 JRZ,                  $20 OP1 JRNZ,
( ----- 008 )
: OP1r CREATE C, DOES> C@ ( r op ) SWAP <<3 OR C, ;
$04 OP1r INCr,                $05 OP1r DECr,
' INCr, OPXY INC(IXY+),        ' DECr, OPXY DEC(IXY+),
\ OP1r also works for conditions
$c0 OP1r RETc,

: OP1r0 CREATE C, DOES> C@ ( r op ) OR C, ;
$80 OP1r0 ADDr,               $88 OP1r0 ADCr,
$a0 OP1r0 ANDr,               $b8 OP1r0 CPr,
$b0 OP1r0 ORr,                $90 OP1r0 SUBr,
$98 OP1r0 SBCr,               $a8 OP1r0 XORr,
' CPr, OPXY CP(IXY+),
( ----- 009 )
: OP1d CREATE C, DOES> C@ ( d op ) SWAP <<4 OR C, ;
$c5 OP1d PUSH,                $c1 OP1d POP,
$03 OP1d INCd,                $0b OP1d DECd,
$09 OP1d ADDHLd,
: ADDIXd, IX DROP ADDHLd, ;  : ADDIXIX, HL ADDIXd, ;
: ADDIYd, IY DROP ADDHLd, ;  : ADDIYIY, HL ADDIYd, ;

: LDrr, ( rd rr ) SWAP <<3 OR $40 OR C, ;
' LDrr, OPXY LDIXYr,
: LDrIXY, ( rd ixy+- HL ) ROT SWAP LDIXYr, ;
: LDri, ( r i ) SWAP <<3 $06 OR C, C, ;
: LDdi, ( d n ) SWAP <<4 $01 OR C, T, ;
: LDd(i), ( d i ) $ed C, SWAP <<4 $4b OR C, T, ;
: LD(i)d, ( i d ) $ed C, <<4 $43 OR C, T, ;
( ----- 010 )
: OPED CREATE C, DOES> $ed C, C@ C, ;
$a1 OPED CPI,       $b1 OPED CPIR,     $a9 OPED CPD,
$b9 OPED CPDR,      $46 OPED IM0,      $56 OPED IM1,
$5e OPED IM2,       $a0 OPED LDI,      $b0 OPED LDIR,
$a8 OPED LDD,       $b8 OPED LDDR,     $44 OPED NEG,
$4d OPED RETI,      $45 OPED RETN,     $a2 OPED INI,
$aa OPED IND,       $a3 OPED OUTI,

: OP2i CREATE C, DOES> C@ ( i op ) C, C, ;
$d3 OP2i OUTiA,     $db OP2i INAi,     $c6 OP2i ADDi,
$e6 OP2i ANDi,      $f6 OP2i ORi,      $d6 OP2i SUBi,
$ee OP2i XORi,      $fe OP2i CPi,

: OP2br CREATE C, DOES>
    $cb C, C@ ( b r op ) ROT <<3 OR OR C, ;
$c0 OP2br SET,      $80 OP2br RES,     $40 OP2br BIT,
( ----- 011 )
\ bitwise rotation ops have a similar sig
: OProt CREATE C, DOES> $cb C, C@ ( r op ) OR C, ;
$10 OProt RL,       $00 OProt RLC,     $18 OProt RR,
$08 OProt RRC,      $20 OProt SLA,     $38 OProt SRL,

\ cell contains both bytes. MSB is spit as-is, LSB is ORed
\ with r.
: OP2r CREATE , DOES> @ L|M ( r lsb msb ) C, SWAP <<3 OR C, ;
$ed41 OP2r OUT(C)r, $ed40 OP2r INr(C),

: OP2d CREATE C, DOES> $ed C, C@ ( d op ) SWAP <<4 OR C, ;
$4a OP2d ADCHLd,    $42 OP2d SBCHLd,
( ----- 012 )
: OP3i CREATE C, DOES> C@ ( i op ) C, T, ;
$cd OP3i CALL,                $c3 OP3i JP,
$22 OP3i LD(i)HL,             $2a OP3i LDHL(i),
$32 OP3i LD(i)A,              $3a OP3i LDA(i),

: RST, $c7 OR C, ;
: JP(IX), IX DROP JP(HL), ;
: JP(IY), IY DROP JP(HL), ;
: JPc, SWAP <<3 $c2 OR C, T, ;
: CALLc, SWAP <<3 $c4 OR C, T, ;
: ;CODE lblnext? JP, ;
( ----- 013 )
\ Place BEGIN, where you want to jump back and AGAIN after
\ a relative jump operator. Just like BSET and BWR.
: BEGIN, PC ;
: BSET BEGIN, TO ;
\ same as BSET, but we need to write a placeholder
: FJR, BEGIN, 0 C, ;
: IFZ, JRNZ, FJR, ;
: IFNZ, JRZ, FJR, ;
: IFC, JRNC, FJR, ;
: IFNC, JRC, FJR, ;
: THEN,
  DUP PC ( l l pc ) -^ 1- ( l off )
  \ warning: l is a PC offset, not a mem addr!
  SWAP ORG + BIN( - ( off addr ) C! ;
: ELSE, JR, FJR, SWAP THEN, ;
( ----- 014 )
: FWR BSET 0 C, ;
: FSET ' EXECUTE THEN, ;
: AGAIN, PC - 1- _bchk C, ;
: BWR ' EXECUTE AGAIN, ;
( ----- 015 )
\ Macros
: SUBHLd, A ORr, SBCHLd, ; \ clear carry + SBC
: PUSH0, BC 0 LDdi, BC PUSH, ;
: PUSH1, BC 1 LDdi, BC PUSH, ;
: PUSHZ, BC 0 LDdi, IFZ, C INCr, THEN, BC PUSH, ;
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
: OP1 CREATE C, DOES> C@ C, ;
$c3 OP1 RET,        $fa OP1 CLI,       $fb OP1 STI,
$f4 OP1 HLT,        $fc OP1 CLD,       $fd OP1 STD,
$90 OP1 NOP,        $98 OP1 CBW,
$f3 OP1 REPZ,       $f2 OP1 REPNZ,     $ac OP1 LODSB,
$ad OP1 LODSW,      $a6 OP1 CMPSB,     $a7 OP1 CMPSW,
$a4 OP1 MOVSB,      $a5 OP1 MOVSW,     $ae OP1 SCASB,
$af OP1 SCASW,      $aa OP1 STOSB,     $ab OP1 STOSW,
( no argument, jumps with relative addrs are special )
$eb OP1 JMPs,       $e9 OP1 JMPn,      $74 OP1 JZ,
$75 OP1 JNZ,        $72 OP1 JC,        $73 OP1 JNC,
$e8 OP1 CALL,

: OP1r CREATE C, DOES> C@ + C, ;
$40 OP1r INCx,      $48 OP1r DECx,
$58 OP1r POPx,      $50 OP1r PUSHx,
( ----- 022 )
: OPr0 ( reg op ) CREATE C, C, DOES>
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
: OPrr CREATE C, DOES> C@ C, <<3 OR $c0 OR C, ;
$31 OPrr XORxx,     $30 OPrr XORrr,
$88 OPrr MOVrr,     $89 OPrr MOVxx,    $28 OPrr SUBrr,
$29 OPrr SUBxx,     $08 OPrr ORrr,     $09 OPrr ORxx,
$38 OPrr CMPrr,     $39 OPrr CMPxx,    $00 OPrr ADDrr,
$01 OPrr ADDxx,     $20 OPrr ANDrr,    $21 OPrr ANDxx,
( ----- 024 )
: OPmodrm ( opbase modrmbase ) CREATE C, C, DOES>
    @ L|M ( disp? modrm opoff modrmbase opbase ) ROT + C,
    ( disp? modrm modrmbase ) + DUP C, ( disp? modrm )
    $c0 AND DUP IF ( has disp ) $80 AND IF
        ( disp low+high ) T, ELSE ( low only ) C, THEN
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

: OPi CREATE C, DOES> C@ C, C, ;
$04 OPi ADDALi,     $24 OPi ANDALi,    $2c OPi SUBALi,
$cd OPi INT,
: OPI CREATE C, DOES> C@ C, T, ;
$05 OPI ADDAXI,     $25 OPI ANDAXI,    $2d OPI SUBAXI,
( ----- 026 )
: CMPri, $80 C, SWAP $f8 OR C, C, ;
: CMPxI, $81 C, SWAP $f8 OR C, T, ;
: CMPxi, $83 C, SWAP $f8 OR C, C, ;
: MOVri, SWAP $b0 OR C, C, ; : MOVxI, SWAP $b8 OR C, T, ;
: MOVsx, $8e C, SWAP <<3 OR $c0 OR C, ;
: MOVrm, $8a C, SWAP <<3 $6 OR C, T, ;
: MOVxm, $8b C, SWAP <<3 $6 OR C, T, ;
: MOVmr, $88 C, <<3 $6 OR C, T, ;
: MOVmx, $89 C, <<3 $6 OR C, T, ;
: PUSHs, <<3 $06 OR C, ; : POPs, <<3 $07 OR C, ;
: SUBxi, $83 C, SWAP $e8 OR C, C, ;
: ADDxi, $83 C, SWAP $c0 OR C, C, ;
: JMPr, $ff C, 7 AND $e0 OR C, ;
: JMPf, ( seg off ) $ea C, T, T, ;
( ----- 027 )
\ Place BEGIN, where you want to jump back and AGAIN after
\ a relative jump operator. Just like BSET and BWR.
: BEGIN, PC ;
: BSET PC TO ;
\ same as BSET, but we need to write a placeholder
: FJR, PC 0 C, ;
: IFZ, JNZ, FJR, ;
: IFNZ, JZ, FJR, ;
: IFC, JNC, FJR, ;
: IFNC, JC, FJR, ;
: THEN,
  DUP PC ( l l pc ) -^ 1- ( l off )
  \ warning: l is a PC offset, not a mem addr!
  SWAP ORG + BIN( - ( off addr ) C! ;
( ----- 028 )
: FWRs BSET 0 C, ;
: FSET ' EXECUTE THEN, ;
: RPCs, PC - 1- _bchk C, ;
: RPCn, PC - 2 - T, ;
: AGAIN, RPCs, ;
\ Use RPCx with appropriate JMP/CALL op. Example:
\ JMPs, $42 RPCs, or CALL, $1234 RPCn,
: PUSHZ, CX 0 MOVxI, IFZ, CX INCx, THEN, CX PUSHx, ;
: ;CODE JMPn, lblnext? RPCn, ;
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
: OPRd CREATE , DOES> @ SWAP _r32c _Rdp T, ;
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
: OPRdRr CREATE C, DOES> C@ ( rd rr op )
    OVER _r32c $10 AND >> >> >> OR ( rd rr op' )
    <<8 OR $ff0f AND ( rd op' )
    SWAP _r32c _Rdp T, ;
$1c OPRdRr ADC,   $0c OPRdRr ADD,    $20 OPRdRr AND,
$14 OPRdRr CP,    $04 OPRdRr CPC,    $10 OPRdRr CPSE,
$24 OPRdRr EOR,   $2c OPRdRr MOV,    $9c OPRdRr MUL,
$28 OPRdRr OR,    $08 OPRdRr SBC,    $18 OPRdRr SUB,

( 0000 0AAd dddd AAAA )
: OPRdA CREATE C, DOES> C@ ( rd A op )
    OVER _r64c $30 AND >> >> >> OR ( rd A op' )
    <<8 OR $ff0f AND ( rd op' ) SWAP _r32c _Rdp T, ;
$b0 OPRdA IN,     $b8 OPRdA _ : OUT, SWAP _ ;
( ----- 033 )
( 0000 KKKK dddd KKKK )
: OPRdK CREATE C, DOES> C@ ( rd K op )
    OVER _r256c $f0 AND >> >> >> >> OR ( rd K op' )
    ROT _r16+c <<4 ROT $0f AND OR ( op' rdK ) C, C, ;
$70 OPRdK ANDI,   $30 OPRdK CPI,     $e0 OPRdK LDI,
$60 OPRdK ORI,    $40 OPRdK SBCI,    $60 OPRdK SBR,
$50 OPRdK SUBI,

( 0000 0000 AAAA Abbb )
: OPAb CREATE C, DOES> C@ ( A b op )
    ROT _r32c <<3 ROT _r8c OR C, C, ;
$98 OPAb CBI,     $9a OPAb SBI,      $99 OPAb SBIC,
$9b OPAb SBIS,
( ----- 034 )
: OPNA CREATE , DOES> @ T, ;
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
: OPb CREATE , DOES> @ ( b op )
    SWAP _r8c _Rdp T, ;
$9488 OPb BCLR,   $9408 OPb BSET,

( 0000 000d dddd 0bbb )
: OPRdb CREATE , DOES> @ ( rd b op )
    ROT _r32c _Rdp SWAP _r8c OR T, ;
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
: RJMP, RJMP T, ; : RCALL, RCALL T, ;
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
    OR OR ( Rd op' ) SWAP _Rdp T, ;
: LD, $8000 _ldst ; : ST, SWAP $8200 _ldst ;
( ----- 039 )
\ LBL! L1 .. L1 ' RJMP LBL,
: LBL! ( -- ) PC TO ;
: LBL, ( opw pc -- ) 1- SWAP EXECUTE T, ;
: SKIP, PC 0 T, ;
: TO, ( opw pc )
  \ warning: pc is a PC offset, not a mem addr!
  << ORG + PC 1- HERE ( opw addr tgt hbkp )
  ROT [*TO] HERE ( opw tgt hbkp )
  SWAP ROT EXECUTE HERE ! ( hbkp ) [*TO] HERE ;
\ FLBL, L1 .. ' RJMP L1 TO,
: FLBL, LBL! 0 T, ;
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
: () ( n ) |T 2 $30 ; ( Extended )
: [] ( n ) |T $9f 3 $20 ; ( Extended Indirect)
( Offset Indexed. We auto-detect 0, 5-bit, 8-bit, 16-bit )
: _0? ?DUP IF 1 ELSE $84 1 0 THEN ;
: _5? DUP $10 + $1f > IF 1 ELSE $1f AND 1 0 THEN ;
: _8? DUP $80 + $ff > IF 1 ELSE <<8 >>8 $88 2 0 THEN ;
: _16 |T $89 3 ;
( ----- 051 )
: R+N CREATE C, DOES> C@ ( roff ) >R
    _0? IF _5? IF _8? IF _16 THEN THEN THEN
    SWAP R> ( roff ) OR SWAP $20 ;
: R+K CREATE C, DOES> C@ 1 $20 ;
: PCR+N ( n ) _8? IF _16 THEN SWAP $8c OR SWAP $20 ;
: [R+N] CREATE C, DOES> C@ $10 OR ( roff ) >R
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
: ,? DUP $ff > IF T, ELSE C, THEN ;
: ,N ( cnt ) 0 DO C, LOOP ;
: OPINH ( inherent ) CREATE , DOES> @ ,? ;
( Targets A or B )
: OP1 CREATE , DOES> @ ( n2? n1 nc opoff op ) + ,? ,N ;
( Targets D/X/Y/S/U. Same as OP1, but spit 2b immediate )
: OP2 CREATE , DOES> @ OVER + ,? IF ,N ELSE DROP T, THEN ;
( Targets memory only. opoff scheme is different than OP1/2 )
: OPMT CREATE , DOES> @
    SWAP $10 - ?DUP IF $50 + + THEN ,? ,N ;
( Targets 2 regs )
: OPRR ( src tgt -- ) CREATE C, DOES> C@ C, SWAP <<4 OR C, ;
' OPINH ALIAS OPBR
CREATE wbr 0 C, ( wide BR? ) : wbr? wbr C@ 0 wbr C! ;
: OPLBR CREATE , DOES> @ ,? 1 wbr C! ;
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

$24 OPBR BCC,        $1024 OPLBR LBCC,  $25 OPBR BCS,
$1025 OPLBR LBCS,    $27 OPBR BEQ,      $1027 OPLBR LBEQ,
$2c OPBR BGE,        $102c OPLBR LBGE,  $2e OPBR BGT,
$102e OPLBR LBGT,    $22 OPBR BHI,      $1022 OPLBR LBHI,
$24 OPBR BHS,        $1024 OPLBR LBHS,  $2f OPBR BLE,
$102f OPLBR LBLE,    $25 OPBR BLO,      $1025 OPLBR LBLO,
$23 OPBR BLS,        $1023 OPLBR LBLS,  $2d OPBR BLT,
$102d OPLBR LBLT,    $2b OPBR BMI,      $102b OPLBR LBMI,
( ----- 057 )
$26 OPBR BNE,        $1026 OPLBR LBNE,  $2a OPBR BPL,
$102a OPLBR LBPL,    $20 OPBR BRA,      $16 OPLBR LBRA,
$21 OPBR BRN,        $1021 OPLBR BRN,   $8d OPBR BSR,
$17 OPLBR LBSR,      $28 OPBR BVC,      $1028 OPLBR LBVC,
$29 OPBR BVS,        $1029 OPLBR LBVS,

: _ ( r c cref mask -- r c ) ROT> OVER = ( r mask c f )
    IF ROT> OR SWAP ELSE NIP THEN ;
: OPP CREATE C, DOES> C@ C, 0 TOWORD BEGIN ( r c )
    '$' $80 _ 'S' $40 _ 'U' $40 _ 'Y' $20 _ 'X' $10 _
    '%' $08 _ 'B' $04 _ 'A' $02 _ 'C' $01 _ 'D' $06 _
    '@' $ff _ DROP IN< DUP WS? UNTIL DROP C, ;
$34 OPP PSHS, $36 OPP PSHU, $35 OPP PULS, $37 OPP PULU,
( ----- 058 )
: BEGIN, ( -- a ) HERE ;
: BSET ( -- ) BEGIN, TO ;
: LPC ( -- ) ' EXECUTE ORG - BIN( + ;
: AGAIN, ( a -- ) HERE - 1- wbr? IF 1- T, ELSE _bchk C, THEN ;
: BBR, ( a -- ) AGAIN, ;
\ same as BSET, but we need to write a placeholder. we need to
\ remember wbr? value so we put it in the placeholder. )
: FBR, ( -- a ) BEGIN, wbr? DUP C, IF 0 C, THEN ;
: IFWORD ( -- a ) CREATE , DOES> @ EXECUTE FBR, ;
: THEN, ( a -- ) DUP HERE -^ 1- ( l off ) OVER C@ ( l off wbr )
    IF 1- SWAP T! ELSE _bchk SWAP C! THEN ;
: ELSE, BRA, FBR, SWAP THEN, ;
: FSET ' EXECUTE THEN, ;
: ;CODE LBRA, lblnext BBR, ;
( ----- 059 )

' BNE, IFWORD IFZ,   ' BEQ, IFWORD IFNZ,
' BCC, IFWORD IFCS,  ' BCS, IFWORD IFCC,
' IFZ, ALIAS IF=,       ' IFNZ, ALIAS IF!=,
' BHS, IFWORD IF<,   ' BHI, IFWORD IF<=,
' BLS, IFWORD IF>,   ' BLO, IFWORD IF>=,
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
  SWAP COLS * 3 + 3 COLS -^ nspcs ( buf c ) OVER _zbuf
  OVER 1+ LNSZ RANGE DO ( buf c )
    DUP EMIT I C! ( buf ) DUP C@ 1+ OVER C! ( inc len )
    KEY DUP SPC < IF LEAVE THEN LOOP 2DROP ;
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
: SCNT 'S S0 -^ >> ;
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
  AWIDTH >> 5 * + ELSE >> CARRY? << SWAP 5 * + THEN 4 + ( y x )
  SWAP AT-XY ;
( ----- 117 )
\ Memory Editor, addr! pos! status type typep
: addr! $fff0 AND [TO] ADDR contents ;
: pos! DUP 0< IF PAGE + THEN DUP PAGE >= IF PAGE - THEN
  [TO] POS showpos ;
: status 0 COLS nspcs
  0 0 AT-XY ." A: " ADDR .X SPC> ." C: " POS .X SPC> ." S: "
  SCNT ?DUP IF 0 DO 'S I << + @ .X SPC> LOOP THEN POS pos! ;
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
\ Useful little words. nC, MIN MAX
\ parse the next n words and write them as chars
: nC, ( n -- ) 0 DO RUN1 C, LOOP ;
: MIN ( n n - n ) 2DUP > IF SWAP THEN DROP ;
: MAX ( n n - n ) 2DUP < IF SWAP THEN DROP ;
\ Compute CRC16 over a memory range
: CRC16[] ( a u -- c ) 0 ROT> RANGE DO I C@ CRC16 LOOP ;
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
: context CREATE CURRENT , DOES> ( a -- )
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
: XCOMPH 201 205 LOADR ; : FONTC 262 263 LOADR ;
: COREL 210 224 LOADR ; : COREH 226 229 LOADR ;
: BLKSUB 230 234 LOADR ; : GRIDSUB 240 241 LOADR ;
: PS2SUB 246 248 LOADR ;
: Z80M 280 LOAD ; : Z80C 281 299 LOADR ;
: 8086M 400 LOAD ; : 8086C 402 417 LOADR ;
: 6809M 450 LOAD ; : 6809C 451 459 LOADR ;
\ Where the BLK buffer will live.
SYSVARS 1024 - VALUE BLK_ADDR
'? HERESTART NOT [IF] 0 VALUE HERESTART [THEN]
0 VALUE XCURRENT \ CURRENT in target system, in target's addr
CREATE (n)* 0 , CREATE (b)* 0 , CREATE 2>R* 0 ,
CREATE (loop)* 0 , CREATE (br)* 0 , CREATE (?br)* 0 ,
CREATE EXIT* 0 ,
( ----- 201 )
: _xoff ORG BIN( - ;
: XENTRY
  WORD TUCK MOVE, XCURRENT T, C, HERE _xoff - [TO] XCURRENT ;
: XCREATE XENTRY 2 C, ;
: ALIAS XENTRY 4 C, T, ; : *ALIAS XENTRY $84 C, T, ;
: VALUE XENTRY 8 C, T, ; : *VALUE XENTRY $88 C, T, ;
: VALUES 0 DO XENTRY 8 C, RUN1 T, LOOP ;
: W= ( sa sl w -- f ) 2DUP 1- C@ $7f AND = IF ( same len )
    ( sa sl w ) OVER - 3 - ( s+1 len w-3-len ) ROT> []=
    ELSE 2DROP DROP 0 THEN ;
: _xfind ( sa sl -- w? f ) PAD C!+ ! XCURRENT BEGIN ( w )
  _xoff + DUP PAD C@+ SWAP @ SWAP ROT W= IF ( w ) 1 EXIT THEN
  3 - ( prev field ) T@ ?DUP NOT UNTIL 0 ( not found ) ;
: XFIND ( sa sl -- w ) _xfind NOT IF (wnf) THEN _xoff - ;
: '? WORD _xfind DUP IF SWAP _xoff - SWAP THEN ;
: X' '? NOT IF (wnf) THEN ;
( ----- 202 )
: _codecheck ( lbl str -- )
    XCURRENT _xoff + W=
    IF XCURRENT SWAP ! ELSE DROP THEN ;
: CODE XENTRY 0 ( native ) C,
    EXIT* LIT" EXIT" _codecheck
    (b)* LIT" (b)" _codecheck
    (n)* LIT" (n)" _codecheck
    2>R* LIT" 2>R" _codecheck
    (loop)* LIT" (loop)" _codecheck
    (br)* LIT" (br)" _codecheck
    (?br)* LIT" (?br)" _codecheck ;
: XWRAP
  COREH HERESTART ?DUP NOT IF PC THEN ORG 8 ( LATEST ) + T!
  XCURRENT ORG 6 ( CURRENT ) + T! ;
( ----- 203 )
: LITN DUP $ff > IF (n)* @ T, T, ELSE (b)* @ T, C, THEN ;
: imm? ( w -- f ) 1- C@ $80 AND ;
: X: XENTRY 1 ( compiled ) C, BEGIN
  WORD LIT" ;" S= IF EXIT* @ T, EXIT THEN
  CURWORD PARSE IF LITN ELSE CURWORD _xfind IF ( w )
    DUP imm? IF ABORT" immed!" THEN _xoff - T,
  ELSE CURWORD FIND IF ( w )
    DUP imm? IF EXECUTE ELSE (wnf) THEN
    ELSE (wnf) THEN
  THEN ( _xfind ) THEN ( PARSE ) AGAIN ;
( ----- 204 )
: X['] WORD XFIND LITN ;
: X, LIT" ," XFIND T, ;
: COMPILE X['] X, ; IMMEDIATE
: DO 2>R* @ T, HERE ; IMMEDIATE
: LOOP (loop)* @ T, HERE - C, ; IMMEDIATE
: IF (?br)* @ T, HERE 1 ALLOT ; IMMEDIATE
: ELSE (br)* @ T, 1 ALLOT [COMPILE] THEN HERE 1- ; IMMEDIATE
: AGAIN (br)* @ T, HERE - C, ; IMMEDIATE
: UNTIL (?br)* @ T, HERE - C, ; IMMEDIATE
( ----- 205 )
: [*TO] X' LITN LIT" *VAL!" XFIND T, ; IMMEDIATE
: ['] X['] ; IMMEDIATE
: LIT" (br)* @ T, HERE 1 ALLOT HERE ," TUCK HERE -^ SWAP
  [COMPILE] THEN SWAP _xoff - LITN LITN ; IMMEDIATE
: [COMPILE] WORD XFIND T, ; IMMEDIATE
: IMMEDIATE XCURRENT _xoff + 1- DUP C@ $80 OR SWAP C! ;
: ENTRY XENTRY ; : CREATE XCREATE ;
: : [ ' X: , ] ;
( ----- 210 )
\ Core Forth words. See doc/cross.txt.
\ Load range low: B210-B224 high: B226-B229
: NOOP ;
SYSVARS $02 + *VALUE CURRENT
SYSVARS $04 + *VALUE HERE
SYSVARS *VALUE IOERR
PS_ADDR VALUE S0
RS_ADDR VALUE R0
\ size of a line. used for INBUF and BLK. Keep this to $40 or
\ you're gonna have a bad time.
$40 VALUE LNSZ
: PAD HERE $40 + ;
( ----- 211 )
\ Core words, low level words
: > SWAP < ; : 0< $7fff > ; : >= < NOT ; : <= > NOT ;
: =><= ( n l h -- f ) OVER - ROT> ( h n l ) - >= ;
: 2DUP OVER OVER ; : 2DROP DROP DROP ;
: NIP SWAP DROP ; : TUCK SWAP OVER ;
: L|M DUP <<8 >>8 SWAP >>8 ;
: RSHIFT ?DUP IF 0 DO >> LOOP THEN ;
: LSHIFT ?DUP IF 0 DO << LOOP THEN ;
: -^ SWAP - ;
: C@+ ( a -- a+1 c ) DUP C@ SWAP 1+ SWAP ;
: C!+ ( c a -- a+1 ) TUCK C! 1+ ;
: LEAVE R> R> DROP I 1- >R >R ; : UNLOOP R> 2R> 2DROP >R ;
( ----- 212 )
\ Core words, low level words
: VAL! ( n addr -- ) 1+ ! ;
: *VAL! ( n addr -- ) 1+ @ ! ;
: / /MOD NIP ;
: MOD /MOD DROP ;
: ALLOT HERE + [*TO] HERE ;
: RANGE ( a u -- ah al ) OVER + SWAP ;
: ALLOT0 ( u -- ) HERE OVER 0 FILL ALLOT ;
: IMMEDIATE CURRENT 1- DUP C@ 128 OR SWAP C! ;
: MOVE, ( a u -- ) HERE OVER ALLOT SWAP MOVE ;
: , HERE ! 2 ALLOT ;
: C, HERE C! 1 ALLOT ;
( ----- 213 )
\ Core words, we begin EMITting
SYSVARS $0e + *ALIAS EMIT
: STYPE RANGE DO I C@ EMIT LOOP ;
5 VALUES EOT $04 BS $08 LF $0a CR $0d SPC $20
SYSVARS $0a + *VALUE NL
: SPC> SPC EMIT ;
: NL> NL L|M ?DUP IF EMIT THEN EMIT ;
: STACK? 'S S0 -^ 0< S0 @ OR S0 2 + @ OR IF
  LIT" stack underflow" STYPE ABORT THEN ;
( ----- 214 )
\ Core words, number formatting
: . ( n -- )
  ?DUP NOT IF '0' EMIT EXIT THEN \ 0 is a special case
  DUP 0< IF '-' EMIT -1 * THEN
  $ff SWAP ( stop ) BEGIN 10 /MOD ( d q ) ?DUP NOT UNTIL
  BEGIN '0' + EMIT DUP 9 > UNTIL DROP ;
: _ DUP 9 > IF [ 'a' 10 - LITN ] ELSE '0' THEN + ;
: .x <<8 >>8 16 /MOD ( l h ) _ EMIT _ EMIT ;
: .X L|M .x .x ;
( ----- 215 )
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
( ----- 216 )
\ Core words, literal parsing
: _c ( sa sl -- n 1 OR sa sl 0 ) \ parse character
  DUP 3 = IF OVER C@ ''' = IF OVER 2 + C@ ''' = IF
    DROP 1+ C@ 1 EXIT THEN THEN THEN 0 ;
: PARSE ( sa sl -- n? f )
  _c ?DUP IF EXIT THEN _h ?DUP NOT IF _d THEN ;
( ----- 217 )
\ Core words, input buffer
SYSVARS $10 + *ALIAS KEY?
: KEY BEGIN KEY? UNTIL ;
SYSVARS $2e + *VALUE IN(
SYSVARS $30 + *VALUE IN> \ current position in IN(
SYSVARS $08 + *ALIAS LN<
: IN) IN( LNSZ + ;
( ----- 218 )
\ Core words, input buffer
: BS? DUP $7f ( DEL ) = SWAP BS = OR ;
: RDLN ( -- ) \ Read 1 line in IN(
  LIT"  ok" STYPE NL> IN( [*TO] IN> IN( LNSZ SPC FILL
  IN( BEGIN ( a )
    KEY DUP BS? IF ( a c )
      DROP DUP IN( > IF 1- BS EMIT THEN SPC> BS EMIT
    ELSE ( a c ) \ non-BS
      DUP LF = IF DROP CR THEN \ same as CR
      DUP SPC >= IF DUP EMIT ( echo back ) THEN
      ( a c ) DUP ROT C!+ ( c a+1 ) DUP IN) = ROT CR = OR IF
        DROP NL> EXIT THEN THEN AGAIN ;
: IN< ( -- c ) \ Read one character from INBUF
  IN> IN) = IF LN< THEN IN> C@ IN> 1+ [*TO] IN> ;
: IN$ ['] RDLN [*TO] LN<
  [ SYSVARS $40 ( INBUF ) + LITN ] [*TO] IN( IN) [*TO] IN> ;
( ----- 219 )
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
( ----- 220 )
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
( ----- 221 )
\ Core words, ENTRY, CREATE etc.
: ENTRY WORD TUCK MOVE, ( len )
  CURRENT , C, \ write prev value and size
  HERE [*TO] CURRENT ;
: CREATE ENTRY 2 ( cellWord ) C, ;
: ALIAS ( addr -- ) ENTRY 4 ( alias ) C, , ;
: *ALIAS ( addr -- ) ENTRY $84 ( ialias ) C, , ;
: VALUE ENTRY 8 C, , ; : *VALUE ENTRY $88 C, , ;
: VALUES ( n -- ) 0 DO ENTRY 8 C, RUN1 , LOOP ;
( ----- 222 )
\ Core words, Dictionary
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
\ Core words, DOES>, S=, [IF]
: DOES> CURRENT ( cur )
  $81 ( does ) OVER C! \ make CURRENT into a DOES
  1+ DUP ( pfa pfa )
  ( move PFA by 2 ) HERE OVER - ( pfa pfa u )
  OVER 2 + SWAP MOVE- 2 ALLOT
  ( Write DOES> pointer ) R> SWAP ( does-addr pfa ) !
  ( Because we've popped RS, we'll exit parent definition ) ;
: S= ( sa1 sl1 sa2 sl2 -- f )
  ROT OVER = IF ( same len, s2 s1 l ) []=
  ELSE DROP 2DROP 0 THEN ;
: [IF]
  IF EXIT THEN LIT" [THEN]" BEGIN 2DUP WORD S= UNTIL 2DROP ;
: [THEN] ;
( ----- 224 )
\ Core words, DUMP, .S
: DUMP ( n a -- )
  SWAP 8 /MOD SWAP IF 1+ THEN 0 DO
    ':' EMIT DUP .x SPC> DUP ( a a )
    4 0 DO C@+ .x C@+ .x SPC> LOOP DROP ( a )
    8 0 DO
      C@+ DUP SPC < IF DROP '.' THEN EMIT LOOP NL>
  LOOP DROP ;
: .S ( -- )
  LIT" SP " STYPE 'S .X SPC> S0 .X
  LIT"  RS " STYPE 'R .X SPC> R0 .X
  LIT"  FREE " STYPE 'S 'R - .X STACK?
\ Weirdness for first PS element is for TOS in register cases.
  'S S0 -^ >> ?DUP IF NL>
    OVER .X SPC> 1- ?DUP IF
      0 DO 'S I << + @ .X SPC> LOOP THEN THEN ;
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
: _bchk DUP $7f + $ff > IF LIT" br ovfl" STYPE ABORT THEN ;
: DO COMPILE 2>R HERE ; IMMEDIATE
: LOOP COMPILE (loop) HERE - _bchk C, ; IMMEDIATE
: LITN DUP >>8 IF COMPILE (n) , ELSE COMPILE (b) C, THEN ;
: : ENTRY 1 ( compiled ) C, BEGIN
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
BLK_ADDR VALUE BLK(
BLK_ADDR 1024 + VALUE BLK)
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
: ASML 2 LOAD ; : ASMH 3 LOAD ;
: Z80A ASML 5 15 LOADR ASMH ;
: 8086A ASML 20 28 LOADR ASMH ;
: AVRA ASML 30 40 LOADR ASMH ;
: 6809A ASML 50 59 LOADR ASMH ;
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
: _lf
    XYPOS COLS / NEWLN COLS * XYPOS! ;
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
( SD Card subsystem Load range: B250-B258 )
: _idle ( -- n ) $ff (spix) ;

( spix $ff until the response is something else than $ff
  for a maximum of 20 times. Returns $ff if no response. )
: _wait ( -- n )
    0 ( dummy ) 20 0 DO
        DROP _idle DUP $ff = NOT IF LEAVE THEN LOOP ;
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
    0 (spie)
;
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
    NOT UNTIL ; ( out of idle mode, success! )
( ----- 257 )
: _ ( dstaddr blkno -- )
  [ SDC_DEVID LITN ] (spie)
  $51 ( CMD17 ) 0 ROT ( a cmd 0 blkno ) _cmd IF _err THEN
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
  $58 ( CMD24 ) 0 ROT ( a cmd 0 blkno ) _cmd IF _err THEN
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
    << OVER J 64 * I + + C@ 'X' = IF 1+ THEN
  LOOP 8 _w - LSHIFT C, LOOP DROP ;
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
( ----- 280 )
\ Z80 port's Macros and constants. See doc/code/z80.txt
0 VALUE lblexec
\ see comment at TICKS' definition
\ 7.373MHz target: 737t. outer: 37t inner: 16t
\ tickfactor = (737 - 37) / 16
44 VALUE tickfactor
\ skip first EXDEHL, when IP is already in HL
: ;CODEHL lblnext? 1+ JP, ;
: CARRY! EXAFAF', SCF, EXAFAF', ;
: HL>BC, B H LDrr, C L LDrr, ;
: BC>HL, H B LDrr, L C LDrr, ;
: A>BC, C A LDrr, B 0 LDri, ;
( ----- 281 )
\ Z80 port's Code. Load range B281-B299
HERE TO ORG ( STABLE ABI )
JR, FWR L1 ( B282 ) NOP, NOP, ( unused ) NOP, NOP, ( 04, BOOT )
NOP, NOP, ( 06 CURRENT ) NOP, NOP, ( 08, LATEST )
NOP, NOP, ( 0a (main) ) 0 JP, ( 0c QUIT ) NOP,
8 ALLOT0
0 JP, ( RST 18 ) 5 ALLOT0
0 JP, ( RST 20 ) 5 ALLOT0
0 JP, ( RST 28 ) 5 ALLOT0
0 JP, ( RST 30 ) 5 ALLOT0
0 JP, ( RST 38 )
( ----- 282 )
FSET L1 ( B281 )
  SP PS_ADDR LDdi, IX RS_ADDR LDdi,
  BIN( $04 ( BOOT ) + LDHL(i),
  JR, FWR L1 ( execute, B283 )
BSET lblnext PC ORG $f + ! ( Stable ABI )
  EXDEHL, LDDE(HL), HL INCd, EXDEHL, ( continue to execute )
( ----- 283 )
BSET lblexec FSET L1 ( B282 ) ( HL -> wordref )
  A (HL) LDrr, HL INCd, ( PFA ) A SRL, IFC, ( XT )
    IFNZ, ( DOES ) BC PUSH, LDBC(HL), ( addr ) HL INCd,
      ( BC=addr, HL=PFA ) A H LDrr, H B LDrr, B A LDrr,
      A L LDrr, L C LDrr, C A LDrr, THEN, ( continue to XT )
    IX INCd, IX INCd,
    0 IX+ E LDIXYr, 1 IX+ D LDIXYr,
    LDDE(HL), HL INCd, ( DE -> wordref HL -> IP )
    EXDEHL, ( DE -> IP HL -> wordref )
    lblexec JP, THEN,
  IFZ, ( native ) JP(HL), THEN,
\ cont.
( ----- 284 )
\ cont.
  A SRL, IFC, ( cell ) BC PUSH, HL>BC, lblnext? JP, THEN,
  A SRL, IFC, ( alias )
    LDHL(HL), ( read PFA )
    IFNZ, ( indirect ) LDHL(HL), THEN, lblexec JP, THEN,
  ( value )
  A SRL, IFNZ, ( indirect ) LDHL(HL), THEN,
  BC PUSH, LDBC(HL), lblnext? JP,
( ----- 285 )
( Native words )
CODE QUIT
BSET L1 ( used in ABORT ) PC ORG $d + ! ( Stable ABI )
  IX RS_ADDR LDdi, BIN( $0a ( main ) + LDHL(i),
  JR, BWR lblexec
CODE ABORT SP PS_ADDR LDdi, HL POP, HL POP, HL 0 LDdi, HL PUSH,
  HL PUSH, JR, BWR L1
CODE EXECUTE BC>HL, BC POP, lblexec JP,
CODE BYE HALT,
CODE EXIT ( put new IP in HL instead of DE for speed )
    L 0 IX+ LDrIXY, H 1 IX+ LDrIXY, IX DECd, IX DECd, ;CODEHL
( ----- 286 )
CODE FIND ( sa sl -- w? f ) BC PUSH, EXX, BC POP, HL POP,
  BC ADDHLd, HL DECd, \ HL points to the last char of s
  DE SYSVARS $02 ( CURRENT ) + LDd(i),
  BEGIN, \ main loop
    DE DECd, LDA(DE), $7f ANDi, ( IMMEDIATE ) C CPr, IFZ,
      HL PUSH, DE PUSH, BC PUSH,
      DE DECd, DE DECd, \ Skip prev field
      BSET L1 ( loop )
        DE DECd, LDA(DE), CPD, JRNZ, FWR L2 ( break! )
      CPE L1 JPc, ( BC not zero? loop ) FSET L2
      BC POP, DE POP, HL POP, THEN,
( ----- 287 )
\ At this point, Z is set if we have a match.
    IFZ, ( match ) DE INCd, DE PUSH, EXX, BC 1 LDdi, ;CODE THEN,
\ no match, go to prev and continue
\ we read prev field backwards
    DE DECd, LDA(DE), EXAFAF', DE DECd, LDA(DE),
    E A LDrr, EXAFAF', D A LDrr,
    E ORr, IFZ, \ DE=0, end of dict
      EXX, BC 0 LDdi, ;CODE THEN,
  JR, AGAIN, \ main loop
( ----- 288 )
CODE (br) BSET L1 ( used in ?br and loop )
  LDA(DE), ( sign extend A into HL )
  L A LDrr, A ADDr, ( sign in carry ) A SBCr, ( FF if neg )
  H A LDrr, DE ADDHLd, ( HL --> new IP ) ;CODEHL
CODE (?br)
  BCZ, BC POP, JRZ, BWR L1 ( br + 1. False, branch )
  ( True, skip next byte and don't branch ) DE INCd, ;CODE
CODE (loop)
  0 IX+ INC(IXY+), IFZ, 1 IX+ INC(IXY+), THEN, ( I++ )
  ( Jump if I <> I' )
  A 0 IX+ LDrIXY, -2 IX+ CP(IXY+), JRNZ, BWR L1 ( branch )
  A 1 IX+ LDrIXY, -1 IX+ CP(IXY+), JRNZ, BWR L1 ( branch )
  ( don't branch )
  IX DECd, IX DECd, IX DECd, IX DECd, DE INCd, ;CODE
( ----- 289 )
CODE (n) BC PUSH, EXDEHL, LDBC(HL), HL INCd, ;CODEHL
CODE (b) BC PUSH, LDA(DE), A>BC, DE INCd, ;CODE
CODE (c) LDA(DE), DE INCd, L A LDrr, H 0 LDri, DE ADDHLd,
  EXDEHL, JP(HL),
( ----- 290 )
CODE ROT ( a b c -- b c a ) ( BC=c )
  HL POP, ( b ) EX(SP)HL, ( a<>b ) BC PUSH, ( c ) HL>BC, ;CODE
CODE ROT> ( a b c -- c a b ) ( BC=c )
  BC>HL, BC POP, ( b ) EX(SP)HL, ( a<>c ) HL PUSH, ;CODE
CODE DUP ( a -- a a ) BSET L1 BC PUSH, ;CODE
CODE ?DUP BCZ, JRNZ, BWR L1 ;CODE
CODE DROP ( a -- ) BC POP, ;CODE
CODE SWAP ( a b -- b a ) HL POP, BC PUSH, HL>BC, ;CODE
CODE OVER ( a b -- a b a )
  HL POP, HL PUSH, BC PUSH, HL>BC, ;CODE
( ----- 291 )
CODE 'S HL 0 LDdi, SP ADDHLd, BC PUSH, HL>BC, ;CODE
CODE 'R BC PUSH, IX PUSH, BC POP, ;CODE
CODE AND HL POP,
  A C LDrr, L ANDr, C A LDrr,
  A B LDrr, H ANDr, B A LDrr, ;CODE
CODE OR HL POP,
  A C LDrr, L ORr, C A LDrr,
  A B LDrr, H ORr, B A LDrr, ;CODE
CODE XOR HL POP,
  A C LDrr, L XORr, C A LDrr,
  A B LDrr, H XORr, B A LDrr, ;CODE
CODE NOT BCZ, BC 0 LDdi, IFZ, BC INCd, THEN, ;CODE
( ----- 292 )
CODE CARRY? EXAFAF', HL 0 LDdi, HL ADCHLd, HL PUSH, ;CODE
CODE + HL POP, BC ADDHLd, EXAFAF', HL>BC, ;CODE
CODE - HL POP, BC SUBHLd, EXAFAF', HL>BC, ;CODE
CODE * BC PUSH, EXX, ( protect DE ) ( DE * BC -> HL )
  DE POP, BC POP,
  HL 0 LDdi, A $10 LDri, BEGIN,
    HL ADDHLd, E RL, D RL,
    IFC, BC ADDHLd, THEN,
    A DECr, JRNZ, AGAIN,
  HL PUSH, EXX, ( unprotect DE ) BC POP, ;CODE
( ----- 293 )
( Divides AC by DE. quotient in AC remainder in HL )
CODE /MOD BC PUSH, EXX, ( protect DE )
  DE POP, BC POP,
  A B LDrr, B 16 LDri, HL 0 LDdi, BEGIN,
    SCF, C RL, RLA,
    HL ADCHLd, DE SBCHLd,
    IFC, DE ADDHLd, C DECr, THEN,
  DJNZ, AGAIN,
  B A LDrr,
  HL PUSH, BC PUSH, EXX, ( unprotect DE ) BC POP, ;CODE
( ----- 294 )
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
    BEGIN, A DECr, ( 4t ) JRNZ, ( 12t ) AGAIN,
  JR, ( 12t ) AGAIN, ( outer: 37t inner: 16t )
( ----- 295 )
CODE ! BC>HL, BC POP,
  (HL) C LDrr, HL INCd, (HL) B LDrr, BC POP, ;CODE
CODE @ BC>HL, LDBC(HL), ;CODE
CODE C! BC>HL, BC POP, (HL) C LDrr, BC POP, ;CODE
CODE C@ LDA(BC), A>BC, ;CODE
CODE PC! HL POP, L OUT(C)r, BC POP, ;CODE
CODE PC@ C INr(C), B 0 LDri, ;CODE
CODE FILL BC PUSH, EXX, DE POP, BC POP, HL POP,
  (HL) E LDrr, BC DECd, BCZ, IFNZ,
    D H LDrr, E L LDrr, DE INCd, LDIR, THEN, EXX, BC POP, ;CODE
CODE MOVE BC PUSH, EXX, BC POP, DE POP, HL POP,
  BCZ, IFNZ, LDIR, THEN, EXX, BC POP, ;CODE
CODE MOVE- BC PUSH, EXX, BC POP, HL POP, DE POP,
  BCZ, IFNZ, BC ADDHLd, HL DECd, EXDEHL, BC ADDHLd, HL DECd,
    LDDR, THEN, EXX, BC POP, ;CODE
( ----- 296 )
CODE I BC PUSH, C 0 IX+ LDrIXY, B 1 IX+ LDrIXY, ;CODE
CODE I' BC PUSH, C -2 IX+ LDrIXY, B -1 IX+ LDrIXY, ;CODE
CODE J BC PUSH, C -4 IX+ LDrIXY, B -3 IX+ LDrIXY, ;CODE
CODE >R IX INCd, IX INCd, 0 IX+ C LDIXYr, 1 IX+ B LDIXYr,
  BC POP, ;CODE
CODE R> BC PUSH, C 0 IX+ LDrIXY, B 1 IX+ LDrIXY,
  IX DECd, IX DECd, ;CODE
CODE 2>R HL POP,
  IX INCd, IX INCd, 0 IX+ L LDIXYr, 1 IX+ H LDIXYr,
  IX INCd, IX INCd, 0 IX+ C LDIXYr, 1 IX+ B LDIXYr,
  BC POP, ;CODE
CODE 2R> BC PUSH,
  C 0 IX+ LDrIXY, B 1 IX+ LDrIXY, IX DECd, IX DECd,
  H 0 IX+ LDrIXY, L 1 IX+ LDrIXY, IX DECd, IX DECd,
  HL PUSH, ;CODE
( ----- 297 )
CODE []= BC PUSH, EXX, ( protect DE ) BC POP, DE POP, HL POP,
  BSET L1 ( loop )
    LDA(DE), DE INCd, CPI,
    IFNZ, EXX, BC 0 LDdi, ;CODE THEN,
    CPE L1 JPc, ( BC not zero? loop )
  EXX, BC 1 LDdi, ;CODE
CODE = HL POP, BC SUBHLd, BC 0 LDdi, IFZ, BC INCd, THEN, ;CODE
CODE < HL POP, BC SUBHLd, BC 0 LDdi, IFC, BC INCd, THEN, ;CODE
CODE (im1) IM1, EI, ;CODE
( ----- 298 )
CODE 1+ BC INCd, ;CODE
CODE 1- BC DECd, ;CODE
CODE CRC16 ( c n -- c ) BC PUSH, EXX, ( protect DE )
  HL POP, ( n ) DE POP, ( c )
  A L LDrr, D XORr, D A LDrr,
  B 8 LDri, BEGIN,
    E SLA, D RL,
    IFC, ( msb is set, apply polynomial )
      A D LDrr, $10 XORi, D A LDrr,
      A E LDrr, $21 XORi, E A LDrr,
    THEN,
  DJNZ, AGAIN,
  DE PUSH, EXX, ( unprotect DE ) BC POP, ;CODE
( ----- 299 )
CODE >> ( n -- n ) B SRL, C RR, EXAFAF', ;CODE
CODE << ( n -- n ) C SLA, B RL, EXAFAF', ;CODE
CODE >>8 ( n -- n ) C B LDrr, B 0 LDri, ;CODE
CODE <<8 ( n -- n ) B C LDrr, C 0 LDri, ;CODE
( ----- 300 )
Z80 drivers

300-310 unused
311 AT28 EEPROM                312 SPI relay
315 TMS9918
320 MC6850 driver              325 Zilog SIO driver
330 Sega Master System VDP     335 SMS PAD
340 SMS KBD                    347 SMS SPI relay
348 SMS Ports
350 TI-84+ LCD                 355 TI-84+ Keyboard
360 TRS-80 4P drivers
370-399 unused
( ----- 311 )
CODE AT28C! ( c a -- )
  BC>HL, BC POP,
  (HL) C LDrr, A C LDrr, ( orig ) B C LDrr, ( save )
  C (HL) LDrr, ( poll ) BEGIN,
    A (HL) LDrr, ( poll ) C CPr, ( same as old? )
    C A LDrr, ( save old poll, Z preserved )
  JRNZ, AGAIN,
\ equal to written? SUB instead of CP to ensure IOERR is NZ
  B SUBr, IFNZ, SYSVARS ( IOERR ) LD(i)A, THEN, BC POP, ;CODE
: AT28! ( n a -- ) 2DUP AT28C! 1+ SWAP >>8 SWAP AT28C! ;
( ----- 312 )
( SPI relay driver. See doc/hw/z80/spi.txt )
CODE (spix) ( n -- n )
  A C LDrr,
  SPI_DATA OUTiA,
  ( wait until xchg is done )
  BEGIN, SPI_CTL INAi, 1 ANDi, JRNZ, AGAIN,
  SPI_DATA INAi,
  C A LDrr, ;CODE
CODE (spie) ( n -- )
  A C LDrr, SPI_CTL OUTiA, BC POP, ;CODE
( ----- 315 )
( Z80 driver for TMS9918. Implements grid protocol. Requires
TMS_CTLPORT, TMS_DATAPORT and ~FNT from the Font compiler at
B520. Patterns are at addr $0000, Names are at $3800.
Load range B315-317 )
CODE _ctl ( a -- sends LSB then MSB )
  A C LDrr, TMS_CTLPORT OUTiA, A B LDrr, TMS_CTLPORT OUTiA,
  BC POP, ;CODE
CODE _data
  A C LDrr, TMS_DATAPORT OUTiA, BC POP, ;CODE
( ----- 316 )
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
( ----- 317 )
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
( ----- 320 )
( MC6850 Driver. Load range B320-B322. Requires:
  6850_CTL for control register
  6850_IO for data register.
  CTL numbers used: $16 = no interrupt, 8bit words, 1 stop bit
  64x divide. $56 = RTS high )
CODE 6850>
  BEGIN,
    6850_CTL INAi, $02 ANDi, ( are we transmitting? )
  JRZ, ( yes, loop ) AGAIN,
  A C LDrr, 6850_IO OUTiA, BC POP, ;CODE
( ----- 321 )
CODE 6850<? BC PUSH,
  A XORr, ( 256x ) A $16 ( RTS lo ) LDri, 6850_CTL OUTiA,
  BC 0 LDdi, ( pre-push a failure )
  BEGIN, EXAFAF', ( preserve cnt )
    6850_CTL INAi, $1 ANDi, ( rcv buff full? )
    IFNZ, ( full )
      6850_IO INAi, PUSHA, C 1 LDri, A XORr, ( end loop )
    ELSE, EXAFAF', ( recall cnt ) A DECr, THEN,
  JRNZ, AGAIN,
  A $56 ( RTS hi ) LDri, 6850_CTL OUTiA, ;CODE
( ----- 322 )
X' 6850<? ALIAS RX<? X' 6850<? ALIAS (key?)
X' 6850> ALIAS TX> X' 6850> ALIAS (emit)
: 6850$ $56 ( RTS high ) [ 6850_CTL LITN ] PC! ;
( ----- 325 )
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
  JRNZ, AGAIN,
  A 5 ( PTR5 ) LDri, SIOA_CTL OUTiA,
  A $6a ( RTS high ) LDri, SIOA_CTL OUTiA, ;CODE
( ----- 326 )
CODE SIOA>
  BEGIN,
    SIOA_CTL INAi, $04 ANDi, ( are we transmitting? )
  JRZ, ( yes, loop ) AGAIN,
  A C LDrr, SIOA_DATA OUTiA, BC POP, ;CODE
CREATE _ ( init data ) $18 C, ( CMD3 )
    $24 C, ( CMD2/PTR4 ) $c4 C, ( WR4/64x/1stop/nopar )
    $03 C, ( PTR3 ) $c1 C, ( WR3/RXen/8char )
    $05 C, ( PTR5 ) $6a C, ( WR5/TXen/8char/RTS )
    $21 C, ( CMD2/PTR1 ) 0 C, ( WR1/Rx no INT )
: SIOA$ _ 9 RANGE DO I C@ [ SIOA_CTL LITN ] PC! LOOP ;
( ----- 327 )
CODE SIOB<? ( copy/paste of SIOA<? )
  A XORr, ( 256x ) BC 0 LDdi, ( pre-push a failure )
  A 5 ( PTR5 ) LDri, SIOB_CTL OUTiA,
  A $68 ( RTS low ) LDri, SIOB_CTL OUTiA,
  BEGIN, EXAFAF', ( preserve cnt )
    SIOB_CTL INAi, $1 ANDi, ( rcv buff full? )
    IFNZ, ( full )
      SIOB_DATA INAi, PUSHA, C 1 LDri, A XORr, ( end loop )
    ELSE, EXAFAF', ( recall cnt ) A DECr, THEN,
  JRNZ, AGAIN,
  A 5 ( PTR5 ) LDri, SIOB_CTL OUTiA,
  A $6a ( RTS high ) LDri, SIOB_CTL OUTiA, ;CODE
( ----- 328 )
CODE SIOB>
  BEGIN,
    SIOB_CTL INAi, $04 ANDi, ( are we transmitting? )
  JRZ, ( yes, loop ) AGAIN,
  A C LDrr, SIOB_DATA OUTiA, BC POP, ;CODE
: SIOB$ _ 9 RANGE DO I C@ [ SIOB_CTL LITN ] PC! LOOP ;
( ----- 330 )
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
( ----- 331 )
: _sfont ( a -- Send font to VDP )
  7 RANGE DO I C@ _data 3 _zero LOOP ( blank row ) 4 _zero ;
: CELL! ( c pos )
  2 * $7800 OR _ctl ( c )
  $20 - ( glyph ) $5f MOD _data ;
( ----- 332 )
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
( ----- 335 )
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
( ----- 336 )
: _chk ( c --, check _sel range )
  _sel C@ DUP $7f > IF $20 _sel C! THEN
  $20 < IF $7f _sel C! THEN ;
CREATE _ '0' C, ':' C, 'A' C, '[' C, 'a' C, $ff C,
: _nxtcls
  _sel @ >R _ BEGIN ( a R:c ) C@+ I > UNTIL ( a R:c ) R> DROP
  1- C@ _sel ! ;
( ----- 337 )
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
( ----- 338 )
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
( ----- 340 )
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
( ----- 341 )
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
( ----- 347 )
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
  DJNZ, AGAIN, CPORT_MEM LD(i)A,
  C L LDrr, ;CODE
( ----- 348 )
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
( ----- 349 )
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
( ----- 350 )
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
  JRC, AGAIN, ;CODE
( ----- 351 )
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
( ----- 352 )
: _clrrows ( n u -- Clears u rows starting at n )
  SWAP _row! ( u ) 0 DO
    _yinc 0 _col!
    11 0 DO 0 _data! LOOP
    _xinc 0 _data! LOOP ;
: NEWLN ( oldln -- newln )
  1+ DUP 1+ FNTH 1+ * _zoff! ( ln )
  DUP FNTH 1+ * FNTH 1+ _clrrows ( newln ) ;
: LCDCLR 0 64 _clrrows ;
( ----- 353 )
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
( ----- 355 )
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
( ----- 356 )
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
( ----- 357 )
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
( ----- 358 )
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
( ----- 359 )
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
( ----- 360 )
\ TRS-80 drivers declarations and macros
$f800 VALUE VIDMEM $bf VALUE CURCHAR
: fdstat $f0 INAi, ;
: fdcmd A SWAP LDri, B $18 LDri,
  $f0 OUTiA, BEGIN, DJNZ, AGAIN, ;
: fdwait BEGIN, fdstat RRCA, JRC, AGAIN, RLCA, ;
: vid+, ( reg -- ) HL VIDMEM LDdi, ADDHLd, ;
( ----- 361 )
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
( ----- 362 )
BSET L2 ( seek, B=trk )
  A 21 LDri, B CPr, FDMEM LDA(i), IFC, $20 ORi, ( WP ) THEN,
  $80 ORi, $f4 OUTiA, \ FD sel
  A B LDrr, ( trk ) $f3 OUTiA, $1c fdcmd RET,
CODE FDRD ( trksec addr -- st ) BC>HL, BC POP,
  L2 CALL, fdwait $18 ANDi, IFZ, DI,
    A C LDrr, $f2 OUTiA, ( sec ) C $f3 LDri, $84 fdcmd ( read )
    BEGIN, BEGIN, fdstat $b6 ANDi, JRZ, AGAIN, \ DRQ
      $b4 ANDi, JRNZ, FWR L3 ( error ) INI, JRNZ, AGAIN, THEN,
  fdwait $3c ANDi, FSET L3 A>BC, EI, ;CODE
CODE FDWR ( trksec addr -- st ) BC>HL, BC POP,
  L2 CALL, fdwait $18 ANDi, IFZ, DI,
    A C LDrr, $f2 OUTiA, ( sec ) C $f3 LDri, $a4 fdcmd ( read )
    BEGIN, BEGIN, fdstat $f6 ANDi, JRZ, AGAIN, \ DRQ
      $f4 ANDi, JRNZ, FWR L3 ( error ) OUTI, JRNZ, AGAIN, THEN,
  fdwait $3c ANDi, FSET L3 A>BC, EI, ;CODE
( ----- 363 )
: _err LIT" FDerr " STYPE .X ABORT ;
: _trksec ( sec -- trksec )
\ 4 256b sectors per block, 18 sec per trk, 40 trk max
  18 /MOD ( sec trk ) DUP 39 > IF $ffff _err THEN <<8 + ;
: FD@! ( blk blk( wref -- )
  >R SWAP << << ( blk( blk*4=sec R:wr )
  4 RANGE DO ( dest R:wr blk )
    I _trksec OVER ( dest trksec dest )
    J ( wr ) EXECUTE ( dest ) ?DUP IF _err THEN $100 +
  LOOP R> 2DROP ;
: FD@ ['] FDRD FD@! ;
: FD! ['] FDWR FD@! ;
: FDSEL ( fdmask -- )
  DUP [ FDMEM LITN ] C! $80 OR $f4 PC! 0 $f0 PC! ( restore ) ;
: FD$ 2 FDSEL ;
( ----- 364 )
: CL$ ( baudcode -- )
  $02 $e8 PC! ( UART RST )
  DUP << << << << OR $e9 PC! ( bauds )
  $6d $ea PC! ( word8 no parity no-RTS ) ;
CODE TX> BEGIN,
    $ea INAi, $40 ANDi, IFNZ, ( TX reg empty )
      $e8 INAi, $80 ANDi, IFZ, ( CTS low )
        A C LDrr, $eb OUTiA, ( send byte ) BC POP, ;CODE
  THEN, THEN, JR, AGAIN,
( ----- 365 )
CODE RX<? BC PUSH,
  A XORr, ( 256x ) BC 0 LDdi, ( pre-push a failure )
  A $6c ( RTS low ) LDri, $ea OUTiA,
  BEGIN, EXAFAF', ( preserve cnt )
    $ea INAi, $80 ANDi, ( rcv buff full? )
    IFNZ, ( full )
      $eb INAi, A>BC, C 1 LDri, A XORr, ( end loop )
    ELSE, EXAFAF', ( recall cnt ) A DECr, THEN,
  JRNZ, AGAIN,
  A $6d ( RTS high ) LDri, $ea OUTiA, ;CODE
( ----- 366 )
BSET L1 6 nC, '`' 'h' 'p' 'x' '0' '8'
BSET L2 8 nC, $0d 0 $ff 0 0 $08 0 $20
PC ORG $39 + T! ( RST 38 )
AF PUSH, HL PUSH, DE PUSH, BC PUSH,
$ec INAi, ( RTC INT ack )
$f440 LDA(i), A ORr, IFNZ, \ 7th row is special
  HL L2 1- LDdi, BEGIN, HL INCd, RRA, JRNC, AGAIN,
  A (HL) LDrr, ELSE, \ not 7th row
  HL L1 LDdi, DE $f401 LDdi, BC $600 LDdi, BEGIN,
    LDA(DE), A ORr, IFNZ,
      C (HL) LDrr, BEGIN, C INCr, RRA, JRNC, AGAIN,
      C DECr, THEN,
    E SLA, HL INCd, DJNZ, AGAIN,
  A C LDrr, THEN, \ cont.
( ----- 367 )
\ A=char or zero if no keypress. Now let's debounce
HL KBD_MEM 2 + LDdi, A ORr, IFZ, \ no keypress, debounce
  (HL) A LDrr, ELSE, \ keypress, is it debounced?
  (HL) CPr, IFNZ, \ != debounce buffer
    C A LDrr, (HL) C LDrr, $ff CPi, IFZ, \ BREAK!
      HL POP, HL POP, HL POP, HL POP, HL POP, EI,
      BIN( $0c + ( QUIT ) JP, THEN,
    HL DECd, $f480 LDA(i), 3 ANDi, (HL) A LDrr, HL DECd,
    (HL) C LDrr, THEN, THEN,
BC POP, DE POP, HL POP, AF POP, EI, RET,
( ----- 368 )
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
( ----- 369 )
: FD0 FLUSH 1 FDSEL ;
: FD1 FLUSH 2 FDSEL ;
( ----- 370 )
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
    BEGIN, fdstat RRA, RRA, JRNC, AGAIN, ( DRQ )
    INI, A $c1 LDri, BEGIN, $f4 OUTiA, INI, JRNZ, AGAIN,
    fdwait $1c ( error mask ) ANDi, IFNZ,
      SPC ADDi, VIDMEM LD(i)A, BEGIN, JR, AGAIN, THEN,
    $f2 INAi, A INCr, $f2 OUTiA, 18 CPi, JRC, AGAIN,
  EXAFAF', ( restore ) A INCr, 3 CPi, JRC, AGAIN, 0 RST,
( ----- 400 )
( 8086 boot code. PS=SP, RS=BP, IP=DX
  Load range. decl: B400 code: B402-B417 )
0 VALUE lblexec
: CARRY! CL 0 MOVri, IFC, CL INCr, THEN,
  SYSVARS $06 ( CARRY? ) + CL MOVmr, ;
( ----- 402 )
HERE TO ORG
JMPn, 0 , ( 00, main ) 0 C, ( 03, boot driveno )
8 ALLOT0 JMPn, 0 , ( 0c QUIT ) 6 ALLOT0
( End of Stable ABI )
BSET lblnext PC ORG $f + ! ( Stable ABI )
    DI DX MOVxx, ( <-- IP ) DX INCx, DX INCx,
    DI [DI] x[] MOV[], ( wordref )
    ( continue to execute )
( ----- 403 )
BSET lblexec ( DI -> wordref )
AL [DI] r[] MOV[], DI INCx, ( PFA ) AL SHRr1, IFC, ( XT )
  IFNZ, ( DOES )
    DI INCx, DI INCx, DI PUSHx, DI [DI] -2 x[]+ MOV[], THEN,
  BP INCx, BP INCx, [BP] 0 DX []+x MOV[], ( pushRS )
  DX DI MOVxx, DX INCx, DX INCx, ( --> IP )
  DI [DI] x[] MOV[], JMPs, lblexec RPCs, THEN,
IFZ, DI JMPr, THEN, ( native )
AL SHRr1, IFC, ( cell ) DI PUSHx, JMPs, lblnext RPCs, THEN,
DI [DI] x[] MOV[], ( read PFA )
AL SHRr1, IFC, ( alias )
  IFNZ, ( indirect ) DI [DI] x[] MOV[], THEN,
  JMPs, lblexec RPCs, THEN, ( cont. )
( ----- 404 )
( cont. ) ( value )
AL SHRr1, IFNZ, ( indirect ) DI [DI] x[] MOV[], THEN,
DI PUSHx, JMPs, lblnext RPCs,
PC 3 - ORG 1+ ! ( main )
    DX POPx, ( boot drive no ) $03 DL MOVmr,
    SP PS_ADDR MOVxI, BP RS_ADDR MOVxI,
    DI $04 ( BOOT ) MOVxm,
    JMPn, lblexec RPCn,
( ----- 405 )
( native words )
CODE EXECUTE
  DI POPx, JMPn, lblexec RPCn,
CODE QUIT
PC $f - ORG $d + ! ( Stable ABI )
BSET L1 ( used in ABORT )
    BP RS_ADDR MOVxI,
    DI $0a ( main ) MOVxm,
    JMPn, lblexec RPCn,
CODE ABORT SP PS_ADDR MOVxI, AX AX XORxx, CX 4 MOVxI,
  DI SP MOVxx, CLD, REPZ, STOSB, JMPs, L1 RPCs,
CODE EXIT
    DX [BP] 0 x[]+ MOV[], BP DECx, BP DECx, ( popRS )
;CODE
( ----- 406 )
CODE (br) BSET L1 ( used in ?br )
  DI DX MOVxx, AL [DI] r[] MOV[], AH AH XORrr, CBW,
  DX AX ADDxx, ;CODE
CODE (?br)
  AX POPx, AX AX ORxx, JZ, L1 RPCs, ( False, branch )
  ( True, skip next byte and don't branch ) DX INCx, ;CODE
CODE (c)
  DI DX MOVxx, AL [DI] r[] MOV[], DI INCx, DX INCx,
  AH AH XORrr, DX AX ADDxx, DI JMPr, ;CODE
( ----- 407 )
CODE (loop)
  [BP] 0 [w]+ INC[], ( I++ )
  ( Jump if I <> I' )
  AX [BP] 0 x[]+ MOV[], AX [BP] -2 x[]+ CMP[],
  JNZ, L1 RPCs, ( branch )
  ( don't branch )
  BP 4 SUBxi, DX INCx,
;CODE
( ----- 408 )
CODE (n)
  DI DX MOVxx, DI [DI] x[] MOV[], DI PUSHx,
  DX INCx, DX INCx, ;CODE
CODE (b)
  DI DX MOVxx, AH AH XORrr, AL [DI] r[] MOV[], AX PUSHx,
  DX INCx, ;CODE
( ----- 409 )
CODE I [BP] 0 [w]+ PUSH[], ;CODE
CODE I' [BP] -2 [w]+ PUSH[], ;CODE
CODE J [BP] -4 [w]+ PUSH[], ;CODE
CODE >R BP INCx, BP INCx, [BP] 0 [w]+ POP[], ;CODE
CODE R> [BP] 0 [w]+ PUSH[], BP DECx, BP DECx, ;CODE
CODE 2>R [BP] 4 [w]+ POP[], [BP] 2 [w]+ POP[], BP 4 ADDxi, ;CODE
CODE 2R>
  [BP] -2 [w]+ PUSH[], [BP] 0 [w]+ PUSH[], BP 4 SUBxi, ;CODE
( ----- 410 )
CODE ROT ( a b c -- b c a ) CX POPx, BX POPx, AX POPx,
  BX PUSHx, CX PUSHx, AX PUSHx, ;CODE
CODE ROT> ( a b c -- c a b ) CX POPx, BX POPx, AX POPx,
  CX PUSHx, AX PUSHx, BX PUSHx, ;CODE
CODE DUP AX POPx, AX PUSHx, AX PUSHx, ;CODE
CODE ?DUP AX POPx, AX AX ORxx, AX PUSHx,
  IFNZ, AX PUSHx, THEN, ;CODE
CODE OVER ( a b -- a b a )
  DI SP MOVxx, AX [DI] 2 x[]+ MOV[], AX PUSHx, ;CODE
( ----- 411 )
CODE SWAP AX POPx, BX POPx, AX PUSHx, BX PUSHx, ;CODE
CODE DROP AX POPx, ;CODE
CODE 'S AX SP MOVxx, AX PUSHx, ;CODE
CODE 'R BP PUSHx, ;CODE
CODE CARRY? AH AH XORrr,
  AL SYSVARS $06 ( CARRY? ) + MOVrm, AX PUSHx, ;CODE
CODE AND AX POPx, BX POPx, AX BX ANDxx, AX PUSHx, ;CODE
( ----- 412 )
CODE OR AX POPx, BX POPx, AX BX ORxx, AX PUSHx, ;CODE
CODE XOR AX POPx, BX POPx, AX BX XORxx, AX PUSHx, ;CODE
CODE NOT AX POPx, AX AX ORxx,
  IFNZ, AX -1 MOVxI, THEN, AX INCx, AX PUSHx, ;CODE
CODE + AX POPx, BX POPx,
  AX BX ADDxx, CARRY! AX PUSHx, ;CODE
CODE - BX POPx, AX POPx,
  AX BX SUBxx, CARRY! AX PUSHx, ;CODE
CODE * AX POPx, BX POPx,
  DX PUSHx, ( protect from MUL ) BX MULx, DX POPx,
  AX PUSHx, ;CODE
CODE /MOD BX POPx, AX POPx, DX PUSHx, ( protect )
  DX DX XORxx, BX DIVx,
  BX DX MOVxx, DX POPx, ( unprotect )
  BX PUSHx, ( modulo ) AX PUSHx, ( division ) ;CODE
( ----- 413 )
CODE ! DI POPx, AX POPx, [DI] AX []x MOV[], ;CODE
CODE @ DI POPx, AX [DI] x[] MOV[], AX PUSHx, ;CODE
CODE C! DI POPx, AX POPx, [DI] AX []r MOV[], ;CODE
CODE C@
    DI POPx, AH AH XORrr, AL [DI] r[] MOV[], AX PUSHx, ;CODE
CODE FILL AX POPx, CX POPx, DI POPx,
  CLD, REPZ, STOSB, ;CODE
CODE MOVE CX POPx, DI POPx, SI POPx,
  CLD, REPZ, MOVSB, ;CODE
CODE MOVE- CX POPx, DI POPx, SI POPx,
  DI CX ADDxx, DI DECx, SI CX ADDxx, SI DECx,
  STD, REPZ, MOVSB, ;CODE
( ----- 414 )
CODE BYE HLT, BEGIN, JMPs, AGAIN,
CODE []= ( a1 a2 u -- f ) CX POPx, SI POPx, DI POPx,
  CLD, REPZ, CMPSB, PUSHZ, ;CODE
CODE = BX POPx, AX POPx, AX BX CMPxx, PUSHZ, ;CODE
CODE < BX POPx, AX POPx, CX CX XORxx,
    AX BX CMPxx, IFC, CX INCx, THEN, CX PUSHx, ;CODE
( ----- 415 )
CODE FIND ( sa sl -- w? f ) CX POPx, SI POPx,
  DI SYSVARS $2 ( CURRENT ) + MOVxm,
  BEGIN, ( loop )
    AL [DI] -1 r[]+ MOV[], $7f ANDALi, ( strlen )
    CL AL CMPrr, IFZ, ( same len )
      SI PUSHx, DI PUSHx, CX PUSHx, ( --> )
        3 ADDALi, ( header ) AH AH XORrr, DI AX SUBxx,
        CLD, REPZ, CMPSB,
      CX POPx, DI POPx, SI POPx, ( <-- )
      IFZ, DI PUSHx, AX 1 MOVxI, AX PUSHx, ;CODE THEN,
    THEN,
    DI 3 SUBxi, DI [DI] x[] MOV[], ( prev ) DI DI ORxx,
  JNZ, AGAIN, ( loop )
  AX AX XORrr, AX PUSHx, ;CODE
( ----- 416 )
CODE 1+ DI SP MOVxx, [DI] [w] INC[], ;CODE
CODE 1- DI SP MOVxx, [DI] [w] DEC[], ;CODE
CODE >> ( n -- n ) AX POPx,
  AX SHRx1, CARRY! AX PUSHx, ;CODE
CODE << ( n -- n ) AX POPx,
  AX SHLx1, CARRY! AX PUSHx, ;CODE
CODE >>8 ( n -- n )
  AX POPx, AL AH MOVrr, AH AH XORrr, AX PUSHx, ;CODE
CODE <<8 ( n -- n )
  AX POPx, AH AL MOVrr, AL AL XORrr, AX PUSHx, ;CODE
( ----- 417 )
( See comment in B294 TODO: test on real hardware. in qemu,
  the resulting delay is more than 10x too long. )
CODE TICKS ( n=100us )
    SI DX MOVxx, ( protect IP )
    AX POPx, BX 100 MOVxI, BX MULx,
    CX DX MOVxx, ( high ) DX AX MOVxx, ( low )
    AX $8600 MOVxI, ( 86h, WAIT ) $15 INT,
    DX SI MOVxx, ( restore IP )
;CODE
( ----- 420 )
( PC/AT drivers. Load range: 420-426 )
CODE (key?)
  AH AH XORrr, $16 INT, AH AH XORrr, AX PUSHx, AX PUSHx,
;CODE
( ----- 421 )
CODE 13H08H ( driveno -- cx dx )
  DI POPx, DX PUSHx, ( protect ) DX DI MOVxx, AX $800 MOVxI,
  ES PUSHs, DI DI XORxx, ES DI MOVsx,
  $13 INT, DI DX MOVxx, ES POPs, DX POPx, ( unprotect )
  CX PUSHx, DI PUSHx, ;CODE
CODE 13H ( ax bx cx dx -- ax bx cx dx )
  SI POPx, ( DX ) CX POPx, BX POPx, AX POPx,
  DX PUSHx, ( protect ) DX SI MOVxx, DI DI XORxx,
  $13 INT, SI DX MOVxx, DX POPx, ( unprotect )
  AX PUSHx, BX PUSHx, CX PUSHx, SI PUSHx, ;CODE
( ----- 422 )
DRV_ADDR VALUE FDSPT
DRV_ADDR 1+ VALUE FDHEADS
: _ ( AX BX sec )
    ( AH=read sectors, AL=1 sector, BX=dest,
      CH=trackno CL=secno DH=head DL=drive )
    FDSPT C@ /MOD ( AX BX sec trk )
    FDHEADS C@ /MOD ( AX BX sec head trk )
    <<8 ROT OR 1+ ( AX BX head CX )
    SWAP <<8 $03 C@ ( boot drive ) OR ( AX BX CX DX )
    13H 2DROP 2DROP ;
( ----- 423 )
\ Sectors are 512b, so blk numbers are all x2. We add 16 to
\ this because blkfs starts at sector 16.
: FD@ ( blkno blk( -- )
  SWAP << ( 2* ) 16 + 2DUP ( a b a b )
  $0201 ROT> ( a b c a b ) _ ( a b )
  1+ SWAP $200 + SWAP $0201 ROT> ( c a b ) _ ;
: FD! ( blkno blk( -- )
  SWAP << ( 2* ) 16 + 2DUP ( a b a b )
  $0301 ROT> ( a b c a b ) _ ( a b )
  1+ SWAP $200 + SWAP $0301 ROT> ( c a b ) _ ;
: FD$
\ get number of sectors per track with command 08H.
  $03 ( boot drive ) C@ 13H08H
  >>8 1+ FDHEADS C!
  $3f AND FDSPT C! ;
( ----- 424 )
2 VALUES COLS 80 LINES 25
CODE CURSOR! ( new old ) AX POPx, ( old ) AX POPx, ( new )
  DX PUSHx, ( protect ) BX 80 MOVxI, DX DX XORxx,
  BX DIVx, ( col in DL, row in AL ) DH AL MOVrr, AH 2 MOVri,
  $10 INT, DX POPx, ( unprotect ) ;CODE
CODE _spit ( c )
  AX POPx, AH $0e MOVri, ( print char ) $10 INT, ;CODE
: CELL! ( c pos -- ) 0 CURSOR! _spit ;
( ----- 450 )
( 6809 declarations )
0 VALUE lblexec
: CARRY! CCR A TFR, SYSVARS $06 ( CARRY? ) + () STA, ;
( ----- 451 )
( 6809 Boot code. IP=Y, PS=S, RS=U  ) HERE TO ORG
BRA, FBR, TO L1 ( main ) $0a ALLOT0 BRA, FBR, TO L2 ( QUIT )
7 ALLOT0 ( end of stable ABI )
BSET lblnext Y++ LDX,
BSET lblexec ( X=wordref )
X+ LDA, LSRA, IFCS, ( XT )
  IFNZ, ( DOES ) X++ LDD, PSHS, X ( PFA ) D X TFR, THEN,
  U++ STY, ( IP->RS ) X Y TFR, Y++ TST, X+0 LDX,
  BRA, lblexec BBR, THEN,
IFZ, X+0 JMP, THEN, ( native )
LSRA, IFCS, ( cell ) PSHS, X ( PFA ) BRA, lblnext BBR, THEN,
X+0 LDX, ( read PFA ) LSRA, IFCS, ( alias )
  IFNZ, ( indirect ) X+0 LDX, THEN, BRA, lblexec BBR, THEN,
( value ) LSRA, IFNZ, ( indirect ) X+0 LDX, THEN,
PSHS, X BRA, lblnext BBR,
( ----- 452 )
FSET L1 ( main ) PS_ADDR # LDS, RS_ADDR # LDU,
BIN( 4 + ( BOOT ) () LDX, BRA, lblexec BBR,
CODE QUIT BSET L1 ( for ABORT ) FSET L2 RS_ADDR # LDU,
  BIN( $0a + ( main ) () LDX, BRA, lblexec BBR,
CODE EXECUTE PULS, X LBRA, lblexec BBR,
CODE ABORT PS_ADDR # LDS, CLRA, CLRB, S+0 STD, 2 S+N STD,
  BRA, L1 ( QUIT ) BBR,
CODE BYE BEGIN, BRA, AGAIN,
CODE EXIT --U LDY, ;CODE
( ----- 453 )
CODE (b) Y+ LDB, CLRA, PSHS, D ;CODE
CODE (n) Y++ LDD, PSHS, D ;CODE
CODE (br) Y+0 LDA, Y+A LEAY, ;CODE
CODE (?br) S+ LDA, S+ ORA,
  IFZ, Y+0 LDA, Y+A LEAY, ELSE, Y+ TST, THEN, ;CODE
CODE (c) Y+ LDA, Y X TFR, Y+A LEAY, X+0 JMP,
CODE (loop) -2 U+N LDD, INCB, IFZ, INCA, THEN, -4 U+N CMPD,
  IFZ, ( exit loop ) --U TST, --U TST, Y+ TST,
  ELSE, ( loop ) -2 U+N STD, Y+0 LDA, Y+A LEAY, THEN, ;CODE
( ----- 454 )
CODE DROP 2 S+N LEAS, ;CODE
CODE DUP ( a -- a a ) S+0 LDD, PSHS, D ;CODE
CODE ?DUP ( a -- a? a )
  S+0 LDD, IFNZ, PSHS, D THEN, ;CODE
CODE SWAP ( a b -- b a )
  S+0 LDD, 2 S+N LDX, S+0 STX, 2 S+N STD, ;CODE
CODE OVER ( a b -- a b a )
  2 S+N LDD, PSHS, D ;CODE
CODE ROT ( a b c -- b c a )
  4 S+N LDX, ( a ) 2 S+N LDD, ( b ) 4 S+N STD, S+0 LDD, ( c )
  2 S+N STD, S+0 STX, ;CODE
CODE ROT> ( a b c -- c a b )
  S+0 LDX, ( c ) 2 S+N LDD, ( b ) S+0 STD, 4 S+N LDD, ( a )
  2 S+N STD, 4 S+N STX, ;CODE
( ----- 455 )
CODE R> --U LDD, PSHS, D ;CODE
CODE >R PULS, D U++ STD, ;CODE
CODE 2R> --U LDD, --U LDX, PSHS, XD ;CODE
CODE 2>R PULS, XD U++ STX, U++ STD, ;CODE
CODE 'S S D TFR, PSHS, D ;CODE
CODE 'R U PSHS, ;CODE
CODE I -2 U+N LDD, PSHS, D ;CODE
CODE I' -4 U+N LDD, PSHS, D ;CODE
CODE J -6 U+N LDD, PSHS, D ;CODE
CODE @ [S+0] LDD, S+0 STD, ;CODE
CODE C@ [S+0] LDB, CLRA, S+0 STD, ;CODE
CODE ! PULS, X PULS, D X+0 STD, ;CODE
CODE C! PULS, X PULS, D X+0 STB, ;CODE
( ----- 456 )
CODE AND PULS, D S+0 ANDA, 1 S+N ANDB, S+0 STD, ;CODE
CODE OR PULS, D S+0 ORA, 1 S+N ORB, S+0 STD, ;CODE
CODE XOR PULS, D S+0 EORA, 1 S+N EORB, S+0 STD, ;CODE
CODE + PULS, D S+0 ADDD, S+0 STD, CARRY! ;CODE
CODE - 2 S+N LDD, S++ SUBD, S+0 STD, CARRY! ;CODE
CODE 1+ 1 S+N INC, LBNE, lblnext BBR, S+0 INC, ;CODE
CODE 1-
  1 S+N TST, IFZ, S+0 DEC, THEN, 1 S+N DEC, ;CODE
CODE << 1 S+N LSL, S+0 ROL, CARRY! ;CODE
CODE >> S+0 LSR, 1 S+N ROR, CARRY! ;CODE
CODE <<8 1 S+N LDA, S+0 STA, 1 S+N CLR, ;CODE
CODE >>8 S+0 LDA, 1 S+N STA, S+0 CLR, ;CODE
CODE CARRY? SYSVARS $06 ( CARRY? ) + () LDB, 1 # ANDB,
  CLRA, PSHS, D ;CODE
( ----- 457 )
CODE /MOD ( a b -- a/b a%b )
  16 # LDA, 0 <> STA, CLRA, CLRB, ( D=running rem ) BEGIN,
    1 # ORCC, 3 S+N ROL, ( a lsb ) 2 S+N ROL, ( a msb )
    ROLB, ROLA, S+0 SUBD,
    IF<, S+0 ADDD, 3 S+N DEC, ( a lsb ) THEN,
  0 <> DEC, BNE, AGAIN,
  2 S+N LDX, 2 S+N STD, ( rem ) S+0 STX, ( quotient ) ;CODE
CODE * ( a b -- a*b )
  S+0 ( bm ) LDA, 3 S+N ( al ) LDB, MUL, S+0 ( bm ) STB,
  2 S+N ( am ) LDA, 1 S+N ( bl ) LDB, MUL,
    S+0 ( bm ) ADDB, S+0 STB,
  1 S+N ( al ) LDA, 3 S+N ( bl ) LDB, MUL,
  S++ ADDA, S+0 STD, ;CODE
( ----- 458 )
BSET L1 ( PUSH Z ) CCR B TFR, LSRB, LSRB,
  1 # ANDB, CLRA, S+0 STD, ;CODE
CODE = PULS, D S+0 CMPD, BRA, L1 ( PUSH Z ) BBR,
CODE NOT S+0 LDB, 1 S+N ORB, BRA, L1 ( PUSH Z ) BBR,
CODE <
  2 S+N LDD, S++ CMPD, CCR B TFR, 1 # ANDB, CLRA, S+0 STD, ;CODE
CODE FILL ( a u c ) PULS, D S+ LDA, INCA, 0 <> STA,
  S+ LDA, PULS, X BEGIN,
    X+ STB, DECA, IFZ, 0 <> DEC, THEN, BNE, AGAIN, ;CODE
CODE MOVE ( a1 a2 u ) 1 <> STY, PULS, DXY ( Y=a1 X=a2 )
  INCA, 0 <> STA, BEGIN, Y+ LDA, X+ STA, DECB,
    IFZ, 0 <> DEC, THEN, BNE, AGAIN, 1 <> LDY, ;CODE
CODE MOVE- ( a1 a2 u ) 1 <> STY, PULS, DXY
  X+D LEAX, Y+D LEAY, INCA, 0 <> STA, BEGIN,
    -Y LDA, -X STA, DECB, IFZ, 0 <> DEC, THEN, BNE, AGAIN,
  1 <> LDY, ;CODE
( ----- 459 )
BSET L1 ( X=s1 Y=s2 B=cnt ) BEGIN,
  X+ LDA, Y+ CMPA, IFNZ, RTS, THEN, DECB, BNE, AGAIN, RTS,
CODE []= ( a1 a2 u -- f TODO: allow u>$ff )
  0 <> STY, PULS, DXY ( B=u, X=a2, Y=a1 ) LPC L1 () JSR,
  IFZ, 1 # LDD, ELSE, CLRA, CLRB, THEN, PSHS, D 0 <> LDY, ;CODE
CODE FIND ( sa sl -- w? f )
  SYSVARS $02 + ( CURRENT ) () LDX,
  0 <> STY, PULS, D 2 <> STB, BEGIN,
    -X LDB, $7f # ANDB, --X TST, 2 <> CMPB, IF=,
      3 <> STX, S+0 LDY, NEGB, X+B LEAX, NEGB, LPC L1 () JSR,
      IFZ, ( match ) 0 <> LDY, 3 <> LDD, 3 # ADDD, S+0 STD,
        1 # LDD, PSHS, D ;CODE THEN,
      3 <> LDX, THEN, \ nomatch, X=prev
  X+0 LDX, BNE, AGAIN, \ not zero, loop
  ( end of dict ) 0 <> LDY, S+0 STX, ( X=0 ) ;CODE
( ----- 460 )
6809 drivers

461 TRS-80 Color Computer 2
( ----- 461 )
( CoCo2 drivers. Load range: 461-463 )
PC ," @HPX08" CR C, ," AIQY19" 0 C,
   ," BJRZ2:" 0 C,  ," CKS_3;" 0 C,
   ," DLT_4," 0 C,  ," EMU" BS C, ," 5-" 0 C,
   ," FNV_6." 0 C,  ," GOW 7/" 0 C,
   ," @hpx0(" CR C, ," aiqy!)" 0 C,
   ," bjrz" '"' C, '*' C, 0 C, ," cks_#+" 0 C,
   ," dlt_$<" 0 C,  ," emu" BS C, ," %=" 0 C,
   ," fnv_&>" 0 C,  ," gow '?" 0 C,
BSET L1 ( PC ) # LDX, $fe # LDA, BEGIN, ( 8 times )
  $ff02 () STA, ( set col ) $ff00 () LDB, ( read row )
  ( ignore 8th row ) $80 # ORB, $7f # CMPA, IF=,
    ( ignore shift row ) $40 # ORB, THEN,
  INCB, IFNZ, ( key pressed ) DECB, RTS, THEN,
  ( inc col ) 7 X+N LEAX, 1 # ORCC, ROLA, BCS, AGAIN,
  ( no key ) CLRB, RTS,
( ----- 462 )
CODE (key?) ( -- c? f ) CLRA, CLRB, PSHS, D LPC L1 () JSR,
  IFNZ, ( key! row mask in B col ptr in X )
    ( is shift pressed? ) $7f # LDA, $ff02 () STA,
    $ff00 () LDA, $40 # ANDA, IFZ, ( shift! )
      56 X+N LEAX, THEN,
    BEGIN, X+ LDA, LSRB, BCS, AGAIN,
    ( A = our char ) 1 S+N STA, TSTA, IFNZ, ( valid key )
      1 # LDD, ( f ) PSHS, D ( wait for keyup )
      BEGIN, LPC L1 () JSR, BNE, AGAIN, THEN,
  THEN, ;CODE
( ----- 463 )
32 VALUE COLS 16 VALUE LINES
: CELL! ( c pos -- )
  SWAP $20 - DUP $5f < IF
    DUP $20 < IF $60 + ELSE DUP $40 < IF $20 + ELSE $40 -
      THEN THEN ( pos glyph )
    SWAP $400 + C! ELSE 2DROP THEN ;
: CURSOR! ( new old -- )
  DROP $400 + DUP C@ $40 XOR SWAP C! ;
( ----- 500 )
AVR Firmware

Pieces of code that are designed to be placed on AVR chips used
in some example designs of Collapse OS.

501 SMS PS/2 controller
( ----- 501 )
SMS PS/2 controller (doc/hw/z80/sms)

To assemble, load the AVR assembler with AVRA, then
"504 522 LOADR".

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
( ----- 502 )
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
( ----- 503 )
R19: Register used for parity computations and tmp value in
     some other places
R20: data being sent to the '164
Y: pointer to the memory location where the next scan code from
   ps/2 will be written.
Z: pointer to the next scan code to push to the 595
( ----- 504 )
18 VALUES SRAM_START $0060 RAMEND $015f SPL $3d SPH $3e
          GPIOR0 $11 MCUCR $35 TCCR0B $33 GIMSK $3b
          TIFR $38 TCNT0 $32 PINB $16 DDRB $17 PORTB $18
          CLK 2 DATA 1 CP 3 LQ 0 LR 4
$100 100 - VALUE TIMER_INITVAL
\ We need a lot of labels in this program...
5 VALUES L4 0 L5 0 L6 0 L7 0 L8 0
( ----- 505 )
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
( ----- 506 )
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
( ----- 507 )
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
( ----- 508 )
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
( ----- 509 )
\ step 3: stop bit
R18 CLR, \ happens in all cases
\ DATA has to be set
R19 TST, \ was DATA set?
' BREQ L1 LBL, \ loop, not set? error, don't push to buf
\ push r17 to the buffer
Y+ R17 ST,
FLBL, L8 \ RCALL checkBoundsY
' RJMP L1 LBL, \ loop
( ----- 510 )
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
( ----- 511 )
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
( ----- 512 )
' BREQ L7 TO, \ processbits2
\ step 2 - parity bit
R1 R19 MOV,
R19 R17 MOV,
FLBL, L5 \ RCALL checkParity
R1 R16 CP,
FLBL, L6 \ BRNE processBitError, r1 != r16? wrong parity
R18 INC,
' RJMP L1 LBL, \ loop
( ----- 513 )
' BRNE L6 TO, \ processBitError
R18 CLR,
R19 $fe LDI,
FLBL, L6 \ RCALL sendToPS2
' RJMP L1 LBL, \ loop

' RJMP L4 TO, \ processbitReset
R18 CLR,
FLBL, L4 \ RCALL resetTimer
' RJMP L1 LBL, \ loop
( ----- 514 )
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
( ----- 515 )
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
( ----- 516 )
' RCALL L2 TO, ' RCALL L4 TO, LBL! L2 \ resetTimer
R16 TIMER_INITVAL LDI,
TCNT0 R16 OUT,
R16 $02 ( TOV0 ) LDI,
TIFR R16 OUT,
RET,
( ----- 517 )
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
( ----- 518 )
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
( ----- 519 )
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
( ----- 520 )
\ We're finished! Enable INT0, reset timer, everything back to
\ normal!
' RCALL L2 LBL, \ resetTimer
CLT, \ also, make sure T isn't mistakely set.
SEI,
RET,
( ----- 521 )
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
( ----- 522 )
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
