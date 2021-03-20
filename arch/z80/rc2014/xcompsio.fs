( RC2014 classic with SIO )
0xff00 CONSTANT RS_ADDR        0xfffa CONSTANT PS_ADDR
RS_ADDR 0xa0 - CONSTANT SYSVARS
0x8000 CONSTANT HERESTART
0x80 CONSTANT SIOA_CTL   0x81 CONSTANT SIOA_DATA
0x82 CONSTANT SIOB_CTL   0x83 CONSTANT SIOB_DATA
4 CONSTANT SPI_DATA 5 CONSTANT SPI_CTL 1 CONSTANT SDC_DEVID
5 LOAD    ( z80 assembler )
280 LOAD  ( boot.z80.decl )    200 205 LOADR ( xcomp ) 
281 307 LOADR ( boot.z80 )
210 231 LOADR ( forth low )    325 327 LOADR ( SIO )
312 LOAD  ( SPI relay )        250 258 LOADR ( SD Card )
311 LOAD  ( AT28 ) X' SIOA<? :* (key?) X' SIOA> :* (emit)
X' SIOA<? :* RX<? X' SIOA> :* TX>
236 239 LOADR ( forth high )
(entry) _ PC ORG @ 8 + ! ," SIOA$ BLK$ " EOT,
