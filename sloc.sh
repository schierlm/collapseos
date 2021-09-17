#!/bin/sh
grep -v \\-\\-\\-\\-\\- blk.fs arch/*/blk.fs | wc -l
