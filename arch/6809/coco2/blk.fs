( ----- 600 )
PC ," @HPX08" CR C, ," AIQY19" 0 C,
   ," BJRZ2:" 0 C,  ," CKS_3:" 0 C,
   ," DLT_4," 0 C,  ," EMU" BS C, ," 5-" 0 C,
   ," FNV_6." 0 C,  ," GOW 7/" 0x80 C,
L1 BSET ( PC ) # LDX, 0xfe # LDA, BEGIN, ( 8 times )
  0xff02 () STA, ( set col ) 0xff00 () LDB, ( read row )
  INCB, IFNZ, ( key pressed ) DECB, RTS, THEN,
  ( inc col ) 7 X+N LEAX, 1 # ORCC, ROLA, BCS, AGAIN,
  ( no key ) CLRB, RTS,
CODE (key?) ( -- c? f ) CLRA, CLRB, PSHS, D L1 LPC () JSR,
  IFNZ, ( key! )
    BEGIN, X+ LDA, LSRB, BCS, AGAIN,
    ( A = our char ) 1 S+N STA, 1 # LDD, ( f ) PSHS, D
    ( wait for keyup ) BEGIN, L1 LPC () JSR, BNE, AGAIN,
  THEN, ;CODE
( ----- 601 )
: (emit)
  0x20 - DUP 0x5f < IF
    DUP 0x20 < IF 0x60 + ELSE DUP 0x40 < IF 0x20 + ELSE 0x40 -
      THEN THEN
    0xa0 RAM+ TUCK @ C!+ SWAP ! ELSE DROP THEN ;
: COCO2$ 0x400 0xa0 RAM+ ! ;
( ----- 602 )
( Work in Progress )
50 LOAD ( 6809 assembler )
0x8000 CONSTANT PS_ADDR 0x7f00 CONSTANT RS_ADDR
0x7e00 CONSTANT SYSVARS
0x0600 CONSTANT HERESTART
0xc000 BIN( !
262 LOAD ( xcomp )
270 LOAD ( xcomp overrides )
470 478 LOADR ( boot.6809 )
353 LOAD ( forth low )
600 601 LOADR ( drivers )
390 LOAD ( forth high )
(entry) _ PC ORG @ 8 + T!
," COCO2$ " EOT,
