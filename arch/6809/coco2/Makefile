TARGET = os.bin
BASE = ../../..
BLKPACK = $(BASE)/tools/blkpack
STAGE = $(BASE)/cvm/stage

.PHONY: all
all: $(TARGET) blkfs
$(TARGET): xcomp.fs $(STAGE) blkfs
	$(STAGE) blkfs < xcomp.fs > $@

$(BLKPACK):
	$(MAKE) -C ../tools

blkfs: $(BLKPACK) $(BASE)/blk.fs blk.fs
	cat $(BASE)/blk.fs blk.fs | $(BLKPACK) > $@

$(STAGE):
	$(MAKE) -C $(BASE)/cvm stage

.PHONY: clean
clean:
	rm -f *.bin blkfs

