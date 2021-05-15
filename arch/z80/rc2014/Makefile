# See /doc/hw/z80/rc2014.txt for details
TARGET = os.bin
BASE = ../../..
CDIR = $(BASE)/cvm
EDIR = $(BASE)/emul/z80
STAGE = $(CDIR)/stage
EMUL = $(EDIR)/rc2014
XCOMP = xcomp.fs
EMUL_ARGS = -c../../../cvm/blkfs
# for SIO, XCOMP=xcompsio.fs and EMUL_ARGS=-s

.PHONY: all
all: $(TARGET) 
$(TARGET): $(XCOMP) $(STAGE)
	$(STAGE) < $(XCOMP) > $@

$(STAGE):
	$(MAKE) -C $(CDIR) stage

$(EMUL):
	$(MAKE) -C $(EDIR) rc2014

.PHONY: emul
emul: $(EMUL) $(TARGET)
	$(EMUL) $(EMUL_ARGS) $(TARGET)

.PHONY: clean
clean:
	rm -f $(TARGET)
