4 CONSTS $bf00 RS_ADDR $bffa PS_ADDR $8000 HERESTART
         $01 KBD_PORT
RS_ADDR $90 - VALUE SYSVARS
SYSVARS $80 + VALUE LCD_MEM
SYSVARS $82 + VALUE GRID_MEM
SYSVARS $85 + VALUE KBD_MEM
ARCHM XCOMP FONTC Z80A XCOMPC

\ TI-84+ requires specific code at specific offsets which
\ come in conflict with Collapse OS' boot code. We thus
\ offset the binary by $100, which is our minimum possible
\ increment and fill the TI stuff with the code below.

$5a jp, $15 ALLOT0 ( $18 )
$5a jp, ( reboot ) $1d ALLOT0 ( $38 )
( handleInterrupt )
di,
AF push,
    ( did we push the ON button? )
    A $04 i) ( PORT_INT_TRIG ) in,
    A 0 ( INT_TRIG_ON ) bit,
    IFNZ,
        ( yes? acknowledge and boot )
        A $03 i) ( PORT_INT_MASK ) in,
        A $00 ( INT_MASK_ON ) res, ( ack interrupt )
        $03 i) ( PORT_INT_MASK ) A out,
        AF pop,
        ei,
        $100 jp,
    THEN,
AF pop,
ei,
reti,

$03 ALLOT0 ( $53 )
$5a jp, ( $56 ) $ff C, $a5 C, $ff C, ( $5a )
( boot )
di,
    im1,
    ( enable the ON key interrupt )
    A $03 i) ( PORT_INT_MASK ) in,
    A $00 ( INT_MASK_ON ) set,
    $03 i) ( PORT_INT_MASK ) A out,
    A $80 i) ld,
    $07 i) ( PORT_BANKB ) A out,
ei,
( LCD off )
A $02 i) ( LCD_CMD_DISABLE ) ld,
$10 i) ( LCD_PORT_CMD ) A out,
halt,

$95 ALLOT0 ( $100 )
( All set, carry on! )

$100 XSTART
Z80C COREL
CREATE ~FNT CPFNT3x5
370 373 LOADR ( LCD )
GRIDSUB
375 379 LOADR ( KBD )
: INIT LCD$ KBD$ GRID$ ;
XWRAP
XORG $100 - TO XORG
