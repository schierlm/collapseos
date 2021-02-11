# See /doc/hw/z80/trs80-4p.txt
TARGET = os.bin
BASE = ../../..
BLKPACK = $(BASE)/tools/blkpack
STAGE = $(BASE)/cvm/stage
EDIR = $(BASE)/emul/z80
EMUL = $(EDIR)/trs80

.PHONY: all
all: $(TARGET) 
$(TARGET): xcomp.fs $(STAGE) blkfs
	cat xcomp.fs | $(STAGE) blkfs > $@

$(BLKPACK):
	$(MAKE) -C ../tools

blkfs: blk.fs $(BLKPACK)
	cat $(BASE)/blk.fs blk.fs | $(BLKPACK) > $@

$(STAGE):
	$(MAKE) -C $(BASE)/cvm stage

$(EMUL):
	$(MAKE) -C $(EDIR)

.PHONY: emul
emul: $(EMUL) $(TARGET)
	$(EMUL) -f blkfs $(TARGET)

.PHONY: clean
clean:
	rm -f *.bin blkfs
