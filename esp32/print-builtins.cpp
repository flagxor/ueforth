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

#include "esp32/platform.h"
#include "esp32/options.h"
#define CALLING_OPCODE_LIST
#define FLOATING_POINT_LIST
#define USER_WORDS

#define OPTIONAL_ASSEMBLERS_SUPPORT
#define OPTIONAL_CAMERA_SUPPORT
#define OPTIONAL_INTERRUPTS_SUPPORT
#define OPTIONAL_OLED_SUPPORT
#define OPTIONAL_RMT_SUPPORT
#define OPTIONAL_SERIAL_BLUETOOTH_SUPPORT
#define OPTIONAL_SPI_FLASH_SUPPORT
#define OPTIONAL_ESPNOW_SUPPORT

#define OPTIONAL_BLUETOOTH_VOCABULARY
#define OPTIONAL_CAMERA_VOCABULARY
#define OPTIONAL_INTERRUPTS_VOCABULARIES
#define OPTIONAL_OLED_VOCABULARY
#define OPTIONAL_RMT_VOCABULARY
#define OPTIONAL_SPI_FLASH_VOCABULARY
#define OPTIONAL_ESPNOW_VOCABULARY

#include "builtins.h"

#define XV(flags, name, op, code) Z(flags, name, op, code)
#define YV(flags, op, code) Z(flags, #op, op, code)
#define X(name, op, code) Z(forth, name, op, code)
#define Y(op, code) Z(forth, #op, op, code)

int main() {
  printf("#define PLATFORM_SIMULATED_OPCODE_LIST \\\n");
#define Z(flags, str, name, code) \
  printf("  Z(%s, \"%s\", %s, DUP; sp = simulated(sp, STR_%s); DROP) \\\n", #flags, str, #name, #name);
  PLATFORM_OPCODE_LIST
#undef Z
  printf("\n");
  return 0;
}
