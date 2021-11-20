TARGETS = stage cos-serial cos-grid
BASEDIR = ..
BLKFS = blkfs
BLK_SRCS = $(BASEDIR)/blk.fs cvm.fs
TOCLEAN = *.o grid.bin
include ../common.mk

OBJS = vm.o

MYCFLAGS = -std=c89 $(CFLAGS)

TEST_COMMAND = 120 LOAD 290 296 LOADR

.SUFFIXES: .c .o
.c.o:
	$(CC) -c $(MYCFLAGS) $< -o $@

stage: stage.c $(OBJS) blkfs
	$(CC) $(MYCFLAGS) \
		-DFBIN_PATH=\"`pwd`/serial.bin\" \
		stage.c $(OBJS) $(LDFLAGS) -o $@

cos-serial: cos-serial.c $(OBJS) blkfs
	$(CC) $(MYCFLAGS) \
		-DFBIN_PATH=\"`pwd`/serial.bin\" \
		cos-serial.c $(OBJS) $(LDFLAGS) -o $@

grid.bin: stage common.fs grid.fs blkfs
	cat common.fs grid.fs | ./stage > $@

cos-grid: cos-grid.c $(OBJS) grid.bin blkfs
	$(CC) $(MYCFLAGS) \
		-DFBIN_PATH=\"`pwd`/grid.bin\" \
		cos-grid.c $(OBJS) $(LDFLAGS) -lcurses -o $@

.PHONY: test
test: cos-serial blkfs
	echo $(TEST_COMMAND) | ./cos-serial

.PHONY: updatebootstrap
updatebootstrap: stage common.fs serial.fs 
	cat common.fs serial.fs | ./stage > new.bin
	mv new.bin serial.bin
