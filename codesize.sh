#!/bin/sh
bytes=$(cat blk.fs arch/*/blk.fs | wc -c)
echo "$((bytes/1024)) KB"
