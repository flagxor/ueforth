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

#include <stdio.h>

#ifndef UEFORTH_SIM
# include "pico/stdlib.h"
# include "boards/pico_ice.h"
# include "tusb.h"
#else
# include <time.h>
#endif

#include "common/tier0_opcodes.h"
#include "common/tier1_opcodes.h"
#include "common/tier2_opcodes.h"
#include "common/floats.h"
#include "common/calls.h"

#ifndef UEFORTH_SIM
# define HEAP_SIZE (100 * 1024)
#else
# define HEAP_SIZE (200 * 1024)
#endif
#define STACK_CELLS (4 * 1024)

#include "pico-ice/builtins.h"

// TODO: Implement faults.
#define FAULT_ENTRY
static void forth_faults_setup(void) {
}

#include "common/bits.h"
#include "common/core.h"
#include "common/calling.h"
#include "common/interp.h"
#include "gen/pico_ice_boot.h"

int main(int argc, char *argv[]) {
#ifndef UEFORTH_SIM
  tusb_init();
  stdio_init_all();
  tud_task();
#endif

  void *heap = malloc(HEAP_SIZE);
  forth_init(argc, argv, heap, HEAP_SIZE, boot, sizeof(boot));
  for (;;) { g_sys->rp = forth_run(g_sys->rp); }
  return 1;
}
