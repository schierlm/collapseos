ARCHM XCOMP Z80A
2 CONSTS $0238 COM_DRV_ADDR $3000 DEST_ADDR
0 VALUE L4
\ We process the $20 exception by pre-putting a mask in the
\ (HL) we're going to write to. If it wasn't a $20, we put a
\ $ff mask. If it was a $20, we put a $7f mask.
: @GET,
  A $03 LD, ( @GET )
  DE COM_DRV_ADDR LD,
  $28 RST, FJR CNZ JR, [TO] L2 ( maybeerror )
  A A OR,
  CZ RET, ( Sending a straight NULL ends the comm. ) ;
: @PUT, ( @PUT that char back )
  C A LD,
  A $04 LD, ( @PUT )
  $28 RST, FJR CNZ JR, [TO] L3 ( error )
  A C LD, ;
0 XSTART
HL DEST_ADDR LD,
BEGIN,
  A $ff LD, (HL) A LD, ( default mask )
  LSET L1 ( loop2 ) @GET, @PUT,
  A $20 CP, FJR CZ JR, TO L4 ( escapechar )
  ( not an escape char, just apply the mask and write )
  A (HL) AND, (HL) A LD,
  HL INC,
BR JR,
L4 FMARK ( escapechar, adjust by setting (hl) to $7f )
7 (HL) RES, L1 BR JR, ( loop2 )
L2 FMARK ( maybeerror, was it an error? )
A A OR, L1 BR CZ JR, ( loop2, not an error )
L3 FMARK ( error )
C A LD, ( error code from @GET/@PUT )
A $1a LD, ( @ERROR ) $28 RST, RET,
