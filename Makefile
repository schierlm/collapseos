.PHONY: clean
clean:
	find * -name Makefile -execdir $(MAKE) clean \;

.PHONY: dist
dist: clean
	tar czf ~/collapseos-`date +%Y%m%d`.tar.gz .
