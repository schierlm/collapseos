( ----- 000 )
6502 MASTER INDEX

301 6502 macros and consts     302 6502 assembler
310 6502 boot code             330 6502 disassembler
335 6502 emulator              350 Virgil's workspace
360 Apple IIe drivers
( ----- 001 )
\ 6502 macros and constants. See doc/code/6502.txt
: 6502A 302 305 LOADR 7 LOAD ( flow ) ;
: 6502M 309 LOAD ;
: 6502C 310 321 LOADR ;
: 6502D 330 334 LOADR ;
: 6502E 335 342 LOADR ;
\ ZP assignments
$06 CONSTANT 'A
$08 CONSTANT 'N
0 VALUE IPL 2 VALUE INDJ
: IPH IPL 1+ ; : INDL INDJ 1+ ; : INDH INDL 1+ ;
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
\ 6502 HAL
ALIAS JMP, JMPi, ALIAS JMP[], JMP(i), ALIAS JSR, CALLi,
: JRi, CLV, BVC, ; \ no BRA!
ALIAS BEQ, JRZi, ALIAS BNE, JRNZi,
ALIAS BCS, JRCi, ALIAS BCC, JRNCi,
: i>, DEX, DEX, DUP # LDA, 0 <X+> STA, >>8 # LDA, 1 <X+> STA, ;
: (i)>,
  DEX, DEX, DUP () LDA, 0 <X+> STA, 1+ () LDA, 1 <X+> STA, ;
( ----- 009 )
\ 6502 port macros

\ helpers
: PS<>, ( src dst ) SWAP <X+> LDA, <X+> STA, ;
: PSCLR16, 0 # LDA, DUP <X+> STA, 1+ <X+> STA, ;
: A>IND+, INDL []Y+ STA, INY, ;
: PS>A, <X+> LDA, ;
: A>PS, <X+> STA, ;
: PSINC, 0 <X+> INC, IFZ, 1 <X+> INC, THEN, ;
: IP+, IPL <> INC, 2 BNE, IPH <> INC, ;
( ----- 010 )
\ 6502 boot code PS=X RS=S
0 JMP, 9 ALLOT0 \ STABLE ABI
PC XORG $01 ( main jmp ) + T!
$6c # LDA, INDJ <> STA, $ff # LDX, TXS, BIN( $04 + JMP[], \ BOOT
LSET lblcell DEX, DEX, PLA, 0 A>PS, PLA, 1 A>PS, PSINC, \ next
LSET lblnext IPH <> LDY, IPL <> LDA, INDH <> STY, INDL <> STA,
LSET L1 CLC, 2 # ADC, IFC, INY, THEN, IPL <> STA, IPH <> STY,
  INDJ JMP,
LSET lblxt PLA, INDL <> STA, PLA, INDH <> STA,
  IPH <> LDA, PHA, IPL <> LDA, PHA,
  INDL <> INC, IFZ, INDH <> INC, THEN,
  INDL <> LDA, INDH <> LDY, L1 JMP,
LSET lbldoes CLC, PLA, TAY, PLA, INY, IFZ, 1 # ADC, THEN,
  INDL <> STY, INDH <> STA, DEX, DEX, 1 <X+> STA, TYA, 2 # ADC,
  IFC, 1 <X+> INC, THEN, 0 <X+> STA, INDJ JMP,
( ----- 011 )
CODE BYE BRK,
CODE QUIT
  TXA, $ff # LDX, TXS, TAX, BIN( $0a ( main ) + JMP[],
CODE ABORT $ff # LDX, X' QUIT BR BNE,
CODE EXIT PLA, IPL <> STA, PLA, IPH <> STA, ;CODE
CODE EXECUTE 0 <X+> LDA, INDL <> STA, 1 <X+> LDA, INDH <> STA,
  INX, INX, INDL JMP[],
CODE SCNT INDL <> STX, DEX, DEX, 0 # LDA, 1 <X+> STA,
  $ff # LDA, SEC, INDL <> SBC, 0 <X+> STA,
  FJR BPL, 1 <X+> DEC, THEN, ;CODE
