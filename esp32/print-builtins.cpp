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
#define ENABLE_OLED_SUPPORT
#include "esp32/options.h"
#define FLOATING_POINT_LIST
#define USER_WORDS
#include "builtins.h"

#define YV(flags, op, code) XV(flags, #op, op, code)
#define X(name, op, code) XV(forth, name, op, code)
#define Y(op, code) XV(forth, #op, op, code)

int main() {
  printf("#define PLATFORM_SIMULATED_OPCODE_LIST \\\n");
#define XV(flags, str, name, code) \
  printf("  XV(%s, \"%s\", %s, DUP; sp = simulated(sp, STR_%s); DROP) \\\n", #flags, str, #name, #name);
  PLATFORM_OPCODE_LIST
#undef XV
  printf("\n");
  return 0;
}
