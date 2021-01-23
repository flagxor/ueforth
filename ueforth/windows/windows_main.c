#define _USING_V110_SDK71_ 1
#include "windows.h"
#include <immintrin.h>

#define CALLTYPE WINAPI
#if defined(_M_X64)
# define SSMOD_FUNC \
  --sp; cell_t b, a = _mul128(*sp, sp[1], &b); b = _div128(b, a, tos, sp); \
  if (*sp < 0) { *sp += tos; tos = b - 1; } else { tos = b; }
#elif defined(_M_IX86)
# define SSMOD_FUNC \
  --sp; __int64 a = (__int64) *sp * (__int64) sp[1]; cell_t b = _div64(a, tos, sp); \
  if (*sp < 0) { *sp += tos; tos = b - 1; } else { tos = b; }
#endif

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
#include "windows/windows_interp.h"

#include "gen/windows_boot.h"

#ifdef UEFORTH_MINIMAL
int WINAPI WinMainCRTStartup(void) {
#else
int WINAPI WinMain(HINSTANCE inst, HINSTANCE prev, LPSTR cmd, int show) {
#endif
  void *heap = VirtualAlloc(
      NULL, HEAP_SIZE, MEM_RESERVE | MEM_COMMIT, PAGE_EXECUTE_READWRITE);
  ueforth(0, 0, heap, boot, sizeof(boot));
}

