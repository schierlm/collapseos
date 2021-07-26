( ----- 000 )
\ Forth testing harness
\ "#" means "assert". We ABORT on failure.
\ Load range: B0-B6
: fail SPC> ABORT" failed" ;
: # IF SPC> ." pass" NL> ELSE fail THEN ;
: #eq 2DUP SWAP . SPC> '=' EMIT SPC> . '?' EMIT = # ;

( ----- 001 )
\ Arithmetics
48 13 + 61 #eq
48 13 - 35 #eq
48 13 * 624 #eq
48 13 / 3 #eq
48 13 MOD 9 #eq
( ----- 002 )
\ Comparisons
$22 $8065 < #
-1 0 > #
-1 0< #
( ----- 003 )
\ Memory
42 C, 43 C, 44 C,
HERE 3 - HERE 3 MOVE
HERE C@ 42 #eq HERE 1+ C@ 43 #eq HERE 2 + C@ 44 #eq
HERE HERE 1+ 3 MOVE ( demonstrate MOVE's problem )
HERE 1+ C@ 42 #eq HERE 2 + C@ 42 #eq HERE 3 + C@ 42 #eq
HERE 3 - HERE 3 MOVE
HERE HERE 1+ 3 MOVE- ( see? better )
HERE 1+ C@ 42 #eq HERE 2 + C@ 43 #eq HERE 3 + C@ 44 #eq

HERE ( ref )
HERE 3 - 3 MOVE,
( ref ) HERE 3 - #eq
HERE 3 - C@ 42 #eq HERE 2 - C@ 43 #eq HERE 1- C@ 44 #eq
( ----- 004 )
\ Parse
'b' $62 #eq
( ----- 005 )
\ Stack
42 43 44 ROT
42 #eq 44 #eq 43 #eq
42 43 44 ROT>
43 #eq 42 #eq 44 #eq
( ----- 006 )
\ CRC
$0000 $00 CRC16 $0000 #eq
$0000 $01 CRC16 $1021 #eq
$5678 $34 CRC16 $34e4 #eq
