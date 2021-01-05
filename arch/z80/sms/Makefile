# See /doc/hw/z80/sms.txt
TARGET = os.sms
BASE = ../../..
STAGE = $(BASE)/cvm/stage
BLKPACK = $(BASE)/tools/blkpack
EMUL = $(BASE)/emul/z80/sms

.PHONY: all
all: $(TARGET) 
$(TARGET): xcomp.fs $(STAGE) blkfs
	cat xcomp.fs | $(STAGE) blkfs > $@

$(BLKPACK):
	$(MAKE) -C ../tools

blkfs: $(BLKPACK) $(BASE)/blk.fs blk.fs
	cat $(BASE)/blk.fs blk.fs | $(BLKPACK) > $@

$(STAGE):
	$(MAKE) -C $(BASE)/cvm stage

$(EMUL):
	$(MAKE) -C ${@:%/sms=%}

.PHONY: emul
emul: $(EMUL) $(TARGET)
	$(EMUL) $(TARGET)
	
.PHONY: clean
clean:
	rm -f *.sms blkfs
