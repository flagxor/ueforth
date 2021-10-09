#define _GNU_SOURCE
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

#include <dlfcn.h>
#include <sys/mman.h>
#include <sys/errno.h>

#include "common/opcodes.h"
#include "common/floats.h"
#include "common/calling.h"
#include "common/calls.h"

#define HEAP_SIZE (10 * 1024 * 1024)
#define STACK_SIZE (64 * 1024)

#define PLATFORM_OPCODE_LIST \
  Y(errno, DUP; tos = (cell_t) errno) \
  Y(DLSYM, tos = (cell_t) dlsym(a1 ? a1 : RTLD_DEFAULT, a0); --sp) \
  FLOATING_POINT_LIST \
  CALLING_OPCODE_LIST \

#include "common/core.h"
#include "common/interp.h"

#include "gen/posix_boot.h"

int main(int argc, char *argv[]) {
  void *heap = mmap(
      (void *) 0x8000000, HEAP_SIZE,
      PROT_EXEC | PROT_READ | PROT_WRITE, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
  forth_init(argc, argv, heap, boot, sizeof(boot));
  for (;;) { g_sys.rp = forth_run(g_sys.rp); }
  return 1;
}
