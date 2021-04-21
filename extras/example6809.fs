( Usage: cos-serial < example6809.fs )
50 LOAD ( 6809 asm )
471 480 LOADR
0x100 allotmem
cpudump
16 MEM @ DUMP
42 # LDA, 2 # ADDA, SYNC,
run
cpudump
16 MEM @ DUMP
