#include "windows.h"

#define CALLTYPE WINAPI

#include "common/opcodes.h"
#include "common/calling.h"

#define HEAP_SIZE (10 * 1024 * 1024)
#define STACK_SIZE (64 * 1024)

#define PLATFORM_OPCODE_LIST \
  X("GETPROCADDRESS", GETPROCADDRESS, \
      tos = (cell_t) GetProcAddress((HMODULE) *sp, (LPCSTR) tos); --sp) \
  X("LOADLIBRARYA", LOADLIBRARYA, \
      tos = (cell_t) LoadLibraryA((LPCSTR) tos)) \
  CALLING_OPCODE_LIST \

#include "common/core.h"

#include "gen/windows_boot.h"

int WINAPI WinMain(HINSTANCE hInstance, HINSTANCE hPrev,
                   PSTR pCmdLine, int nCmdShow) {
  void *heap = VirtualAlloc(
      NULL, HEAP_SIZE, MEM_RESERVE | MEM_COMMIT, PAGE_EXECUTE_READWRITE);
  ueforth(0, 0, heap, boot, sizeof(boot));
  return 1;
}