CODE RCNT TXA, TSX, INDL <> STX, TAX, DEX, DEX, 0 # LDA,
  1 <X+> STA, $ff # LDA, SEC, INDL <> SBC, 0 <X+> STA, ;CODE
( ----- 012 )
CODE (b) 0 # LDY, IPL []Y+ LDA, DEX, DEX, 0 A>PS, 0 # LDA,
  1 A>PS, IP+, ;CODE
CODE (n) 0 # LDY, IPL []Y+ LDA, DEX, DEX, 0 A>PS, INY,
  IPL []Y+ LDA, 1 A>PS, IP+, IP+, ;CODE
CODE (br) 0 # LDY, IPL []Y+ LDA, FJR BPL, IPH <> DEC, THEN,
  CLC, IPL <> ADC, IFC, IPH <> INC, THEN, IPL <> STA, ;CODE
CODE (?br) 0 <X+> LDA, 1 <X+> ORA, INX, INX,
  0 # ORA, X' (br) BR BEQ, IP+, ;CODE
CODE (next) PLA, TAY, IFZ, \ ovfl, always jump
  PLA, SEC, 1 # SBC, PHA, $ff # LDA, PHA, X' (br) JMP, THEN,
  DEY, IFNZ, ( no zero, jump ) TYA, PHA, X' (br) JMP, THEN,
  PLA, IFNZ, PHA, 0 # LDA, PHA, X' (br) JMP, THEN,
  ( finished! ) IP+, ;CODE
( ----- 013 )
CODE C@ 0 [X+] LDA, 0 <X+> STA, 0 # LDA, 1 <X+> STA, ;CODE
CODE @ LSET L1 0 [X+] LDA, TAY, PSINC, 0 [X+] LDA,
  0 <X+> STY, 1 <X+> STA, ;CODE
LSET lblval DEX, DEX, PLA, 0 A>PS, PLA, 1 A>PS, PSINC, L1 JMP,
CODE C! 2 <X+> LDA, 0 [X+] STA, INX, INX, INX, INX, ;CODE
CODE ! 2 <X+> LDA, 0 [X+] STA, PSINC, 3 <X+> LDA, 0 [X+] STA,
  INX, INX, INX, INX, ;CODE
CODE 1+ PSINC, ;CODE
CODE 1- 0 <X+> LDA, IFZ, 1 <X+> DEC, THEN, 0 <X+> DEC, ;CODE
CODE + CLC, 2 <X+> LDA, 0 <X+> ADC, 2 <X+> STA, 3 <X+> LDA,
  1 <X+> ADC, 3 <X+> STA, INX, INX, ;CODE
CODE - 2 <X+> LDA, SEC, 0 <X+> SBC, 2 <X+> STA, 3 <X+> LDA,
  1 <X+> SBC, 3 <X+> STA, INX, INX, ;CODE
CODE < 3 PS>A, 1 <X+> CMP, IFZ, 2 PS>A, 0 <X+> CMP, THEN,
  INX, INX, 0 # LDA, 1 A>PS, 0 # ADC, 1 # EOR, 0 A>PS, ;CODE
( ----- 014 )
CODE << 0 <X+> ASL, 1 <X+> ROL, ;CODE
CODE >> 1 <X+> LSR, 0 <X+> ROR, ;CODE
CODE <<8 0 1 PS<>, 0 # LDA, 0 <X+> STA, ;CODE
CODE >>8 1 0 PS<>, 0 # LDA, 1 <X+> STA, ;CODE
CODE AND 0 <X+> LDA, 2 <X+> AND, 2 <X+> STA, 1 <X+> LDA,
  3 <X+> AND, 3 <X+> STA, INX, INX, ;CODE
CODE OR 0 <X+> LDA, 2 <X+> ORA, 2 <X+> STA, 1 <X+> LDA,
  3 <X+> ORA, 3 <X+> STA, INX, INX, ;CODE
