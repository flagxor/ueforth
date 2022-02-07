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
#define ADDR_DOCOLON ((void *) OP_DOCOLON)
#define ADDR_DOCREATE ((void *) OP_DOCREATE)
#define ADDR_DODOES ((void *) OP_DODOES)

enum {
  OP_DOCOLON = 0,
  OP_DOCREATE,
  OP_DODOES,
#define XV(flags, name, op, code) OP_ ## op,
  PLATFORM_OPCODE_LIST
  EXTRA_OPCODE_LIST
  OPCODE_LIST
#undef XV
};

static cell_t *forth_run(cell_t *init_rp) {
  static const BUILTIN_WORD builtins[] = {
#define XV(flags, name, op, code) \
    name, ((VOC_ ## flags >> 8) & 0xff) | BUILTIN_MARK, sizeof(name) - 1, \
    (VOC_ ## flags & 0xff), (void *) OP_ ## op,
    PLATFORM_OPCODE_LIST
    EXTRA_OPCODE_LIST
    OPCODE_LIST
#undef XV
    0, 0, 0,
  };

  if (!init_rp) {
    g_sys.DOCREATE_OP = ADDR_DOCREATE;
    g_sys.builtins = builtins;
    return 0;
  }
  register cell_t *ip, *rp, *sp, tos, w;
  register float *fp;
  rp = init_rp; UNPARK;
  for (;;) {
next:
    w = *ip++;
work:
    switch (*(cell_t *) w & 0xff) {
#define XV(flags, name, op, code) case OP_ ## op: { code; } NEXT;
  PLATFORM_OPCODE_LIST
  EXTRA_OPCODE_LIST
  OPCODE_LIST
#undef XV
      case OP_DOCOLON: ++rp; *rp = (cell_t) ip; ip = (cell_t *) (w + sizeof(cell_t)); NEXT;
      case OP_DOCREATE: DUP; tos = w + sizeof(cell_t) * 2; NEXT;
      case OP_DODOES: DUP; tos = w + sizeof(cell_t) * 2;
                      ++rp; *rp = (cell_t) ip;
                      ip = (cell_t *) *(cell_t *) (w + sizeof(cell_t)); NEXT;
    }
  }
}
