.PHONY: run
run:
	$(MAKE) -C ../cvm all
	$(MAKE) -C ../emul/z80 all
	$(MAKE) -C ../emul/8086 all
	cd all && ./runtests.sh
	cd z80 && ./runtests.sh

.PHONY: clean
clean: # do nothing
