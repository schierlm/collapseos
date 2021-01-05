# See /doc/hw/z80/z80mbc2.txt
TARGET = os.bin
BASEDIR = ../../..
CDIR = $(BASEDIR)/cvm
STAGE = $(CDIR)/stage

.PHONY: all
all: $(TARGET) 
$(TARGET): xcomp.fs $(STAGE)
	cat xcomp.fs | $(STAGE) > $@

$(STAGE):
	$(MAKE) -C $(CDIR) stage

.PHONY: clean
clean:
	rm -f *.bin blkfs
