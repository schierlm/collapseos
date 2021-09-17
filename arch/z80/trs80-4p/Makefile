# See /doc/hw/z80/trs80-4p.txt
TARGET = os.bin
TARGETS = $(TARGET) recv.bin boot.bin
XCOMP = xcomp.fs
EMULEXEC = noemul
include ../common.mk

recv.bin: recv.fs $(STAGE)
	$(STAGE) < recv.fs > $@

boot.bin: boot.fs $(STAGE)
	$(STAGE) < boot.fs > $@
