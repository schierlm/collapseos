: COLS 0x03 PC@ ; : LINES 0x04 PC@ ;
: CURSOR! ( new old -- )
    DROP COLS /MOD 6 PC! ( y ) 5 PC! ( x ) ;
: CELL! ( c pos -- ) 0 CURSOR! 0 PC! ;

402 403 LOADR ( Grid )
390 LOAD  ( xcomp core high )
(entry) _
( Update LATEST )
PC ORG @ 8 + !
," BLK$ ' EFS@ ' BLK@* **! ' EFS! ' BLK!* **! GRID$ " EOT,
