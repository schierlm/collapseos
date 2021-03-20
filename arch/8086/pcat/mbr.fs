20 LOAD ( 8086 asm )
HERE ORG ! 0x7c00 BIN( ! ( BIOS loads boot bin at 0x7c00 )
JMPs, L1 FWRs ( start )
ORG @ 0x25 + H ! ( bypass BPB )
L1 FSET ( start )
CLI, CLD, AX 0x800 MOVxI, DS AX MOVsx, ES AX MOVsx,
SS AX MOVsx, DX PUSHx, ( will be popped by OS ) STI,
AH 2 MOVri, DH 0 MOVri, CH 0 MOVri, CL 2 MOVri, AL 15 MOVri,
BX 0 MOVxI, 0x13 INT, ( read sectors 2-15 of boot floppy )
( TODO: reading 12 sectors like this probably doesn't work
  on real vintage PC/AT with floppy. Make this more robust. )
0x800 0 JMPf,
ORG @ 0x1fe + H ! 0x55 C, 0xaa C,
