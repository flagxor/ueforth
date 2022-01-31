/*
 * Copyright 2022 Bradley D. Nelson
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

#include <stdio.h>

#define SIM_PRINT_ONLY
#include "esp32/options.h"
#define FLOATING_POINT_LIST
#define USER_WORDS
#include "builtins.h"

#define Y(name, code) X(#name, name, code)

int main() {
  printf("#define PLATFORM_OPCODE_LIST \\\n");
#define X(str, name, code) printf("  X(\"%s\", %s, ) \\\n", str, #name);
  PLATFORM_OPCODE_LIST
#undef X
  printf("\n");
  return 0;
}
