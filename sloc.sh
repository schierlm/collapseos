#!/bin/sh
find . -name blk.fs -exec grep -v \\-\\-\\-\\-\\- {} + | wc -l ; 
