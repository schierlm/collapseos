# See /doc/hw/z80/sms.txt
TARGET = os.sms
BASE = ../../..
STAGE = $(BASE)/cvm/stage
EMUL = $(BASE)/emul/z80/sms
XCOMP = xcomp.fs
EMUL_ARGS = -c../../../cvm/blkfs # -k for KBD

.PHONY: all
all: $(TARGET) 
$(TARGET): $(XCOMP) $(STAGE) 
	$(STAGE) < $(XCOMP) > $@

$(STAGE):
	$(MAKE) -C $(BASE)/cvm stage

$(EMUL):
	$(MAKE) -C ${@:%/sms=%}

.PHONY: emul
emul: $(EMUL) $(TARGET)
	$(EMUL) $(EMUL_ARGS) $(TARGET)
	
.PHONY: clean
clean:
	rm -f $(TARGET)
