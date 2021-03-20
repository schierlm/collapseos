: (emit) 0 PC! ;
236 239 LOADR ( forth high )
(entry) _
( Update LATEST )
PC ORG @ 8 + !
," BLK$ "
," ' EFS@ ' BLK@* **! "
," ' EFS! ' BLK!* **! "
EOT,
