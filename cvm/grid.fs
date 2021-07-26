: COLS $03 PC@ ; : LINES $04 PC@ ;
: CURSOR! ( new old -- )
    DROP COLS /MOD 6 PC! ( y ) 5 PC! ( x ) ;
: CELL! ( c pos -- ) 0 CURSOR! 0 PC! ;

240 241 LOADR \ Grid
: INITG INIT GRID$ ;
XWRAP INITG
