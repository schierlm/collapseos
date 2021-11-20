( ----- 000 )
6502 MASTER INDEX

301 6502 macros and consts     302 6502 assembler
310 6502 boot code             320 6502 disassembler
325 6502 emulator
340 Virgil's workspace
( ----- 001 )
\ 6502 macros and constants. See doc/code/6502.txt
: 6502A ASML 302 305 LOADR ASMH ;
: 6502H 306 LOAD ;
: 6502C 310 313 LOADR ;
: 6502D 320 324 LOADR ;
: 6502E 325 332 LOADR ;
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
: i>, $caca M, ( DEX DEX )
      $a9 C, DUP C, ( LDA ) $9500 M, ( STA X+ )
      $a9 C, >>8 C, ( LDA ) $9501 M, ( STA X+ ) ;
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
CODE BYE BRK,
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
CODE C! 2 <X+> LDA, 0 [X+] STA, INX, INX, INX, INX, ;CODE
CODE ! 2 <X+> LDA, 0 [X+] STA, INLINE 1+
  3 <X+> LDA, 0 [X+] STA, INX, INX, INX, INX, ;CODE
CODE AC!+ INLINE A> INLINE C! INLINE A+ ;CODE
( ----- 014 )
: STYPE >R >A BEGIN AC@+ (emit) NEXT ;
: BOOT INIT LIT" Collapse OS" STYPE BYE ;
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
59 CONSTANT OPCNT $ff CONSTANT NUL 20 VALUE DISCNT
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
13 WORDTBL _ inh. imm. acc. zp. zp,X. zp,Y. abs. abs,X. abs,Y.
  ind. ind,X. ind,Y. rel.
( ----- 024 )
: mode. ( a opcode -- a ) modeid << _ + @ EXECUTE ;
: op. ( a -- a ) C@+ DUP opid DUP opid. SPC>
  OPCNT < IF mode. ELSE DROP THEN ;
: dump ( a u -- ) >R BEGIN C@+ .x SPC> NEXT DROP ;
: dis ( a -- ) DISCNT >R BEGIN
  DUP ORG - BIN( + .X SPC> DUP op. SPC>
  TUCK OVER - dump NL> NEXT DROP ;
( ----- 025 )
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
( ----- 026 )
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
( ----- 027 )
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
( ----- 028 )
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
( ----- 029 )
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
( ----- 030 )
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
( ----- 031 )
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
( ----- 032 )
2 VALUES VERBOSE 'BRK?
: BRK? 'BRK? DUP IF EXECUTE THEN ;
: run1 ( -- )
  'P C@ $10 AND IF ABORT" CPU halted" THEN
  'PC @ mc@+ TUCK eard 'PC ! oprun
  VERBOSE IF cpu. THEN
  BRK? IF ABORT" breakpoint reached" THEN ;
: runN >R BEGIN run1 NEXT ; : run BEGIN run1 AGAIN ;
( ----- 040 )
\ play around with emulator: ARCHM 6502A 6502D 6502E
6502E$ 1 TO VERBOSE
HERE MEM $200 + 'HERE !
$203 JMP, $02 # LDA, TAY, $12 # ADC, 1 <> SBC, BRK,
'HERE !
( ----- 041 )
\ xcomp for emulated 6502 machine
$200 TO BIN(
( ----- 042 )
\ extra words for emulating COS. Load after 6502E
: pullX ( -- b ) 'X C@ mc@+ SWAP 'X C! ;
: pushX ( b -- ) 'X C@ 1- <<8 >>8 DUP 'X C! mc! ;
( ----- 043 )
\ test 6502 bare native words under emulator
\ do regular xcomp until *before* blk containing BOOT.
\ then load this. Copy to emul's MEM+$200 then run.
\ toPC with ORG+04 will get you to BOOT. then, toBRK
CODE BOOT 42 i>, $1234 i>, INLINE + BRK,
XCURRENT ORG $04 + T!
( ----- 044 )
\ same as prev block, but BOOT is a XT word
: BOOT INIT 'X' (emit) 'Y' (emit) BYE ;
XCURRENT ORG $04 + T!
( ----- 045 )
\ drivers for 6502 emulator. simply emit in memory at page $700
\ $7ff contains the current emit position.
CODE (emit) 0 <X+> LDA, INX, INX, $7ff () LDY, $700 (Y+) STA,
$7ff () INC, ;CODE
CODE INIT 0 # LDA, $7ff () STA, ;CODE
