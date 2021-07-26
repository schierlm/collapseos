\ RC2014 classic with SIO
10 VALUES RS_ADDR $ff00 PS_ADDR $fffa HERESTART $8000
         SIOA_CTL $80 SIOA_DATA $81
         SIOB_CTL $82 SIOB_DATA $83
         SPI_DATA 4 SPI_CTL 5 SDC_DEVID 1
RS_ADDR $80 - VALUE SYSVARS
5 LOAD    ( z80 assembler )
280 LOAD  ( boot.z80.decl )    200 205 LOADR ( xcomp )
281 299 LOADR ( boot.z80 )
210 224 LOADR ( forth low )    325 327 LOADR ( SIO )
312 LOAD  ( SPI relay )        250 258 LOADR ( SD Card )
311 LOAD  ( AT28 )
X' SDC@ ALIAS (blk@)
X' SDC! ALIAS (blk!)
230 233 LOADR \ BLK subsystem
: INIT SIOA$ BLK$ ;
X' SIOA<? ALIAS (key?) X' SIOA> ALIAS (emit)
X' SIOA<? ALIAS RX<? X' SIOA> ALIAS TX>
XWRAP INIT
