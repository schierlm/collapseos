TARGETS = forth stage
OBJS = vm.o
BLKPACK = ../tools/blkpack

.PHONY: all
all: $(TARGETS)

$(BLKPACK):
	$(MAKE) -C ../tools blkpack 

stage: stage.c $(OBJS) blkfs
	$(CC) -DFBIN_PATH=\"`pwd`/stage.bin\" -DBLKFS_PATH=\"`pwd`/blkfs\" stage.c $(OBJS) -o $@

blkfs: ../blk.fs $(BLKPACK)
	$(BLKPACK) < ../blk.fs > $@

forth.bin: stage common.fs forth.fs blkfs
	cat common.fs forth.fs | ./stage > $@

forth: forth.c $(OBJS) forth.bin blkfs
	$(CC) -DFBIN_PATH=\"`pwd`/forth.bin\" -DBLKFS_PATH=\"`pwd`/blkfs\" forth.c $(OBJS) -lcurses -o $@

.PHONY: updatebootstrap
updatebootstrap: stage common.fs stage.fs 
	cat common.fs stage.fs | ./stage > new.bin
	mv new.bin stage.bin

.PHONY: clean
clean:
	rm -f $(TARGETS) *.o forth.bin blkfs
