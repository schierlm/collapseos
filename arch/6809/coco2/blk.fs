( ----- 600 )
PC ," @HPX08" CR C, ," AIQY19" 0 C,
   ," BJRZ2:" 0 C,  ," CKS_3:" 0 C,
   ," DLT_4," 0 C,  ," EMU" BS C, ," 5-" 0 C,
   ," FNV_6." 0 C,  ," GOW 7/" 0x80 C,
CODE (key?) ( -- c? f )
  ( PC ) # LDX,
  CLRA, CLRB, PSHS, D 0xfe # LDA, BEGIN, ( 8 times )
    0xff02 () STA, ( set col ) 0xff00 () LDB, ( read row )
    INCB, IFNZ, ( key pressed )
      DECB, 0xff # LDA, BEGIN, INCA, LSRB, BCS, AGAIN,
      ( X+A = our char ) X+A LDB, 1 S+N STB, ( char )
      1 # LDD, ( f ) PSHS, D CLRA, THEN,
    ( inc col ) 7 X+N LEAX,
    1 # ORCC, ROLA, BCS, AGAIN,
  CLRA, 0xff00 # LDX, 2 X+N STA, BEGIN, ( wait for keyup )
    X+0 LDA, INCA, BNE, AGAIN, ;CODE
( ----- 601 )
( WIP: only works if you comment out the LOADR+ in B353 )
50 LOAD ( 6809 assembler )
0x8000 CONSTANT PS_ADDR 0x7f00 CONSTANT RS_ADDR
0x7e00 CONSTANT SYSVARS
0xc000 BIN( !
262 LOAD  ( xcomp )
270 LOAD  ( xcomp overrides )
470 478 LOADR ( boot.6809 )
353 357 LOADR ( forth low )
600 LOAD ( drivers )
479 LOAD
