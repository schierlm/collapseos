ARCHM XCOMP Z80A
2 CONSTS $0238 COM_DRV_ADDR $3000 DEST_ADDR
0 VALUE L4
\ We process the $20 exception by pre-putting a mask in the
\ (HL) we're going to write to. If it wasn't a $20, we put a
\ $ff mask. If it was a $20, we put a $7f mask.
: @GET,
  A $03 i) ld, ( @GET )
  DE COM_DRV_ADDR i) ld,
  $28 rst, FJR CNZ jrc, TO L2 ( maybeerror )
  A A or,
  CZ retc, ( Sending a straight NULL ends the comm. ) ;
: @PUT, ( @PUT that char back )
  C A ld,
  A $04 i) ld, ( @PUT )
  $28 rst, FJR CNZ jrc, TO L3 ( error )
  A C ld, ;
0 XSTART
HL DEST_ADDR i) ld,
BEGIN,
  A $ff i) ld, (HL) A ld, ( default mask )
  LSET L1 ( loop2 ) @GET, @PUT,
  A $20 i) cp, FJR CZ jrc, TO L4 ( escapechar )
  \ not an escape char, just apply the mask and write
  A (HL) and, (HL) A ld,
  HL inc,
BR jr,
L4 FMARK \ escapechar, adjust by setting (hl) to $7f
(HL) 7 res, L1 BR jr, ( loop2 )
L2 FMARK ( maybeerror, was it an error? )
A A or, L1 BR CZ jrc, ( loop2, not an error )
L3 FMARK ( error )
C A ld, ( error code from @GET/@PUT )
A $1a i) ld, ( @ERROR ) $28 rst, ret,
