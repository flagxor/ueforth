// Copyright 2022 Bradley D. Nelson
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

#include "esp32/config.h"
#include "esp32/options.h"
#include "common/opcodes.h"
#include "common/floats.h"
#include "common/calling.h"
#include <time.h>
#include <unistd.h>

#define PLATFORM_OPCODE_LIST \
  FLOATING_POINT_LIST \
  REQUIRED_MEMORY_SUPPORT \
  REQUIRED_SYSTEM_SUPPORT \
  REQUIRED_SERIAL_SUPPORT \
  PLATFORM_MOCK_OPCODE_LIST

#define REQUIRED_MEMORY_SUPPORT \
  Y(MALLOC, SET malloc(n0)) \
  Y(SYSFREE, free(a0); DROP) \
  Y(REALLOC, SET realloc(a1, n0); NIP) \
  Y(heap_caps_malloc, SET malloc(n1); NIP) \
  Y(heap_caps_free, free(a0); DROP) \
  Y(heap_caps_realloc, \
      tos = (cell_t) realloc(a2, n1); NIPn(2))

#define REQUIRED_SYSTEM_SUPPORT \
  X("MS-TICKS", MS_TICKS, PUSH time(0) * 1000) \
  X("RAW-YIELD", RAW_YIELD, ) \
  Y(TERMINATE, exit(n0))

#define REQUIRED_SERIAL_SUPPORT \
  X("Serial.begin", SERIAL_BEGIN, DROP) \
  X("Serial.end", SERIAL_END, ) \
  X("Serial.available", SERIAL_AVAILABLE, PUSH -1) \
  X("Serial.readBytes", SERIAL_READ_BYTES, n0 = read(0, b1, n0); NIP) \
  X("Serial.write", SERIAL_WRITE, n0 = write(1, b1, n0); NIP) \
  X("Serial.flush", SERIAL_FLUSH, )

#include "gen/esp32_sim_opcodes.h"
#include "common/core.h"
#include "common/interp.h"
#include "gen/esp32_boot.h"
#include "esp32/main.cpp"

int main(int argc, char *argv[]) {
  setup();
  for (;;) {
    loop();
  }
}
