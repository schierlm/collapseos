# See /doc/hw/z80/rc2014.txt for details
TARGET = os.bin
BASE = ../../..
CDIR = $(BASE)/cvm
EDIR = $(BASE)/emul/z80
STAGE = $(CDIR)/stage
BLKPACK = $(BASE)/tools/blkpack
EMUL = $(EDIR)/rc2014

.PHONY: all
all: $(TARGET) 
$(TARGET): xcomp.fs $(STAGE) blkfs
	$(STAGE) blkfs < xcomp.fs > $@

$(BLKPACK):
	$(MAKE) -C ../tools

blkfs: $(BLKPACK)
	cat $(BASE)/blk.fs blk.fs | $(BLKPACK) > $@

$(STAGE):
	$(MAKE) -C $(CDIR) stage

$(EMUL):
	$(MAKE) -C $(EDIR)

.PHONY: emul
emul: $(EMUL) $(TARGET)
	$(EMUL) $(TARGET)

.PHONY: clean
clean:
	rm -f *.bin blkfs
