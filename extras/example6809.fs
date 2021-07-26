( Usage: cos-serial < example6809.fs )
50 LOAD ( 6809 asm )
530 546 LOADR
$40 CONSTANT SZ
SZ allotmem
SZ S REG T!
MEM @ *TO HERE
$0804 # LDD, MUL, SYNC,
16 MEM @ DUMP
run
cpudump
SZ MEM @ DUMP
