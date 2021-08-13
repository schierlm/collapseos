Z80A
3 VALUES COM_DRV_ADDR $0238 DEST_ADDR $3000 L4 0
\ We process the $20 exception by pre-putting a mask in the
\ (HL) we're going to write to. If it wasn't a $20, we put a
\ $ff mask. If it was a $20, we put a $7f mask.
: @GET,
  A $03 LDri, ( @GET )
  DE COM_DRV_ADDR LDdi,
  $28 RST, JRNZ, FWR L2 ( maybeerror )
  A ORr,
  CZ RETc, ( Sending a straight NULL ends the comm. ) ;
: @PUT, ( @PUT that char back )
  C A LDrr,
  A $04 LDri, ( @PUT )
  $28 RST, JRNZ, FWR L3 ( error )
  A C LDrr, ;
HERE TO ORG
HL DEST_ADDR LDdi,
BEGIN,
  A $ff LDri, (HL) A LDrr, ( default mask )
  BSET L1 ( loop2 ) @GET, @PUT,
  $20 CPi, JRZ, FWR L4 ( escapechar )
  ( not an escape char, just apply the mask and write )
  (HL) ANDr, (HL) A LDrr,
  HL INCd,
JR, AGAIN,
FSET L4 ( escapechar, adjust by setting (hl) to $7f )
7 (HL) RES, JR, BWR L1 ( loop2 )
FSET L2 ( maybeerror, was it an error? )
A ORr, JRZ, BWR L1 ( loop2, not an error )
FSET L3 ( error )
C A LDrr, ( error code from @GET/@PUT )
A $1a LDri, ( @ERROR ) $28 RST, RET,
