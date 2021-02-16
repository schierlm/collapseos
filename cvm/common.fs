( This is xcomp code that is common to both stage and forth
  binaries. )
0xff00 CONSTANT RS_ADDR
0xfffa CONSTANT PS_ADDR
RS_ADDR 0xb0 - CONSTANT SYSVARS
SYSVARS 0xa0 + CONSTANT GRID_MEM
0 CONSTANT HERESTART
2 LOAD
: CODE ( natidx -- ) (entry) 0 C, C, ;
262 LOAD  ( xcomp )
270 LOAD  ( xcomp overrides )

HERE ORG !
0x15 ALLOT0
( END OF STABLE ABI )
HERE 4 + XCURRENT ! ( make next CODE have 0 prev field )
0x00 CODE EXIT
0x01 CODE (br)
0x02 CODE (?br)
0x03 CODE (loop)
0x04 CODE (b)
0x05 CODE (n)
0x06 CODE (s)
0x07 CODE >R
0x08 CODE R>
0x09 CODE 2>R
0x0a CODE 2R>
0x0b CODE EXECUTE
0x0c CODE ROT
0x0d CODE DUP
0x0e CODE ?DUP
0x0f CODE DROP
0x10 CODE SWAP
0x11 CODE OVER
0x12 CODE PICK
0x13 CODE 2DROP
0x14 CODE 2DUP
0x15 CODE 'S
0x16 CODE AND
0x17 CODE OR
0x18 CODE XOR
0x19 CODE NOT
0x1a CODE +
0x1b CODE -
0x1c CODE *
0x1d CODE /MOD
0x1e CODE !
0x1f CODE @
0x20 CODE C!
0x21 CODE C@
0x22 CODE PC!
0x23 CODE PC@
0x24 CODE I
0x25 CODE I'
0x26 CODE J
0x27 CODE BYE
0x28 CODE ABORT
0x29 CODE QUIT
0x2a CODE S=
0x2b CODE CMP
0x2c CODE _find
0x2d CODE 1+
0x2e CODE 1-
0x29 CODE RSHIFT
0x30 CODE LSHIFT
0x31 CODE TICKS
0x32 CODE ROT>
0x33 CODE |L
0x34 CODE |M
353 LOAD ( xcomp core )
: (key?) 0 PC@ 1 ;
: EFS@
    1 3 PC! ( read )
    |M 3 PC! 3 PC! ( blkid )
    BLK( |M 3 PC! 3 PC! ( dest )
;
: EFS!
    2 3 PC! ( write )
    |M 3 PC! 3 PC! ( blkid )
    BLK( |M 3 PC! 3 PC! ( dest )
;
( fork between stage and forth begins here )
