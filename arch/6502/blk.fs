( ----- 000 )
6502 MASTER INDEX

301 6502 macros and consts     302 6502 assembler
310 6502 boot code
( ----- 001 )
\ 6502 macros and constants. See doc/code/6502.txt
: 6502A ASML 302 305 LOADR ;
: 6502C 310 311 LOADR ;
1 VALUE JROPLEN -1 VALUE JROFF
( ----- 002 )
\ 6502 assembler, Addressing modes.
\ output: n n-is-2b opoff
: # ( n ) 0 $09 ; \ Immediate
: <> ( n ) 0 $05 ; \ ZeroPage
: <X+> ( n ) 0 $15 ; \ ZeroPage+X
: <Y+> ( n ) 0 $15 ; \ Only for LDX
: () ( n ) 1 $0d ; \ Absolute
: (X+) ( n ) 1 $1d ; \ Absolute+X
: (Y+) ( n ) 1 $19 ; \ Absolute+Y
: [X+] ( n ) 0 $01 ; \ Indirect+X
: []Y+ ( n ) 0 $11 ; \ Indirect+Y
: ?, ( n n-is-2b -- ) IF L, ELSE C, THEN ;
( ----- 003 )
\ 6502 asm, Groups 1 and 2, 3-with-AM
: OPG1 DOER C, DOES> C@ OR C, ?, ;
$60 OPG1 ADC,  $20 OPG1 AND,  $c0 OPG1 CMP,  $40 OPG1 EOR,
$a0 OPG1 LDA,  $00 OPG1 ORA,  $e0 OPG1 SBC,  $80 OPG1 STA,

: _09repl DUP $09 = IF DROP 1 THEN ;
: OPG2 DOER C, DOES> C@ SWAP _09repl OR 1+ C, ?, ;
$00 OPG2 ASL,  $c0 OPG2 DEC,  $e0 OPG2 INC,  $a0 OPG2 LDX,
$40 OPG2 LSR,  $20 OPG2 ROL,  $60 OPG2 ROR,  $80 OPG2 STX,

: OPG3 DOER C, DOES> C@ SWAP _09repl OR 1- C, ?, ;
$20 OPG3 BIT,  $e0 OPG3 CPX,  $c0 OPG3 CPY,  $a0 OPG3 LDY,
$80 OPG3 STY,
( ----- 004 )
\ 6502 asm, implied, branching
: OP DOER C, DOES> C@ C, ;
$0a OP ASLA, $00 OP BRK,  $18 OP CLC,  $d8 OP CLD,  $58 OP CLI,
$b8 OP CLV,  $ca OP DEX,  $88 OP DEY,  $e8 OP INX,  $c8 OP INY,
$4a OP LSRA, $ea OP NOP,  $48 OP PHA,  $08 OP PHP,  $68 OP PLA,
$28 OP PLP,  $2a OP ROLA, $6a OP RORA, $40 OP RTI,  $60 OP RTS,
$38 OP SEC,  $f8 OP SED,  $78 OP SEI,  $aa OP TAX,  $a8 OP TAY,
$98 OP TYA,  $ba OP TSX,  $8a OP TXA,  $9a OP TXS,

: OPBR DOER C, DOES> C@ C, C, ;
$90 OPBR BCC, $b0 OPBR BCS, $f0 OPBR BEQ, $30 OPBR BMI,
$d0 OPBR BNE, $10 OPBR BPL, $50 OPBR BVC, $70 OPBR BVS,

: OPBR2 DOER C, DOES> C@ C, L, ;
$20 OPBR2 JSR, $4c OPBR2 JMP, $6c OPBR2 JMP[],
( ----- 005 )
\ 6502 asm, HAL underpinnings
0 VALUE ?JROP
' JMP, ALIAS JMPi,
: JRi, PC + [ JROFF JROPLEN - LITN ] - JMP, ; \ no BRA!
: ?JRi, ?JROP C, C, ;
: Z? $f0 [TO] ?JROP ; : C? $b0 [TO] ?JROP ;
: ^? ?JROP $20 XOR [TO] ?JROP ;
' JSR, ALIAS CALLi,
( ----- 010 )
\ 6502 boot code PS=SP RS=Y IP=1<> 0<>=6C (JMP[])
HERE TO ORG 0 JMP, 9 ALLOT0 \ STABLE ABI
PC ORG $01 ( main jmp ) + T!
$ff # LDX, TXS, ( PS ) 0 # LDY, ( RS ) $6c # LDA, 0 <> STA,
BIN( $04 ( BOOT ) + JMP[],
LSET lblxt INY, INY, 1 <> LDA, $100 (Y+) STA, 2 <> LDA,
  $101 (Y+) STA, PLA, 1 <> STA, PLA, 2 <> STA,
  1 <> INC, IFZ, 2 <> INC, THEN, $00 JMP,
LSET lblnext 1 <> LDA, CLC, 2 # ADC, 1 <> STA,
  IFC, 2 <> INC, THEN, $00 JMP,
( ----- 011 )
CODE EXIT DEY, DEY, $100 (Y+) LDA, 1 <> STA, $101 (Y+) LDA,
  2 <> STA, ;CODE
LSET L2 11 C, ," Collapse OS"
CODE foo L2 () LDA, 3 <> STA, 1 # LDX, BEGIN,
    L2 (X+) LDA, $80 # ORA, $fded JSR, INX, 3 <> DEC,
  BR Z? ^? ?JRi, BEGIN, BR JRi,
: BOOT foo ;
XCURRENT ORG $04 ( stable ABI BOOT ) + T!
