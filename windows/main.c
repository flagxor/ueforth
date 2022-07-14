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

#include "common/tier0_opcodes.h"
#include "common/tier1_opcodes.h"
#include "common/tier2_opcodes.h"
#include "common/floats.h"
#include "common/calling.h"
#include "common/calls.h"

#define HEAP_SIZE (10 * 1024 * 1024)
#define STACK_CELLS (8 * 1024)

static LRESULT WindowProcShim(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);

#define PLATFORM_OPCODE_LIST \
  Y(GetProcAddress, \
      tos = (cell_t) GetProcAddress((HMODULE) *sp, (LPCSTR) tos); --sp) \
  Y(LoadLibraryA, tos = (cell_t) LoadLibraryA((LPCSTR) tos)) \
  Y(WindowProcShim, DUP; tos = (cell_t) &WindowProcShim) \
  CALLING_OPCODE_LIST \
  FLOATING_POINT_LIST

#define VOCABULARY_LIST V(forth) V(internals)

#include "common/bits.h"
#include "common/core.h"
#include "windows/interp.h"

#include "gen/windows_boot.h"

static LRESULT WindowProcShim(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam) {
  if (msg == WM_NCCREATE) {
    SetWindowLongPtr(
        hwnd, GWLP_USERDATA,
        (LONG_PTR) ((CREATESTRUCT *) lParam)->lpCreateParams);
  }
  if (!GetWindowLongPtr(hwnd, GWLP_USERDATA)) {
    return DefWindowProc(hwnd, msg, wParam, lParam);
  }
  cell_t stacks[STACK_CELLS * 3 + 4];
  cell_t *at = stacks;
  at += 4;
  float *fp = (float *) (at + 1); at += STACK_CELLS;
  cell_t *rp = at + 1; at += STACK_CELLS;
  cell_t *sp = at + 1; at += STACK_CELLS;
  cell_t *ip = (cell_t *) GetWindowLongPtr(hwnd, GWLP_USERDATA);
  cell_t tos = 0;
  DUP; tos = (cell_t) hwnd;
  DUP; tos = (cell_t) msg;
  DUP; tos = (cell_t) wParam;
  DUP; tos = (cell_t) lParam;
  PARK;
  rp = forth_run(rp);
  UNPARK;
  return tos;
}

#ifdef UEFORTH_MINIMAL
int WINAPI WinMainCRTStartup(void) {
#else
int WINAPI WinMain(HINSTANCE inst, HINSTANCE prev, LPSTR cmd, int show) {
#endif
  void *heap = VirtualAlloc(
      (void *) 0x8000000, HEAP_SIZE,
      MEM_RESERVE | MEM_COMMIT, PAGE_EXECUTE_READWRITE);
  forth_init(0, 0, heap, HEAP_SIZE, boot, sizeof(boot));
  for (;;) { g_sys->rp = forth_run(g_sys->rp); }
}

