TARGETS = forth pcat
BASEDIR = ../..
BLKFS = blkfs
BLK_SRCS = $(BASEDIR)/blk.fs $(BASEDIR)/arch/8086/blk.fs
OBJS = cpu.o
TOCLEAN = $(OBJS) forth.bin
include $(BASEDIR)/common.mk

forth: forth.c forth.bin $(OBJS) $(BLKFS)
	$(CC) -DFBIN_PATH=\"`pwd`/forth.bin\" -DBLKFS_PATH=\"`pwd`/$(BLKFS)\" forth.c $(OBJS) -lcurses -o $@

forth.bin: xcomp.fs $(STAGE) $(BLKFS)
	$(STAGE) $(BLKFS) < xcomp.fs > $@

pcat: pcat.c $(OBJS) $(BLKFS)
	$(CC) -DBLKFS_PATH=\"`pwd`/$(BLKFS)\" pcat.c $(OBJS) -lcurses -o $@
