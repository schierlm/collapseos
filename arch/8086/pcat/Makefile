# See /doc/hw/8086/pcat.txt
TARGET = os.bin
TARGETS = disk.bin
XCOMP = xcomp.fs
EMULEXEC = pcat
EMUL_TARGET = disk.bin
TOCLEAN = disk.bin mbr.bin
include ../common.mk

mbr.bin: mbr.fs $(STAGE)
	$(STAGE) < mbr.fs > $@

disk.bin: mbr.bin $(TARGET)
	cat mbr.bin $(TARGET) > $@
	dd if=blkfs of=$@ bs=512 seek=16
