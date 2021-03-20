# See /doc/hw/z80/trs80-4p.txt
TARGET = os.bin
BASE = ../../..
STAGE = $(BASE)/cvm/stage
EDIR = $(BASE)/emul/z80
EMUL = $(EDIR)/trs80

.PHONY: all
all: $(TARGET) recv.bin
$(TARGET): xcomp.fs $(STAGE) 
	$(STAGE) < xcomp.fs > $@

recv.bin: $(STAGE)
	$(STAGE) < recv.fs > $@

$(STAGE):
	$(MAKE) -C $(BASE)/cvm stage

$(EMUL):
	$(MAKE) -C $(EDIR)

.PHONY: emul
emul: $(EMUL) $(TARGET)
	$(EMUL) -f $(BASE)/cvm/blkfs $(TARGET)

.PHONY: clean
clean:
	rm -f *.bin 
