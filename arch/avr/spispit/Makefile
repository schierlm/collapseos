# See doc/hw/avr/spispit.txt for details
TARGET = spispit.bin
TARGETS = $(TARGET) $(COMBINED)
COMBINED = combined.bin
XCOMP ?= xcomp.fs
AVRDUDEMCU ?= atmega328p
USBDEV ?= /dev/ttyACM0
BAUDS ?= 115200
AVRDUDEARGS ?= -F -V -c arduino -P $(USBDEV) -b $(BAUDS)

include ../common.mk

$(COMBINED): $(TARGET) data
	./combine.sh $(TARGET) data > $@

send: $(COMBINED)
	avrdude $(AVRDUDEARGS) -p $(AVRDUDEMCU) -U flash:w:$(COMBINED):r
