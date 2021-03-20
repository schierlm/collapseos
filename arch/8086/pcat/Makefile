# See /doc/hw/8086/pcat.txt
TARGET = disk.bin
BASE = ../../..
CDIR = $(BASE)/cvm
BLKPACK = $(BASE)/tools/blkpack
STAGE = $(CDIR)/stage
EMUL = $(BASE)/emul/8086/pcat

.PHONY: all
all: $(TARGET) 
os.bin: xcomp.fs $(STAGE) 
	$(STAGE) < xcomp.fs > $@

$(STAGE):
	$(MAKE) -C $(CDIR) stage

mbr.bin: mbr.fs $(STAGE)
	$(STAGE) < mbr.fs > $@

$(TARGET): mbr.bin os.bin
	cat mbr.bin os.bin > $@
	dd if=$(CDIR)/blkfs of=$@ bs=512 seek=16

$(EMUL):
	$(MAKE) -C $(BASE)/emul/8086

.PHONY: emul
emul: $(TARGET) $(EMUL)
	$(EMUL) $(TARGET)

.PHONY: clean
clean:
	rm -f *.bin 
