( ----- 600 )
TRS-80 Recipe

Support code for the TRS-80 recipe. Contains drivers for the
keyboard, video and floppy. At the moment, they are thin layer
over the drivers provided by TRSDOS' SVC.

Load with "602 LOAD".

The native KBD driver in 614 is a WIP: it doesn't work well as
long as TRS-DOS is there to muck up things underneath. But it
works well enough to be nearly complete.

There is also the RECV program at B612.
( ----- 602 )
1 9 LOADR+
( ----- 603 )
CODE (key?)
    A 0x08 LDri, ( @KBD )
    0x28 RST,
    IFZ, 0xb1 CPi, IFZ, A '|' LDri, THEN,
    0xad CPi, IFZ, A '~' LDri, THEN,
    PUSHA, PUSH1, ELSE, PUSH0, THEN, ;CODE
CODE (emit) EXX, ( protect BC )
    BC POP, ( c == @DSP arg ) chkPS,
    A 0x02 LDri, ( @DSP )
    0x28 RST,
EXX, ( unprotect BC ) ;CODE
( ----- 604 )
CODE AT-XY EXX, ( protect BC )
    DE POP, H E LDrr, ( Y )
    DE POP, L E LDrr, ( X ) chkPS,
    A 0x0f LDri, ( @VDCTL ) B 3 LDri, ( setcur )
    0x28 RST,
EXX, ( unprotect BC ) ;CODE
: LINES 24 ; : COLS 80 ;
: XYMODE 0x70 RAM+ ;
: CELL! COLS /MOD AT-XY (emit) ;
CODE BYE
    HL 0 LDdi,
    A 0x16 LDri, ( @EXIT )
    0x28 RST,
( ----- 605 )
CODE @RDSEC ( drv cylsec addr -- f ) EXX, ( protect BC )
    HL POP,
    DE POP,
    BC POP,
    chkPS,
    A 0x31 LDri, ( @RDSEC )
    0x28 RST,
    PUSHZ,
EXX, ( unprotect BC ) ;CODE
( ----- 606 )
CODE @WRSEC ( drv cylsec addr -- f ) EXX, ( protect BC )
    HL POP,
    DE POP,
    BC POP,
    chkPS,
    A 0x35 LDri, ( @WRSEC )
    0x28 RST,
    PUSHZ,
EXX, ( unprotect BC ) ;CODE
( ----- 607 )
CODE @DCSTAT ( drv -- f ) EXX, ( protect BC )
    BC POP,
    chkPS,
    A 0x28 LDri, ( @DCSTAT )
    0x28 RST,
    PUSHZ,
EXX, ( unprotect BC ) ;CODE

: FD0 FLUSH 0 [ DRVMEM LITN ] C! ;
: FD1 FLUSH 1 [ DRVMEM LITN ] C! ;
: FDDRV [ DRVMEM LITN ] C@ ;
( ----- 608 )
: _err LIT" FDerr" ERR ;
: _cylsec ( sec -- cs, return sector/cylinder for given secid )
    ( 4 256b sectors per block, 10 sec per cyl, 40 cyl max )
    10 /MOD ( sec cyl )
    DUP 39 > IF _err THEN
    8 LSHIFT + ( cylsec )
