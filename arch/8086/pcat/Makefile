# See /doc/hw/8086/pcat.txt
TARGET = disk.bin
BASE = ../../..
CDIR = $(BASE)/cvm
BLKPACK = $(BASE)/tools/blkpack
STAGE = $(CDIR)/stage
EMUL = $(BASE)/emul/8086/pcat

.PHONY: all
all: $(TARGET) 
os.bin: xcomp.fs $(STAGE) blkfs
	$(STAGE) blkfs < xcomp.fs > $@

$(BLKPACK):
	$(MAKE) -C $(BASE)/tools

blkfs: $(BLKPACK) blk.fs
	cat $(BASE)/blk.fs blk.fs | $(BLKPACK) > $@

$(STAGE):
	$(MAKE) -C $(CDIR) stage

mbr.bin: mbr.fs $(STAGE) blkfs
	cat mbr.fs | $(STAGE) blkfs > $@

$(TARGET): mbr.bin os.bin
	cat mbr.bin os.bin > $@
	dd if=blkfs of=$@ bs=512 seek=16

$(EMUL):
	$(MAKE) -C $(BASE)/emul/8086

.PHONY: emul
emul: $(TARGET) $(EMUL)
	$(EMUL) $(TARGET)

.PHONY: clean
clean:
	rm -f *.bin blkfs
