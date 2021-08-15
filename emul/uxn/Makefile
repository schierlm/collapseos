TARGETS = uxncli forth.rom
OBJS = uxn/uxn.o
MYCFLAGS = -std=c89 $(CFLAGS)
CDIR = ../../cvm
STAGE = $(CDIR)/stage
BLKFS = $(CDIR)/blkfs

.PHONY: all
all: $(TARGETS)

%.o: %.c
	$(CC) -c $(MYCFLAGS) $< -o $@

uxncli: $(OBJS)
	$(CC) $(MYCFLAGS) uxn/uxncli.c $(OBJS) -o $@

forth.rom: xcomp.fs $(STAGE) $(BLKFS)
	$(CDIR)/stage < xcomp.fs > $@

$(BLKFS): $(STAGE)

$(STAGE):
	$(MAKE) -C $(CDIR) all

.PHONY: emul
emul: $(TARGETS)
	./uxncli forth.rom

.PHONY: clean
clean:
	rm -f $(TARGETS) $(OBJS)
