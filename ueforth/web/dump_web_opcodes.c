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

#include <stdio.h>
#include <string.h>

#include "common/opcodes.h"

#define PLATFORM_OPCODE_LIST \
  X("CALL", CALL, sp = Call(sp|0, tos|0) | 0; DROP) \

enum {
  OP_DOCOLON = 0,
  OP_DOCREATE = 1,
  OP_DODOES = 2,
#define X(name, op, code) OP_ ## op,
  PLATFORM_OPCODE_LIST
  OPCODE_LIST
#undef X
};

int main(int argc, char *argv[]) {
  if (argc == 2 && strcmp(argv[1], "cases") == 0) {
#define X(name, op, code) \
    printf("          case %d:  // %s\n            %s; break;\n", OP_ ## op, name, #code);
    PLATFORM_OPCODE_LIST
    OPCODE_LIST
#undef X
  } else if (argc == 2 && strcmp(argv[1], "dict") == 0) {
#define X(name, op, code) printf("  create(" #name ", %d);\n", OP_ ## op);
    PLATFORM_OPCODE_LIST
    OPCODE_LIST
#undef X
  } else {
    fprintf(stderr, "USAGE: %s cases/dict\n", argv[1]);
    return 1;
  }
  return 0;
}
