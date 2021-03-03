# See /doc/hw/z80/rc2014.txt for details
TARGET = os.bin
BASE = ../../..
CDIR = $(BASE)/cvm
EDIR = $(BASE)/emul/z80
STAGE = $(CDIR)/stage
BLKPACK = $(BASE)/tools/blkpack
EMUL = $(EDIR)/rc2014
XCOMP = xcomp.fs
EMUL_ARGS =
# for SIO, XCOMP=xcompsio.fs and EMUL_ARGS=-s

.PHONY: all
all: $(TARGET) 
$(TARGET): $(XCOMP) $(STAGE) blkfs
	$(STAGE) blkfs < $(XCOMP) > $@

$(BLKPACK):
	$(MAKE) -C ../tools

blkfs: $(BLKPACK) blk.fs
	cat $(BASE)/blk.fs blk.fs | $(BLKPACK) > $@

$(STAGE):
	$(MAKE) -C $(CDIR) stage

$(EMUL):
	$(MAKE) -C $(EDIR) rc2014

.PHONY: emul
emul: $(EMUL) $(TARGET)
	$(EMUL) $(EMUL_ARGS) $(TARGET)

.PHONY: clean
clean:
	rm -f *.bin blkfs
