4 VALUES RS_ADDR $bf00 PS_ADDR $bffa HERESTART $8000
         KBD_PORT $01
RS_ADDR $90 - VALUE SYSVARS
SYSVARS $80 + VALUE LCD_MEM
SYSVARS $82 + VALUE GRID_MEM
SYSVARS $85 + VALUE KBD_MEM
120 LOAD \ nC, for KBD driver
ARCHM Z80A XCOMPL FONTC Z80H XCOMPH

\ TI-84+ requires specific code at specific offsets which
\ come in conflict with Collapse OS' stable ABI. We thus
\ offset the binary by $100, which is our minimum possible
\ increment and fill the TI stuff with the code below.

$5a JP, $15 ALLOT0 ( $18 )
$5a JP, ( reboot ) $1d ALLOT0 ( $38 )
( handleInterrupt )
DI,
AF PUSH,
    ( did we push the ON button? )
    $04 ( PORT_INT_TRIG ) INAi,
    0 ( INT_TRIG_ON ) A BIT,
    IFNZ,
        ( yes? acknowledge and boot )
        $03 ( PORT_INT_MASK ) INAi,
        $00 ( INT_MASK_ON ) A RES, ( ack interrupt )
        $03 ( PORT_INT_MASK ) OUTiA,
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
    $03 ( PORT_INT_MASK ) INAi,
    $00 ( INT_MASK_ON ) A SET,
    $03 ( PORT_INT_MASK ) OUTiA,
    A $80 LDri,
    $07 ( PORT_BANKB ) OUTiA,
EI,
( LCD off )
A $02 ( LCD_CMD_DISABLE ) LDri,
$10 ( LCD_PORT_CMD ) OUTiA,
HALT,

$95 ALLOT0 ( $100 )
( All set, carry on! )

$100 TO BIN(
Z80C COREL Z80H ASMH
CREATE ~FNT CPFNT3x5
370 373 LOADR ( LCD )
GRIDSUB
375 379 LOADR ( KBD )
: INIT LCD$ KBD$ GRID$ ;
XWRAP
ORG $100 - TO ORG
