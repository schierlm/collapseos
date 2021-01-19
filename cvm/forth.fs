: COLS 80 ; : LINES 32 ;
: CURSOR! ( new old -- )
    DROP COLS /MOD 6 PC! ( y ) 5 PC! ( x ) ;
: CELL! ( c pos -- ) 0 CURSOR! 0 PC! ;

402 403 LOADR ( Grid )
390 LOAD  ( xcomp core high )
(entry) _
( Update LATEST )
PC ORG @ 8 + !
," BLK$ ' EFS@ ' BLK@* **! ' EFS! ' BLK!* **! GRID$ " EOT,
ORG @ |M 2 PC! 2 PC!
H@ |M 2 PC! 2 PC!