;
: FD@! ( wref blk -- )
    1 @DCSTAT NOT IF _err THEN
    2 LSHIFT ( 4 * -- wr sec )
    4 0 DO ( wr sec )
        DUP I + _cylsec ( wr sec cs )
        I 8 LSHIFT BLK( + ( wr sec cs addr )
        FDDRV ROT> ( wr sec drv cs addr )
        4 PICK EXECUTE NOT IF _err THEN
    LOOP 2DROP ;
( ----- 609 )
: FD@ ['] @RDSEC SWAP FD@! ;
: FD! ['] @WRSEC SWAP FD@! ;
: FD$ ['] FD@ ['] BLK@* **! ['] FD! ['] BLK!* **! FD1 ;
( ----- 610 )
: CL$ ( baudcode -- )
0x02 0xe8 PC! ( UART RST ) DUP 4 LSHIFT OR 0xe9 PC! ( bauds )
  0b01101101 0xea PC! ( word8 no parity no-RTS ) ;
CODE CL> HL POP, chkPS,
  BEGIN, A 0x6a ( @CKBRKC ) LDri, 0x28 RST, IFNZ, JPNEXT, THEN,
    0xea INAi, 0x40 ANDi, IFNZ, ( TX reg empty )
      0xe8 INAi, 0x80 ANDi, IFZ, ( CTS low )
        A L LDrr, 0xeb OUTiA, ( send byte ) JPNEXT,
  THEN, THEN, JR, AGAIN,
( ----- 611 )
CODE CL<?
    A XORr, ( 256x ) PUSH0, ( pre-push a failure )
    A 0b01101100 ( RTS low ) LDri, 0xea OUTiA,
    BEGIN, EXAFAF', ( preserve cnt )
        0xea INAi, 0x80 ANDi, ( rcv buff full? )
        IFNZ, ( full )
            HL POP, ( pop failure )
            0xeb INAi, PUSHA, PUSH1, A XORr, ( end loop )
        ELSE, EXAFAF', ( recall cnt ) A DECr, THEN,
    JRNZ, AGAIN,
    A 0b01101101 ( RTS high ) LDri, 0xea OUTiA, ;CODE
( ----- 612 )
( We process the 0x20 exception by pre-putting a mask in the
  (HL) we're going to write to. If it wasn't a 0x20, we put a
  0xff mask. If it was a 0x20, we put a 0x7f mask. )
: @GET,
    A 0x03 LDri, ( @GET )
    DE COM_DRV_ADDR LDdi,
    0x28 RST, JRNZ, L2 FWR ( maybeerror )
    A ORr,
    CZ RETc, ( Sending a straight NULL ends the comm. ) ;
: @PUT, ( @PUT that char back )
        C A LDrr,
        A 0x04 LDri, ( @PUT )
        0x28 RST, JRNZ, L3 FWR ( error )
        A C LDrr, ;
HERE ORG !
HL DEST_ADDR LDdi,                                    ( cont. )
( ----- 613 )
BEGIN,
    A 0xff LDri, (HL) A LDrr, ( default mask )
    L1 BSET ( loop2 ) @GET, @PUT,
    0x20 CPi, JRZ, L4 FWR ( escapechar )
	( not an escape char, just apply the mask and write )
    (HL) ANDr, (HL) A LDrr,
    HL INCd,
JR, AGAIN,
L4 FSET ( escapechar, adjust by setting (hl) to 0x7f )
7 (HL) RES, JR, L1 BWR ( loop2 )
L2 FSET ( maybeerror, was it an error? )
A ORr, JRZ, L1 BWR ( loop2, not an error )
L3 FSET ( error )
C A LDrr, ( error code from @GET/@PUT )
A 0x1a LDri, ( @ERROR ) 0x28 RST, RET,
( ----- 614 )
( Native KBD driver. doesnt work well with TRSDOS in mem )
L1 BSET A (HL) LDrr, L SLA, A ORr, RET,
L2 BSET BEGIN, E INCr, RRA, JRNC, AGAIN, A E LDrr, RET,
L3 BSET A 4 LDri, 0x84 OUTiA, ( mmap 1 )
  HL 0x3801 LDdi, L1 @ CALL, IFNZ, E '@' 1- LDri, L2 @ CALL,
  ELSE, ( 02 ) L1 @ CALL, IFNZ, E 'G' LDri, L2 @ CALL,
  ELSE, ( 04 ) L1 @ CALL, IFNZ, E 'O' LDri, L2 @ CALL,
  ELSE, ( 08 ) L1 @ CALL, 7 ANDi, IFNZ, E 'W' LDri, L2 @ CALL,
  ELSE, ( 10 ) L1 @ CALL, IFNZ, E '/' LDri, L2 @ CALL,
  ELSE, ( 20 ) L1 @ CALL, IFNZ, E '7' LDri, L2 @ CALL,
    '<' CPi, IFNC, 0x10 SUBi, THEN,
  ELSE, ( 40 ) A (HL) LDrr, 0xa5 ANDi, IFNZ,
    RLA, IFC, A SPC LDri, ELSE, RLA, RLA, IFC, A BS LDri,
    ELSE, RLA, RLA, RLA, IFC, A 0x80 LDri,
    ELSE, A CR LDri, THEN, THEN, THEN,
  THEN, THEN, THEN, THEN, THEN, THEN, THEN, ( A = key or 0 )
( ----- 615 )
  A ORr, IFNZ, ( keypress )
  L 0x80 LDri, E (HL) LDrr,
  E RR, IFC, ( L shift )
    0x30 CPi, IFC, 0x10 ORi, ELSE, 0x20 ORi,
    0x60 CPi, IFC, 0xef ANDi, THEN, THEN,
  ELSE, E RR, IFC, ( R shift ) '@' CPi, IFZ, 0x1f ADDi, ELSE,
     0x2f ADDi, 0x61 CPi, IFNC, 0x14 ADDi, THEN, THEN, THEN,
  THEN, THEN, A ORr, ( ensure correct Z )
  EXAFAF', A 7 LDri, 0x84 OUTiA, ( mmap 4 ) EXAFAF', RET,
( kbd memory is flaky. it sometimes returns garbage. To ensure
  reliable results, we poll twice and compare. )
CODE (key?)
  L3 @ CALL, IFZ, PUSH0, ELSE, D A LDrr, L3 @ CALL, D CPr,
    IFZ, PUSHA, PUSH1, BEGIN, L3 @ CALL, JRNZ, AGAIN,
    ELSE, PUSH0, THEN, THEN, ;CODE
( ----- 620 )
( xcomp consts )
0xff00 CONSTANT RS_ADDR 0xfffa CONSTANT PS_ADDR
RS_ADDR 0xb0 - CONSTANT SYSVARS 0 CONSTANT HERESTART
SYSVARS 0xa0 + CONSTANT DRVMEM
( ----- 621 )
( xcomp wrapup )
(entry) _ PC ORG @ 8 + !
," CR 0x50 RAM+ ! BLK$ FD$ " EOT,
ORG @ DUP .X HERE -^ .X
