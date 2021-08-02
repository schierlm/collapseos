( ----- 000 )
( see emul6809.txt for doc )
( mapping: D X Y U S PC CC/DP )
CREATE CPU 14 ALLOT0
: REG ( regid -- addr )
( D, X, Y, U, S and PC map directly without problem. we only
  need to double them. A, B, CCR, DPR need special stuff. )
  DUP A < IF ( regular ) << ELSE
    DUP CCR < IF 8 - ELSE 2 + THEN THEN
  CPU + ;
\ OPCODE = last read opcode
\ TGT = register const or $ff for mem
7 VALUES MEM 0 EA 0 WIDE 0 HALT? 0 PAGE 0 OPCODE 0 TGT 0
( ----- 001 )
: allotmem ( size -- ) HERE [TO] MEM ALLOT0 ;
: WORDTBL ( n -- ) CREATE 0 DO ' , LOOP ;
: word@ ( opcode page -- opword ) SWAP $0f AND << + @ ;
: >>4 >> >> >> >> ;
( ----- 002 )
: W?@ WIDE IF T@ ELSE C@ THEN ;
: W?! WIDE IF T! ELSE C! THEN ;
: neg? ( n -- f ) WIDE IF 0< ELSE << >>8 THEN ;
: signext ( b -- n ) DUP $7f > IF $ff00 OR THEN ;
: MEM+ ( off -- addr ) MEM + ;
: PC@ PCR REG T@ ; : PC! PCR REG T! ;
: PC+ ( -- pc ) PC@ DUP 1+ PC! ;
: PC@+ ( -- b ) PC+ MEM+ C@ ;
: PC++ ( -- pc ) PC@ DUP 2 + PC! ;
: PC@++ ( -- n ) PC++ MEM+ T@ ;
: PC+n! ( n -- ) PC@ + PC! ; : PC+b! ( b -- ) signext PC+n! ;
: EA@@ EA MEM+ W?@ ; : EA@! EA MEM+ W?! ;
( ----- 003 )
: push8 S REG T@ 1- DUP S REG T! MEM+ C! ;
: push16 DUP >>8 push8 push8 ;
: pull8 S REG T@ DUP 1+ S REG T! MEM+ C@ ;
: pull16 pull8 <<8 pull8 OR ;
: CC@ CCR REG C@ ; : CC! CCR REG C! ;
: carry> ( -- f ) CC@ $01 AND ;
: >carry ( f -- ) CC@ $fe AND OR CC! ;
: >>CC ( b -- b>>1 ) >> CARRY? >carry ;
: <<CC ( b -- b<<1 ) << DUP $ff > >carry ;
: cpudump ." A B  X    Y    U    S    PC   CC DP" NL>
  6 0 DO I REG T@ .X SPC> LOOP CC@ .x SPC> DPR REG C@ .x ;
: NIL ." invalid opcode " OPCODE .x NL> cpudump ABORT ;
( ----- 004 )
( sig: reg -- ea )
: r+ DUP T@ 2DUP 1+ SWAP T! ; : r++ DUP T@ 2DUP 2 + SWAP T! ;
: -r DUP T@ 1- SWAP T! ; : --r DUP T@ 2 - SWAP T! ;
: r+0 T@ ; : r+8 T@ PC@+ + ; : r+16 T@ PC@++ + ;
: r+A T@ A REG C@ + ; : r+B T@ B REG C@ + ;
: r+D T@ D REG T@ + ;
: PC+8 DROP PC@+ PC@ + ; : PC+16 DROP PC@++ PC@ + ;
: iext DROP PC@++ ;
( ----- 005 )
( maps lower nibble )
16 WORDTBL IMODES
  r+  r++  -r  --r r+0  r+B   r+A NIL
  r+8 r+16 NIL r+D PC+8 PC+16 NIL iext
: r+5b ( code reg -- ea ) T@ SWAP $1f AND + ;
: indexed
  PC@+ DUP << << << >>8 3 AND ( regid-1 ) 1+ REG ( code reg )
  OVER $80 AND IF
    OVER $f AND << IMODES + @ EXECUTE ( code ea )
    SWAP $10 AND IF ( indirect ) T@ THEN
  ELSE r+5b THEN ( ea ) DUP .X  SPC> [TO] EA ;
( ----- 006 )
: nop ;
: imm WIDE IF PC++ ELSE PC+ THEN [TO] EA ;
: direct PC@+ DPR REG C@ <<8 + [TO] EA ;
: extended PC@++ [TO] EA ;
16 WORDTBL ADDRS
  direct nop    nop     nop      nop nop    indexed extended
  imm    direct indexed extended imm direct indexed extended
