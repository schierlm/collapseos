8086A XCOMPL 8086M HALC
HERE TO ORG $7c00 TO BIN( ( BIOS loads boot bin at $7c00 )
HERE $200 0 FILL
FJR JRi, TO L1 ( start )
ORG $25 + *TO HERE ( bypass BPB ) L1 FMARK ( start )
CLI, CLD, AX $800 MOVxI,
DS AX MOVsx, ES AX MOVsx, SS AX MOVsx,
SP $ffff MOVxI, DX PUSHx, ( will be popped by OS ) STI,
AH 2 MOVri, DH 0 MOVri, CH 0 MOVri, CL 2 MOVri, AL 15 MOVri,
BX 0 MOVxI, $13 INT, \ read sectors 2-15 of boot floppy
\ TODO: reading 12 sectors like this probably doesn't work
\ on real vintage PC/AT with floppy. Make this more robust.
$800 0 JMPf,
ORG $1fe + *TO HERE $55aa M,
