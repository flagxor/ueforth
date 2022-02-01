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

static cell_t *simulated(cell_t *sp, const char *op);

#define PLATFORM_OPCODE_LIST \
  FLOATING_POINT_LIST \
  PLATFORM_SIMULATED_OPCODE_LIST

#include "gen/esp32_sim_opcodes.h"

#define X(str, name, code) static const char *STR_ ## name = str;
PLATFORM_SIMULATED_OPCODE_LIST
#undef X

#include "common/core.h"
#include "common/interp.h"
#include "gen/esp32_boot.h"
#include "esp32/main.cpp"
#include <time.h>
#include <unistd.h>
#include <stdio.h>

static cell_t *simulated(cell_t *sp, const char *op) {
  if (op == STR_MALLOC) {
    *sp = (cell_t) malloc(*sp);
    return sp;
  } else if (op == STR_SYSFREE) {
    free((void*) *sp--);
    return sp;
  } else if (op == STR_SERIAL_BEGIN) {
    --sp;
    return sp;
  } else if (op == STR_SERIAL_READ_BYTES) {
    sp[-1] = read(0, (void *) sp[-1], sp[0]); --sp;
    return sp;
  } else if (op == STR_SERIAL_WRITE) {
    sp[-1] = write(1, (void *) sp[-1], sp[0]); --sp;
    return sp;
  } else if (op == STR_MS_TICKS) {
    struct timespec tm;
    clock_gettime(CLOCK_MONOTONIC, &tm);
    *++sp = tm.tv_sec * 1000 + tm.tv_nsec / 1000000;
    return sp;
  } else if (op == STR_RAW_YIELD) {
    return sp;
  } else if (op == STR_SPIFFS_BEGIN) {
    sp -= 2; *sp = 0;
    return sp;
  } else if (op == STR_pinMode) {
    sp -= 2;
    return sp;
  } else if (op == STR_digitalWrite) {
    sp -= 2;
    return sp;
  } else if (op == STR_gpio_install_isr_service) {
    --sp;
    *sp = 0;
    return sp;
  } else if (op == STR_SERIAL_AVAILABLE) {
    *++sp = 1;
    return sp;
  } else if (op == STR_TERMINATE) {
    exit(*sp);
    return sp;
  } else {
    fprintf(stderr, "MISSING SIM OPCODE: %s\n", op);
    return sp;
  }
}

int main(int argc, char *argv[]) {
  setup();
  for (;;) {
    loop();
  }
}
