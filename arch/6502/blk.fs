( ----- 000 )
6502 MASTER INDEX

301 6502 macros and consts     302 6502 assembler
310 6502 boot code
( ----- 001 )
\ 6502 macros and constants. See doc/code/6502.txt
: 6502A ASML 302 305 LOADR ASMH ;
: 6502H 306 LOAD ;
: 6502C 310 312 LOADR ;
1 CONSTANT JROPLEN -1 CONSTANT JROFF
\ ZP assignments
0 VALUE IPL 2 VALUE AL 4 VALUE INDJ
: IPH IPL 1+ ; : AH AL 1+ ;
: INDL INDJ 1+ ; : INDH INDL 1+ ;
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
ALIAS JMP, JMPi,
: JRi, PC + [ JROFF JROPLEN - LITN ] - JMP, ; \ no BRA!
: ?JRi, ?JROP C, C, ;
: Z? $f0 [TO] ?JROP ; : C? $b0 [TO] ?JROP ;
: ^? ?JROP $20 XOR [TO] ?JROP ;
ALIAS JSR, CALLi,
( ----- 006 )
\ 6502 HAL
: IP>, $caca M, ( DEX DEX )
       $a5 C, IPL C, ( LDA ) $9500 M, ( STA X+ )
       $a5 C, IPH C, ( LDA ) $9501 M, ( STA X+ ) ;
: >IP, $b500 M, ( LDA X+ ) $85 C, IPL C, ( STA )
       $b501 M, ( LDA X+ ) $85 C, IPH C, ( STA )
       $e8e8 M, ( INX INX ) ;
: IP+, $e6 C, IPL C, ( INC ) $d002 M, ( BNE ) $e6 C, IPH C, ;
( ----- 010 )
\ 6502 boot code PS=X RS=S
HERE TO ORG 0 JMP, 9 ALLOT0 \ STABLE ABI
PC ORG $01 ( main jmp ) + T!
$ff # LDX, ( PS ) TXS, ( RS )
$6c # LDA, INDJ <> STA, BIN( $04 ( BOOT ) + JMP[],
LSET lblnext IPH <> LDY, IPL <> LDA, INDH <> STY, INDL <> STA,
LSET L1 CLC, 2 # ADC, IFC, INY, THEN, IPL <> STA, IPH <> STY,
  INDJ JMP,
LSET lblxt PLA, INDL <> STA, PLA, INDH <> STA,
  IPH <> LDA, PHA, IPL <> LDA, PHA,
  INDL <> INC, IFZ, INDH <> INC, THEN,
  INDL <> LDA, INDH <> LDY, L1 JMP,
( ----- 011 )
CODE BYE BEGIN, BR JRi,
CODE 1+ 0 <X+> INC, IFZ, 1 <X+> INC, THEN, ;CODE
CODE EXIT PLA, IPL <> STA, PLA, IPH <> STA, ;CODE
CODE C@ 0 [X+] LDA, 0 <X+> STA, 0 # LDA, 1 <X+> STA, ;CODE
CODE @ 0 [X+] LDA, TAY, INLINE 1+ 0 [X+] LDA, 0 <X+> STY,
  1 <X+> STA, ;CODE
CODE (b) IP>, IP+, X' C@ JMP,
CODE (n) IP>, IP+, IP+, X' @ JMP,
CODE (br) 0 # LDY, IPL []Y+ LDA, FJR BPL, IPH <> DEC, THEN,
  CLC, IPL <> ADC, IFC, IPH <> INC, THEN, IPL <> STA, ;CODE
( ----- 012 )
CODE bar 2 <X+> LDA, INDL <> STA, 3 <X+> LDA, INDH <> STA,
  0 # LDY, BEGIN,
    INDL []Y+ LDA, $80 # ORA, $fded JSR, INY, 0 <X+> DEC,
  BR Z? ^? ?JRi, ;CODE
: BOOT LIT" Collapse OS" bar BYE ;
XCURRENT ORG $04 ( stable ABI BOOT ) + T!
