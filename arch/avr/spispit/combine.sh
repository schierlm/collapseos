#!/bin/bash
fsize=$((($(du -b --apparent-size "$2" | cut -f1) / 256) + 1))
dd if=$1 bs=255 conv=sync
printf "\x$(printf %x $fsize)"
cat $2
