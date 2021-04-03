( xcomp for a SMS with PS/2 keyboard on port A and SPI relay
  on port B, with SD card attached )
( 8K of onboard RAM )
0xdd00 CONSTANT RS_ADDR
( Memory register at the end of RAM. Must not overwrite )
0xddca CONSTANT PS_ADDR
RS_ADDR 0xb0 - CONSTANT SYSVARS
0xc000 CONSTANT HERESTART
0xbf   CONSTANT TMS_CTLPORT
0xbe   CONSTANT TMS_DATAPORT
SYSVARS 0xa0 + CONSTANT GRID_MEM
SYSVARS 0xa3 + CONSTANT CPORT_MEM
0x3f   CONSTANT CPORT_CTL
0xdc   CONSTANT CPORT_D1
0xdd   CONSTANT CPORT_D2
SYSVARS 0xa4 + CONSTANT PS2_MEM
1 CONSTANT SDC_DEVID
5 LOAD  ( z80 assembler )
262 263 LOADR ( font compiler )
165 LOAD  ( Sega ROM signer )
280 LOAD  ( boot.z80.decl )
200 205 LOADR ( xcomp )

DI, 0x100 JP, 0x62 ALLOT0 ( 0x66 )
RETN, 0x98 ALLOT0 ( 0x100 )
( All set, carry on! )
CURRENT @ XCURRENT !
0x100 BIN( !
281 303 LOADR ( boot.z80 )
210 231 LOADR ( forth core low )
CREATE ~FNT CPFNT7x7
315 317 LOADR ( TMS9918 )
330 332 LOADR ( VDP )
240 241 LOADR ( Grid )
348 349 LOADR ( SMS ports )
340 LOAD ( KBD ) : (ps2kc) (ps2kcA) ; 246 249 LOADR
347 LOAD ( SPI )
250 258 LOADR ( SDC )
236 239 LOADR ( forth core high )
(entry) _ ( Update LATEST ) PC ORG @ 8 + !
," VDP$ GRID$ PS2$ BLK$ (im1) " EOT,
( start/stop range for SMS is a bit special )
ORG @ 0x100 - DUP ORG !
DUP 1 ( 16K ) segasig
0x4000 + HERE - ALLOT
