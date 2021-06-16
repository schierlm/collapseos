\ RC2014 classic with MC6850
8 VALUES RS_ADDR 0xff00 PS_ADDR 0xfffa HERESTART 0x8000
         6850_CTL 0x80 6850_IO 0x81
         SPI_DATA 4 SPI_CTL 5 SDC_DEVID 1
RS_ADDR 0x80 - VALUE SYSVARS
5 LOAD    ( z80 assembler )
280 LOAD  ( boot.z80.decl )    200 205 LOADR ( xcomp )
281 300 LOADR ( boot.z80 )
210 231 LOADR ( forth low )    320 322 LOADR ( MC6850 )
312 LOAD  ( SPI relay )        250 258 LOADR ( SD Card )
311 LOAD  ( AT28 )
: INIT 6850$ BLK$ ;
236 239 LOADR ( forth high )
XWRAP INIT
