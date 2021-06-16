( Usage: cos-serial < example6809.fs )
50 LOAD ( 6809 asm )
470 486 LOADR
0x40 CONSTANT SZ
SZ allotmem
SZ S REG T!
MEM @ *TO HERE
0x0804 # LDD, MUL, SYNC,
16 MEM @ DUMP
run
cpudump
SZ MEM @ DUMP
