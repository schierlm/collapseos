TARGETS = forth rc2014 sms ti84
BASEDIR = ../..
BLKFS = blkfs
BLK_SRCS = $(BASEDIR)/blk.fs $(BASEDIR)/arch/z80/blk.fs
TOCLEAN = *.o *.bin
include $(BASEDIR)/common.mk

OBJS = emul.o z80.o
RC2014_OBJS = $(OBJS) sio.o acia.o sdc.o rc2014_spi.o at28.o
SMS_OBJS = $(OBJS) tms9918.o sms_vdp.o sms_ports.o sms_pad.o ps2_kbd.o sdc.o \
	sms_spi.o
TI84_OBJS = $(OBJS) t6a04.o ti84_kbd.o 

forth: forth.c $(OBJS) $(BLKFS)
	$(CC) -DFBIN_PATH=\"`pwd`/forth.bin\" -DBLKFS_PATH=\"`pwd`/$(BLKFS)\" forth.c $(OBJS) -lcurses -o $@

rc2014: rc2014.c $(RC2014_OBJS)
	$(CC) rc2014.c $(RC2014_OBJS) -o $@

sms: sms.c $(SMS_OBJS)
	$(CC) sms.c $(SMS_OBJS) -o $@ `pkg-config --cflags --libs xcb`

ti84: ti84.c $(TI84_OBJS)
	$(CC) ti84.c $(TI84_OBJS) -o $@ `pkg-config --cflags --libs xcb`

emul.o: emul.c forth.bin $(BLKFS)
	$(CC) -c -o emul.o emul.c

forth.bin: xcomp.fs $(STAGE) $(BLKFS)
	$(STAGE) < xcomp.fs > $@
