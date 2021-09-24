# See /doc/hw/z80/sms.txt
TARGET = os.sms
TARGETS = $(TARGET) ps2avr.bin
XCOMP = xcomp.fs
EMULEXEC = sms
EMUL_ARGS = -c$(BLKFS) # -k for KBD
AVR_BLKFS = blkfsavr
AVR_BLK_SRCS = $(BASEDIR)/blk.fs $(BASEDIR)/arch/avr/blk.fs 
PS2_XCOMP = xcompps2.fs
TOCLEAN = $(AVR_BLKFS)
include ../common.mk

$(AVR_BLKFS): $(AVR_BLK_SRCS) $(BLKPACK)
	cat $(AVR_BLK_SRCS) | $(BLKPACK) > $@

ps2avr.bin: $(PS2_XCOMP) $(STAGE) $(AVR_BLKFS)
	$(STAGE) $(AVR_BLKFS) < $(PS2_XCOMP) > $@
