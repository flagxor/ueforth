#include <dlfcn.h>
#include <sys/mman.h>

#include "common/opcodes.h"

#define STACK_SIZE (16 * 1024)

#define PLATFORM_OPCODE_LIST \
  X("DLSYM", OP_DLSYM, tos = (cell_t) dlsym((void *) *sp, (void *) tos); --sp) \
  X("CALL0", OP_CALL0, tos = ((cell_t (*)()) tos)()) \
  X("CALL1", OP_CALL1, tos = ((cell_t (*)()) tos)(*sp); --sp) \
  X("CALL2", OP_CALL2, tos = ((cell_t (*)()) tos)(sp[-1], *sp); sp -= 2) \
  X("CALL3", OP_CALL3, tos = ((cell_t (*)()) tos)(sp[-2], sp[-1], *sp); sp -= 3) \
  X("CALL4", OP_CALL4, tos = ((cell_t (*)()) tos)(sp[-3], sp[-2], sp[-1], *sp); sp -= 4) \
  X("CALL5", OP_CALL5, tos = ((cell_t (*)()) tos)(sp[-4], sp[-3], sp[-2], sp[-1], *sp); sp -= 5) \
  X("CALL6", OP_CALL6, tos = ((cell_t (*)()) tos)(sp[-5], sp[-4], sp[-3], sp[-2], sp[-1], *sp); sp -= 6) \
  X("CALL7", OP_CALL7, tos = ((cell_t (*)()) tos)(sp[-6], sp[-5], sp[-4], sp[-3], sp[-2], sp[-1], *sp); sp -= 7) \
  X("CALL8", OP_CALL8, tos = ((cell_t (*)()) tos)(sp[-7], sp[-6], sp[-5], sp[-4], sp[-3], sp[-2], sp[-1], *sp); sp -= 8) \
  X("CALL9", OP_CALL9, tos = ((cell_t (*)()) tos)(sp[-8], sp[-7], sp[-6], sp[-5], sp[-4], sp[-3], sp[-2], sp[-1], *sp); sp -= 9) \

#include "common/core.h"

#include "gen/posix_boot.h"

#define HEAP_SIZE (10 * 1024 * 1024)

int main(int argc, char *argv[]) {
  void *heap = mmap(0, HEAP_SIZE, PROT_EXEC | PROT_READ | PROT_WRITE, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
  ueforth(argc, argv, heap, boot, sizeof(boot));
}
