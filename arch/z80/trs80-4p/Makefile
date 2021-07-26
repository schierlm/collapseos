# See /doc/hw/z80/trs80-4p.txt
TARGETS = os.bin recv.bin boot.bin
BASE = ../../..
STAGE = $(BASE)/cvm/stage
EMUL = $(EDIR)/trs80

.PHONY: all
all: $(TARGETS)
os.bin: xcomp.fs $(STAGE) 
	$(STAGE) < xcomp.fs > $@

recv.bin: recv.fs $(STAGE)
	$(STAGE) < recv.fs > $@

boot.bin: boot.fs $(STAGE)
	$(STAGE) < boot.fs > $@

$(STAGE):
	$(MAKE) -C $(BASE)/cvm stage

.PHONY: clean
clean:
	rm -f *.bin 
