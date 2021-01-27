: (emit) 0 PC! ;
390 LOAD  ( xcomp core high )
(entry) _
( Update LATEST )
PC ORG @ 8 + !
," BLK$ "
," ' EFS@ ' BLK@* **! "
," ' EFS! ' BLK!* **! "
EOT,
ORG @ |M 2 PC! 2 PC!
HERE |M 2 PC! 2 PC!
