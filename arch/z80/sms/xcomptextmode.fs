\ xcomp using the Text Mode if the VDP. Only works on actual
\ SMS. The Megadrive's VDP doesn't have TMS9918 modes in it.
9 VALUES RS_ADDR 0xdd00 PS_ADDR 0xddca HERESTART 0xc000
         TMS_CTLPORT 0xbf TMS_DATAPORT 0xbe
         CPORT_CTL 0x3f CPORT_D1 0xdc CPORT_D2 0xdd
         SDC_DEVID 1
RS_ADDR 0x90 - VALUE SYSVARS
SYSVARS 0x80 + VALUE GRID_MEM
SYSVARS 0x83 + VALUE CPORT_MEM
SYSVARS 0x84 + VALUE PS2_MEM
120 LOAD \ nC, for PS/2 subsystem
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
210 231 LOADR ( forth core low )
CREATE ~FNT CPFNT5x7
315 317 LOADR ( TMS9918 )
240 241 LOADR ( Grid )
348 349 LOADR ( SMS ports )
340 LOAD ( KBD ) : (ps2kc) (ps2kcA) ; 246 249 LOADR
347 LOAD ( SPI )
250 258 LOADR ( SDC )
: INIT TMS$ GRID$ PS2$ BLK$ ['] SDC@ [*TO] BLK@* (im1) ;
236 239 LOADR ( forth core high )
XWRAP INIT
\ start/stop range for SMS is a bit special
ORG 0x100 - DUP TO ORG
DUP 1 ( 16K ) segasig
0x4000 + HERE - ALLOT
