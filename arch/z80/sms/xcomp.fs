\ SMS with 8K of onboard RAM
\ Memory register at the end of RAM. Must not overwrite
8 VALUES RS_ADDR 0xdd00 PS_ADDR 0xddca HERESTART 0xc000
         TMS_CTLPORT 0xbf TMS_DATAPORT 0xbe
         CPORT_CTL 0x3f CPORT_D1 0xdc CPORT_D2 0xdd
RS_ADDR 0x90 - VALUE SYSVARS
SYSVARS 0x80 + VALUE GRID_MEM
SYSVARS 0x83 + VALUE CPORT_MEM
SYSVARS 0x84 + VALUE PAD_MEM
5 LOAD  ( z80 assembler )
262 263 LOADR ( font compiler )
165 LOAD  ( Sega ROM signer )
280 LOAD  ( boot.z80.decl )
200 205 LOADR ( xcomp )

DI, 0x100 JP, 0x62 ALLOT0 ( 0x66 )
RETN, 0x98 ALLOT0 ( 0x100 )
( All set, carry on! )
CURRENT TO XCURRENT
0x100 TO BIN(
281 300 LOADR ( boot.z80 )
210 227 LOADR ( forth core low, no BLK )
CREATE ~FNT CPFNT7x7
315 317 LOADR ( TMS9918 )
330 332 LOADR ( VDP )
240 241 LOADR ( Grid )
348 349 LOADR ( SMS ports )
335 338 LOADR ( PAD )
: INIT VDP$ GRID$ PAD$ (im1) ;
236 239 LOADR ( forth core high )
XWRAP INIT
\ start/stop range for SMS is a bit special
ORG 0x100 - DUP TO ORG
DUP 1 ( 16K ) segasig
0x4000 + HERE - ALLOT

