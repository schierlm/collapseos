( ----- 600 )
601 MC6850 driver              606 Zilog SIO driver
615 SPI relay                  619 Xcomp unit
( ----- 601 )
( MC6850 Driver. Load range B601-B603. Requires:
  6850_CTL for control register
  6850_IO for data register.
  CTL numbers used: 0x16 = no interrupt, 8bit words, 1 stop bit
  64x divide. 0x56 = RTS high )
CODE 6850>
    HL POP, chkPS,
    BEGIN,
        6850_CTL INAi, 0x02 ANDi, ( are we transmitting? )
    JRZ, ( yes, loop ) AGAIN,
    A L LDrr, 6850_IO OUTiA,
;CODE
( ----- 602 )
CODE 6850<?
    A XORr, ( 256x ) A 0x16 ( RTS lo ) LDri, 6850_CTL OUTiA,
    PUSH0, ( pre-push a failure )
    BEGIN, EXAFAF', ( preserve cnt )
        6850_CTL INAi, 0x1 ANDi, ( rcv buff full? )
        IFNZ, ( full )
            HL POP, ( pop failure )
            6850_IO INAi, PUSHA, PUSH1, A XORr, ( end loop )
        ELSE, EXAFAF', ( recall cnt ) A DECr, THEN,
    JRNZ, AGAIN,
    A 0x56 ( RTS hi ) LDri, 6850_CTL OUTiA, ;CODE
( ----- 603 )
X' 6850<? :* (key?)
X' 6850> :* (emit)
: 6850$ 0x56 ( RTS high ) [ 6850_CTL LITN ] PC! ;
( ----- 605 )
( Zilog SIO driver. Load range B605-608. Requires:
  SIOA_CTL for ch A control register SIOA_DATA for data
  SIOB_CTL for ch B control register SIOB_DATA for data )
CODE SIOA<?
    A XORr, ( 256x ) PUSH0, ( pre-push a failure )
    A 5 ( PTR5 ) LDri, SIOA_CTL OUTiA,
    A 0b01101000 ( RTS low ) LDri, SIOA_CTL OUTiA,
    BEGIN, EXAFAF', ( preserve cnt )
        SIOA_CTL INAi, 0x1 ANDi, ( rcv buff full? )
        IFNZ, ( full )
            HL POP, ( pop failure )
            SIOA_DATA INAi, PUSHA, PUSH1, A XORr, ( end loop )
        ELSE, EXAFAF', ( recall cnt ) A DECr, THEN,
    JRNZ, AGAIN,
    A 5 ( PTR5 ) LDri, SIOA_CTL OUTiA,
    A 0b01101010 ( RTS low ) LDri, SIOA_CTL OUTiA, ;CODE
( ----- 606 )
CODE SIOA>
    HL POP, chkPS,
    BEGIN,
        SIOA_CTL INAi, 0x04 ANDi, ( are we transmitting? )
    JRZ, ( yes, loop ) AGAIN,
    A L LDrr, SIOA_DATA OUTiA,
;CODE
CREATE _ ( init data ) 0x18 C, ( CMD3 )
    0x24 C, ( CMD2/PTR4 ) 0b11000100 C, ( WR4/64x/1stop/nopar )
    0x03 C, ( PTR3 ) 0b11000001 C, ( WR3/RXen/8char )
    0x05 C, ( PTR5 ) 0b01101010 C, ( WR5/TXen/8char/RTS )
    0x21 C, ( CMD2/PTR1 ) 0 C, ( WR1/Rx no INT )
: SIOA$ 9 0 DO _ I + C@ [ SIOA_CTL LITN ] PC! LOOP ;
( ----- 607 )
CODE SIOB<? ( copy/paste of SIOA<? )
    A XORr, ( 256x ) PUSH0, ( pre-push a failure )
    A 5 ( PTR5 ) LDri, SIOB_CTL OUTiA,
    A 0b01101000 ( RTS low ) LDri, SIOB_CTL OUTiA,
    BEGIN, EXAFAF', ( preserve cnt )
        SIOB_CTL INAi, 0x1 ANDi, ( rcv buff full? )
        IFNZ, ( full )
            HL POP, ( pop failure )
            SIOB_DATA INAi, PUSHA, PUSH1, A XORr, ( end loop )
        ELSE, EXAFAF', ( recall cnt ) A DECr, THEN,
    JRNZ, AGAIN,
    A 5 ( PTR5 ) LDri, SIOB_CTL OUTiA,
    A 0b01101010 ( RTS low ) LDri, SIOB_CTL OUTiA, ;CODE
( ----- 608 )
CODE SIOB>
    HL POP, chkPS,
    BEGIN,
        SIOB_CTL INAi, 0x04 ANDi, ( are we transmitting? )
    JRZ, ( yes, loop ) AGAIN,
    A L LDrr, SIOB_DATA OUTiA,
;CODE
: SIOB$ 9 0 DO _ I + C@ [ SIOB_CTL LITN ] PC! LOOP ;
( ----- 619 )
( RC2014 classic with MC6850 )
0xff00 CONSTANT RS_ADDR        0xfffa CONSTANT PS_ADDR
RS_ADDR 0xa0 - CONSTANT SYSVARS
0x8000 CONSTANT HERESTART
0x80 CONSTANT 6850_CTL 0x81 CONSTANT 6850_IO
4 CONSTANT SPI_DATA 5 CONSTANT SPI_CTL 1 CONSTANT SDC_DEVID
5 LOAD    ( z80 assembler )
262 LOAD  ( xcomp )            281 LOAD  ( boot.z80.decl )
270 LOAD  ( xcomp overrides )  282 312 LOADR ( boot.z80 )
353 LOAD  ( xcomp core low )   601 603 LOADR ( MC6850 )
323 LOAD  ( SPI relay )        423 436 LOADR ( SD Card )
321 LOAD  ( AT28 )
390 LOAD  ( xcomp core high )
(entry) _
PC ORG @ 8 + ! ( Update LATEST )
," 6850$ BLK$ " EOT,
( ----- 620 )
( RC2014 classic with SIO )
0xff00 CONSTANT RS_ADDR        0xfffa CONSTANT PS_ADDR
RS_ADDR 0xa0 - CONSTANT SYSVARS
0x8000 CONSTANT HERESTART
0x80 CONSTANT SIOA_CTL   0x81 CONSTANT SIOA_DATA
0x82 CONSTANT SIOB_CTL   0x83 CONSTANT SIOB_DATA
4 CONSTANT SPI_DATA 5 CONSTANT SPI_CTL 1 CONSTANT SDC_DEVID
5 LOAD    ( z80 assembler )
262 LOAD  ( xcomp )            281 LOAD  ( boot.z80.decl )
270 LOAD  ( xcomp overrides )  282 312 LOADR ( boot.z80 )
353 LOAD  ( xcomp core low )   605 607 LOADR ( SIO )
323 LOAD  ( SPI relay )        423 436 LOADR ( SD Card )
321 LOAD  ( AT28 ) X' SIOA<? :* (key?) X' SIOA> :* (emit)
390 LOAD  ( xcomp core high )
(entry) _ PC ORG @ 8 + ! ( Update LATEST )
," SIOA$ BLK$ " EOT,