: setEA OPCODE >>4 << ADDRS + @ EXECUTE ;
( ----- 007 )
CREATE TGTS $ff C, 0 C, 0 C, 0 C, A C, B C, $ff C, $ff C,
            A C,    A C, A C, A C, B C, B C, B C,    B C,
: setTGT ( opcode -- ) 0 [TO] WIDE >>4 TGTS + C@ [TO] TGT ;
: TGT@ TGT DUP $ff = IF DROP EA MEM+ ELSE REG THEN ;
: ZNVupd ( old new -- ) $ff AND
  ( Z? ) DUP 0 = ( old new z ) ( N? ) SWAP neg? ( old z n )
  ( V? n != oldn ) ROT neg? OVER = NOT ( z n v )
  << SWAP <<3 OR ( z f ) SWAP << << OR
  ( f = 0000NZV0 ) CC@ $f1 AND OR CC! ;
: OPexec ( opword )
  setEA TGT@ W?@ DUP ROT ( n n op ) EXECUTE ( old new f )
  DUP $1 AND IF OVER TGT@ W?! THEN
  $2 AND IF ZNVupd ELSE 2DROP THEN ;
( ----- 008 )
( branches bit0: "invert", bit 1-3: conditions in this order:
  Always, C+Z=0, C=0, Z=0, V=0, N=0, N=V, N=V Z=0 )
( sig: cc -- !f, where !f is *false* when condition is met )
: CBRA 0 ; : CBHI 5 AND ; : CBCC 1 AND ; : CBNE 4 AND ;
: CBVC 2 AND ; : CBPL 8 AND ;
: CBGE DUP CBVC SWAP CBPL = NOT ;
: CBGT DUP CBGE SWAP CBNE OR ;
8 WORDTBL COND CBRA CBHI CBCC CBNE CBVC CBPL CBGE CBGT
: br? ( opcode -- f )
  $0f AND >> CARRY? ( condidx invert? )
  SWAP << COND + @ ( invert? cond ) CC@ SWAP EXECUTE
  ( invert? result ) = ;
: BRA PC@+ PC+b! ; : LBRA PC@++ PC+n! ;
: grp2 ( opcode -- )
  br? IF PAGE IF LBRA ELSE BRA THEN ELSE PC+ DROP THEN ;
( ----- 009 )
( op signature: n -- n f
  f = bit0 = update target bit1 = update CC )
: ASR DUP >>CC SWAP $80 AND OR 3 ;
: COM $ff XOR 3 ;
: CLR DROP 0 1 CC@ $f0 AND $04 OR CC! ;
: DEC 1- 3 ; : INC 1+ 3 ;
: JMP DROP EA PC! 0 ;
: LSL <<CC 3 ; : LSR >>CC 3 ;
: NEG 0 -^ 3 ; : TST 2 ;
: ROL carry> SWAP <<CC OR 3 ;
: ROR carry> <<8 >> SWAP >>CC OR 3 ;
16 WORDTBL OPS
  NEG NIL NIL COM LSR NIL ROR ASR
  LSL ROL DEC NIL INC TST JMP CLR
: grp0 ( opcode -- ) OPS word@ OPexec ;
( ----- 010 )
( op signature: n -- n f, same as previous )
: ADD EA@@ WIDE IF + CARRY? ELSE + DUP $ff > THEN >carry 3 ;
: ADC carry> + ADD ;
: SUB EA@@ - CARRY? >carry 3 ; : CMP SUB 1- ;
: SBC carry> - SUB ;
: AND_ EA@@ AND 3 ; : BIT AND 1- ;
: EOR EA@@ XOR 3 ; : OR_ EA@@ OR 3 ;
: LD DROP EA@@ 3 ; : ST EA@! 0 2 ;
( op sig: -- )
: JSR PC@ push16 EA PC! ;
: BSR PC@+ PC@ push16 PC+b! ;
( ----- 011 )
( bit0: grp8-b/grpc-f bit1: page1 bit2: page2 )
6 WORDTBL OPS SUB ADD CMP NIL CMP NIL
CREATE TGTS D C, D C, U C,
: _d ( opcode -- )
  1 [TO] WIDE PAGE TGTS + C@ [TO] TGT
  $40 AND << << >>8 PAGE << + OPS word@ OPexec ;
