0xbf00 CONSTANT RS_ADDR
0xbffa CONSTANT PS_ADDR
RS_ADDR 0xb0 - CONSTANT SYSVARS
0x8000 CONSTANT HERESTART
SYSVARS 0xa0 + CONSTANT LCD_MEM
SYSVARS 0xa2 + CONSTANT GRID_MEM
SYSVARS 0xa5 + CONSTANT KBD_MEM
0x01 CONSTANT KBD_PORT
5 LOAD  ( z80 assembler )
262 263 LOADR ( font compiler )
280 LOAD  ( boot.z80.decl )
200 205 LOADR ( xcomp )

( TI-84+ requires specific code at specific offsets which
  come in conflict with Collapse OS' stable ABI. We thus
  offset the binary by 0x100, which is our minimum possible
  increment and fill the TI stuff with the code below. )

0x5a JP, 0x15 ALLOT0 ( 0x18 )
0x5a JP, ( reboot ) 0x1d ALLOT0 ( 0x38 )
( handleInterrupt )
DI,
AF PUSH,
    ( did we push the ON button? )
    0x04 ( PORT_INT_TRIG ) INAi,
    0 ( INT_TRIG_ON ) A BIT,
    IFNZ,
        ( yes? acknowledge and boot )
        0x03 ( PORT_INT_MASK ) INAi,
        0x00 ( INT_MASK_ON ) A RES, ( ack interrupt )
        0x03 ( PORT_INT_MASK ) OUTiA,
        AF POP,
        EI,
        0x100 JP,
    THEN,
AF POP,
EI,
RETI,

0x03 ALLOT0 ( 0x53 )
0x5a JP, ( 0x56 ) 0xff C, 0xa5 C, 0xff C, ( 0x5a )
( boot )
DI,
    IM1,
    ( enable the ON key interrupt )
    0x03 ( PORT_INT_MASK ) INAi,
    0x00 ( INT_MASK_ON ) A SET,
    0x03 ( PORT_INT_MASK ) OUTiA,
    A 0x80 LDri,
    0x07 ( PORT_BANKB ) OUTiA,
EI,
( LCD off )
A 0x02 ( LCD_CMD_DISABLE ) LDri,
0x10 ( LCD_PORT_CMD ) OUTiA,
HALT,

0x95 ALLOT0 ( 0x100 )
( All set, carry on! )

0x100 BIN( !
281 303 LOADR ( boot.z80 )
210 231 LOADR ( forth core low )
CREATE ~FNT CPFNT3x5
350 353 LOADR ( LCD )
240 241 LOADR ( Grid )
355 359 LOADR ( KBD )
236 239 LOADR ( forth core high )
XWRAP" LCD$ KBD$ GRID$ "
ORG @ 0x100 - ORG ! ( for staging output )
