# See /doc/hw/z80/ti84.txt
TARGET = os.bin
TARGETS = $(TARGET)
XCOMP = xcomp.fs
EMULEXEC = ti84
TOCLEAN = *.rom *.8xu
include ../common.mk

MKTIUPGRADE = mktiupgrade

os.rom: $(TARGET)
	dd if=$(TARGET) bs=1M of=$@ conv=sync

os.8xu: os.rom
	$(MKTIUPGRADE) -p -k keys/0A.key -d TI-84+ os.rom $@ 00

.PHONY: send
send: os.8xu
	tilp -n --calc ti84+ --cable DirectLink os.8xu
