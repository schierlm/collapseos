\ RC2014 classic with MC6850
8 VALUES RS_ADDR $ff00 PS_ADDR $fffa HERESTART $8000
         6850_CTL $80 6850_IO $81
         SPI_DATA 4 SPI_CTL 5 SDC_DEVID 1
RS_ADDR $80 - VALUE SYSVARS
5 LOAD    ( z80 assembler )
280 LOAD  ( boot.z80.decl )    200 205 LOADR ( xcomp )
281 299 LOADR ( boot.z80 )
210 224 LOADR ( forth low )    320 322 LOADR ( MC6850 )
312 LOAD  ( SPI relay )        250 258 LOADR ( SD Card )
311 LOAD  ( AT28 )
X' SDC@ ALIAS (blk@)
X' SDC! ALIAS (blk!)
230 233 LOADR \ BLK subsystem
: INIT 6850$ BLK$ ;
XWRAP INIT
