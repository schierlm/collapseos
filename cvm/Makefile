TARGETS = stage cos-serial cos-grid
OBJS = vm.o
BLKPACK = ../tools/blkpack
# By default, extras are included in the blkfs. If you don't want to comment
# them in the line below.
BLK_SRCS = ../blk.fs ../extras/emul6809.fs

.PHONY: all
all: $(TARGETS)

$(BLKPACK):
	$(MAKE) -C ../tools blkpack 

blkfs: $(BLK_SRCS) $(BLKPACK)
	cat $(BLK_SRCS) | $(BLKPACK) > $@

stage: stage.c $(OBJS) blkfs
	$(CC) -DFBIN_PATH=\"`pwd`/serial.bin\" -DBLKFS_PATH=\"`pwd`/blkfs\" stage.c $(OBJS) -o $@

cos-serial: cos-serial.c $(OBJS) blkfs
	$(CC) -DFBIN_PATH=\"`pwd`/serial.bin\" -DBLKFS_PATH=\"`pwd`/blkfs\" cos-serial.c $(OBJS) -o $@

grid.bin: stage common.fs grid.fs blkfs
	cat common.fs grid.fs | ./stage > $@

cos-grid: cos-grid.c $(OBJS) grid.bin blkfs
	$(CC) -DFBIN_PATH=\"`pwd`/grid.bin\" -DBLKFS_PATH=\"`pwd`/blkfs\" cos-grid.c $(OBJS) -lcurses -o $@

.PHONY: updatebootstrap
updatebootstrap: stage common.fs serial.fs 
	cat common.fs serial.fs | ./stage > new.bin
	mv new.bin serial.bin

.PHONY: clean
clean:
	rm -f $(TARGETS) *.o grid.bin blkfs
