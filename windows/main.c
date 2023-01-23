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
#include "common/calls.h"

#define HEAP_SIZE (10 * 1024 * 1024)
#define STACK_CELLS (8 * 1024)

static LRESULT WindowProcShim(HWND hwnd, UINT msg, WPARAM wParam, LPARAM lParam);
static void SetupCtrlBreakHandler(void);
static cell_t GetBootExtra(cell_t *start);

#define PLATFORM_OPCODE_LIST \
  YV(windows, GetProcAddress, \
      tos = (cell_t) GetProcAddress((HMODULE) *sp, (LPCSTR) tos); --sp) \
  YV(windows, LoadLibraryA, tos = (cell_t) LoadLibraryA((LPCSTR) tos)) \
  YV(windows, WindowProcShim, DUP; tos = (cell_t) &WindowProcShim) \
  YV(windows, SetupCtrlBreakHandler, SetupCtrlBreakHandler()) \
  YV(windows, boot_extra, DUP; DUP; tos = GetBootExtra(sp)) \
  CALLING_OPCODE_LIST \
  FLOATING_POINT_LIST

#define VOCABULARY_LIST V(forth) V(internals) V(windows)

#include "common/bits.h"
#include "common/core.h"
#include "common/calling.h"
#include "windows/interp.h"

#include "gen/windows_boot.h"
#include "gen/windows_boot_extra.h"

static DWORD forth_main_thread_id;
static uintptr_t forth_main_thread_resume_sp;
static uintptr_t forth_main_thread_resume_bp;

static BOOL WINAPI forth_ctrl_handler(DWORD fdwCtrlType) {
  HANDLE main_thread;
  CONTEXT context = { 0 };

  if (fdwCtrlType == CTRL_C_EVENT ||
      fdwCtrlType == CTRL_BREAK_EVENT) {
    // Using explicit instead of THREAD_ALL_ACCESS to be explicit as per docs.
    // THREAD_QUERY_INFORMATION seems to be required for reasons unknown on x64.
    main_thread = OpenThread(THREAD_QUERY_INFORMATION |
                             THREAD_SET_CONTEXT |
                             THREAD_GET_CONTEXT |
                             THREAD_SUSPEND_RESUME, FALSE, forth_main_thread_id);
    SuspendThread(main_thread);
    context.ContextFlags = CONTEXT_CONTROL;
    GetThreadContext(main_thread, &context);
#ifdef _WIN64
    context.Rip = 0;
    context.Rsp = forth_main_thread_resume_sp;
    context.Rbp = forth_main_thread_resume_bp;
#else
    context.Eip = 0;
    context.Esp = forth_main_thread_resume_sp;
    context.Ebp = forth_main_thread_resume_bp;
#endif
    SetThreadContext(main_thread, &context);
    ResumeThread(main_thread);
    CloseHandle(main_thread);
    return TRUE;
  }
  return FALSE;
}

static void SetupCtrlBreakHandler(void) {
  forth_main_thread_id = GetCurrentThreadId();
  SetConsoleCtrlHandler(forth_ctrl_handler, TRUE);
  CONTEXT context = { 0 };
  context.ContextFlags = CONTEXT_CONTROL;
  GetThreadContext(GetCurrentThread(), &context);
#ifdef _WIN64
  forth_main_thread_resume_sp = context.Rsp;
  forth_main_thread_resume_bp = context.Rbp;
#else
  forth_main_thread_resume_sp = context.Esp;
  forth_main_thread_resume_bp = context.Ebp;
#endif
}

static cell_t GetBootExtra(cell_t *start) {
  *start = (cell_t) boot_extra;
  return (cell_t) sizeof(boot_extra) - 1;
}

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

