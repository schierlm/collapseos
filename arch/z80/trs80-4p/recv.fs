5 LOAD ( z80 asm )
$0238 VALUE COM_DRV_ADDR
$3000 VALUE DEST_ADDR
( We process the $20 exception by pre-putting a mask in the
  (HL) we're going to write to. If it wasn't a $20, we put a
  $ff mask. If it was a $20, we put a $7f mask. )
: @GET,
  A $03 LDri, ( @GET )
  DE COM_DRV_ADDR LDdi,
  $28 RST, JRNZ, L2 FWR ( maybeerror )
  A ORr,
  CZ RETc, ( Sending a straight NULL ends the comm. ) ;
: @PUT, ( @PUT that char back )
  C A LDrr,
  A $04 LDri, ( @PUT )
  $28 RST, JRNZ, L3 FWR ( error )
  A C LDrr, ;
HERE TO ORG
HL DEST_ADDR LDdi,
BEGIN,
  A $ff LDri, (HL) A LDrr, ( default mask )
  L1 BSET ( loop2 ) @GET, @PUT,
  $20 CPi, JRZ, L4 FWR ( escapechar )
  ( not an escape char, just apply the mask and write )
  (HL) ANDr, (HL) A LDrr,
  HL INCd,
JR, AGAIN,
L4 FSET ( escapechar, adjust by setting (hl) to $7f )
7 (HL) RES, JR, L1 BWR ( loop2 )
L2 FSET ( maybeerror, was it an error? )
A ORr, JRZ, L1 BWR ( loop2, not an error )
L3 FSET ( error )
C A LDrr, ( error code from @GET/@PUT )
A $1a LDri, ( @ERROR ) $28 RST, RET,
