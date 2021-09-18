// Copyright 2021 Bradley D. Nelson
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
#include "common/floats.h"
#include "common/calling.h"
#include "common/calls.h"

#define HEAP_SIZE (10 * 1024 * 1024)
#define STACK_SIZE (64 * 1024)

#define PLATFORM_OPCODE_LIST \
  Y(GETPROCADDRESS, \
      tos = (cell_t) GetProcAddress((HMODULE) *sp, (LPCSTR) tos); --sp) \
  Y(LOADLIBRARYA, \
      tos = (cell_t) LoadLibraryA((LPCSTR) tos)) \
  FLOATING_POINT_LIST \
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
      (void *) 0x8000000, HEAP_SIZE,
      MEM_RESERVE | MEM_COMMIT, PAGE_EXECUTE_READWRITE);
  forth_init(0, 0, heap, boot, sizeof(boot));
  for (;;) { g_sys.rp = forth_run(g_sys.rp); }
}

