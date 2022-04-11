( ----- 000 )
MASTER INDEX

001 Useful little words       010 RX/TX tools
020 Block editor              035 Memory Editor
040 AVR SPI programmer        045 Sega ROM signer
050 Virgil's workspace        060-199 unused
200 Cross compilation
210 Core words                230 BLK subsystem
235 RX/TX subsystem           237 Media Span subsystem
240 Grid subsystem
245 PS/2 keyboard subsystem   250 SD Card subsystem
260 Fonts                     290 Automated tests
300 Arch-specific content
( ----- 001 )
\ Useful little words. MIN MAX MOVE-
\ parse the next n words and write them as chars
: MIN ( n n - n ) 2DUP > IF SWAP THEN DROP ;
: MAX ( n n - n ) 2DUP < IF SWAP THEN DROP ;
\ Compute CRC16 over a memory range
: CRC16[] ( a u -- c ) >R >A 0 BEGIN AC@+ CRC16 NEXT ;
: MOVE- ( a1 a2 u -- ) \ *A* MOVE starting from the end
  ?DUP IF >R OVER - ( a1 diff ) SWAP R@ + >A
    BEGIN ( diff ) A- A> OVER + AC@ SWAP C! NEXT DROP
  ELSE 2DROP THEN ;
( ----- 002 )
\ Useful little words. MEM>BLK BLK>MEM
\ *A* Copy an area of memory into blocks.
: MEM>BLK ( addr blkno blkcnt )
  >R BEGIN ( a blk )
    DUP BLK@ 1+ SWAP DUP BLK( $400 MOVE BLK!! $400 + SWAP NEXT
  DROP FLUSH ;
\ *A* Copy subsequent blocks in an area of memory
: BLK>MEM ( blkno blkcnt addr )
  ROT> >R BEGIN ( a blk )
    DUP BLK@ 1+ SWAP BLK( OVER $400 MOVE $400 + SWAP NEXT
  DROP ;
( ----- 003 )
\ Context. Allows multiple concurrent dictionaries.
\ See doc/usage.txt

0 VALUE saveto \ where to save CURRENT in next switch
: context DOER CURRENT , DOES> ( a -- )
  saveto IF CURRENT [TO] saveto THEN ( a )
  DUP [TO] saveto ( a )
  @ CURRENT ! ;
( ----- 004 )
\ Grid applications helper words. nspcs clrscr
: nspcs ( pos n ) >R BEGIN SPC OVER CELL! 1+ NEXT DROP ;
: clrscr 0 COLS LINES * nspcs ;
( ----- 005 )
\ Word table. See doc/wordtbl
: WORDTBL ( n -- a ) CREATE HERE SWAP << ALLOT0 1 HERE C! ;
: W+ ( a -- a+2? ) 1+ 1+ DUP @ IF DROP THEN ;
: :W ( a -- a+2? ) HERE XTCOMP OVER ! W+ ;
: 'W ( a -- a+2? ) ' OVER ! W+ ;
: WEXEC ( tbl idx -- ) << + @ EXECUTE ;
( ----- 006 )
\ Pager. See doc/pager
4 VALUES ''EMIT ''KEY? chrcnt lncnt
20 VALUE PGSZ
: realKEY BEGIN ''KEY? EXECUTE UNTIL ;
: back ''EMIT 'EMIT ! ''KEY? 'KEY? ! ;
: emit ( c -- )
  chrcnt 1+ [TO] chrcnt
  DUP CR = chrcnt LNSZ = OR IF
   0 [TO] chrcnt lncnt 1+ [TO] lncnt THEN
  ''EMIT EXECUTE lncnt PGSZ = IF
    0 [TO] lncnt NL> ." Press q to quit, any key otherwise" NL>
    realKEY 'q' = IF back QUIT THEN THEN ;
: key? back KEY? ;
: page 'EMIT @ [TO] ''EMIT 'KEY? @ [TO] ''KEY?
  ['] emit 'EMIT ! ['] key? 'KEY? ! ;
( ----- 007 )
\ Flow words
'? PC NOT [IF] ALIAS HERE PC [THEN]
'? PC2A NOT [IF] : PC2A ; [THEN]
ALIAS PC BEGIN,
: LSET PC TO ;
: BR PC - 2 - _bchk ;
: FJR BEGIN, 1+ 0 ;
: IFZ, FJR JRNZi, ; : IFNZ, FJR JRZi, ;
: IFC, FJR JRNCi, ; : IFNC, FJR JRCi, ;
\ warning: l is a PC value, not a mem addr!
\ also, in 6502, JRi, is 3b instead of 2, hence the hack.
: FMARK ( l -- ) PC2A DUP C@ IF ( hack ) 1+ THEN DUP HERE -^ 1-
  SWAP C! ;
: THEN, FMARK ; : ELSE, FJR JRi, SWAP FMARK ;
( ----- 010 )
\ Communicate blocks with block server. See doc/blksrv.
CREATE h16 '$' C, 4 ALLOT
: RX>h16 ( -- n ) \ *A*
  h16 1+ >A 4 >R BEGIN RX< DUP EMIT SPC> AC!+ NEXT
  h16 5 PARSE NOT IF 0 THEN ;
: csumchk ( c1 c2 ) = NOT IF ABORT" bad csum" THEN ;
: blksrv< ( blkno -- ) \ *A*
  RX<< TX[ 'G' EMIT .X ]TX 0 ( csum ) BLK( >A 1024 >R BEGIN
    RX< DUP AC!+ + NEXT RX>h16 csumchk ;
: blksrv> ( blkno -- ) \ *A*
  RX<< TX[ 'P' EMIT .X ]TX 0 ( csum ) BLK( >A 1024 >R BEGIN
    AC@+ DUP TX> + NEXT TX[ .X ]TX ;
( ----- 011 )
\ Remote shell. See doc/rxtx
: RX<?? RX<? ?DUP NOT IF 100 TICKS RX<? THEN ;
: _<< \ print everything available from RX<?
  BEGIN RX<?? IF EMIT ELSE EXIT THEN AGAIN ;
: _<<1r RX< EMIT _<< ;
: rsh BEGIN
  KEY? IF DUP EOT = IF DROP EXIT ELSE TX> THEN THEN _<< AGAIN ;
( ----- 012 )
\ rupload. See doc/rxtx
: CR> CR EMIT ;
: unpack DUP $f0 OR SWAP $0f OR ;
: out unpack TX> TX> ; : out2 L|M out out ;
: rdok \ read RX until after "ok"
    BEGIN RX< WS? NOT UNTIL _<<1r ;
: rupload ( loca rema u -- )
  TX[ ." : in KEY $f0 AND KEY $0f AND OR ;" CR> rdok
      ." : in2 in <<8 in OR ;" CR> rdok
      \ sig: chk -- chk, a and then u are KEYed in
      ." : _ in2 >A in2 >R BEGIN in TUCK + SWAP AC!+ NEXT ;"
      CR> rdok DUP ROT ( loca u u rema )
      ." 0 _" CR> out2 out2 ]TX
  >R >A 0 BEGIN ( chk ) '.' EMIT AC@ out AC@+ + NEXT
  _<<1r TX[ ." .X FORGET in" CR> ]TX rdok .X ;
( ----- 013 )
\ XMODEM routines. See doc/rxtx
: _<<s BEGIN RX<? IF DROP ELSE EXIT THEN AGAIN ;
: _rx>mem1 ( addr -- f, Receive single packet, f=eot )
  RX< 1 = NOT IF ( EOT ) $6 ( ACK ) TX> 1 EXIT THEN
  '.' EMIT RX< RX< 2DROP ( packet num )
  >A 0 ( crc ) 128 >R BEGIN ( crc )
    RX< DUP ( crc n n ) AC!+ ( crc n ) CRC16 NEXT
  RX< <<8 RX< OR ( sender's CRC )
  = IF $6 ( ACK ) ELSE $15 'N' EMIT ( NACK ) THEN TX> 0 ;
: RX>MEM ( addr --, Receive packets into addr until EOT )
  _<<s 'C' TX> BEGIN ( a )
  DUP _rx>mem1 SWAP 128 + SWAP UNTIL DROP ;
: RX>BLK ( -- )
  _<<s 'C' TX> BLK( BEGIN ( a )
  DUP BLK) = IF DROP BLK( BLK! BLK> 1+ 'BLK> ! THEN
  DUP _rx>mem1 SWAP 128 + SWAP UNTIL 2DROP ;
( ----- 014 )
: _snd128 ( A:a -- A:a )
    0 128 >R BEGIN ( crc )
      AC@+ DUP TX> ( crc n ) CRC16 ( crc ) NEXT
    L|M TX> TX> ;
: _ack? 0 BEGIN DROP RX< DUP 'C' = NOT UNTIL
	DUP $06 ( ACK ) = IF DROP 1
    ELSE $15 = NOT IF ABORT" out of sync" THEN 0 THEN ;
: _waitC
  ." Waiting for C..." BEGIN RX<? IF 'C' = ELSE 0 THEN UNTIL ;
: _mem>tx ( addr pktstart pktend -- )
  OVER - >R SWAP >A BEGIN ( pkt )
    'P' EMIT DUP . SPC> $01 ( SOH ) TX> ( pkt )
    1+ ( pkt start at 1 ) DUP TX> $ff OVER - TX> ( pkt+1 )
    _snd128 _ack? NOT IF LEAVE THEN NEXT DROP ;
( ----- 015 )
: MEM>TX ( a u -- Send u bytes to TX )
  _waitC 128 /MOD SWAP IF 1+ THEN ( pktcnt ) 0 SWAP _mem>tx
  $4 ( EOT ) TX> RX< DROP ;
: BLK>TX ( b1 b2 -- )
  _waitC OVER - ( cnt ) >R BEGIN ( blk )
    'B' EMIT DUP . SPC> DUP BLK@ BLK( ( blk a )
    OVER 8 * DUP 8 + ( a pktstart pktend ) _mem>tx 1+ NEXT
  $4 ( EOT ) TX> RX< DROP ;
( ----- 020 )
\ Block editor. see doc/ed.
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
( ----- 021 )
\ Block editor, private helpers
: _lpos ( ln -- a ) LNSZ * 'pos ;
: _pln ( ln -- ) \ print line no ln with pos caret
  DUP _lpos DUP >A LNLEN 1 MAX >R BEGIN ( lno )
    A> 'EDPOS = IF '^' EMIT THEN
    AC@+ SPC MAX EMIT NEXT ( lno ) SPC> 1+ . ;
: _zline ( a -- ) LNSZ SPC FILL ; \ zero-out a line
: _type ( buf -- ) \ *A* type into buf until end of INBUF
  IN<? ?DUP NOT IF DROP EXIT THEN OVER 1+ DUP _zline >A BEGIN
    ( buf c ) AC!+ IN<? ?DUP NOT UNTIL ( buf )
  A> OVER - 1- ( buf len ) SWAP C! ;
( ----- 022 )
\ Block editor, T P U
\ user-facing lines are 1-based
: T 1- DUP LNSZ * EDPOS! _pln ;
: P IBUF _type IBUF+ 'EDPOS LNSZ MOVE BLK!! ;
: _mvln+ ( ln -- move ln 1 line down )
    DUP 14 > IF DROP EXIT THEN
    _lpos DUP LNSZ + LNSZ MOVE ;
: _U ( U without P, used in VE )
  15 EDPOS LNSZ / - ?DUP IF
    >R 14 BEGIN DUP _mvln+ 1- NEXT DROP THEN ;
: U _U P ;
( ----- 023 )
\ Block editor, F i
: _F ( F without _type and _pln. used in VE )
  'EDPOS 1+ BEGIN ( a )
    FBUF+ C@ OVER BLK) OVER - ( a c a u ) [C]?
    DUP 0< IF 2DROP EXIT THEN ( a idx ) + ( a )
    DUP FBUF+ FLEN []= IF BLK( - EDPOS! EXIT THEN 1+ AGAIN ;
: F FBUF _type _F EDPOS LNSZ / _pln ;
: _rbufsz ( size of linebuf to the right of curpos )
  EDPOS LNSZ MOD LNSZ -^ ;
: _I ( I without _pln and _type. used in VE )
  _rbufsz ILEN OVER < IF ( rsize )
    ILEN - ( chars-to-move )
    'EDPOS DUP ILEN + ROT ( a a+ilen ctm ) MOVE- ILEN
  THEN ( len-to-insert )
  IBUF+ 'EDPOS ROT MOVE ( ilen ) BLK!! ;
: I IBUF _type _I EDPOS LNSZ / _pln ;
( ----- 024 )
\ Block editor, X E Y
: icpy ( n -- copy n chars from cursor to IBUF )
  DUP IBUF C! IBUF+ _zline 'EDPOS IBUF+ ( n a buf ) ROT MOVE ;
: _del ( n -- ) ?DUP NOT IF EXIT THEN _rbufsz MIN
  'EDPOS 2DUP + ( n a1 a1+n ) SWAP _rbufsz MOVE ( n )
  \ get to next line - n
  DUP EDPOS $ffc0 AND $40 + -^ 'pos ( n a )
  SWAP SPC FILL BLK!! ;
: _X ( n -- ) ?DUP NOT IF EXIT THEN _rbufsz MIN DUP icpy _del ;
: X _X EDPOS LNSZ / _pln ;
: _E FLEN _X ;
: E FLEN X ;
: Y FBUF IBUF LNSZ 1+ MOVE ;
( ----- 025 )
\ Visual text editor. VALUEs, lg? width pos@ mode! ...
3 VALUES PREVPOS xoff ACC
LNSZ 3 + VALUE MAXW
10 VALUE MARKCNT
CREATE MARKS MARKCNT << << ALLOT0 \ 4b: blk/edpos
: nspcs ( pos n ) SPC FILLC ;
: lg? COLS MAXW > ; : col- MAXW COLS MIN -^ ;
: width lg? IF LNSZ ELSE COLS THEN ;
: acc@ ACC 1 MAX ; : pos@ ( x y -- ) EDPOS LNSZ /MOD ;
: num ( c -- ) \ c is in range 0-9
  '0' - ACC 10 * + [TO] ACC ;
: mode! ( c -- ) 4 col- CELL! ;
( ----- 026 )
\ VE, rfshln contents selblk pos! xoff? setpos
: _ ( ln -- ) \ refresh line ln
  DUP _lpos xoff + SWAP 3 + COLS * lg? IF 3 + THEN
  width CELLS! ;
: rfshln pos@ NIP _ ; \ refresh active line
: contents 16 >R 0 BEGIN DUP _ 1+ NEXT DROP ;
: selblk BLK@ contents ;
: pos! ( newpos -- ) EDPOS [TO] PREVPOS
    DUP 0< IF DROP 0 THEN 1023 MIN EDPOS! ;
: xoff? pos@ DROP ( x )
  xoff ?DUP IF < IF 0 [TO] xoff contents THEN ELSE
    width >= IF LNSZ COLS - [TO] xoff contents THEN THEN ;
: setpos ( -- ) pos@ 3 + ( header ) SWAP ( y x ) xoff -
  lg? IF 3 + ( gutter ) THEN SWAP AT-XY ;
: 'mark ( -- a ) ACC MARKCNT MOD << << MARKS + ;
( ----- 027 )
\ VE, cmv buftype bufprint bufs
: cmv ( n -- , char movement ) acc@ * EDPOS + pos! ;
: buftype ( buf ln -- ) \ type into buf at ln
  3 OVER AT-XY KEY DUP SPC < IF 2DROP DROP EXIT THEN ( b ln c )
  SWAP COLS * 3 + 3 col- nspcs ( buf c )
  IN( SWAP LNTYPE DROP BEGIN ( buf a ) KEY LNTYPE UNTIL
  IN( - ( buf len ) SWAP C!+ IN( SWAP LNSZ MOVE IN$ ;
: _ ( buf sa sl pos )
  DUP >R STYPEC ( buf ) C@+ ( buf sz ) R> 3 + STYPEC ; 
: bufs ( -- ) \ refresh I and F lines
  IBUF S" I: " COLS _ FBUF S" F: " COLS 2 * _ ;
: insl _U EDPOS $3c0 AND DUP pos! 'pos _zline BLK!! contents ;
( ----- 028 )
\ VE cmds
31 VALUE cmdcnt
CREATE cmdl ," G[]IFnNYEXChlkjHLg@!wWb&mtfROoD"
cmdcnt WORDTBL cmds
:W ( G ) ACC selblk ;
:W ( [ ) BLK> acc@ - selblk ; :W ( ] ) BLK> acc@ + selblk ;
: insert 'I' mode! IBUF 1 buftype _I bufs rfshln ;
'W insert ( I )
:W ( F ) 'F' mode! FBUF 2 buftype _F bufs setpos ;
:W ( n ) _F setpos ;
:W ( N ) EDPOS _F EDPOS = IF 0 EDPOS! acc@ >R BEGIN
    BLK> 1+ BLK@ _F EDPOS IF LEAVE THEN NEXT
    contents setpos THEN ;
:W ( Y ) Y bufs ; :W ( E ) _E bufs rfshln ;
:W ( X ) acc@ _X bufs rfshln ;
:W ( C ) FLEN _del rfshln insert ;
( ----- 029 )
\ VE cmds
:W ( h ) -1 cmv ; :W ( l ) 1 cmv ;
:W ( k ) -64 cmv ; :W ( j ) 64 cmv ;
: bol EDPOS $3c0 AND pos! ;
'W bol ( H )
:W ( L ) EDPOS DUP $3f OR 2DUP = IF 2DROP EXIT THEN SWAP BEGIN
    ( res p ) 1+ DUP 'pos C@ WS? NOT IF NIP DUP 1+ SWAP THEN
    DUP $3f AND $3f = UNTIL DROP pos! ;
:W ( g ) ACC 1 MAX 1- 64 * pos! ;
:W ( @ ) BLK> BLK( (blk@) 0 BLKDTY ! contents ;
:W ( ! ) BLK> FLUSH 'BLK> ! ;
( ----- 030 )
\ VE cmds
: C@- DUP 1- SWAP C@ ;
: word>> BEGIN C@+ WS? UNTIL ;
: ws>> BEGIN C@+ WS? NOT UNTIL ;
: word<< BEGIN C@- WS? UNTIL ;
: ws<< BEGIN C@- WS? NOT UNTIL ;
: bpos! BLK( - pos! ;
:W ( w ) 'EDPOS acc@ >R BEGIN word>> ws>> NEXT 1- bpos! ;
:W ( W ) 'EDPOS acc@ >R BEGIN ws>> word>> NEXT 1- bpos! ;
:W ( b ) 'EDPOS acc@ >R BEGIN 1- ws<< word<< NEXT 1+ 1+ bpos! ;
:W ( & ) WIPE contents ;
:W ( m ) BLK> 'mark ! EDPOS 'mark 1+ 1+ ! ;
:W ( t ) 'mark 1+ 1+ @ pos! 'mark @ selblk ;
( ----- 031 )
\ VE cmds
:W ( f ) EDPOS PREVPOS 2DUP = IF 2DROP EXIT THEN
  2DUP > IF DUP pos! SWAP THEN
  ( p1 p2, p1 < p2 ) OVER - LNSZ MIN ( pos len ) DUP FBUF C!
  FBUF+ _zline SWAP 'pos FBUF+ ( len src dst ) ROT MOVE bufs ;
:W ( R ) 'R' mode! BEGIN
  setpos KEY DUP BS? IF -1 EDPOS+! DROP 0 THEN
  DUP SPC >= IF
  DUP EMIT 'EDPOS C! 1 EDPOS+! BLK!! 0 THEN UNTIL ;
'W insl ( O )
:W ( o ) EDPOS $3c0 < IF EDPOS 64 + EDPOS! insl THEN ;
:W ( D ) bol LNSZ icpy acc@ LNSZ * ( delsz ) BLK) 'EDPOS - MIN
  >R 'EDPOS R@ + 'EDPOS ( src dst )
  BLK) OVER - MOVE BLK) R@ - R> SPC FILL BLK!! bufs contents ;
( ----- 032 )
\ VE final: status nums gutter handle VE
: status 0 $20 nspcs 0 0 AT-XY ." BLK" SPC> BLK> . SPC> ACC .
  SPC> pos@ 1+ . ',' EMIT . xoff IF '>' EMIT THEN SPC>
  BLKDTY @ IF '*' EMIT THEN SPC mode! ;
: nums 16 >R BEGIN R@ HERE FMTD R@ 2 + COLS * STYPEC NEXT ;
: gutter lg? IF 19 >R BEGIN
  '|' R@ 1- COLS * MAXW + CELL! NEXT THEN ;
: handle ( c -- f )
  DUP '0' '9' =><= IF num 0 EXIT THEN
  DUP cmdl cmdcnt [C]? 1+ ?DUP IF 1- cmds SWAP WEXEC THEN
  0 [TO] ACC 'q' = ;
: VE BLK> 0< IF 0 BLK@ THEN
  CLRSCR 0 [TO] ACC 0 [TO] PREVPOS
  nums bufs contents gutter
  BEGIN xoff? status setpos KEY handle UNTIL 0 19 AT-XY ;
( ----- 035 )
\ Memory Editor. See doc/me
CREATE CMD '#' C, 0 C,
CREATE BUF '$' C, 4 ALLOT \ always hex
\ POS is relative to ADDR
4 VALUES ADDR POS HALT? ASCII?
16 VALUE AWIDTH
LINES 2 - CONSTANT AHEIGHT
AHEIGHT AWIDTH * CONSTANT PAGE
COLS 33 < [IF] 8 TO AWIDTH [THEN]
: addr ADDR POS + ;
CREATE _ ," 0123456789abcdef"
: hex! ( c pos -- )
  OVER 16 / _ + C@ OVER CELL! ( c pos )
  1+ SWAP $f AND _ + C@ SWAP CELL! ;
: bottom 0 LINES 1- AT-XY ;
( ----- 036 )
\ Memory Editor, line rfshln contents showpos
: line ( ln -- )
  DUP AWIDTH * ADDR + >A 1+ COLS * ( pos )
  ':' OVER CELL! A> <<8 >>8 OVER 1+ hex! 4 + ( pos+4 )
  AWIDTH >> >R A> SWAP BEGIN ( a-old pos )
    AC@+ ( a-old pos c ) OVER hex! ( a-old pos )
    1+ 1+ AC@+ OVER hex! 3 + ( a-old pos+5 ) NEXT
  SWAP >A AWIDTH >R BEGIN ( pos )
    AC@+ DUP SPC - $5e > IF DROP '.' THEN OVER CELL! 1+ NEXT
  DROP ;
: rfshln POS AWIDTH / line ;
: contents LINES 2 - >R BEGIN R@ 1- line NEXT ;
: showpos
  POS AWIDTH /MOD ( r q ) 1+ SWAP ( y r ) ASCII? IF
  AWIDTH >> 5 * + ELSE DUP 1 AND << SWAP >> 5 * + THEN
  4 + ( y x ) SWAP AT-XY ;
( ----- 037 )
\ Memory Editor, addr! pos! status type typep
: addr! $fff0 AND [TO] ADDR contents ;
: pos! DUP 0< IF PAGE + THEN DUP PAGE >= IF PAGE - THEN
  [TO] POS showpos ;
: status 0 COLS nspcs
  0 0 AT-XY ." A: " ADDR .X SPC> ." C: " POS .X SPC> ." S: "
  PSDUMP POS pos! ;
: type ( cnt -- sa sl ) BUF 1+ >A >R BEGIN
  KEY DUP SPC < IF DROP LEAVE ELSE DUP EMIT AC!+ THEN NEXT
  BUF A> BUF - ;
: typep ( cnt -- n? f )
  type ( sa sl ) DUP IF PARSE ELSE NIP THEN ;
( ----- 038 )
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
( ----- 039 )
\ Memory Editor, #q handle ME
: #q 1 [TO] HALT? ;
: handle ( c -- f )
  CMD 1+ C! CMD 2 FIND IF EXECUTE THEN ;
: ME 0 [TO] HALT? clrscr contents 0 pos! BEGIN
    status KEY handle HALT? UNTIL bottom ;
( ----- 040 )
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
( ----- 041 )
( fuse access. read/write one byte at a time )
: aspfl@ ( -- lfuse ) 0 0 0 $50 _cmd ;
: aspfh@ ( -- hfuse ) 0 0 $08 $58 _cmd ;
: aspfe@ ( -- efuse ) 0 0 $00 $58 _cmd ;
: aspfl! ( lfuse -- ) 0 $a0 $ac _cmd ;
: aspfh! ( hfuse -- ) 0 $a8 $ac _cmd ;
: aspfe! ( efuse -- ) 0 $a4 $ac _cmd ;
( ----- 042 )
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
( ----- 043 )
: aspe@ ( addr -- byte, read from EEPROM )
    0 SWAP L|M SWAP ( 0 msb lsb )
    $a0 ( 0 msb lsb $a0 ) _cmd ;
: aspe! ( byte addr --, write to EEPROM )
    L|M SWAP ( b msb lsb )
    $c0 ( b msb lsb $c0 ) _cmd DROP asprdy ;
( ----- 045 )
( Sega ROM signer. See doc/sega.txt )
: segasig ( addr size -- )
  $2000 OVER LSHIFT ( a sz bytesz ) $10 - >R ( a sz )
  SWAP >A 0 BEGIN ( sz csum ) AC@+ + NEXT ( sz csum )
  'T' AC!+ 'M' AC!+ 'R' AC!+ SPC AC!+ 'S' AC!+
  'E' AC!+ 'G' AC!+ 'A' AC!+ 0 AC!+ 0 AC!+
  ( sum's LSB ) DUP AC!+ ( MSB ) >>8 AC!+
  ( sz ) 0 AC!+ 0 AC!+ 0 AC!+ $4a + AC!+ ;
( ----- 200 )
\ Cross compilation program, generic part. See doc/cross
0 VALUE BIN( \ binary start in target's addr
0 VALUE XORG \ binary start address in host's addr
0 VALUE BIGEND? \ is target big-endian?
3 VALUES L1 L2 L3
: PC HERE XORG - BIN( + ;
: PC2A ( pc -- a ) HERE PC - ( org ) + ;
: XSTART ( bin( -- ) [TO] BIN( HERE [TO] XORG ;
: OALLOT ( oa -- ) XORG + HERE - ALLOT0 ;
: |T L|M BIGEND? NOT IF SWAP THEN ;
: T! ( n a -- ) SWAP |T ROT C!+ C! ;
: T, ( n -- ) |T C, C, ;
: T@ C@+ SWAP C@ BIGEND? IF SWAP THEN <<8 OR ;
: XCOMPC 201 205 LOADR ; : FONTC 262 263 LOADR ;
( ----- 201 )
\ Cross compilation program. COS-specific. See doc/cross
: COREL 210 224 LOADR ; : COREH 225 229 LOADR ;
: BLKSUB 230 234 LOADR ; : GRIDSUB 240 241 LOADR ;
: PS2SUB 246 248 LOADR ; : RXTXSUB 235 LOAD ;
: MSPANSUB 237 LOAD ; : SDCSUB 250 258 LOADR ;
'? HERESTART NOT [IF] 0 VALUE HERESTART [THEN]
0 VALUE XCURRENT \ CURRENT in target system, in target's addr
8 VALUES lblnext lblcell lbldoes lblxt lblval
  lblhere lblmain lblboot
'? 'A NOT [IF] SYSVARS $06 + VALUE 'A [THEN]
'? 'N NOT [IF] SYSVARS $18 + VALUE 'N [THEN]
6 VALUES (n)* (b)* (br)* (?br)* EXIT* (next)*
CREATE '~ 2 ALLOT
( ----- 202 )
\ Cross compilation program
: _xoff ( a -- a ) XORG BIN( - ;
: _wl ( w -- len ) 1- C@ $7f AND ;
: _ws ( w len -- sa ) - 3 - ;
: _xfind ( sa sl -- w? f ) >R >A XCURRENT BEGIN ( w R:sl )
  _xoff + DUP _wl R@ = IF ( w ) DUP R@ _ws A> R@ ( w a1 a2 u )
  []= IF ( w ) R~ 1 EXIT THEN THEN
  3 - ( prev field ) T@ ?DUP NOT UNTIL R~ 0 ( not found ) ;
: XFIND ( sa sl -- w ) _xfind NOT IF (wnf) THEN _xoff - ;
: X' WORD XFIND ;
: '? WORD _xfind DUP IF NIP THEN ;
: _ ( lbl str -- )
  CURWORD S= IF XCURRENT SWAP VAL! ELSE DROP THEN ;
: ENTRY
  WORD TUCK MOVE, XCURRENT T, C, HERE _xoff - [TO] XCURRENT ;
( ----- 203 )
\ Cross compilation program
: ;CODE lblnext JMPi, ;
: ALIAS X' ENTRY JMPi, ; : *ALIAS ENTRY JMP(i), ;
: CONSTANT ENTRY i>, ;CODE ;
: CONSTS >R BEGIN RUN1 CONSTANT NEXT ;
: *VALUE ENTRY (i)>, ;CODE ; : CREATE ENTRY lblcell CALLi, ;
: CODE ENTRY ['] EXIT* S" EXIT" _ ['] (b)* S" (b)" _
  ['] (n)* S" (n)" _ ['] (br)* S" (br)" _
  ['] (?br)* S" (?br)" _ ['] (next)* S" (next)" _ ;
: LITN DUP $ff > IF (n)* T, T, ELSE (b)* T, C, THEN ;
( ----- 204 )
\ Cross compilation program
: imm? ( w -- f ) 1- C@ $80 AND ;
: compile BEGIN WORD S" ;" S= IF EXIT* T, EXIT THEN
  CURWORD PARSE IF LITN ELSE CURWORD _xfind IF ( w )
    DUP imm? IF ABORT" immed!" THEN _xoff - T,
  ELSE CURWORD FIND IF ( w )
    DUP imm? IF EXECUTE ELSE (wnf) THEN
    ELSE (wnf) THEN
  THEN ( _xfind ) THEN ( PARSE ) AGAIN ;
: :~ HERE _xoff - '~ ! lblxt CALLi, compile ;
: ~ '~ @ T, ; IMMEDIATE
: _ CODE lblxt CALLi, compile ; \ : can't have its name now
: ?: '? IF S" ;" WAITW ELSE CURWORD WORD! _ THEN ;
: ~DOER ENTRY lbldoes CALLi, [COMPILE] ~ ;
( ----- 205 )
\ Cross compilation program
: XWRAP COREH XCURRENT lblhere PC2A T!
  HERESTART ?DUP NOT IF PC THEN lblhere PC2A 1+ 1+ T! ;
: ['] WORD XFIND LITN ; IMMEDIATE
: COMPILE [COMPILE] ['] S" ," XFIND T, ; IMMEDIATE
: IF (?br)* T, HERE 1 ALLOT ; IMMEDIATE
: ELSE (br)* T, 1 ALLOT [COMPILE] THEN HERE 1- ; IMMEDIATE
: AGAIN (br)* T, HERE - C, ; IMMEDIATE
: UNTIL (?br)* T, HERE - C, ; IMMEDIATE
: NEXT (next)* T, HERE - C, ; IMMEDIATE
: S" (br)* T, HERE 1 ALLOT HERE ," TUCK HERE -^ SWAP
  [COMPILE] THEN SWAP _xoff - LITN LITN ; IMMEDIATE
: [COMPILE] WORD XFIND T, ; IMMEDIATE
: IMMEDIATE XCURRENT _xoff + 1- DUP C@ $80 OR SWAP C! ;
':' ' _ 4 - C! \ give : its real name now
0 XSTART
( ----- 210 )
\ Core Forth words. See doc/cross
SYSVARS $02 + DUP CONSTANT 'CURRENT *VALUE CURRENT
SYSVARS $04 + DUP CONSTANT 'HERE *VALUE HERE
SYSVARS CONSTANT IOERR
$40 CONSTANT LNSZ
CODE NOOP ;CODE
?: = - NOT ;
?: > SWAP < ;
?: 0< $7fff > ; ?: 0>= $8000 < ; ?: >= < NOT ; ?: <= > NOT ;
?: -^ SWAP - ;
?: 1+ 1 + ; ?: 1- 1 - ;
( ----- 211 )
\ Core words, 2DROP 2DUP NIP TUCK ROT> =><= / MOD 
?: 2DROP DROP DROP ;
?: 2DUP OVER OVER ;
?: NIP SWAP DROP ;
?: TUCK SWAP OVER ;
?: ROT> ROT ROT ;
?: =><= ( n l h -- f ) OVER - ROT> ( h n l ) - >= ;
: / /MOD NIP ; : MOD /MOD DROP ;
( ----- 212 )
\ Core words, << >> <<8 >>8 L|M RSHIFT LEAVE +! VAL! A> >A ...
?: << 2 * ; ?: >> 2 / ;
?: RSHIFT ?DUP IF >R BEGIN >> NEXT THEN ;
?: LSHIFT ?DUP IF >R BEGIN << NEXT THEN ;
?: <<8 8 LSHIFT ; ?: >>8 8 RSHIFT ;
?: L|M DUP <<8 >>8 SWAP >>8 ;
?: +! ( n a -- ) TUCK @ + SWAP ! ;
?: A> [ 'A LITN ] @ ; ?: >A [ 'A LITN ] ! ;
?: A>R R> A> >R >R ; ?: R>A R> R> >A >R ;
?: A+ 1 [ 'A LITN ] +! ; ?: A- -1 [ 'A LITN ] +! ;
?: AC@ A> C@ ; ?: AC! A> C! ;
: AC@+ AC@ A+ ; : AC!+ AC! A+ ;
: LEAVE R> R~ 1 >R >R ;
: VAL! 3 + ! ;
( ----- 213 )
\ Core words, C@+ ALLOT FILL IMMEDIATE , L, M, MOVE MOVE, ..
?: C@+ DUP 1+ SWAP C@ ;
?: C!+ TUCK C! 1+ ;
: ALLOT 'HERE +! ;
?: FILL ( a u b -- ) \ *A*
  ROT> >R >A BEGIN DUP AC!+ NEXT DROP ;
: ALLOT0 ( u -- ) HERE OVER 0 FILL ALLOT ;
: IMMEDIATE CURRENT 1- DUP C@ $80 OR SWAP C! ;
: , HERE ! 2 ALLOT ; : C, HERE C! 1 ALLOT ;
: L, DUP C, >>8 C, ; : M, DUP >>8 C, C, ;
?: MOVE ( src dst u -- ) ?DUP IF
  >R >A BEGIN ( src ) C@+ AC!+ NEXT DROP THEN ;
: MOVE, ( a u -- ) HERE OVER ALLOT SWAP MOVE ;
( ----- 214 )
\ Core words, [C]? CRC16 []= JMPi!
?: JMPi! [ X' NOOP PC2A C@ ( jmp op ) LITN ] SWAP C!+ ! 3 ;
?: CALLi! [ X' MOVE, PC2A C@ ( call op ) LITN ] SWAP C!+ ! 3 ;
?: [C]? ( c a u -- i ) \ Guards A
  ?DUP NOT IF 2DROP -1 EXIT THEN A>R OVER >R >R >A ( c )
  BEGIN DUP AC@+ = IF LEAVE THEN NEXT ( c )
  A- AC@ = IF A> R> - ( i ) ELSE R~ -1 THEN R>A ;
?: []= ( a1 a2 u -- f ) \ Guards A
  ?DUP NOT IF 2DROP 1 EXIT THEN A>R >R >A ( a1 )
  BEGIN AC@+ OVER C@ = NOT IF R~ R>A DROP 0 EXIT THEN 1+ NEXT
  DROP R>A 1 ;
?: CRC16 ( c n -- c )
  <<8 XOR 8 >R BEGIN ( c )
    DUP 0< IF << $1021 XOR ELSE << THEN NEXT ;
( ----- 215 )
\ Core words, we begin EMITting
SYSVARS $0e + DUP CONSTANT 'EMIT *ALIAS EMIT
: STYPE >R >A BEGIN AC@+ EMIT NEXT ;
5 CONSTS $04 EOT $08 BS $0a LF $0d CR $20 SPC
SYSVARS $0a + CONSTANT NL
: SPC> SPC EMIT ;
: NL> NL @ L|M ?DUP IF EMIT THEN EMIT ;
: STACK? SCNT 0< IF S" stack underflow" STYPE ABORT THEN ;
( ----- 216 )
\ Core words, number formatting
: FMTD ( n a -- sa sl ) \ *A*
  6 + >A A>R DUP >R DUP 0< IF 0 -^ THEN BEGIN ( n )
    10 /MOD ( d q ) A- SWAP '0' + AC! ?DUP NOT UNTIL
  R> 0< IF A- '-' AC! THEN R> A> TUCK - ;
PC TO L1 ," 0123456789abcdef"
:~ ( n a 'len -- sa sl ) \ *A*
  C@ DUP >R DUP >R + >A BEGIN ( n ) 16 /MOD ( d q ) A- SWAP
    [ L1 LITN ] + C@ AC! NEXT DROP A> R> ;
~DOER FMTx 2 C, ~DOER FMTX 4 C,
:~ ( n 'w -- sa sl ) @ A>R HERE SWAP EXECUTE STYPE R>A ;
~DOER . X' FMTD T,
~DOER .x X' FMTx T,
~DOER .X X' FMTX T,
( ----- 217 )
\ Core words, literal parsing
:~ ( sl -- n? f ) \ parse unsigned decimal
  >R 0 BEGIN ( r )
    10 * AC@+ ( r c ) '0' - DUP 9 > IF
      2DROP R~ 0 EXIT THEN + NEXT ( r ) 1 ;
: PARSE ( sa sl -- n? f ) \ *A*
  OVER C@ ''' = IF ( sa sl )
    3 = IF 1+ DUP 1+ C@ ''' = IF C@ 1 EXIT THEN THEN
    DROP 0 EXIT THEN ( sa sl )
  OVER C@ '$' = IF ( sa sl ) 1- >R 1+ >A 0 BEGIN ( r )
    16 * AC@+ ( r c ) $20 OR [ L1 LITN ] ( B216 ) $10 [C]?
    DUP 0< IF 2DROP R~ 0 EXIT THEN + NEXT ( r ) 1 EXIT THEN
  SWAP >A DUP 1 > AC@ '-' = AND IF ( sl )
    A+ 1- ~ IF 0 -^ 1 ELSE 0 THEN ELSE ~ THEN ;
( ----- 218 )
\ Core words, input buffer
SYSVARS $10 + DUP CONSTANT 'KEY? *ALIAS KEY?
: KEY BEGIN KEY? UNTIL ;
SYSVARS $20 + CONSTANT INBUF
SYSVARS $1c + DUP CONSTANT 'IN( *VALUE IN(
SYSVARS $1e + DUP CONSTANT 'IN> *VALUE IN>
SYSVARS $08 + CONSTANT LN<
: IN) IN( LNSZ + ;
PC BS C, $7f ( DEL ) C,
: BS? [ ( PC ) LITN ] 2 [C]? 0>= ;
( ----- 219 )
\ Core words, input buffer
\ type c into ptr inside INBUF. f=true if typing should stop
: LNTYPE ( ptr c -- ptr+-1 f )
  DUP BS? IF ( ptr c )
    DROP DUP IN( > IF 1- BS EMIT THEN SPC> BS EMIT 0
  ELSE ( ptr c ) \ non-BS
    DUP SPC < IF DROP DUP IN) OVER - 0 FILL 1 ELSE
      TUCK EMIT C!+ DUP IN) = THEN THEN ;
: RDLN ( -- ) \ Read 1 line in IN(
  S"  ok" STYPE NL> IN( BEGIN KEY LNTYPE UNTIL DROP NL> ;
: IN<? ( -- c-or-0 )
  IN> IN) < IF IN> C@+ SWAP 'IN> ! ELSE 0 THEN ;
: IN< ( -- c ) IN<? ?DUP NOT IF
    LN< @ EXECUTE IN( 'IN> ! SPC THEN ;
: IN$ ['] RDLN LN< ! INBUF 'IN( ! IN) 'IN> ! ;
( ----- 220 )
\ Core words, WORD parsing
: ," BEGIN IN< DUP '"' = IF DROP EXIT THEN C, AGAIN ;
: WS? SPC <= ;
: TOWORD ( -- ) BEGIN IN< WS? NOT UNTIL ;
SYSVARS $12 + CONSTANT 'CURWORD
: CURWORD ( -- sa sl ) 'CURWORD 1+ @ 'CURWORD C@ ;
:~ ( f sa sl -- ) 'CURWORD C!+ TUCK ! 1+ 1+ C! ;
: WORD ( -- sa sl )
  'CURWORD 3 + C@ IF CURWORD ELSE
    TOWORD IN> 1- 0 ( sa sl ) BEGIN 1+ IN<? WS? UNTIL THEN
  ( sa sl ) 2DUP 0 ROT> ~ ;
: WORD! 1 ROT> ~ ;
( ----- 221 )
\ Core words, FIND and INTERPRET loop
?: FIND ( sa sl -- w? f ) \ Guards A
  A>R >R >A CURRENT BEGIN ( w R:sl )
  DUP 1- C@ $7f AND ( wlen ) R@ = IF ( w )
    DUP R@ - 3 - A> R@ ( w a1 a2 u )
    []= IF ( w ) R~ 1 R>A EXIT THEN THEN
  3 - ( prev field ) @ ?DUP NOT UNTIL R~ 0 R>A ( not found ) ;
: (wnf) CURWORD STYPE S"  word not found" STYPE ABORT ;
: RUN1 \ read next word in stream and interpret it
  WORD PARSE NOT IF
    CURWORD FIND IF EXECUTE STACK? ELSE (wnf) THEN THEN ;
: INTERPRET BEGIN RUN1 AGAIN ;
: nC, ( n -- ) >R BEGIN RUN1 C, NEXT ;
( ----- 222 )
\ Core words, CODE '? ' TO FORGET
: CODE WORD TUCK MOVE, ( len )
  CURRENT , C, \ write prev value and size
  HERE 'CURRENT ! ;
: '? WORD FIND DUP IF NIP THEN ;
: ' WORD FIND NOT IF (wnf) THEN ;
: TO ' VAL! ;
: FORGET
  ' DUP ( w w )
  \ HERE must be at the end of prev's word, that is, at the
  \ beginning of w.
  DUP 1- C@ ( len ) $7f AND ( rm IMMEDIATE )
  3 + ( fixed header len ) - 'HERE ! ( w )
  ( get prev addr ) 3 - @ 'CURRENT ! ;
( ----- 223 )
\ Core words, S= WAITW [IF] _bchk
: S= ( sa1 sl1 sa2 sl2 -- f )
  ROT OVER = IF ( same len, s2 s1 l ) []=
  ELSE DROP 2DROP 0 THEN ;
: WAITW ( sa sl -- ) BEGIN 2DUP WORD S= UNTIL 2DROP ;
: [IF] NOT IF S" [THEN]" WAITW THEN ;
ALIAS NOOP [THEN]
: _bchk DUP $80 + $ff > IF S" br ovfl" STYPE ABORT THEN ;
( ----- 224 )
\ Core words, DUMP .S
: DUMP ( n a -- ) \ *A*
  >A 8 /MOD SWAP IF 1+ THEN >R BEGIN
    ':' EMIT A> DUP .x SPC> ( a )
    4 >R BEGIN AC@+ .x AC@+ .x SPC> NEXT ( a ) >A
    8 >R BEGIN AC@+ DUP SPC - $5e > IF DROP '.' THEN EMIT NEXT
  NL> NEXT ;
: PSDUMP SCNT NOT IF EXIT THEN
  SCNT >A BEGIN DUP .X SPC> >R SCNT NOT UNTIL
  BEGIN R> SCNT A> = UNTIL ;
: .S ( -- )
  S" SP " STYPE SCNT .x SPC> S" RS " STYPE RCNT .x SPC>
  S" -- " STYPE STACK? PSDUMP ;
( ----- 225 )
\ Core high, LITN CREATE DOER DOES> CODE ALIAS VALUE
: LITN DUP >>8 IF COMPILE (n) , ELSE COMPILE (b) C, THEN ;
: ;CODE [ lblnext LITN ] HERE JMPi! ALLOT ;
: CREATE CODE [ lblcell LITN ] HERE CALLi! ALLOT ;
: VARIABLE CREATE 2 ALLOT ;
: DOER CODE [ lbldoes LITN ] HERE CALLi! 1+ 1+ ALLOT ;
: _ R> CURRENT 3 + ! ; \ Popping RS makes us EXIT from parent
: DOES> COMPILE _ [ lblxt LITN ] HERE CALLi! ALLOT ; IMMEDIATE
: ALIAS ' CODE HERE JMPi! ALLOT ;
: VALUE CODE [ lblval LITN ] HERE CALLi! ALLOT , ;
: VALUES >R BEGIN 0 VALUE NEXT ;
: CONSTS >R BEGIN RUN1 VALUE NEXT ;
( ----- 226 )
\ Core high, BOOT
:~ IN$ INTERPRET BYE ;
'~ @ lblmain PC2A T! \ set jump in QUIT
PC TO lblhere 4 ALLOT \ CURRENT, HERESTART
: BOOT [ lblhere LITN ] 'CURRENT 4 MOVE
  ['] (emit) 'EMIT ! ['] (key?) 'KEY? !
  0 'CURWORD 3 + C!
  0 IOERR ! $0d0a ( CR/LF ) NL !
  INIT S" Collapse OS" STYPE ABORT ;
XCURRENT lblboot PC2A T! \ initial jump to BOOT
( ----- 227 )
\ Core high, :
: XTCOMP [ lblxt LITN ] HERE CALLi! ALLOT BEGIN
    WORD S" ;" S= IF COMPILE EXIT EXIT THEN
    CURWORD PARSE IF LITN ELSE CURWORD FIND IF
      DUP 1- C@ $80 AND ( imm? ) IF EXECUTE ELSE , THEN
    ELSE (wnf) THEN THEN
  AGAIN ;
: : CODE XTCOMP ;
( ----- 228 )
\ Core high, IF..ELSE..THEN ( \
: IF ( -- a | a: br cell addr )
  COMPILE (?br) HERE 1 ALLOT ( br cell allot ) ; IMMEDIATE
: THEN ( a -- | a: br cell addr )
  DUP HERE -^ _bchk SWAP ( a-H a ) C! ; IMMEDIATE
: ELSE ( a1 -- a2 | a1: IF cell a2: ELSE cell )
  COMPILE (br) 1 ALLOT [COMPILE] THEN
  HERE 1- ( push a. 1- for allot offset ) ; IMMEDIATE
: ( S" )" WAITW ; IMMEDIATE
: \ IN) 'IN> ! ; IMMEDIATE
: S"
  COMPILE (br) HERE 1 ALLOT HERE ," TUCK HERE -^ SWAP
  [COMPILE] THEN SWAP LITN LITN ; IMMEDIATE
( ----- 229 )
\ Core high, .", ABORT", BEGIN..AGAIN..UNTIL, many others.
: ." [COMPILE] S" COMPILE STYPE ; IMMEDIATE
: ABORT" [COMPILE] ." COMPILE ABORT ; IMMEDIATE
: BEGIN HERE ; IMMEDIATE
: AGAIN COMPILE (br) HERE - _bchk C, ; IMMEDIATE
: UNTIL COMPILE (?br) HERE - _bchk C, ; IMMEDIATE
: NEXT COMPILE (next) HERE - _bchk C, ; IMMEDIATE
: [TO] ' LITN COMPILE VAL! ; IMMEDIATE
: [ INTERPRET ; IMMEDIATE
: ] R~ R~ ; \ INTERPRET+RUN1
: COMPILE ' LITN ['] , , ; IMMEDIATE
: [COMPILE] ' , ; IMMEDIATE
: ['] ' LITN ; IMMEDIATE
( ----- 230 )
\ BLK subsystem. See doc/blk
BLK_MEM CONSTANT BLK( \ $400 + "\S "
BLK_MEM $400 + CONSTANT BLK)
\ Current blk pointer -1 means "invalid"
BLK_MEM $403 + DUP CONSTANT 'BLK> *VALUE BLK>
\ Whether buffer is dirty
BLK_MEM $405 + CONSTANT BLKDTY
BLK_MEM $407 + CONSTANT BLKIN>
: BLK$ 0 BLKDTY ! -1 'BLK> ! S" \S " BLK) SWAP MOVE ;
( ----- 231 )
: BLK! ( -- ) BLK> BLK( (blk!) 0 BLKDTY ! ;
: FLUSH BLKDTY @ IF BLK! THEN -1 'BLK> ! ;
: BLK@ ( n -- )
  DUP BLK> = IF DROP EXIT THEN
  FLUSH DUP 'BLK> ! BLK( (blk@) ;
: BLK!! 1 BLKDTY ! ;
: WIPE BLK( 1024 SPC FILL BLK!! ;
: COPY ( src dst -- ) FLUSH SWAP BLK@ 'BLK> ! BLK! ;
( ----- 232 )
: LNLEN ( a -- len ) \ len based on last visible char in line
  1- LNSZ >R BEGIN
    DUP R@ + C@ SPC > IF DROP R> EXIT THEN NEXT DROP 0 ;
: EMITLN ( a -- ) \ emit LNSZ chars from a or stop at CR
  DUP LNLEN ?DUP IF
    >R >A BEGIN AC@+ EMIT NEXT ELSE DROP THEN NL> ;
: LIST ( n -- ) \ print contents of BLK n
  BLK@ 16 >R 0 BEGIN ( n )
    DUP 1+ DUP 10 < IF SPC> THEN . SPC>
    DUP LNSZ * BLK( + EMITLN 1+ NEXT DROP ;
: INDEX ( b1 b2 -- ) \ print first line of blocks b1 through b2
  OVER - 1+ >R BEGIN
    DUP . SPC> DUP BLK@ BLK( EMITLN 1+ NEXT DROP ;
( ----- 233 )
: \S BLK) 'IN( ! IN( 'IN> ! ;
:~ ( -- ) IN) 'IN( ! ;
: LOAD
  IN> BLKIN> ! [ '~ @ LITN ] LN< ! BLK@ BLK( 'IN( ! IN( 'IN> !
  BEGIN RUN1 IN( BLK) = UNTIL IN$ BLKIN> @ 'IN> ! ;
\ >R R> around LOAD is to avoid bad blocks messing PS up
: LOADR OVER - 1+ >R BEGIN
  DUP . SPC> DUP >R LOAD R> 1+ NEXT DROP ;
( ----- 234 )
\ Application loader, to include in boot binary
: ED 1 LOAD ( MOVE- ) 20 24 LOADR ;
: VE 5 LOAD ( wordtbl ) ED 25 32 LOADR ;
: ME 35 39 LOADR ;
: ARCHM 301 LOAD ;
: RXTX 10 15 LOADR ;
: XCOMP 200 LOAD ;
( ----- 235 )
\ RX/TX subsystem. See doc/rxtx
RXTX_MEM CONSTANT _emit
RXTX_MEM 2 + CONSTANT _key
: RX< BEGIN RX<? UNTIL ;
: RX<< 0 BEGIN DROP RX<? NOT UNTIL ;
: TX[ 'EMIT @ _emit ! ['] TX> 'EMIT ! ;
: ]TX _emit @ 'EMIT ! ;
: RX[ 'KEY? @ _key ! ['] RX<? 'KEY? ! ;
: ]RX _key @ 'KEY? ! ;
( ----- 237 )
\ Media Spanning subsystem. see doc/mspan
MSPAN_MEM CONSTANT MSPAN_DISK

:~ ( dsk -- ) DUP MSPAN_DISK C! S" Need disk " STYPE . SPC> ;
'? DRVSWAP NOT [IF] : prompt ~ KEY DROP ; [THEN]
'? DRVSWAP [IF] : prompt ( dsk -- )
  ~ KEY $20 OR 's' = IF DRVSWAP THEN ; [THEN]
: MSPAN$ 0 MSPAN_DISK C! ;
: dskchk ( blk -- newblk )
  [ MSPAN_SZ LITN ] /MOD ( blk dsk ) DUP MSPAN_DISK C@ = NOT IF
    prompt ELSE DROP THEN ( blk ) ;
:~ ( blk dest 'w -- ) SWAP dskchk SWAP @ EXECUTE ;
~DOER (blk@) X' (ms@) T,
~DOER (blk!) X' (ms!) T,
( ----- 240 )
\ Grid subsystem. See doc/grid.
GRID_MEM DUP CONSTANT 'XYPOS *VALUE XYPOS
?: CURSOR! 2DROP ;
: XYPOS! COLS LINES * MOD DUP XYPOS CURSOR! 'XYPOS ! ;
: AT-XY ( x y -- ) COLS * + XYPOS! ;
?: NEWLN ( oldln -- newln )
  1+ LINES MOD DUP COLS * ( pos )
  COLS >R BEGIN SPC OVER CELL! 1+ NEXT DROP ;
?: CELLS! ( a pos u -- )
  ?DUP IF >R SWAP >A BEGIN ( pos ) AC@+ OVER CELL! 1+ NEXT
    ELSE DROP THEN DROP ;
: STYPEC ( sa sl pos -- ) SWAP CELLS! ;
?: FILLC ( pos n c )
  SWAP >R SWAP BEGIN ( b pos ) 2DUP CELL! 1+ NEXT 2DROP ;
: CLRSCR 0 COLS LINES * SPC FILLC 0 XYPOS! ;
( ----- 241 )
:~ ( line feed ) XYPOS COLS / NEWLN COLS * XYPOS! ;
?: (emit)
    DUP BS? IF
      DROP SPC XYPOS TUCK CELL! ( pos ) 1- XYPOS! EXIT THEN
    DUP CR = IF DROP SPC XYPOS CELL! ~ EXIT THEN
    DUP SPC < IF DROP EXIT THEN
    XYPOS CELL!
    XYPOS 1+ DUP COLS MOD IF XYPOS! ELSE DROP ~ THEN ;
: GRID$ 0 'XYPOS ! ;
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
SDC_MEM CONSTANT SDC_SDHC
: _idle ( -- n ) $ff (spix) ;

( spix $ff until the response is something else than $ff
  for a maximum of 20 times. Returns $ff if no response. )
: _wait ( -- n )
  0 ( dummy ) 20 >R BEGIN
    DROP _idle DUP $ff = NOT IF LEAVE THEN NEXT ;

( adjust block for LBA for SD/SDHC )
: _badj ( arg1 arg2 -- arg1 arg2 )
  SDC_SDHC @ IF 0 SWAP ELSE DUP 128 / SWAP <<8 << THEN ;
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
  XOR 8 >R BEGIN ( c )
    << ( c<<1 ) DUP >>8 IF
      ( MSB was set, apply polynomial )
      <<8 >>8 $12 XOR ( $09 << 1, we apply CRC on high bits )
    THEN NEXT ;
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
  AND SDC_SDHC ! DROP ;
( ----- 255 )
: _err 0 (spie) S" SDerr" STYPE ABORT ;

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
  10 >R BEGIN _idle DROP NEXT
  0 ( dummy ) 10 >R BEGIN  ( r )
    DROP $40 0 0 SDCMDR1  ( CMD0 )
    1 = DUP IF LEAVE THEN
  NEXT NOT IF _err THEN
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
:~ ( dstaddr blkno -- )
  [ SDC_DEVID LITN ] (spie)
  $51 ( CMD17 ) SWAP _badj ( a cmd arg1 arg2 ) _cmd IF _err THEN
  _wait $fe = NOT IF _err THEN
  >A 512 >R 0 BEGIN ( crc1 )
    _idle ( crc1 b ) DUP AC!+ ( crc1 b ) CRC16 NEXT ( crc1 )
    _idle <<8 _idle + ( crc1 crc2 )
    _wait DROP 0 (spie) = NOT IF _err THEN ;
: SDC@ ( blkno blk( -- )
  SWAP << ( 2x ) 2DUP ( a b a b ) ~
  ( a b ) 1+ SWAP 512 + SWAP ~ ;
( ----- 258 )
:~ ( srcaddr blkno -- )
  [ SDC_DEVID LITN ] (spie)
  $58 ( CMD24 ) SWAP _badj ( a cmd arg1 arg2 ) _cmd IF _err THEN
  _idle DROP $fe (spix) DROP
  >A 512 >R 0 BEGIN ( crc )
    AC@+ ( crc b ) DUP (spix) DROP CRC16 NEXT ( crc )
    DUP >>8 ( crc msb ) (spix) DROP (spix) DROP
    _wait $1f AND 5 = NOT IF _err THEN _ready 0 (spie) ;
: SDC! ( blkno blk( -- )
  SWAP << ( 2x ) 2DUP ( a b a b ) ~
  ( a b ) 1+ SWAP 512 + SWAP ~ ;
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
2 VALUES _w _h
: _g ( given a top-left of dot-X in BLK(, spit H bin lines )
  DUP >A _h >R BEGIN _w >R 0 BEGIN ( a r )
      << AC@+ 'X' = IF 1+ THEN NEXT
    8 _w - LSHIFT C, 64 + DUP >A NEXT DROP ;
: _l ( a u -- a,  spit a line of u glyphs )
  >R DUP BEGIN ( a ) DUP _g _w + NEXT DROP ;
( ----- 263 )
: CPFNT3x5 3 [TO] _w 5 [TO] _h
  _h ALLOT0 ( space char )
  265 BLK@ BLK( 21 _l 320 + 21 _l 320 + 21 _l DROP ( 63 )
  266 BLK@ BLK( 21 _l 320 + 10 _l DROP ( 94! ) ;
: CPFNT5x7 5 [TO] _w 7 [TO] _h
  _h ALLOT0 ( space char )
  3 >R 267 BEGIN ( b )
    DUP BLK@ BLK( 12 _l 448 + 12 _l DROP 1+ NEXT ( 72 )
  ( 270 ) BLK@ BLK( 12 _l 448 + 10 _l DROP ( 94! ) ;
: CPFNT7x7 7 [TO] _w 7 [TO] _h
  _h ALLOT0 ( space char )
  5 >R 271 BEGIN ( b )
    DUP BLK@ BLK( 9 _l 448 + 9 _l DROP 1+ NEXT ( 90 )
  ( 276 ) BLK@ BLK( 4 _l DROP ( 94! ) ;
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
( ----- 290 )
\ Automated tests. "1 LOAD 290 296 LOADR" to run.
\ "#" means "assert". We ABORT on failure.
: fail SPC> ABORT" failed" ;
: # IF SPC> ." pass" NL> ELSE fail THEN ;
: #eq 2DUP SWAP . SPC> '=' EMIT SPC> . '?' EMIT = # ;
( ----- 291 )
\ Arithmetics
48 13 + 61 #eq
48 13 - 35 #eq
48 13 * 624 #eq
48 13 / 3 #eq
48 13 MOD 9 #eq
5 3 LSHIFT 40 #eq
155 5 RSHIFT 4 #eq
( ----- 292 )
\ Comparisons
$22 $8065 < #
-1 0 > #
-1 0< #
( ----- 293 )
\ Memory
42 C, 43 C, 44 C,
HERE 3 - HERE 3 MOVE
HERE C@ 42 #eq HERE 1+ C@ 43 #eq HERE 2 + C@ 44 #eq
HERE HERE 1+ 3 MOVE ( demonstrate MOVE's problem )
HERE 1+ C@ 42 #eq HERE 2 + C@ 42 #eq HERE 3 + C@ 42 #eq
HERE 3 - HERE 3 MOVE
HERE HERE 1+ 3 MOVE- ( see? better )
HERE 1+ C@ 42 #eq HERE 2 + C@ 43 #eq HERE 3 + C@ 44 #eq

HERE ( ref )
HERE 3 - 3 MOVE,
( ref ) HERE 3 - #eq
HERE 3 - C@ 42 #eq HERE 2 - C@ 43 #eq HERE 1- C@ 44 #eq
( ----- 294 )
\ Parse
'b' $62 #eq
( ----- 295 )
\ Stack
42 43 44 ROT
42 #eq 44 #eq 43 #eq
42 43 44 ROT>
43 #eq 42 #eq 44 #eq
( ----- 296 )
\ CRC
$0000 $00 CRC16 $0000 #eq
$0000 $01 CRC16 $1021 #eq
$5678 $34 CRC16 $34e4 #eq
