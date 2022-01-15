#!/bin/bash
fsize=$(du -k --apparent-size "$2" | cut -f1)
dd if=$1 bs=255 conv=sync
printf "\x$(printf %x $fsize)"
cat $2
