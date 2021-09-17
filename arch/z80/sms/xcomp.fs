\ SMS with 8K of onboard RAM
\ Memory register at the end of RAM. Must not overwrite
8 VALUES RS_ADDR $dd00 PS_ADDR $ddca HERESTART $c000
         TMS_CTLPORT $bf TMS_DATAPORT $be
         CPORT_CTL $3f CPORT_D1 $dc CPORT_D2 $dd
RS_ADDR $90 - VALUE SYSVARS
SYSVARS $80 + VALUE GRID_MEM
SYSVARS $83 + VALUE CPORT_MEM
SYSVARS $84 + VALUE PAD_MEM
Z80A XCOMPL FONTC
165 LOAD  \ Sega ROM signer
ARCHM Z80H HALC XCOMPH

DI, $100 JP, $62 ALLOT0 ( $66 )
RETN, $98 ALLOT0 ( $100 )
( All set, carry on! )
$100 TO BIN(
Z80C COREL Z80H HALC
CREATE ~FNT CPFNT7x7
335 337 LOADR ( TMS9918 )
350 352 LOADR ( VDP )
GRIDSUB
368 369 LOADR ( SMS ports )
355 358 LOADR ( PAD )
: INIT VDP$ GRID$ PAD$ (im1) ;
XWRAP
\ start/stop range for SMS is a bit special
ORG $100 - DUP TO ORG
DUP 1 ( 16K ) segasig
$4000 + HERE - ALLOT
