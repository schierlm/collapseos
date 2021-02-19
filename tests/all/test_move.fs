42 C, 43 C, 44 C,
HERE 3 - HERE 3 MOVE
HERE C@ 42 #eq HERE 1+ C@ 43 #eq HERE 2 + C@ 44 #eq
HERE HERE 1+ 3 MOVE ( demonstrate MOVE's problem )
HERE 1+ C@ 42 #eq HERE 2 + C@ 42 #eq HERE 3 + C@ 42 #eq
HERE 3 - HERE 3 MOVE
HERE HERE 1+ 3 MOVE- ( see? better )
HERE 1+ C@ 42 #eq HERE 2 + C@ 43 #eq HERE 3 + C@ 44 #eq

HERE ( ref )
HERE 3 - 3 MOVE,
( ref ) HERE 3 - #eq
HERE 3 - C@ 42 #eq HERE 2 - C@ 43 #eq HERE 1- C@ 44 #eq
