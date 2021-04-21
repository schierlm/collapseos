( ----- 000 )
6809 Emulator

Requires 6809 asm loaded.

( ----- 001 )
: REG CREATE 0 , ;
REG X REG Y REG U REG S REG PC REG D REG DP REG CC
D CONSTANT A
D 1+ CONSTANT B
VARIABLE MEM VARIABLE EA VARIABLE WIDE? CREATE HALT? 0 ,
: allotmem ( size -- ) HERE MEM ! ALLOT0 ;
( ----- 002 )
: 2* 1 LSHIFT ;
: _ DUP STYPE SPC> FIND DROP EXECUTE T@ .X SPC> ;
: cpudump LIT" X" _ LIT" Y" _ LIT" U" _ LIT" S" _ LIT" D" _
  LIT" PC" _ LIT" DP" _ LIT" CC" _ ;
: MEM+ ( off -- addr ) MEM @ + ;
: PC+ ( -- pc ) PC T@ DUP 1+ PC T! ;
: PC@+ ( -- n ) PC+ MEM+ C@ ;
: PC++ ( -- pc ) PC T@ DUP 2 + PC T! ;
: PC@++ ( -- n ) PC++ MEM+ T@ ;
: EA@@ EA @ MEM+ C@ ; : EA@! EA @ MEM+ C! ;
( ----- 003 )
: carry> ( -- f ) CC C@ 0x01 AND ;
: >carry ( f -- ) CC C@ 0xfe AND OR CC C! ;
: >>CC ( b -- b>>1 ) 1 RSHIFT CARRY? >carry ;
: <<CC ( b -- b<<1 ) 1 LSHIFT CARRY? >carry ;
: ZNVupd ( old new -- ) 0xff AND
  ( Z? ) DUP 0 = ( old new z )
  ( N? ) SWAP 7 RSHIFT ( old z n )
  ( V? n != oldn ) ROT 7 RSHIFT OVER = NOT ( z n v )
  1 LSHIFT SWAP 3 LSHIFT OR ( z f ) SWAP 2 LSHIFT OR
  ( f = 0000NZV0 ) CC C@ 0xf1 AND OR CC C! ;
( ----- 004 )
: operr? ?DUP NOT IF ABORT" invalid opcode" THEN ;
: getOP ( page opcode -- opword ) 0x0f AND 2* + @ operr? ;
: immediate ( -- ea ) PC+ ;
: direct ( -- ea ) PC@+ DP C@ 8 LSHIFT + ;
: extended ( -- ea ) PC@++ ;
: OP8 ( opword tgtaddr )
  DUP >R C@ DUP ROT ( n n op ) EXECUTE ( old new f )
  DUP 0x1 AND IF OVER R> C! ELSE R> DROP THEN
  0x2 AND IF ZNVupd ELSE 2DROP THEN ;
: OPmem8 ( opword ea ) DUP EA ! MEM+ OP8 ;
: OPA ( opword ea ) EA ! A OP8 ;
: OPB ( opword ea ) EA ! B OP8 ;
( ----- 005 )
( op signature: n -- n f
  f = bit0 = update target bit1 = update CC )
: ASR DUP >>CC SWAP 0x80 AND OR 3 ;
: COM 0xff XOR 3 ;
: CLR DROP 0 1 CC C@ 0xf0 AND 0x04 OR CC C! ;
: DEC 1- 3 ; : INC 1+ 3 ;
: JMP DROP EA @ PC T! 0 ;
: LSL <<CC 3 ; : LSR >>CC 3 ;
: NEG 0 -^ 3 ; : TST 2 ;
: ROL carry> SWAP <<CC OR 3 ;
: ROR carry> 7 LSHIFT SWAP >>CC OR 3 ;
( ----- 006 )
CREATE TYPE0457 ( 0x00, 0x40, 0x50 and 0x70 )
  ' NEG , 0 , 0 , ' COM , ' LSR , 0 , ' ROR , ' ASR ,
  ' LSL , ' ROL , ' DEC , 0 , ' INC , ' TST , ' JMP , ' CLR ,
: p1t0 ( opcode -- ) TYPE0457 SWAP getOP direct OPmem8 ;
: p1t4 ( opcode -- ) TYPE0457 SWAP getOP 0 OPA ;
: p1t5 ( opcode -- ) TYPE0457 SWAP getOP 0 OPB ;
: p1t7 ( opcode -- ) TYPE0457 SWAP getOP extended OPmem8 ;
( ----- 007 )
( op signature: n -- n f, same as TYPE0457 )
: ADD EA@@ + DUP 0xff > >carry 3 ; : ADC carry> + ADD ;
: SUB EA@@ - DUP 0< >carry 3 ; : CMP SUB 1- ;
: SBC carry> - SUB ;
: AND_ EA@@ AND 3 ; : BIT AND 1- ;
: EOR EA@@ XOR 3 ; : OR_ EA@@ OR 3 ;
: LD DROP EA@@ 3 ; : ST EA@! 0 2 ;
( ----- 008 )
CREATE TYPE89 ( 0x80, 0x90 )
  ' SUB , ' CMP , ' SBC , 0 , ' AND_ , ' BIT , ' LD , ' ST ,
  ' EOR , ' ADC , ' OR_ , ' ADD , 0 , 0 , 0 , 0 ,
: p1t8 ( opcode -- ) TYPE89 SWAP getOP immediate OPA ;
: p1t9 ( opcode -- ) TYPE89 SWAP getOP direct OPA ;
( ----- 009 )
: SYNC 1 HALT? ! ; : NOP ;
CREATE TYPE1 ( 0x10, special stuff, opwords self-contained )
  0 , 0 , ' NOP , ' SYNC , 0 , 0 , 0 , 0 ,
  0 , 0 , 0 , 0 , 0 , 0 , 0 , 0 ,
: p1t1 ( opcode -- ) TYPE1 SWAP getOP EXECUTE ;
( ----- 010 )
CREATE PAGE1
  ' p1t0 , ' p1t1 , 0 , 0 , ' p1t4 , ' p1t5 , 0 , ' p1t7 ,
  ' p1t8 , ' p1t9 , 0 , 0 , 0 , 0 , 0 , 0 ,
: runop ( opcode -- )
  DUP 4 RSHIFT 2* PAGE1 + @ operr? EXECUTE ;
: run1 0 WIDE? ! PC@+ runop ;
: run BEGIN run1 HALT? @ UNTIL ;
