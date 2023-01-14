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

#if defined(HAS_SIGNALS) || defined(ENABLE_ESP32_FORTH_FAULT_HANDLING)
# include <setjmp.h>
#endif
#if defined(HAS_SIGNALS)
# include <signal.h>
#endif

#define JMPW goto **(void **) w
#define NEXT w = *ip++; JMPW
#define ADDROF(x) (&& OP_ ## x)

#if defined(HAS_SIGNALS) || defined(ENABLE_ESP32_FORTH_FAULT_HANDLING)
static __thread jmp_buf g_forth_fault;
static __thread int g_forth_signal;
#endif

#if defined(HAS_SIGNALS)
static void forth_signal_handler(int sig) {
  g_forth_signal = sig;
  sigset_t ss;
  sigemptyset(&ss);
  sigprocmask(SIG_SETMASK, &ss, 0);
  longjmp(g_forth_fault, 1);
}
#endif

#if defined(ENABLE_ESP32_FORTH_FAULT_HANDLING)
# include "soc/soc.h"
# include <xtensa/xtensa_api.h>
static __thread uint32_t g_forth_setlevel;
static void IRAM_ATTR forth_exception_handler(XtExcFrame *frame) {
  g_forth_signal = frame->exccause;
  XTOS_RESTORE_INTLEVEL(g_forth_setlevel);
  longjmp(g_forth_fault, 1);
}
#endif

static cell_t *forth_run(cell_t *init_rp) {
  static const BUILTIN_WORD builtins[] = {
#define Z(flags, name, op, code) \
    name, ((VOC_ ## flags >> 8) & 0xff) | BUILTIN_MARK, \
    sizeof(name) - 1, (VOC_ ## flags & 0xff), && OP_ ## op,
    PLATFORM_OPCODE_LIST
    TIER2_OPCODE_LIST
    TIER1_OPCODE_LIST
    TIER0_OPCODE_LIST
#undef Z
    0, 0, 0, 0, 0,
  };

  if (!init_rp) {
    g_sys->DOCREATE_OP = ADDROF(DOCREATE);
    g_sys->builtins = builtins;
#if defined(HAS_SIGNALS)
    struct sigaction sa;
    memset(&sa, 0, sizeof(sa));
    sa.sa_handler = forth_signal_handler;
    sigaction(SIGSEGV, &sa, 0);
    sigaction(SIGBUS, &sa, 0);
    sigaction(SIGINT, &sa, 0);
    sigaction(SIGFPE, &sa, 0);
#endif
#if defined(ENABLE_ESP32_FORTH_FAULT_HANDLING)
    xt_set_exception_handler(EXCCAUSE_LOAD_STORE_ERROR, forth_exception_handler);
    xt_set_exception_handler(EXCCAUSE_PRIVILEGED, forth_exception_handler);
    xt_set_exception_handler(EXCCAUSE_UNALIGNED, forth_exception_handler);
    xt_set_exception_handler(EXCCAUSE_DIVIDE_BY_ZERO, forth_exception_handler);
    xt_set_exception_handler(EXCCAUSE_INSTR_ERROR, forth_exception_handler);
    xt_set_exception_handler(EXCCAUSE_ILLEGAL, forth_exception_handler);
    xt_set_exception_handler(EXCCAUSE_LOAD_PROHIBITED, forth_exception_handler);
    xt_set_exception_handler(EXCCAUSE_STORE_PROHIBITED, forth_exception_handler);
    xt_set_exception_handler(EXCCAUSE_INSTR_PROHIBITED, forth_exception_handler);
    uint32_t default_setlevel = XTOS_SET_INTLEVEL(XCHAL_EXCM_LEVEL);
    XTOS_RESTORE_INTLEVEL(default_setlevel);
    g_forth_setlevel = default_setlevel;
#endif
    return 0;
  }
  register cell_t *ip, *rp, *sp, tos, w;
  register float *fp, ft;
  rp = init_rp; UNPARK;
#if defined(HAS_SIGNALS) || defined(ENABLE_ESP32_FORTH_FAULT_HANDLING)
  if (setjmp(g_forth_fault)) {
    rp = *g_sys->throw_handler;
    *g_sys->throw_handler = (cell_t *) *rp--;
    sp = (cell_t *) *rp--;
    fp = (float *) *rp--;
    ip = (cell_t *) *rp--;
    --sp;
    tos = -g_forth_signal;
  }
#endif
  NEXT;
#define Z(flags, name, op, code) OP_ ## op: { code; } NEXT;
  PLATFORM_OPCODE_LIST
  TIER2_OPCODE_LIST
  TIER1_OPCODE_LIST
  TIER0_OPCODE_LIST
#undef Z
}
