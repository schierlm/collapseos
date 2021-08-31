\ xcomp using the Text Mode if the VDP. Only works on actual
\ SMS. The Megadrive's VDP doesn't have TMS9918 modes in it.
9 VALUES RS_ADDR $dd00 PS_ADDR $ddca HERESTART $c000
         TMS_CTLPORT $bf TMS_DATAPORT $be
         CPORT_CTL $3f CPORT_D1 $dc CPORT_D2 $dd
         SDC_DEVID 1
RS_ADDR $90 - VALUE SYSVARS
SYSVARS $80 + VALUE GRID_MEM
SYSVARS $83 + VALUE CPORT_MEM
SYSVARS $84 + VALUE PS2_MEM
SYSVARS $85 + VALUE SDC_MEM
120 LOAD \ nC, for PS/2 subsystem
Z80A XCOMPL FONTC
165 LOAD  \ Sega ROM signer
Z80M Z80H HALC XCOMPH

DI, $100 JP, $62 ALLOT0 ( $66 )
RETN, $98 ALLOT0 ( $100 )
( All set, carry on! )
$100 TO BIN(
Z80C COREL Z80H HALC
CREATE ~FNT CPFNT5x7
315 317 LOADR ( TMS9918 )
GRIDSUB
348 349 LOADR ( SMS ports )
340 LOAD ( KBD )
: (ps2kc) (ps2kcA) ;
PS2SUB
347 LOAD ( SPI )
250 258 LOADR ( SDC )
X' SDC@ ALIAS (blk@)
X' SDC! ALIAS (blk!)
BLKSUB
: INIT TMS$ GRID$ PS2$ BLK$ (im1) ;
XWRAP
\ start/stop range for SMS is a bit special
ORG $100 - DUP TO ORG
DUP 1 ( 16K ) segasig
$4000 + HERE - ALLOT
