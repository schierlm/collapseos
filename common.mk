# Requires TARGETS BASEDIR BLKFS BLK_SRCS TOCLEAN
TOOLSDIR = $(BASEDIR)/tools
BLKPACK = $(TOOLSDIR)/blkpack
CDIR = $(BASEDIR)/cvm
STAGE = $(CDIR)/stage

.PHONY: all
all: $(TARGETS)

$(BLKPACK):
	$(MAKE) -C $(TOOLSDIR) blkpack 

$(BLKFS): $(BLK_SRCS) $(BLKPACK)
	cat $(BLK_SRCS) | $(BLKPACK) > $@

$(STAGE): $(BLKFS)
	$(MAKE) -C $(CDIR) stage

.PHONY: clean
clean:
	rm -f $(TARGETS) $(BLKFS) memdump $(TOCLEAN)
