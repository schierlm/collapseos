; *** JUMP TABLE ***
; include the addresses of the actual table into user.h for userspace utilities

	jp	strncmp 
	jp	upcase
	jp	findchar
	jp	parseHex
	jp 	parseDecimal
	jp	blkSel
	jp	blkSet
	jp	fsFindFN
	jp	fsOpen
	jp	fsGetB
	jp	fsPutB
	jp	fsSetSize
	jp	fsOn
	jp	fsIter
	jp	fsAlloc
	jp	fsHandle
	jp	fsblkTell
	jp	printstr
	jp	printnstr
	jp	printcrlf
	jp	stdioPutC
	jp	stdioGetC
	jp	stdioReadLine
	jp	_blkGetB
	jp	_blkPutB
	jp	_blkSeek
	jp	_blkTell