8 WORDTBL OPS CMP JSR LD ST LD ST LD ST
( bit2:0: grp8-b/grpc-f bit3: page1 bit4: page2 )
CREATE TGTS X C, X C, X C, X C, D C, D C, U C, U C,
            Y C, Y C, Y C, Y C, S C, S C, S C, S C,
            S C, S C, S C, S C, S C, S C, S C, S C,
: _xyus ( opcode -- )
  DUP $8d = IF DROP BSR EXIT THEN
  1 [TO] WIDE ( put bits 6 and 1:0 together )
  DUP $03 AND SWAP $40 AND << << >>8 << << OR
  DUP PAGE <<3 + TGTS + C@ [TO] TGT OPS word@ OPexec ;
( ----- 012 )
12 WORDTBL OPS
  SUB CMP SBC NIL AND_ BIT LD  ST EOR ADC OR_ ADD
: _ab ( opcode -- ) OPS word@ OPexec ;
16 WORDTBL OPSETS
  _ab _ab _ab _d  _ab   _ab   _ab   _ab
  _ab _ab _ab _ab _xyus _xyus _xyus _xyus
: grp8-f ( opcode -- ) DUP OPSETS word@ EXECUTE ;
( ----- 013 )
( $30, special stuff, opwords self-contained )
: LEA ( regid ) indexed EA SWAP REG T! ;
: LEAX X LEA ; : LEAY Y LEA ; : LEAS S LEA ; : LEAU U LEA ;
CREATE ORDER PCR C, U C, Y C, X C, DPR C, B C, A C, CCR C,
: PSHS PC@+ <<8 8 0 DO << CARRY? IF I ORDER + C@ REG
    I 4 < IF T@ push16 ELSE C@ push8 THEN THEN LOOP ;
: PULS PC@+ 8 0 DO >> CARRY? IF 7 I - ORDER + C@ REG
    I 4 < IF pull8 SWAP C! ELSE pull16 SWAP T! THEN THEN LOOP ;
: R<>R ( r1 r2 -- ) OVER W?@ >R DUP W?@ ROT W?! R> SWAP W?! ;
: U<>S U REG S REG R<>R ;
: PSHU U<>S PSHS U<>S ; : PULU U<>S PULS U<>S ;
: TODO ABORT" TODO" ; : RTS TODO ; : ABX TODO ; : RTI TODO ;
: CWAI TODO ; : SWI TODO ;
: MUL A REG C@ B REG C@ * DUP D REG T! ( n )
  DUP NOT << << ( n z ) SWAP $80 AND << >>8 ( z c ) OR
  ( f = 00000Z0C ) CC@ $fa AND OR CC! ;
( ----- 014 )
16 WORDTBL OPS
  LEAX LEAY LEAS LEAU PSHS PULS PSHU PULU
  NIL  RTS  ABX  RTI  CWAI MUL  NIL  SWI
: grp3 ( opcode -- ) OPS word@ EXECUTE ;
( ----- 015 )
( $10, special stuff, opwords self-contained )
: SYNC 1 [TO] HALT? ;
: DAA TODO ; : LBSR PC@++ PC@ push16 PC+n! ;
: ORCC ['] OR_ CCR REG OPexec ;
: ANDCC ['] AND_ CCR REG OPexec ;
: SEX B REG C@ signext DUP D REG T! DUP ZNVupd ;
: _regs ( -- rd rs )
  PC@+ DUP $80 < [TO] WIDE DUP $0f AND REG SWAP >>4 REG ;
: EXG _regs R<>R ; : TFR _regs W?@ SWAP W?! ;
16 WORDTBL OPS
  NIL NIL nop  SYNC NIL   NIL LBRA LBSR
  NIL DAA ORCC NIL  ANDCC SEX EXG  TFR
: grp1 ( opcode -- ) OPS word@ EXECUTE ;
( ----- 016 )
8 WORDTBL GRP0-7 grp0 grp1 grp2 grp3 grp0 grp0 grp0 grp0
: grp0-7 DUP >>4 << GRP0-7 + @ EXECUTE ;
: run1
  PC@+ DUP $10 = IF DROP 1 [TO] PAGE PC@+ THEN
  DUP $11 = IF DROP 2 [TO] PAGE PC@+ THEN
  DUP [TO] OPCODE DUP setTGT
  DUP $80 AND IF grp8-f ELSE grp0-7 THEN 0 [TO] PAGE ;
: run BEGIN run1 HALT? UNTIL ;
