// Copyright 2023 Bradley D. Nelson
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

#if defined(ENABLE_ESP32_FORTH_FAULT_HANDLING)

#include <setjmp.h>
#include "soc/soc.h"
#include <xtensa/xtensa_api.h>

static __thread jmp_buf g_forth_fault;
static __thread int g_forth_signal;
static __thread uint32_t g_forth_setlevel;

#define FAULT_ENTRY \
  if (setjmp(g_forth_fault)) { THROWIT(g_forth_signal); }

static void IRAM_ATTR forth_exception_handler(XtExcFrame *frame) {
  switch (frame->exccause) {
    case EXCCAUSE_LOAD_STORE_ERROR:
    case EXCCAUSE_LOAD_PROHIBITED:
    case EXCCAUSE_STORE_PROHIBITED:
    case EXCCAUSE_LOAD_STORE_DATA_ERROR:
    case EXCCAUSE_LOAD_STORE_RING:
    case EXCCAUSE_LOAD_STORE_ADDR_ERROR:
      g_forth_signal = -9;
      break;
    case EXCCAUSE_DIVIDE_BY_ZERO: g_forth_signal = -10; break;
    case EXCCAUSE_UNALIGNED: g_forth_signal = -23; break;
    default: g_forth_signal = -256 - frame->exccause; break;
  }
  XTOS_RESTORE_INTLEVEL(g_forth_setlevel);
  longjmp(g_forth_fault, 1);
}

static void forth_faults_setup(void) {
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
}

#else

#define forth_faults_setup()
#define FAULT_ENTRY

#endif
