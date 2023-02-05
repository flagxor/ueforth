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
  xt_set_exception_handler(EXCCAUSE_ILLEGAL, forth_exception_handler);                // 0
  // EXCCAUSE_SYSCALL - used for syscalls                                             // 1
  xt_set_exception_handler(EXCCAUSE_INSTR_ERROR, forth_exception_handler);            // 2
  xt_set_exception_handler(EXCCAUSE_LOAD_STORE_ERROR, forth_exception_handler);       // 3
  xt_set_exception_handler(EXCCAUSE_LEVEL1_INTERRUPT, forth_exception_handler);       // 4
  // EXCCAUSE_ALLOCA - used to grow with alloca                                       // 5
  xt_set_exception_handler(EXCCAUSE_DIVIDE_BY_ZERO, forth_exception_handler);         // 6
  xt_set_exception_handler(EXCCAUSE_PC_ERROR, forth_exception_handler);               // 7
  xt_set_exception_handler(EXCCAUSE_PRIVILEGED, forth_exception_handler);             // 8
  xt_set_exception_handler(EXCCAUSE_UNALIGNED, forth_exception_handler);              // 9
  xt_set_exception_handler(EXCCAUSE_EXTREG_PRIVILEGE, forth_exception_handler);       // 10
  xt_set_exception_handler(EXCCAUSE_EXCLUSIVE_ERROR, forth_exception_handler);        // 11
  xt_set_exception_handler(EXCCAUSE_INSTR_DATA_ERROR, forth_exception_handler);       // 12
  xt_set_exception_handler(EXCCAUSE_LOAD_STORE_DATA_ERROR, forth_exception_handler);  // 13
  xt_set_exception_handler(EXCCAUSE_INSTR_ADDR_ERROR, forth_exception_handler);       // 14
  xt_set_exception_handler(EXCCAUSE_LOAD_STORE_ADDR_ERROR, forth_exception_handler);  // 15
  xt_set_exception_handler(EXCCAUSE_ITLB_MISS, forth_exception_handler);              // 16
  xt_set_exception_handler(EXCCAUSE_ITLB_MULTIHIT, forth_exception_handler);          // 17
  xt_set_exception_handler(EXCCAUSE_INSTR_RING, forth_exception_handler);             // 18
  // Reserved                                                                         // 19
  xt_set_exception_handler(EXCCAUSE_INSTR_PROHIBITED, forth_exception_handler);       // 20
  // Reserved                                                                         // 21
  // Reserved                                                                         // 22
  // Reserved                                                                         // 23
  xt_set_exception_handler(EXCCAUSE_DTLB_MISS, forth_exception_handler);              // 24
  xt_set_exception_handler(EXCCAUSE_DTLB_MULTIHIT, forth_exception_handler);          // 25
  xt_set_exception_handler(EXCCAUSE_LOAD_STORE_RING, forth_exception_handler);        // 26
  // Reserved                                                                         // 27
  xt_set_exception_handler(EXCCAUSE_LOAD_PROHIBITED, forth_exception_handler);        // 28
  xt_set_exception_handler(EXCCAUSE_STORE_PROHIBITED, forth_exception_handler);       // 29
  // Reserved                                                                         // 30
  // Reserved                                                                         // 31
  for (int i = 0; i < 8; ++i) {
    xt_set_exception_handler(EXCCAUSE_CP_DISABLED(i), forth_exception_handler);       // 32-39
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
