.equ	BAS_RAMEND	0xa098 ; (41112)
; BASIC shell records the SP on init, 
; the storage addr+6 is BAS_RAMEND 

.equ 	USER_CODE 	@+527 ; 41639

.org	USER_CODE	; overrideable

.equ	IYBAS		23610

.equ	MMAP_START	0xc000
.equ 	RAMEND 		0xffff
.equ	MMAP_LEN	RAMEND-MMAP_START+1

.equ	TAP_RAMEND	BAS_RAMEND+265
; for tapeutil.bin

.equ	strncmp 	0x5f00
.equ	upcase 		@+3 ; 0x5f03
.equ	findchar 	@+3 ; 0x5f06
.equ	parseHex 	@+3 ; 0x5f09
.equ 	parseDecimal 	@+3 ; 0x5f0c
.equ	blkSel 		@+3 ; 0x5f0f
.equ	blkSet 		@+3 ; 0x5f12
.equ	fsFindFN 	@+3 ; 0x5f15
.equ	fsOpen 		@+3 ; 0x5f18
.equ	fsGetB	 	@+3 ; 0x5f1b
.equ	fsPutB 		@+3 ; 0x5f1e
.equ	fsSetSize 	@+3 ; 0x5f21
.equ	fsOn 		@+3 ; 0x5f24
.equ	fsIter 		@+3 ; 0x5f27
.equ	fsAlloc 	@+3 ; 0x5f2a
.equ	fsHandle 	@+3 ; 0x5f2d
.equ	fsblkTell 	@+3 ; 0x5f30
.equ	printstr 	@+3 ; 0x5f33
.equ	printnstr 	@+3 ; 0x5f36
.equ	printcrlf 	@+3 ; 0x5f39
.equ	stdioPutC 	@+3 ; 0x5f3c
.equ	stdioGetC 	@+3 ; 0x5f3f
.equ	stdioReadLine 	@+3 ; 0x5f42
.equ	_blkGetB 	@+3 ; 0x5f45
.equ	_blkPutB 	@+3 ; 0x5f48
.equ	_blkSeek 	@+3 ; 0x5f4b
.equ	_blkTell 	@+3 ; 0x5f4e
