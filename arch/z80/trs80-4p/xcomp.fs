620 LOAD ( xcomp consts )
5 LOAD   ( z80 assembler )
262 LOAD ( xcomp )
281 LOAD ( boot.z80.decl )
270 LOAD ( xcomp overrides )
0x3000 BIN( !
282 312 LOADR ( boot.z80 )
353 LOAD ( xcomp core low )
602 LOAD ( trs80 )
390 LOAD ( xcomp core high )
(entry) _
( Update LATEST )
PC ORG @ 8 + !
( TRS-80 wants CR-only newlines )
," CR 0x50 RAM+ C! BLK$ FD$ " EOT,
ORG @ |M 2 PC! 2 PC!
HERE |M 2 PC! 2 PC!
