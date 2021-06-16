: COLS 0x03 PC@ ; : LINES 0x04 PC@ ;
: CURSOR! ( new old -- )
    DROP COLS /MOD 6 PC! ( y ) 5 PC! ( x ) ;
: CELL! ( c pos -- ) 0 CURSOR! 0 PC! ;

240 241 LOADR ( Grid )
: INITG INIT GRID$ ;
236 239 LOADR ( forth high )
XWRAP INITG
