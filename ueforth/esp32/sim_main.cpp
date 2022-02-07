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

#include "esp32/options.h"
#include "common/opcodes.h"
#include "common/extra_opcodes.h"
#include "common/floats.h"
#include "common/calling.h"

#define SIM_HEAP_SIZE (100 * 1024 + 1024 * 1024)

static cell_t *simulated(cell_t *sp, const char *op);

#define PLATFORM_OPCODE_LIST \
  PLATFORM_SIMULATED_OPCODE_LIST \
  FLOATING_POINT_LIST

#include "gen/esp32_sim_opcodes.h"

#define XV(flags, str, name, code) static const char *STR_ ## name = str;
PLATFORM_SIMULATED_OPCODE_LIST
#undef XV

#define MALLOC_CAP_INTERNAL 0
#define heap_caps_get_largest_free_block(x) SIM_HEAP_SIZE
#define heap_caps_get_free_size(x) SIM_HEAP_SIZE

#include "common/core.h"
#include "common/interp.h"
#include "gen/esp32_boot.h"
#include "esp32/main.cpp"
#include <errno.h>
#include <fcntl.h>
#include <stdio.h>
#include <sys/stat.h>
#include <sys/types.h>
#include <time.h>
#include <unistd.h>

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
    cell_t len = *sp--;
    *sp = read(0, (void *) *sp, len);
    return sp;
  } else if (op == STR_SERIAL_WRITE) {
    cell_t len = *sp--;
    *sp = write(1, (void *) *sp, len);
    return sp;
  } else if (op == STR_MS_TICKS) {
    struct timespec tm;
    clock_gettime(CLOCK_MONOTONIC, &tm);
    *++sp = tm.tv_sec * 1000 + tm.tv_nsec / 1000000;
    return sp;
  } else if (op == STR_RAW_YIELD) {
    return sp;
  } else if (op == STR_SPIFFS_BEGIN) {
    sp -= 2;
    *sp = 0;
    return sp;
  } else if (op == STR_pinMode) {
    sp -= 2;
    return sp;
  } else if (op == STR_digitalWrite) {
    sp -= 2;
    return sp;
  } else if (op == STR_gpio_install_isr_service) {
    *sp = 0;
    return sp;
  } else if (op == STR_SERIAL_AVAILABLE) {
    *++sp = 1;
    return sp;
  } else if (op == STR_TERMINATE) {
    exit(*sp--);
    return sp;
  } else if (op == STR_R_O) {
    *++sp = O_RDONLY;
    return sp;
  } else if (op == STR_OPEN_FILE) {
    cell_t mode = *sp--;
    cell_t len = *sp--;
    char filename[1024];
    memcpy(filename, (void *) *sp, len); filename[len] = 0;
    cell_t ret = open(filename, mode, 0777);
    *sp = ret;
    *++sp = ret < 0 ? errno : 0;
    return sp;
  } else if (op == STR_FILE_SIZE) {
    struct stat st;
    cell_t w = fstat(*sp, &st);
    *sp = (cell_t) st.st_size;
    *++sp = w < 0 ? errno : 0;
    return sp;
  } else if (op == STR_READ_FILE) {
    cell_t fd = *sp--;
    cell_t len = *sp--;
    cell_t ret = read(fd, (void *) *sp, len);
    *sp = ret;
    *++sp = ret < 0 ? errno : 0;
    return sp;
  } else if (op == STR_CLOSE_FILE) {
    cell_t ret = close(*sp);
    *sp = ret ? errno : 0;
    return sp;
  } else if (op == STR_getChipModel) {
    *++sp = (cell_t) "FAKE-ESP32";
    return sp;
  } else if (op == STR_getCpuFreqMHz) {
    *++sp = 240;
    return sp;
  } else if (op == STR_getChipCores) {
    *++sp = 2;
    return sp;
  } else if (op == STR_getFlashChipSize) {
    *++sp = 4 * 1024 * 1024;
    return sp;
  } else if (op == STR_getFreeHeap) {
    *++sp = 90000;
    return sp;
  } else if (op == STR_getHeapSize) {
    *++sp = 320 * 1024;
    return sp;
  } else if (op == STR_getMaxAllocHeap) {
    *++sp = 80 * 1024;
    return sp;
  } else {
    fprintf(stderr, "MISSING SIM OPCODE: %s\n", op);
    exit(1);
    return sp;
  }
}

int main(int argc, char *argv[]) {
  setup();
  for (;;) {
    loop();
  }
}
