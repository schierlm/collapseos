ASML
560 562 LOADR \ UXN assembler
HERE TO ORG $100 TO BIN(
uxn[ FWR8 L1 JMP ] ( -> main )
BSET L2 ," Hello World!" 0 C,
FSET8 L1 ( <- main ) L2 lit16, ( sa )
uxn[ BEGIN, LDAk $18 DEO $0001 ADD2 LDAk AGAIN, JCN BRK ]
