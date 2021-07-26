\ xcomp for a SMS with PS/2 keyboard on port A and SPI relay
\ on port B, with SD card attached
9 VALUES RS_ADDR $dd00 PS_ADDR $ddca HERESTART $c000
         TMS_CTLPORT $bf TMS_DATAPORT $be
         CPORT_CTL $3f CPORT_D1 $dc CPORT_D2 $dd
         SDC_DEVID 1
RS_ADDR $90 - VALUE SYSVARS
SYSVARS $80 + VALUE GRID_MEM
SYSVARS $83 + VALUE CPORT_MEM
SYSVARS $84 + VALUE PS2_MEM
120 LOAD \ nC, for PS/2 subsystem
5 LOAD  ( z80 assembler )
262 263 LOADR ( font compiler )
165 LOAD  ( Sega ROM signer )
280 LOAD  ( boot.z80.decl )
200 205 LOADR ( xcomp )

DI, $100 JP, $62 ALLOT0 ( $66 )
RETN, $98 ALLOT0 ( $100 )
( All set, carry on! )
CURRENT TO XCURRENT
$100 TO BIN(
281 299 LOADR ( boot.z80 )
210 224 LOADR ( core low )
CREATE ~FNT CPFNT7x7
315 317 LOADR ( TMS9918 )
330 332 LOADR ( VDP )
240 241 LOADR ( Grid )
348 349 LOADR ( SMS ports )
340 LOAD ( KBD )
: (ps2kc) (ps2kcA) ;
246 249 LOADR \ PS/2
347 LOAD ( SPI )
250 258 LOADR ( SDC )
X' SDC@ ALIAS (blk@)
X' SDC! ALIAS (blk!)
230 233 LOADR \ BLK subsystem
: INIT VDP$ GRID$ PS2$ BLK$ (im1) ;
XWRAP INIT
\ start/stop range for SMS is a bit special
ORG $100 - DUP TO ORG
DUP 1 ( 16K ) segasig
$4000 + HERE - ALLOT
