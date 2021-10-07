( ----- 000 )
MASTER INDEX

002 Common assembler words    05-99 unused
100 Block editor              115 Memory Editor
120 Useful little words
130-149 unused                150 Remote Shell
160 AVR SPI programmer        165 Sega ROM signer
170-199 unused                200 Cross compilation
210 Core words                230 BLK subsystem
240 Grid subsystem            245 PS/2 keyboard subsystem
250 SD Card subsystem         260 Fonts
300 Arch-specific content
( ----- 002 )
\ Common assembler words, low
3 VALUES ORG BIN( BIGEND?
: PC HERE ORG - BIN( + ;
: <<3 << << << ; : <<4 <<3 << ;
8 VALUES L1 L2 L3 lblnext lblcell lbldoes lblxt lblval
: |T L|M BIGEND? NOT IF SWAP THEN ;
: T! ( n a -- ) SWAP |T ROT C!+ C! ;
: T, ( n -- ) |T C, C, ;
: T@ C@+ SWAP C@ BIGEND? IF SWAP THEN <<8 OR ;
: LSET PC TO ;
( ----- 003 )
\ Common assembler words, high
\ NOTE: this code needs to be cross-compilable
: BEGIN, PC ;
: BR PC - [ JROFF JROPLEN - LITN ] + _bchk ;
: FJR BEGIN, [ JROPLEN LITN ] + 0 ;
: IFZ, Z? ^? FJR ?JRi, ; : IFNZ, Z? FJR ?JRi, ;
: IFC, C? ^? FJR ?JRi, ; : IFNC, C? FJR ?JRi, ;
: FMARK DUP PC ( l l pc ) -^ [ JROFF LITN ] + ( l off )
  \ warning: l is a PC offset, not a mem addr!
  SWAP ORG + BIN( - ( off addr ) C! ;
: THEN, FMARK ; : ELSE, FJR JRi, SWAP FMARK ;
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
4 VALUES PREVPOS PREVBLK xoff ACC
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
  SWAP COLS * 3 + 3 col- nspcs ( buf c )
  IN( SWAP LNTYPE DROP BEGIN ( buf a ) KEY LNTYPE UNTIL
  IN( - ( buf len ) SWAP C!+ IN( SWAP LNSZ MOVE IN$ ;
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
: %@ BLK> BLK( (blk@) 0 BLKDTY ! contents ;
: %! BLK> FLUSH 'BLK> ! ;
( ----- 109 )
\ VE cmds: w W b B
' NOOP VALUE C@+-
: C@- ( a -- a-1 c ) DUP C@ SWAP 1- SWAP ;
: go>> ['] C@+ [TO] C@+- ;
: go<< ['] C@- [TO] C@+- ;
: word>> BEGIN C@+- EXECUTE WS? UNTIL ;
: ws>> BEGIN C@+- EXECUTE WS? NOT UNTIL ;
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
  BLKDTY @ IF '*' EMIT THEN SPC mode! ;
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
4 VALUES ADDR POS HALT? ASCII?
16 VALUE AWIDTH
LINES 2 - CONSTANT AHEIGHT
AHEIGHT AWIDTH * CONSTANT PAGE
COLS 33 < [IF] 8 TO AWIDTH [THEN]
: _ ( n -- c ) DUP 9 > IF [ 'a' 10 - LITN ] ELSE '0' THEN + ;
: _p ( c -- n ) '0' - DUP 9 > IF $df AND 'A' '0' - - DUP 6 < IF
    10 + ELSE DROP $100 THEN THEN ;
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
  AWIDTH >> 5 * + ELSE DUP 1 AND << SWAP >> 5 * + THEN
  4 + ( y x ) SWAP AT-XY ;
( ----- 117 )
\ Memory Editor, addr! pos! status type typep
: addr! $fff0 AND [TO] ADDR contents ;
: pos! DUP 0< IF PAGE + THEN DUP PAGE >= IF PAGE - THEN
  [TO] POS showpos ;
: status 0 COLS nspcs
  0 0 AT-XY ." A: " ADDR .X SPC> ." C: " POS .X SPC> ." S: "
  PSDUMP POS pos! ;
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
\ Useful little words. nC, MIN MAX MOVE-
\ parse the next n words and write them as chars
: nC, ( n -- ) 0 DO RUN1 C, LOOP ;
: MIN ( n n - n ) 2DUP > IF SWAP THEN DROP ;
: MAX ( n n - n ) 2DUP < IF SWAP THEN DROP ;
\ Compute CRC16 over a memory range
: CRC16[] ( a u -- c ) 0 ROT> RANGE DO I C@ CRC16 LOOP ;
: MOVE- ( a1 a2 u -- ) \ MOVE starting from the end
  ?DUP IF 1- TUCK + ( a1 u-1 a2+ ) ROT> TUCK + ( a2+ u-1 a1+ )
    SWAP 1+ ( u ) 0 DO ( a2 a1 )
      DUP C@ ROT> 1- ( c a2 a1- ) ROT> TUCK C! 1- SWAP
  LOOP THEN 2DROP ;
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
: context DOER CURRENT , DOES> ( a -- )
  saveto IF CURRENT [TO] saveto THEN ( a )
  DUP [TO] saveto ( a )
  @ CURRENT ! ;
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
  DUP BLK) = IF DROP BLK( BLK! BLK> 1+ 'BLK> ! THEN
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
: XCOMPH 201 204 LOADR ; : FONTC 262 263 LOADR ;
: COREL 210 224 LOADR ; : COREH 225 229 LOADR ;
: BLKSUB 230 234 LOADR ; : GRIDSUB 240 241 LOADR ;
: PS2SUB 246 248 LOADR ;
'? HERESTART NOT [IF] 0 CONSTANT HERESTART [THEN]
0 VALUE XCURRENT \ CURRENT in target system, in target's addr
7 VALUES (n)* (b)* 2>R* (loop)* (br)* (?br)* EXIT*
( ----- 201 )
: _xoff ORG BIN( - ;
: W= ( sa sl w -- f ) 2DUP 1- C@ $7f AND = IF ( same len )
    ( sa sl w ) OVER - 3 - ( s+1 len w-3-len ) ROT> []=
    ELSE 2DROP DROP 0 THEN ;
: _xfind ( sa sl -- w? f ) PAD C!+ ! XCURRENT BEGIN ( w )
  _xoff + DUP PAD C@+ SWAP @ SWAP ROT W= IF ( w ) 1 EXIT THEN
  3 - ( prev field ) T@ ?DUP NOT UNTIL 0 ( not found ) ;
: XFIND ( sa sl -- w ) _xfind NOT IF (wnf) THEN _xoff - ;
: '? WORD _xfind DUP IF SWAP _xoff - SWAP THEN ;
: X' '? NOT IF (wnf) THEN ;
: _ ( lbl str -- )
  XCURRENT _xoff + W= IF XCURRENT SWAP VAL! ELSE DROP THEN ;
: ENTRY
  WORD TUCK MOVE, XCURRENT T, C, HERE _xoff - [TO] XCURRENT ;
( ----- 202 )
: ;CODE lblnext JMPi, ;
: ALIAS X' ENTRY JMPi, ; : *ALIAS ENTRY (i)>, >JMP, ;
: CONSTANT ENTRY i>, ;CODE ; : CONSTS 0 DO RUN1 CONSTANT LOOP ;
: *VALUE ENTRY (i)>, ;CODE ;
: CREATE ENTRY lblcell CALLi, ;
: CODE ENTRY ['] EXIT* LIT" EXIT" _ ['] (b)* LIT" (b)" _
  ['] (n)* LIT" (n)" _ ['] 2>R* LIT" 2>R" _
  ['] (loop)* LIT" (loop)" _ ['] (br)* LIT" (br)" _
  ['] (?br)* LIT" (?br)" _ ;
: INLINE
  X' >R XCURRENT BEGIN ( R:ref w )
    DUP _xoff + 3 - T@ ( w prev ) DUP I =
    IF DROP 1 ELSE NIP 0 THEN UNTIL ( w )
  _xoff + 1- DUP C@ - 5 - ( 2 for prev, 3 for ;CODE )
  R> _xoff + TUCK - MOVE, ;
( ----- 203 )
: XWRAP
  COREH HERESTART ?DUP NOT IF PC THEN ORG 8 ( LATEST ) + T!
  XCURRENT ORG 6 ( CURRENT ) + T! ;
: LITN DUP $ff > IF (n)* T, T, ELSE (b)* T, C, THEN ;
: imm? ( w -- f ) 1- C@ $80 AND ;
: _ CODE lblxt CALLi, BEGIN \ : can't have its name right now
  WORD LIT" ;" S= IF EXIT* T, EXIT THEN
  CURWORD PARSE IF LITN ELSE CURWORD _xfind IF ( w )
    DUP imm? IF ABORT" immed!" THEN _xoff - T,
  ELSE CURWORD FIND IF ( w )
    DUP imm? IF EXECUTE ELSE (wnf) THEN
    ELSE (wnf) THEN
  THEN ( _xfind ) THEN ( PARSE ) AGAIN ;
( ----- 204 )
: ['] WORD XFIND LITN ; IMMEDIATE
: COMPILE [COMPILE] ['] LIT" ," XFIND T, ; IMMEDIATE
: DO 2>R* T, HERE ; IMMEDIATE
: LOOP (loop)* T, HERE - C, ; IMMEDIATE
: IF (?br)* T, HERE 1 ALLOT ; IMMEDIATE
: ELSE (br)* T, 1 ALLOT [COMPILE] THEN HERE 1- ; IMMEDIATE
: AGAIN (br)* T, HERE - C, ; IMMEDIATE
: UNTIL (?br)* T, HERE - C, ; IMMEDIATE
: LIT" (br)* T, HERE 1 ALLOT HERE ," TUCK HERE -^ SWAP
  [COMPILE] THEN SWAP _xoff - LITN LITN ; IMMEDIATE
: [COMPILE] WORD XFIND T, ; IMMEDIATE
: IMMEDIATE XCURRENT _xoff + 1- DUP C@ $80 OR SWAP C! ;
':' ' _ 4 - C! \ give : its real name now
( ----- 210 )
\ Core Forth words. See doc/cross.txt.
CODE NOOP ;CODE
CODE 2DROP INLINE DROP INLINE DROP ;CODE
CODE EXIT INLINE R> >IP, ;CODE
CODE EXECUTE >JMP,
CODE (b) IP>, INLINE C@ IP+, ;CODE
CODE (n) IP>, INLINE @ IP+, IP+, ;CODE
CODE (c) IP>, INLINE (br) INLINE 1+ >JMP,
( ----- 211 )
\ Core words, CURRENT HERE SYSVARS ?DUP 2>R 2R> NOT = < > ...
SYSVARS $02 + DUP CONSTANT 'CURRENT *VALUE CURRENT
SYSVARS $04 + DUP CONSTANT 'HERE *VALUE HERE
ALIAS HERE PC
SYSVARS $18 + CONSTANT PAD
3 CONSTS 0 ORG BIN( BIN( SYSVARS SYSVARS
SYSVARS CONSTANT IOERR
$40 CONSTANT LNSZ
CODE ?DUP @Z, IFNZ, INLINE DUP THEN, ;CODE
CODE 2>R INLINE SWAP INLINE >R INLINE >R ;CODE
CODE 2R> INLINE R> INLINE R> INLINE SWAP ;CODE
CODE NOT @Z, Z>!, ;CODE
CODE = INLINE - Z>!, ;CODE
CODE > INLINE SWAP INLINE - C>!, ;CODE
CODE < INLINE - C>!, ;CODE
( ----- 212 )
\ Core words, 0< >= <= =><= 2DUP 2DROP NIP TUCK L|M A* ..
: 0< $7fff > ; : >= < NOT ; : <= > NOT ;
: =><= ( n l h -- f ) OVER - ROT> ( h n l ) - >= ;
CODE 2DUP INLINE OVER INLINE OVER ;CODE
CODE NIP INLINE SWAP INLINE DROP ;CODE
CODE TUCK INLINE SWAP INLINE OVER ;CODE
: L|M DUP <<8 >>8 SWAP >>8 ;
: RSHIFT ?DUP IF 0 DO >> LOOP THEN ;
: LSHIFT ?DUP IF 0 DO << LOOP THEN ; : -^ SWAP - ;
: LEAVE R> R> DROP I 1- >R >R ; : UNLOOP R> 2R> 2DROP >R ;
CODE VAL! 3 i>, INLINE + INLINE ! ;CODE
: / /MOD NIP ; : MOD /MOD DROP ;
CODE A@ INLINE A> INLINE @ ;CODE
CODE A! INLINE A> INLINE ! ;CODE
CODE AC@ INLINE A> INLINE C@ ;CODE
CODE AC! INLINE A> INLINE C! ;CODE
( ----- 213 )
\ Core words, C@+ ALLOT FILL IMMEDIATE , L, M, MOVE MOVE, ..
CODE C@+ INLINE DUP INLINE >A INLINE 1+ INLINE AC@ ;CODE
: C!+ TUCK C! 1+ ;
: +! >A A@ + A! ; : ALLOT 'HERE +! ;
: RANGE ( a u -- ah al ) OVER + SWAP ;
: FILL ( a u b -- ) ROT> RANGE DO ( b ) DUP I C! LOOP DROP ;
: ALLOT0 ( u -- ) HERE OVER 0 FILL ALLOT ;
: IMMEDIATE CURRENT 1- DUP C@ 128 OR SWAP C! ;
: , HERE ! 2 ALLOT ; : C, HERE C! 1 ALLOT ;
: L, DUP C, >>8 C, ; : M, DUP >>8 C, C, ;
: MOVE ( src dst u -- )
  ?DUP IF RANGE DO ( src ) C@+ ( src+1 b ) I C! LOOP
  ELSE DROP THEN DROP ;
: MOVE, ( a u -- ) HERE OVER ALLOT SWAP MOVE ;
( ----- 214 )
\ Core words, we begin EMITting
SYSVARS $0e + DUP CONSTANT 'EMIT *ALIAS EMIT
: STYPE RANGE DO I C@ EMIT LOOP ;
5 CONSTS $04 EOT $08 BS $0a LF $0d CR $20 SPC
SYSVARS $0a + CONSTANT NL
: SPC> SPC EMIT ;
: NL> NL @ L|M ?DUP IF EMIT THEN EMIT ;
: STACK? SCNT 0< IF LIT" stack underflow" STYPE ABORT THEN ;
( ----- 215 )
\ Core words, number formatting
: . ( n -- )
  ?DUP NOT IF '0' EMIT EXIT THEN \ 0 is a special case
  DUP 0< IF '-' EMIT -1 * THEN
  $ff SWAP ( stop ) BEGIN 10 /MOD ( d q ) ?DUP NOT UNTIL
  BEGIN '0' + EMIT DUP 9 > UNTIL DROP ;
: _ DUP 9 > IF [ 'a' 10 - LITN ] ELSE '0' THEN + ;
: .x <<8 >>8 16 /MOD ( l h ) _ EMIT _ EMIT ;
: .X L|M .x .x ;
( ----- 216 )
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
( ----- 217 )
\ Core words, literal parsing, CRC16
: _c ( sa sl -- n 1 OR sa sl 0 ) \ parse character
  DUP 3 = IF OVER C@ ''' = IF OVER 2 + C@ ''' = IF
    DROP 1+ C@ 1 EXIT THEN THEN THEN 0 ;
: PARSE ( sa sl -- n? f )
  _c ?DUP IF EXIT THEN _h ?DUP NOT IF _d THEN ;
CODE CRC16 ( c n -- c )
  INLINE <<8 INLINE XOR 8 i>, INLINE >A BEGIN, ( c )
    INLINE << IFC, $1021 i>, INLINE XOR THEN,
  INLINE A- Z? ^? BR ?JRi, ;CODE
( ----- 218 )
\ Core words, input buffer
SYSVARS $10 + DUP CONSTANT 'KEY? *ALIAS KEY?
: KEY BEGIN KEY? UNTIL ;
SYSVARS $2e + DUP CONSTANT 'IN( *VALUE IN(
SYSVARS $30 + DUP CONSTANT 'IN> *VALUE IN>
SYSVARS $08 + CONSTANT LN<
: IN) IN( LNSZ + ;
: BS? DUP $7f ( DEL ) = SWAP BS = OR ;
( ----- 219 )
\ Core words, input buffer
\ type c into ptr inside INBUF. f=true if typing should stop
: LNTYPE ( ptr c -- ptr+-1 f )
  DUP BS? IF ( ptr c )
    DROP DUP IN( > IF 1- BS EMIT THEN SPC> BS EMIT 0
  ELSE ( ptr c ) \ non-BS
    DUP SPC < IF DROP DUP IN) OVER - SPC FILL 1 ELSE
      TUCK EMIT C!+ DUP IN) = THEN THEN ;
: RDLN ( -- ) \ Read 1 line in IN(
  LIT"  ok" STYPE NL> IN( 'IN> !
  IN( BEGIN KEY LNTYPE UNTIL DROP NL> ;
: IN< ( -- c ) \ Read one character from INBUF
  IN> IN) = IF LN< @ EXECUTE THEN IN> C@ IN> 1+ 'IN> ! ;
: IN$ ['] RDLN LN< !
  [ SYSVARS $40 ( INBUF ) + LITN ] 'IN( ! IN) 'IN> ! ;
( ----- 220 )
\ Core words, WORD parsing
: ," BEGIN IN< DUP '"' = IF DROP EXIT THEN C, AGAIN ;
: WS? SPC <= ;
: TOWORD ( -- c ) \ Advance IN> to first non-WS and yield it.
  0 ( dummy ) BEGIN DROP IN< DUP WS? NOT UNTIL ;
: CURWORD ( -- sa sl ) [ SYSVARS $12 + LITN ] C@+ SWAP @ SWAP ;
: WORD ( -- sa sl )
  TOWORD DROP IN> 1- DUP BEGIN ( old a )
    C@+ WS? OVER IN) = OR UNTIL ( old new )
  DUP 'IN> ! ( old new ) OVER - ( old len )
  IN> 1- C@ WS? IF 1- THEN ( adjust len when not EOL )
  2DUP [ SYSVARS $12 ( CURWORD ) + LITN ] C!+ ! ;
( ----- 221 )
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
( ----- 222 )
\ Core words, Dictionary
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
  DUP 1- C@ ( len ) << >> ( rm IMMEDIATE )
  3 + ( fixed header len ) - 'HERE ! ( w )
  ( get prev addr ) 3 - @ 'CURRENT ! ;
( ----- 223 )
\ Core words, INLINE S= [IF] _bchk
: INLINE
  ' >R CURRENT BEGIN ( R:ref w )
    DUP 3 - @ ( w prev ) DUP I = IF DROP 1 ELSE NIP 0 THEN
  UNTIL ( w ) \ w is word following target
  1- DUP C@ - 5 - ( 2 for prev, 3 for ;CODE )
  R> TUCK - MOVE, ;
: S= ( sa1 sl1 sa2 sl2 -- f )
  ROT OVER = IF ( same len, s2 s1 l ) []=
  ELSE DROP 2DROP 0 THEN ;
: [IF]
  IF EXIT THEN LIT" [THEN]" BEGIN 2DUP WORD S= UNTIL 2DROP ;
: [THEN] ;
: _bchk DUP $80 + $ff > IF LIT" br ovfl" STYPE ABORT THEN ;
( ----- 224 )
\ Core words, DUMP, .S
: DUMP ( n a -- )
  SWAP 8 /MOD SWAP IF 1+ THEN 0 DO
    ':' EMIT DUP .x SPC> DUP ( a a )
    4 0 DO C@+ .x C@+ .x SPC> LOOP DROP ( a )
    8 0 DO
      C@+ DUP SPC < IF DROP '.' THEN EMIT LOOP NL>
  LOOP DROP ;
: PSDUMP SCNT NOT IF EXIT THEN
  SCNT >A BEGIN DUP .X SPC> >R SCNT NOT UNTIL
  BEGIN R> SCNT A> = UNTIL ;
: .S ( -- )
  LIT" SP " STYPE SCNT .x SPC> LIT" RS " STYPE RCNT .x SPC>
  LIT" -- " STYPE STACK? PSDUMP ;
( ----- 225 )
\ Core high, CREATE DOER DOES>
: ;CODE [ lblnext LITN ] JMPi, ;
: CREATE CODE [ lblcell LITN ] CALLi, ;
: DOER CODE [ lbldoes LITN ] CALLi, 2 ALLOT ;
\ Because we pop RS below, we'll exit parent definition
: _ R> CURRENT 3 + ! ;
: DOES> COMPILE _ [ lblxt LITN ] CALLi, ; IMMEDIATE
: CDOES> COMPILE _ R> DROP ; IMMEDIATE
: ALIAS ' CODE JMPi, ;
: VALUE CODE [ lblval LITN ] CALLi, , ;
: VALUES 0 DO 0 VALUE LOOP ;
: CONSTANT CODE i>, ;CODE ; : CONSTS 0 DO RUN1 CONSTANT LOOP ;
( ----- 226 )
\ Core high, BOOT
: (main) IN$ INTERPRET BYE ;
XCURRENT ORG $0a ( stable ABI (main) ) + T!
: BOOT
  [ BIN( $06 ( CURRENT ) + LITN ] @ 'CURRENT !
  [ BIN( $08 ( LATEST ) + LITN ] @ 'HERE !
  ['] (emit) 'EMIT ! ['] (key?) 'KEY? !
  0 IOERR ! $0d0a ( CR/LF ) NL !
  INIT LIT" Collapse OS" STYPE ABORT ;
XCURRENT ORG $04 ( stable ABI BOOT ) + T!
( ----- 227 )
\ Core high, See bootstrap doc. DO..LOOP, LITN, :
: DO COMPILE 2>R HERE ; IMMEDIATE
: LOOP COMPILE (loop) HERE - _bchk C, ; IMMEDIATE
: LITN DUP >>8 IF COMPILE (n) , ELSE COMPILE (b) C, THEN ;
: : CODE [ lblxt LITN ] CALLi, BEGIN
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
: CODE[ COMPILE (c) HERE 1 ALLOT INTERPRET ; IMMEDIATE
: ]CODE ;CODE [COMPILE] THEN 2R> 2DROP ;
: ( LIT" )" BEGIN 2DUP WORD S= UNTIL 2DROP ; IMMEDIATE
: \ IN) 'IN> ! ; IMMEDIATE
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
: [ INTERPRET ; IMMEDIATE
: ] 2R> 2DROP ; \ INTERPRET+RUN1
: COMPILE ' LITN ['] , , ; IMMEDIATE
: [COMPILE] ' , ; IMMEDIATE
: ['] ' LITN ; IMMEDIATE
( ----- 230 )
\ BLK subsystem. See doc/blk.txt. Load range: B230-234
\ Current blk pointer -1 means "invalid"
SYSVARS $38 + DUP CONSTANT 'BLK> *VALUE BLK>
\ Whether buffer is dirty
SYSVARS $3a + CONSTANT BLKDTY
SYSVARS 1024 - CONSTANT BLK(
SYSVARS CONSTANT BLK)
: BLK$ 0 BLKDTY ! -1 'BLK> ! ;
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
  IN) 'IN( ! IN( 'IN> ! ;
: LOAD
  IN> >R ['] _ LN< ! BLK@ BLK( 'IN( ! IN( 'IN> !
  INTERPRET IN$ R> 'IN> ! ;
: LOADR 1+ SWAP DO I DUP . SPC> LOAD LOOP ;
( ----- 234 )
\ Application loader, to include in boot binary
: ED 120 LOAD 100 104 LOADR ;
: VE ED 123 LOAD 105 111 LOADR ;
: ME 123 LOAD 115 119 LOADR ;
: ARCHM 301 LOAD ; : ASML 2 LOAD ; : ASMH 3 LOAD ;
: RSH 150 154 LOADR ;
: AVRP 160 163 LOADR ;
: XCOMPL 200 LOAD ;
( ----- 240 )
\ Grid subsystem. See doc/grid.txt. Load range: B240-B241
GRID_MEM DUP CONSTANT 'XYPOS *VALUE XYPOS
'? CURSOR! NOT [IF] : CURSOR! 2DROP ; [THEN]
: XYPOS! COLS LINES * MOD DUP XYPOS CURSOR! 'XYPOS ! ;
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
: _lf XYPOS COLS / NEWLN COLS * XYPOS! ;
: _bs SPC XYPOS TUCK CELL! ( pos ) 1- XYPOS! ;
: (emit)
    DUP BS? IF DROP _bs EXIT THEN
    DUP CR = IF DROP SPC XYPOS CELL! _lf EXIT THEN
    DUP SPC < IF DROP EXIT THEN
    XYPOS CELL!
    XYPOS 1+ DUP COLS MOD IF XYPOS! ELSE DROP _lf THEN ;
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
    0 ( dummy ) 20 0 DO
        DROP _idle DUP $ff = NOT IF LEAVE THEN LOOP ;

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
    0 (spie) ;
: _rdsdhc ( -- ) $7A ( CMD58 ) 0 0 SDCMDR7 DROP $4000
  AND SDC_SDHC ! DROP ;
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
    NOT UNTIL _rdsdhc ; ( out of idle mode, success! )
( ----- 257 )
: _ ( dstaddr blkno -- )
  [ SDC_DEVID LITN ] (spie)
  $51 ( CMD17 ) SWAP _badj ( a cmd arg1 arg2 ) _cmd IF _err THEN
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
  $58 ( CMD24 ) SWAP _badj ( a cmd arg1 arg2 ) _cmd IF _err THEN
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
2 VALUES _w _h
: _g ( given a top-left of dot-X in BLK(, spit H bin lines )
  _h 0 DO 0 _w 0 DO ( a r )
    << OVER I + C@ 'X' = IF 1+ THEN
  LOOP 8 _w - LSHIFT C, 64 + LOOP DROP ;
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
( ----- 299 )
