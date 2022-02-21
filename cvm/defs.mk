TARGETS = stage cos-serial cos-grid
BASEDIR = ..
BLKFS = blkfs
BLK_SRCS = $(BASEDIR)/blk.fs cvm.fs
TOCLEAN = *.o grid.bin

OBJS = vm.o

MYCFLAGS = -std=c89 $(CFLAGS)

TEST_COMMAND = 1 LOAD 290 296 LOADR
