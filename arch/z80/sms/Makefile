# See /doc/hw/z80/sms.txt
TARGET = os.sms
BASE = ../../..
STAGE = $(BASE)/cvm/stage
EMUL = $(BASE)/emul/z80/sms
XCOMP = xcomp.fs

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
	$(EMUL) $(TARGET)
	
.PHONY: clean
clean:
	rm -f $(TARGET)
