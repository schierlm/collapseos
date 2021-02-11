TARGET = os.bin
BASE = ../../..
STAGE = $(BASE)/cvm/stage

.PHONY: all
all: $(TARGET) 
$(TARGET): xcomp.fs $(STAGE)
	cat xcomp.fs | $(STAGE) > $@

$(STAGE):
	$(MAKE) -C $(BASE)/cvm stage

.PHONY: clean
clean:
	rm -f *.bin blkfs

