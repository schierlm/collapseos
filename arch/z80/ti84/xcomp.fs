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

$5a JP, $15 ALLOT0 ( $18 )
$5a JP, ( reboot ) $1d ALLOT0 ( $38 )
( handleInterrupt )
DI,
AF PUSH,
    ( did we push the ON button? )
    A $04 ( PORT_INT_TRIG ) IN,
    0 ( INT_TRIG_ON ) A BIT,
    IFNZ,
        ( yes? acknowledge and boot )
        A $03 ( PORT_INT_MASK ) IN,
        $00 ( INT_MASK_ON ) A RES, ( ack interrupt )
        $03 ( PORT_INT_MASK ) A OUT,
        AF POP,
        EI,
        $100 JP,
    THEN,
AF POP,
EI,
RETI,

$03 ALLOT0 ( $53 )
$5a JP, ( $56 ) $ff C, $a5 C, $ff C, ( $5a )
( boot )
DI,
    IM1,
    ( enable the ON key interrupt )
    A $03 ( PORT_INT_MASK ) IN,
    $00 ( INT_MASK_ON ) A SET,
    $03 ( PORT_INT_MASK ) A OUT,
    A $80 LD,
    $07 ( PORT_BANKB ) A OUT,
EI,
( LCD off )
A $02 ( LCD_CMD_DISABLE ) LD,
$10 ( LCD_PORT_CMD ) A OUT,
HALT,

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