CODE XOR 0 <X+> LDA, 2 <X+> EOR, 2 <X+> STA, 1 <X+> LDA,
  3 <X+> EOR, 3 <X+> STA, INX, INX, ;CODE
CODE NOT 0 # LDY, 0 <X+> LDA, 1 <X+> ORA, 1 <X+> STY,
  IFZ, INY, THEN, 0 <X+> STY, ;CODE
( ----- 015 )
CODE * DEX, DEX, 16 # LDY, 0 PSCLR16,
  BEGIN, 0 <X+> ASL, 1 <X+> ROL, 4 <X+> ASL, 5 <X+> ROL,
    IFC, CLC, 2 <X+> LDA, 0 <X+> ADC, 0 <X+> STA, 3 <X+> LDA,
      1 <X+> ADC, 1 <X+> STA, THEN, DEY, BR BNE,
  0 4 PS<>, 1 5 PS<>, INX, INX, INX, INX, ;CODE
CODE /MOD \ a b -- r q
  DEX, DEX, DEX, 16 # LDA, 0 <X+> STA, ( cnt )
  1 PSCLR16, ( remaining )
  \ 3-4 = divisor 5-6 = dividend
  BEGIN, 5 <X+> ASL, 6 <X+> ROL, 1 <X+> ROL, 2 <X+> ROL,
    1 <X+> LDA, SEC, 3 <X+> SBC, TAY, 2 <X+> LDA, 4 <X+> SBC,
    IFC, 2 <X+> STA, 1 <X+> STY, 5 <X+> INC, THEN,
    0 <X+> DEC, BR BNE,
  5 3 PS<>, 6 4 PS<>, 1 5 PS<>, 2 6 PS<>, INX, INX, INX, ;CODE
( ----- 016 )
CODE DUP LSET L1 DEX, DEX, 2 0 PS<>, 3 1 PS<>, ;CODE
CODE ?DUP 0 <X+> LDA, 1 <X+> ORA, L1 BR BNE, ;CODE
CODE DROP INX, INX, ;CODE
CODE SWAP 0 <X+> LDA, 2 <X+> LDY, 0 <X+> STY, 2 <X+> STA,
  1 <X+> LDA, 3 <X+> LDY, 1 <X+> STY, 3 <X+> STA, ;CODE
CODE OVER DEX, DEX, 4 0 PS<>, 5 1 PS<>, ;CODE
CODE ROT ( a b c -- b c a ) 5 <X+> LDY, 3 5 PS<>, 1 3 PS<>,
  1 <X+> STY, 4 <X+> LDY, 2 4 PS<>, 0 2 PS<>, 0 <X+> STY, ;CODE
CODE ROT> ( a b c -- c a b ) 1 <X+> LDY, 3 1 PS<>, 5 3 PS<>,
  5 <X+> STY, 0 <X+> LDY, 2 0 PS<>, 4 2 PS<>, 4 <X+> STY, ;CODE
CODE R@ DEX, DEX, PLA, 0 <X+> STA, TAY, PLA, 1 <X+> STA, PHA,
  TYA, PHA, ;CODE
CODE >R 1 <X+> LDA, PHA, 0 <X+> LDA, PHA, INX, INX, ;CODE
CODE R> DEX, DEX, PLA, 0 <X+> STA, PLA, 1 <X+> STA, ;CODE
CODE R~ PLA, PLA, ;CODE
( ----- 017 )
CODE JMPi! ( pc a -- len ) $4c # LDA, PHA, LSET L1
  0 PS>A, INDL <> STA, 1 PS>A, INDH <> STA, 0 # LDY, PLA,
  A>IND+, 2 PS>A, A>IND+, 3 PS>A, A>IND+, INX, INX, 0 <X+> STY,
  0 # LDA, 1 A>PS, ;CODE
