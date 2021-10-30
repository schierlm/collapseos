( ----- 000 )
6502 MASTER INDEX

301 6502 macros and consts     302 6502 assembler
310 6502 boot code             320 6502 disassembler
( ----- 001 )
\ 6502 macros and constants. See doc/code/6502.txt
: 6502A ASML 302 305 LOADR ASMH ;
: 6502H 306 LOAD ;
: 6502C 310 313 LOADR ;
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
CODE (next) PLA, TAY, IFZ, \ ovfl, always jump
  PLA, CLC, 1 # SBC, PHA, $ff # LDA, PHA, X' (br) JMP, THEN,
  DEY, IFNZ, ( no zero, jump ) TYA, PHA, X' (br) JMP, THEN,
  PLA, IFNZ, PHA, 0 # LDA, PHA, X' (br) JMP, THEN,
  ( finished! ) IP+, ;CODE
( ----- 012 )
CODE R@ DEX, DEX, PLA, 0 <X+> STA, TAY, PLA, 1 <X+> STA, PHA,
  TYA, PHA, ;CODE
CODE >R 1 <X+> LDA, PHA, 0 <X+> LDA, PHA, INX, INX, ;CODE
CODE SWAP 0 <X+> LDA, 2 <X+> LDY, 0 <X+> STY, 2 <X+> STA,
  1 <X+> LDA, 3 <X+> LDY, 1 <X+> STY, 3 <X+> STA, ;CODE
CODE OVER DEX, DEX, 4 <X+> LDA, 0 <X+> STA, 5 <X+> LDA,
  1 <X+> STA, ;CODE
CODE + 1 <X+> LDA, TAY, 0 <X+> LDA, INX, INX, CLC, 0 <X+> ADC,
  0 <X+> STA, TYA, 1 <X+> ADC, 1 <X+> STA, ;CODE
CODE >A 0 <X+> LDA, AL <> STA, 1 <X+> LDA, AH <> STA,
  INX, INX, ;CODE
CODE A> DEX, DEX, AL <> LDA, 0 <X+> STA, AH <> LDA,
  1 <X+> STA, ;CODE
CODE A+ AL <> INC, IFZ, AH <> INC, THEN, ;CODE
CODE AC@+ INLINE A> INLINE C@ INLINE A+ ;CODE
( ----- 013 )
CODE EMIT 0 <X+> LDA, INX, INX, $80 # ORA, $fded JSR, ;CODE
: STYPE >R >A BEGIN AC@+ EMIT NEXT ;
: BOOT LIT" Collapse OS" STYPE BYE ;
XCURRENT ORG $04 ( stable ABI BOOT ) + T!
( ----- 020 )
\ 6502 disassembler
\ order below represent "opid", also used in emulator
CREATE OPNAME ," ORAANDEORADCSTALDACMPSBC" \ 1/5/9/d x8
," ASLROLLSRRORSTXLDXDECINC" \ 6/a/e x8
," BITJMPSTYLDYCPYCPX" \ 4/c x6
," BRKBPLJSRBMIRTIBVCRTSBVSBCCLDYBCSCPYBNECPXBEQ" \ 0 x15
," PHPCLCPLPSECPHACLIPLASEIDEYTYATAYCLVINYCLDINXSED" \ 8 x16
," TXATXSTAXTSXDEXNOP" \ a x6
59 CONSTANT OPCNT $ff CONSTANT NUL 20 CONSTANT DISCNT
: >>4 >> >> >> >> ;
: opid. DUP OPCNT < IF
  3 * OPNAME + 3 STYPE ELSE DROP ." ???" THEN ;
: WORDTBL ( n -- ) CREATE >R BEGIN ' , NEXT ;
: spcs ( n -- ) >R BEGIN SPC> NEXT ;
( ----- 021 )
: id159d ( opcode -- opid )
  DUP $89 = IF DROP NUL ELSE >>4 >> THEN ;
CREATE _ 24 nC, $c $c $d $d $e $e $f $f
                $35 $36 $37 $38 $39 NUL $3a NUL
                $c NUL $d $d $e $e $f $f
: id6ae DUP $80 < IF ( ASL/ROL/LSR/ROR )
    DUP $1f AND $1a = IF DROP NUL EXIT THEN >>4 >> 8 + EXIT THEN
  DUP >> >> 1- 3 AND 8 * _ + ( op tbl ) SWAP >>4 7 AND + C@ ;
CREATE _ 32 nC,
NUL NUL $10 NUL NUL NUL NUL NUL $12 $12 $13 $13 $14 $14 $15 NUL
NUL NUL $10 NUL $11 NUL $11 NUL $12 NUL $13 $13 $14 NUL $15 NUL
: id4c _ OVER $8 AND IF $10 + THEN SWAP >>4 + C@ ;
: idnul DROP NUL ;
: id0 >>4 DUP 8 = IF DROP NUL EXIT THEN
  DUP 8 > IF 1- THEN 22 + ;
: id8 >>4 37 + ;
( ----- 022 )
: id2 $a2 = IF $0d ELSE NUL THEN ;
16 WORDTBL _ id0 id159d id2 idnul id4c
  id159d id6ae idnul id8 id159d
  id6ae idnul id4c id159d id6ae idnul
: opid DUP $f AND << _ + @ EXECUTE ;

\ 0=inh/imm 1=(,X) 2=imm 4,5,6=ZP 8=inh 9=imm a=inh c,d,e=abs
\ 0=inh 1=(),Y 4,5=ZP+X 6=ZP+X/Y 8=inh 9=abs+Y a=inh
\ c,d,e=abs+X, BE=exception
( ----- 023 )
: inh. ( a -- a ) 7 spcs ; : byte. C@+ .x ;
: $. '$' EMIT byte. ; : zp. $. 4 spcs ; ALIAS zp. rel.
: imm. '#' EMIT byte. 4 spcs ;
: $$. '$' EMIT C@+ SWAP C@+ .x SWAP .x ; : abs. $$. 2 spcs ;
: ind. '(' EMIT $$. ')' EMIT ;
: acc. 'A' EMIT 6 spcs ;
: ,X. ',' EMIT 'X' EMIT ;
: ,Y. ',' EMIT 'Y' EMIT ;
: zp,X. $. ,X. 2 spcs ; : zp,Y. $. ,Y. 2 spcs ;
: abs,X. $$. ,X. ; : abs,Y. $$. ,Y. ;
: ind,X. '(' EMIT $. ,X. ')' EMIT ;
: ind,Y. '(' EMIT $. ')' EMIT ,Y. ;
$20 WORDTBL _ inh. ind,X. imm. inh. zp. zp. zp. inh. inh. imm.
  acc. inh. abs. abs. abs. inh. rel. ind,Y. inh. inh. zp,X.
  zp,X. zp,X. inh. inh. abs,Y. inh. inh. abs,X. abs,X.
  abs,X. inh.
( ----- 024 )
: mode. ( a opcode -- a )
  DUP $6c = IF DROP ind. EXIT THEN
  DUP $be = IF DROP abs,Y. EXIT THEN
  $1f AND << _ + @ EXECUTE ;
: op. ( a -- a ) C@+ DUP opid DUP opid. SPC>
  OPCNT < IF mode. ELSE DROP THEN ;
: dump ( a u -- ) >R BEGIN C@+ .x SPC> NEXT DROP ;
: dis ( a -- ) DISCNT >R BEGIN
  DUP .X SPC> DUP op. SPC> TUCK OVER - dump NL> NEXT DROP ;
