: COLS $03 PC@ ; : LINES $04 PC@ ;
: CURSOR! ( new old -- )
    DROP COLS /MOD 6 PC! ( y ) 5 PC! ( x ) ;
: CELL! ( c pos -- ) 0 CURSOR! 0 PC! ;
: NEWLN ( ln -- ln ) 1+ DUP LINES = IF 1- 0 7 PC! THEN ;
GRIDSUB
: INITG INIT GRID$ ;
XWRAP INITG
