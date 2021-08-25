TARGETS = uxncli forth.rom
OBJS = uxn/uxn.o
MYCFLAGS = -std=c89 $(CFLAGS)
ROOT = ../..
CDIR = $(ROOT)/cvm
STAGE = $(CDIR)/stage
TOOLDIR = $(ROOT)/tools
BLKPACK = $(TOOLDIR)/blkpack
BLK_SRCS = $(ROOT)/blk.fs uxn.fs

.PHONY: all
all: $(TARGETS)

%.o: %.c
	$(CC) -c $(MYCFLAGS) $< -o $@

uxncli: $(OBJS)
	$(CC) $(MYCFLAGS) uxn/uxncli.c $(OBJS) -o $@

$(BLKPACK):
	$(MAKE) -C $(TOOLDIR) blkpack 

$(STAGE):
	$(MAKE) -C $(CDIR) all

blkfs: $(BLK_SRCS) $(BLKPACK)
	cat $(BLK_SRCS) | $(BLKPACK) > $@

forth.rom: xcomp.fs $(STAGE) blkfs
	$(CDIR)/stage blkfs < xcomp.fs > $@

.PHONY: emul
emul: $(TARGETS)
	./uxncli forth.rom

.PHONY: clean
clean:
	rm -f $(TARGETS) $(OBJS)
