.PHONY: clean
clean:
	find arch cvm emul tests tools -name Makefile -execdir $(MAKE) clean \;
	find . -name memdump -or -name "*.core" -delete

.PHONY: dist
dist: clean
	tar czf ~/collapseos-`date +%Y%m%d`.tar.gz .
