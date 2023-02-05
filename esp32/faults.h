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

# if defined(CONFIG_IDF_TARGET_ESP32C3)

#include <setjmp.h>
#include "riscv/csr.h"
#include "esp_heap_caps.h"

#define FORTH_VECTOR_TABLE_SIZE 32

static __thread jmp_buf g_forth_fault;
static __thread int g_forth_signal;
static void **g_forth_vector_table;
extern void *_vector_table;

#define FAULT_ENTRY \
  if (setjmp(g_forth_fault)) { THROWIT(g_forth_signal); }

static void forth_faults_setup(void) {
  g_forth_vector_table = (void **) malloc(sizeof(void *) * FORTH_VECTOR_TABLE_SIZE);
  //g_forth_vector_table = (void **) heap_caps_malloc(sizeof(void *) * FORTH_VECTOR_TABLE_SIZE,
  //                                                  MALLOC_CAP_EXEC);
  void **vector_table = (void **) &_vector_table;
  for (int i = 0; i < FORTH_VECTOR_TABLE_SIZE; ++i) {
    g_forth_vector_table[i] = vector_table[i];
  }
  // TODO: Actually apply it.
/*
  uint32_t mtvec_val = (uint32_t) g_forth_vector_table;
  mtvec_val |= 1;
  RV_WRITE_CSR(mtvec, mtvec_val);
*/
  //rv_utils_set_mtvec((uint32_t) g_forth_vector_table);
}

# else

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
  // Install exception handler for everything, as window + alloca handlers
  // don't actually get dispatched.
  for (int i = 0; i < 64; ++i) {
    xt_set_exception_handler(i, forth_exception_handler);
  }
  uint32_t default_setlevel = XTOS_SET_INTLEVEL(XCHAL_EXCM_LEVEL);
  XTOS_RESTORE_INTLEVEL(default_setlevel);
  g_forth_setlevel = default_setlevel;
}

# endif

#else

#define forth_faults_setup()
#define FAULT_ENTRY

#endif
