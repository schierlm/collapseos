TARGETS = stage cos-serial cos-grid
BASEDIR = ..
BLKFS = blkfs
# By default, extras are included in the blkfs. If you don't want to comment
# them in the line below.
BLK_SRCS = $(BASEDIR)/blk.fs \
           cvm.fs \
           $(BASEDIR)/extras/emul6809.fs \
           $(BASEDIR)/extras/tests.fs
TOCLEAN = *.o grid.bin
include ../common.mk

OBJS = vm.o
MYCFLAGS = -std=c89 $(CFLAGS)
# depends on extra blk order/size. change when appropriate.
TEST_COMMAND = 120 LOAD 340 346 LOADR

%.o: %.c
	$(CC) -c $(MYCFLAGS) $< -o $@

stage: stage.c $(OBJS) blkfs
	$(CC) $(MYCFLAGS) -DFBIN_PATH=\"`pwd`/serial.bin\" -DBLKFS_PATH=\"blkfs\" stage.c $(OBJS) -o $@

cos-serial: cos-serial.c $(OBJS) blkfs
	$(CC) $(MYCFLAGS) -I. -DFBIN_PATH=\"`pwd`/serial.bin\" -DBLKFS_PATH=\"`pwd`/blkfs\" cos-serial.c $(OBJS) -o $@

grid.bin: stage common.fs grid.fs blkfs
	cat common.fs grid.fs | ./stage > $@

cos-grid: cos-grid.c $(OBJS) grid.bin blkfs
	$(CC) $(MYCFLAGS) -DFBIN_PATH=\"`pwd`/grid.bin\" -DBLKFS_PATH=\"`pwd`/blkfs\" cos-grid.c $(OBJS) -lcurses -o $@

.PHONY: test
test: cos-serial blkfs
	echo $(TEST_COMMAND) | ./cos-serial

.PHONY: updatebootstrap
updatebootstrap: stage common.fs serial.fs 
	cat common.fs serial.fs | ./stage > new.bin
	mv new.bin serial.bin
