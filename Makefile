.PHONY: clean
clean:
	find arch cvm emul tests tools -name Makefile -execdir $(MAKE) clean \;

.PHONY: dist
dist: clean
	tar czf ~/collapseos-`date +%Y%m%d`.tar.gz .
