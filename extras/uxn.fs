( ----- 000 )
\ UXN assembler
1 TO BIGEND?
CREATE ops ," BRK   NOPPOPDUPSWPOVRROTEQUNEQGTHLTHJMPJCNJSRSTH"
           ," LDZSTZLDRSTRLDASTADEIDEOADDSUBMULDIVANDORAEORSFT"
CREATE flags ," 2rk"
: findop ( sa sl -- op? f ) \ find op "sa sl" in ops
  DUP 3 < IF 2DROP 0 EXIT THEN $20 0 DO ( sa sl )
    OVER I 3 * ops + 3 []= IF 2DROP I 1 UNLOOP EXIT THEN
  LOOP 2DROP 0 ;
: findf ( c -- mask )
  $20 flags 3 RANGE DO ( c m ) OVER I C@ = IF
    NIP UNLOOP EXIT THEN << LOOP (wnf) ;
: flags! ( sa sl op -- op ) \ apply 2KR flags to op
  SWAP 3 - ?DUP IF ( sa op sl-3 )
    ROT 3 + SWAP RANGE DO ( op ) I C@ findf OR LOOP
  ELSE NIP THEN ;
( ----- 001 )
: lit8, ( n -- ) 1 C, C, ; : lit16, $21 C, T, ;
: lit, ( sl n -- )
  TUCK $100 < SWAP 4 < AND ( n f ) IF lit8, ELSE lit16, THEN ;
: uxn<, ( -- f ) \ read and parse one uxn upcode
  WORD OVER C@ ']' = IF 2DROP 0 EXIT THEN
  2DUP PARSE IF ROT DROP lit, 1 EXIT THEN
  2DUP findop IF
    flags! C, 1 ELSE
    FIND IF EXECUTE 1 ELSE (wnf) THEN THEN ;
: uxn[ BEGIN uxn<, NOT UNTIL ; IMMEDIATE
( ----- 002 )
: BEGIN, PC ;
: BSET BEGIN, TO ;
: FJR, BEGIN, 0 lit8, ;
: THEN, ( orig-pc -- )
  DUP PC ( opc opc pc ) -^ 3 - ( opc off )
\ warning: l is a PC offset, not a mem addr!
  SWAP ORG + BIN( - 1+ ( LIT ) ( off addr ) C! ;
: FWR8 BSET 0 lit8, ; : FWR16 BSET 0 lit16, ;
: FSET8 ' EXECUTE THEN, ; : FSET16 PC ' EXECUTE T! ;
: AGAIN, PC - 3 - _bchk lit8, ;
: BWR ' EXECUTE AGAIN, ;
: ;CODE lblnext? lit16, $2c ( JMP2 ) C, ;
