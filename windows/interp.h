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

#define NEXT goto next
#define JMPW goto work
#define ADDROF(x) ((void *) OP_ ## x)

enum {
#define Z(flags, name, op, code) OP_ ## op,
  PLATFORM_OPCODE_LIST
  TIER2_OPCODE_LIST
  TIER1_OPCODE_LIST
  TIER0_OPCODE_LIST
#undef Z
};

static cell_t *forth_run(cell_t *init_rp) {
  static const BUILTIN_WORD builtins[] = {
#define Z(flags, name, op, code) \
    name, ((VOC_ ## flags >> 8) & 0xff) | BUILTIN_MARK, sizeof(name) - 1, \
    (VOC_ ## flags & 0xff), (void *) OP_ ## op,
    PLATFORM_OPCODE_LIST
    TIER2_OPCODE_LIST
    TIER1_OPCODE_LIST
    TIER0_OPCODE_LIST
#undef Z
    0, 0, 0,
  };

  if (!init_rp) {
    g_sys->DOCREATE_OP = ADDROF(DOCREATE);
    g_sys->builtins = builtins;
    return 0;
  }
  register cell_t *ip, *rp, *sp, tos, w;
  register float *fp, ft;
  rp = init_rp; UNPARK;
  for (;;) {
    __try {
      for (;;) {
next:
        w = *ip++;
work:
        switch (*(cell_t *) w & 0xff) {
#define Z(flags, name, op, code) case OP_ ## op: { code; } NEXT;
  PLATFORM_OPCODE_LIST
  TIER2_OPCODE_LIST
  TIER1_OPCODE_LIST
  TIER0_OPCODE_LIST
#undef Z
        }
      }
    } __except (EXCEPTION_EXECUTE_HANDLER) {
      DWORD code = GetExceptionCode();
      switch (code) {
        case EXCEPTION_INT_DIVIDE_BY_ZERO: THROWIT(-10); break;
        case EXCEPTION_ACCESS_VIOLATION: THROWIT(-9); break;
        default: THROWIT(code); break;
      }
    }
  }
}