CODE CALLi! $20 # LDA, PHA, L1 BR BNE,
( ----- 018 )
LSET L1 \ cmp strs at [INDL] and ['N] with cnt <X+0>
  0 # LDY, BEGIN,
    INDL []Y+ LDA, 'N []Y+ CMP, IFNZ, RTS, THEN,
    INY, 0 <X+> DEC, BR BNE, RTS,
CODE []= ( a1 a2 u -- f )
  2 <X+> LDA, INDL <> STA, 3 <X+> LDA, INDH <> STA,
  4 <X+> LDA, 'N <> STA, 5 <X+> LDA, 'N 1+ <> STA,
  0 4 PS<>, 1 <X+> LDY, INY, 5 <X+> STY, INX, INX, INX, INX,
  BEGIN,
    L1 JSR, IFNZ, ( fail ) 0 PSCLR16, ;CODE THEN,
    1 <X+> DEC, BR BNE,
  ( success ) 0 <X+> INC, ;CODE
( ----- 019 )
CODE FIND ( sa sl -- w? f ) \ 0=cnt 1=sl 2-3=curword N=sa
  2 <X+> LDA, 'N <> STA, 3 <X+> LDA, 'N 1+ <> STA, 0 1 PS<>,
  SYSVARS $02 + DUP () LDA, 2 <X+> STA, 1+ () LDA, 3 <X+> STA,
  BEGIN,
    3 <X+> LDA, INDH <> STA, 2 <X+> LDA,
    SEC, 3 # SBC, IFNC, INDH <> DEC, THEN, INDL <> STA,
    0 # LDY, INDL []Y+ LDA, PHA, INY, INDL []Y+ LDA, PHA, \ prev
    INY, INDL []Y+ LDA, $7f # AND, 1 <X+> CMP, IFZ,
      0 <X+> STA, INDL <> LDA, SEC, 0 <X+> SBC, INDL <> STA,
      IFNC, INDH <> DEC, THEN, L1 JSR, IFZ, \ match
        PLA, PLA, 0 # LDY, 1 <X+> STY, INY, 0 <X+> STY, ;CODE
      THEN, THEN,
    PLA, 3 <X+> STA, PLA, 2 <X+> STA, 3 <X+> ORA, IFZ, \ end
      INX, INX, 0 <X+> STA, 1 <X+> STA, ;CODE THEN,
  JMP,
( ----- 020 )
CODE [C]? ( c a u -- i ) \ X+0=c X+1=iH
  2 PS>A, INDL <> STA, 3 PS>A, INDH <> STA,
  0 PS>A, 'N <> STA, 1 PS>A, 'N 1+ <> STA,
  INX, INX, INX, INX, 'N <> ORA, IFNZ, ( u!=0 )
    0 # LDY, 1 <X+> STY, BEGIN, 0 PS>A, ( A=c ) BEGIN,
        INDL []Y+ CMP, IFZ, ( match ) 0 <X+> STY, ;CODE THEN,
        INY, IFZ, 1 <X+> INC, INDH <> INC, THEN,
        'N <> CPY, BR BNE,
      1 PS>A, 'N 1+ <> CMP, BR BNE, ( no match ) THEN,
  $ff # LDA, 0 A>PS, 1 A>PS, ;CODE
( ----- 021 )
CODE A> DEX, DEX, 'A <> LDA, 0 A>PS, 'A 1+ <> LDA, 1 A>PS, ;CODE
CODE >A 0 PS>A, 'A <> STA, 1 PS>A, 'A 1+ <> STA, INX, INX, ;CODE
CODE A>R 'A 1+ <> LDA, PHA, 'A <> LDA, PHA, ;CODE
CODE R>A PLA, 'A <> STA, PLA, 'A 1+ <> STA, ;CODE
CODE A+ 'A <> INC, IFZ, 'A 1+ <> INC, THEN, ;CODE
CODE A- 'A <> LDA, IFZ, 'A 1+ <> DEC, THEN, 'A <> DEC, ;CODE
CODE AC@
  DEX, DEX, 0 # LDY, 1 <X+> STY, 'A []Y+ LDA, 0 A>PS, ;CODE
CODE AC!  0 # LDY, 0 PS>A, 'A []Y+ STA, INX, INX, ;CODE
( ----- 030 )
\ 6502 disassembler
\ order below represent "opid", also used in emulator
CREATE OPNAME ," ORAANDEORADCSTALDACMPSBC" \ 1/5/9/d x8
," ASLROLLSRRORSTXLDXDECINC" \ 6/a/e x8
," BITJMPSTYLDYCPYCPX" \ 4/c x6
," BRKBPLJSRBMIRTIBVCRTSBVSBCCLDYBCSCPYBNECPXBEQ" \ 0 x15
," PHPCLCPLPSECPHACLIPLASEIDEYTYATAYCLVINYCLDINXSED" \ 8 x16
," TXATXSTAXTSXDEXNOP" \ a x6
59 CONSTANT OPCNT $ff CONSTANT NUL 20 VALUE DISCNT
: >>4 >> >> >> >> ;
: opid. DUP OPCNT < IF
  3 * OPNAME + 3 STYPE ELSE DROP ." ???" THEN ;
: WORDTBL ( n -- ) CREATE >R BEGIN ' , NEXT ;
: spcs ( n -- ) >R BEGIN SPC> NEXT ;
( ----- 031 )
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
( ----- 032 )
: id2 $a2 = IF $0d ELSE NUL THEN ;
16 WORDTBL _ id0 id159d id2 idnul id4c
  id159d id6ae idnul id8 id159d
  id6ae idnul id4c id159d id6ae idnul
: opid DUP $f AND << _ + @ EXECUTE ;
\ 0=inh 1=imm 2=acc 3=zp 4=zp,X 5=zp,Y 6=abs 7=abs,X 8=abs,Y
\ 9=ind 10=ind,X 11=ind,Y 12=rel
CREATE _ $40 nC, 0  10 1 0 3 3 3 0 0 1 2 0 6 6 6 0
                 12 11 0 0 4 4 4 0 0 8 0 0 7 7 7 0
                 1  10 1 0 3 3 3 0 0 1 0 0 6 6 6 0
                 12 11 0 0 4 4 4 0 0 8 0 0 7 7 7 0
: modeid ( opcode -- id )
  DUP $20 = IF DROP 6 EXIT THEN DUP $6c = IF DROP 9 EXIT THEN
  DUP $be = IF DROP 8 EXIT THEN
  DUP $80 AND >> >> SWAP $1f AND OR _ + C@ ;
( ----- 033 )
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
13 WORDTBL _ inh. imm. acc. zp. zp,X. zp,Y. abs. abs,X. abs,Y.
  ind. ind,X. ind,Y. rel.
( ----- 034 )
: mode. ( a opcode -- a ) modeid << _ + @ EXECUTE ;
: op. ( a -- a ) C@+ DUP opid DUP opid. SPC>
  OPCNT < IF mode. ELSE DROP THEN ;
: dump ( a u -- ) >R BEGIN C@+ .x SPC> NEXT DROP ;
HERE PC - VALUE OFFSET
: dis ( a -- ) DISCNT >R BEGIN
  DUP OFFSET - .X SPC> DUP op. SPC>
  TUCK OVER - dump NL> NEXT DROP ;
( ----- 035 )
\ 6502 emulator
CREATE 'A 7 ALLOT
'A 1+ CONSTANT 'X 'X 1+ CONSTANT 'Y 'Y 1+ CONSTANT 'S
'S 1+ CONSTANT 'P 'P 1+ CONSTANT 'PC
0 VALUE EA \ effective addr in *target*. ffff means accumulator
$800 CONSTANT MEMSZ \ 2k ought to be enough for anybody
CREATE MEM MEMSZ ALLOT
: 6502E$ 0 'P C! $200 'PC ! ;
: oor? ( pc -- pc ) DUP MEMSZ >= IF
  .X ABORT"  addr out of range" THEN ;
: mem+ oor? MEM + ; : ea@ EA $ffff = IF 'A ELSE EA mem+ THEN ;
: mc@ mem+ C@ ; : mc@+ DUP mc@ SWAP 1+ SWAP ; : mc! mem+ C! ;
: m@ mem+ @ ; : m@+ DUP m@ SWAP 1+ 1+ SWAP ;
: X+ 'X C@ + ; : Y+ 'Y C@ + ; : a@ 'A C@ ; : a! 'A C! ;
: pc@ 'PC @ ; : mpc@ pc@ mem+ ;
( ----- 036 )
: ea! ( pc -- ) oor? [TO] EA ;
: inh ( pc -- pc+? ) 0 ea! ; ALIAS inh acc
: zp mc@+ ea! ;
: imm DUP ea! 1+ ;
: abs m@+ ea! ;
: ind m@+ m@ ea! ;
: zp,X mc@+ X+ <<8 >>8 ea! ;
: zp,Y mc@+ Y+ <<8 >>8 ea! ;
: abs,X m@+ X+ oor? ea! ;
: abs,Y m@+ Y+ oor? ea! ;
: ind,X mc@+ X+ m@ ea! ;
: ind,Y mc@+ m@ Y+ ea! ;
13 WORDTBL _ inh imm acc zp zp,X zp,Y abs abs,X abs,Y ind ind,X
  ind,Y imm
: eard ( pc opcode -- pc+? ) modeid << _ + @ EXECUTE ;
( ----- 037 )
: p! ( n mask -- ) 'P C@ AND OR 'P C! ;
: carry! ( n -- n ) L|M NOT NOT ( n cf ) $fe p! ;
: carry? ( -- f ) 'P C@ 1 AND ;
: nz! ( n -- ) DUP NOT << SWAP $80 AND OR $7d p! ;
: v! ( n -- ) $80 AND a@ $80 AND XOR >> $bf p! ;
: a!nz DUP a! nz! ; : a!nzv DUP v! a!nz ;
: ora EA mc@ a@ OR a!nz ;
: and EA mc@ a@ AND a!nz ;
: eor EA mc@ a@ XOR a!nz ;
: adc EA mc@ carry? + a@ + carry! a!nzv ;
: sbc a@ EA mc@ carry? + - carry! a!nzv ;
: asl ea@ DUP C@ << carry! DUP nz! SWAP C! ;
: rol ea@ DUP C@ << carry? OR carry! DUP nz! SWAP C! ;
( ----- 038 )
: lsr ea@ DUP C@ DUP 1 AND $fe p! >> DUP nz! SWAP C! ;
: ror
  ea@ DUP C@ carry? <<8 OR DUP 1 AND $fe p! >> DUP nz! SWAP C! ;
: _ DOER , DOES> @ C@ ea@ C! ;
'A _ sta 'X _ stx 'Y _ sty
: _ DOER , DOES> @ EA mc@ DUP nz! SWAP C! ;
'A _ lda 'X _ ldx 'Y _ ldy
: _ DOER , DOES> @ C@ EA mc@ - carry! DUP v! nz! ;
'A _ cmp 'X _ cpx 'Y _ cpy
: pc+ea EA mc@ DUP $7f > IF $ff00 OR THEN 'PC @ + 'PC ! ;
: _ DOER C, DOES> C@ 'P C@ AND IF pc+ea THEN ;
$01 _ bcs $02 _ beq $40 _ bvs $80 _ bmi
: _ DOER C, DOES> C@ 'P C@ AND NOT IF pc+ea THEN ;
$01 _ bcc $02 _ bne $40 _ bvc $80 _ bpl
( ----- 039 )
: _ DOER C, DOES> C@ 'P C@ OR 'P C! ;
$01 _ sec $08 _ sed $04 _ sei $10 _ brk
: _ DOER C, DOES> C@ 'P C@ AND 'P C! ;
$fe _ clc $f7 _ cld $fb _ cli $bf _ clv
: pull ( -- b ) 'S C@ $100 OR mc@+ SWAP 'S C! ;
: push ( b -- ) 'S C@ 1- <<8 >>8 DUP 'S C! $100 OR mc! ;
: pla pull 'A C! ; : plp pull 'P C! ;
: pha a@ push ; : php 'P C@ push ;
: rti plp pull pull <<8 OR 'PC ! ;
: rts pull pull <<8 OR 1+ 'PC ! ;
: jmp EA 'PC ! ;
: jsr pc@ 1- L|M push push jmp ;
: bit EA mc@ DUP a@ AND NOT << OR $cd p! ;
( ----- 040 )
: inc EA mc@ 1+ DUP nz! EA mc! ;
: dec EA mc@ 1- DUP nz! EA mc! ;
: dex 'X C@ 1- DUP nz! 'X C! ;
: dey 'Y C@ 1- DUP nz! 'Y C! ;
: inx 'X C@ 1+ DUP nz! 'X C! ;
: iny 'Y C@ 1+ DUP nz! 'Y C! ;
: txa 'X C@ 'A C! ;
: tax 'A C@ 'X C! ;
: tya 'Y C@ 'A C! ;
: tay 'A C@ 'Y C! ;
: txs 'X C@ 'S C! ;
: tsx 'S C@ 'X C! ;
ALIAS NOOP nop
( ----- 041 )
\ opid same as in disassembler
OPCNT WORDTBL _ ora and eor adc sta lda cmp sbc asl rol lsr ror
  stx ldx dec inc bit jmp sty ldy cpy cpx brk bpl jsr bmi rti
  bvc rts bvs bcc ldy bcs cpy bne cpx beq php clc plp sec pha
  cli pla sei dey tya tay clv iny cld inx sed txa txs tax tsx
  dex nop
: nulop ( op -- ) .x ABORT"  invalid opcode" ;
: oprun ( opcode -- ) opid DUP OPCNT < IF
  << _ + @ EXECUTE ELSE nulop THEN ;
CREATE _ ," AXYSP"
: cpu. _ >A 'A >B 5 >R BEGIN
  AC@+ EMIT SPC> B> C@ .x B+ SPC> NEXT ." PC " 'PC @ .X NL> ;
( ----- 042 )
2 VALUES VERBOSE 'BRK?
: BRK? 'BRK? DUP IF EXECUTE THEN ;
: run1 ( -- )
  'P C@ $10 AND IF ABORT" CPU halted" THEN
  'PC @ mc@+ TUCK eard 'PC ! oprun
  VERBOSE IF cpu. THEN
  BRK? IF ABORT" breakpoint reached" THEN ;
: runN >R BEGIN run1 NEXT ; : run BEGIN run1 AGAIN ;
( ----- 050 )
\ APPLE IIE xcomp, constants and macros
$300 CONSTANT SYSVARS
$91f0 CONSTANT BLK_MEM
SYSVARS $60 + CONSTANT GRID_MEM
GRID_MEM 2 + CONSTANT MSPAN_MEM
MSPAN_MEM 1+ CONSTANT SDC_MEM
4 CONSTS 100 MSPAN_SZ $c0b4 SPI_DATA $c0b5 SPI_CTL
  2 SDC_DEVID
\ ARCM ( D3-01 ) XCOMP ( D2-00 )
\ 6502A ( D3-02-05 D0-07 ) 6502M ( D3-09 )
\ XCOMPC ( D2-01-05 ) $2000 XSTART 6502C ( D3-10-20 )
\ COREL ( D2-10-24 ) Drivers ( D3-60-63 )
( ----- 051 )
\ Apple IIe: xwrap D2 in drive
\ ALIAS FD@ (ms@)
\ ALIAS FD! (ms!)
\ MSPANSUB ( D2-37 )
\ BLKSUB ( D2-30-34 ) GRIDSUB ( D2-40-41 )
: INIT CR NL ! 80col GRID$ BLK$ MSPAN$ ;
\ XWRAP
( ----- 053 )
: _ ( a -- ) DUP 40 + SWAP ( src dst ) LINES 1- 40 * MOVE ;
: scroll 0 pos2a _ 1 pos2a _ ;
: scroll LINES >R BEGIN LINES R@ - COLS * DUP COLS + ( dst src )
    2DUP pos2a SWAP pos2a 40 MOVE ( dst src ) 1+ pos2a SWAP 1+
    pos2a 40 MOVE NEXT ;
( ----- 060 )
\ Apple IIe drivers, (key?)
CODE (key?) ( -- c? f )
  DEX, DEX, 0 # LDA, 0 A>PS, 1 A>PS, $c000 () LDA, FJR BPL,
    $7f # AND, 0 A>PS, DEX, DEX, 0 # LDA, 1 A>PS,
    1 # LDA, 0 A>PS, $c010 () STA,
  THEN, ;CODE
( ----- 061 )
\ Apple IIe drivers, grid
CODE pos2a ( pos -- a )
  ( div by 80 ) 80 # LDA, INDL <> STA, 1 <X+> LDA, 8 # LDY,
  0 <X+> ASL, BEGIN, ROLA,
    IFNC, INDL <> CMP, FJR BCC, TO L1 THEN,
    INDL <> SBC, SEC, L1 FMARK 0 <X+> ROL, DEY, BR BNE,
  ( 0=y A=x ) TAY, 0 PS>A, 0 <X+> STY, ( 0=x A=y )
  $c054 () STA, CLC, RORA, 0 <X+> ROR,
  IFNC, $c055 () STA, THEN, TAY, 3 # AND, 4 # ORA, 1 A>PS, TYA,
  $0c # AND, IFNZ, 8 # CMP, 40 # LDA, IFC, CLC, ROLA, THEN,
    0 <X+> ADC, 0 A>PS, THEN, ;CODE
( ----- 062 )
\ Apple IIe drivers, grid
2 CONSTS 80 COLS 24 LINES
CODE 80col $c300 JSR, ;CODE
CODE hi ( c pos ) 2 PS>A, $7f # AND, $60 # CMP,
  IFNC, $3f # AND, THEN, 2 A>PS, ;CODE
CODE lo ( c pos ) 2 PS>A, $80 # ORA, 2 A>PS, ;CODE
: CELL! ( c pos ) pos2a lo C! ;
: CURSOR! ( new old -- )
  pos2a DUP C@ SWAP lo C!
  pos2a DUP C@ SWAP hi C! ;
( ----- 063 )
\ Apple IIe drivers, Floppy Drive
\ NOTE: this write 3 bytes over allocated space after N. This
\ might be a problem depending on how variables are arranged.
CODE _p ( blkno addr -- ) \ blkno = ProDOS 512b blk!
  3 # LDA, 'N <> STA, $60 # LDA, 'N 1+ <> STA, 0 <X+> LDA,
  'N 2 + <> STA, 1 <X+> LDA, 'N 3 + <> STA, 2 <X+> LDA,
  'N 4 + <> STA, 3 <X+> LDA, 'N 5 + <> STA,
  INX, INX, INX, INX, ;CODE
: _e LIT" FDErr " STYPE .x ABORT ;
LSET L1 DEX, DEX, 0 <X+> STA, 0 # LDA, 1 <X+> STA, X' _e JMP,
CODE _r $bf00 JSR, $80 C, 'N L, L1 BR BCS, ;CODE
CODE _w $bf00 JSR, $81 C, 'N L, L1 BR BCS, ;CODE
: FD@ ( blk blk( -- ) SWAP << TUCK 1+ OVER $200 + _p _r _p _r ;
: FD! ( blk blk( -- ) SWAP << TUCK 1+ OVER $200 + _p _w _p _w ;
( ----- 064 )
\ Apple IIe, SPI
CODE (spie) ( n -- )
  0 <X+> LDA, INX, INX, SPI_CTL () STA, ;CODE
CODE (spix) ( n -- n )
  0 <X+> LDA, SPI_DATA () STA, 0 # LDA, 1 <X+> STA,
  SPI_DATA () LDA, 0 <X+> STA, ;CODE
