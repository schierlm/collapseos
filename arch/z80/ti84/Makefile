# See /doc/hw/z80/ti84.txt
TARGET = os.bin
BASE = ../../..
CDIR = $(BASE)/cvm
STAGE = $(CDIR)/stage
BLKPACK = $(BASE)/tools/blkpack
EMUL = $(BASE)/emul/z80/ti84
MKTIUPGRADE = mktiupgrade

.PHONY: all
all: $(TARGET) 
$(TARGET): xcomp.fs $(STAGE) blkfs
	$(STAGE) blkfs < xcomp.fs > $@

$(BLKPACK):
	$(MAKE) -C ../tools

blkfs: $(BLKPACK) $(BASE)/blk.fs blk.fs
	cat $(BASE)/blk.fs blk.fs | $(BLKPACK) > $@

$(STAGE):
	$(MAKE) -C $(CDIR) stage

$(EMUL):
	$(MAKE) -C ${@:%/ti84=%}

.PHONY: emul
emul: $(EMUL) $(TARGET)
	$(EMUL) $(TARGET)

os.rom: $(TARGET)
	dd if=$(TARGET) bs=1M of=$@ conv=sync

os.8xu: os.rom
	$(MKTIUPGRADE) -p -k keys/0A.key -d TI-84+ os.rom $@ 00

.PHONY: send
send: os.8xu
	tilp -n --calc ti84+ --cable DirectLink os.8xu

.PHONY: clean
clean:
	rm -f *.bin *.rom *.8xu blkfs
