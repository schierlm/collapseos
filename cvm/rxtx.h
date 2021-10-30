#include "vm.h"
#define RXTX_PORT 0x08
int rxtx_init(VM *vm, int port);
void rxtx_deinit();
