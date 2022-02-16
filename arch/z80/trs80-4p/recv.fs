ARCHM XCOMP Z80A 0 XSTART
2 CONSTS $0238 COM_DRV_ADDR $3000 DEST_ADDR
0 VALUE L4
\ We process the $20 exception by pre-putting a mask in the
\ (HL) we're going to write to. If it wasn't a $20, we put a
\ $ff mask. If it was a $20, we put a $7f mask.
: @GET,
  A $03 LDri, ( @GET )
  DE COM_DRV_ADDR LDdi,
  $28 RST, FJR JRNZ, [TO] L2 ( maybeerror )
  A ORr,
  CZ RETc, ( Sending a straight NULL ends the comm. ) ;
: @PUT, ( @PUT that char back )
  C A LDrr,
  A $04 LDri, ( @PUT )
  $28 RST, FJR JRNZ, [TO] L3 ( error )
  A C LDrr, ;
0 XSTART
HL DEST_ADDR LDdi,
BEGIN,
  A $ff LDri, (HL) A LDrr, ( default mask )
  LSET L1 ( loop2 ) @GET, @PUT,
  $20 CPi, FJR JRZ, TO L4 ( escapechar )
  ( not an escape char, just apply the mask and write )
  (HL) ANDr, (HL) A LDrr,
  HL INCd,
BR JR,
L4 FMARK ( escapechar, adjust by setting (hl) to $7f )
7 (HL) RES, L1 BR JR, ( loop2 )
L2 FMARK ( maybeerror, was it an error? )
A ORr, L1 BR JRZ, ( loop2, not an error )
L3 FMARK ( error )
C A LDrr, ( error code from @GET/@PUT )
A $1a LDri, ( @ERROR ) $28 RST, RET,
