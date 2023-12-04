// Copyright 2023 Bradley D. Nelson
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
#include <errno.h>
#include <unistd.h>
#include <fcntl.h>
#include <sys/types.h>
#include <sys/stat.h>

#ifndef UEFORTH_SIM
# include "pico/time.h"
# include "ice_cram.h"
# include "ice_flash.h"
# include "ice_fpga.h"
# include "ice_led.h"
# include "ice_spi.h"
# include "ice_sram.h"
#endif

// TODO: Implement RESIZE-FILE.
// TODO: Implement FLUSH-FILE.

#define PLATFORM_OPCODE_LIST \
  REQUIRED_MEMORY_SUPPORT \
  REQUIRED_SYSTEM_SUPPORT \
  REQUIRED_FILES_SUPPORT \
  OPTIONAL_CRAM_SUPPORT \
  OPTIONAL_FLASH_SUPPORT \
  OPTIONAL_FPGA_SUPPORT \
  OPTIONAL_LED_SUPPORT \
  OPTIONAL_SPI_SUPPORT \
  OPTIONAL_SRAM_SUPPORT \
  CALLING_OPCODE_LIST \
  FLOATING_POINT_LIST

#define REQUIRED_MEMORY_SUPPORT \
  YV(internals, MALLOC, SET malloc(n0)) \
  YV(internals, SYSFREE, free(a0); DROP) \
  YV(internals, REALLOC, SET realloc(a1, n0); NIP)

#ifndef UEFORTH_SIM
# define REQUIRED_SYSTEM_SUPPORT \
  X("MS-TICKS", MS_TICKS, PUSH us_to_ms(get_absolute_time())) \
  XV(internals, "RAW-YIELD", RAW_YIELD, tud_task()) \
  XV(internals, "RAW-TERMINATE", RAW_TERMINATE, exit(n0); DROP) \
  YV(internals, getchar_timeout_us, n0 = getchar_timeout_us(n0))
#else
# define REQUIRED_SYSTEM_SUPPORT \
  X("MS-TICKS", MS_TICKS, PUSH (time(0) * 1000)) \
  XV(internals, "RAW-YIELD", RAW_YIELD, sleep(0)) \
  XV(internals, "RAW-TERMINATE", RAW_TERMINATE, exit(n0); DROP) \
  YV(internals, getchar_timeout_us, DROP; PUSH fgetc(stdin))
#endif

#define REQUIRED_FILES_SUPPORT \
  X("R/O", R_O, PUSH O_RDONLY) \
  X("W/O", W_O, PUSH O_WRONLY) \
  X("R/W", R_W, PUSH O_RDWR) \
  Y(BIN, ) \
  X("CLOSE-FILE", CLOSE_FILE, tos = close(tos); tos = tos ? errno : 0) \
  X("OPEN-FILE", OPEN_FILE, cell_t mode = n0; DROP; cell_t len = n0; DROP; \
    memcpy(filename, a0, len); filename[len] = 0; \
    n0 = open(filename, mode, 0777); PUSH n0 < 0 ? errno : 0) \
  X("CREATE-FILE", CREATE_FILE, cell_t mode = n0; DROP; cell_t len = n0; DROP; \
    memcpy(filename, a0, len); filename[len] = 0; \
    n0 = open(filename, mode | O_CREAT | O_TRUNC); PUSH n0 < 0 ? errno : 0) \
  X("DELETE-FILE", DELETE_FILE, cell_t len = n0; DROP; \
    memcpy(filename, a0, len); filename[len] = 0; \
    n0 = unlink(filename); n0 = n0 ? errno : 0) \
  X("RENAME-FILE", RENAME_FILE, \
    cell_t len = n0; DROP; memcpy(filename, a0, len); filename[len] = 0; DROP; \
    cell_t len2 = n0; DROP; memcpy(filename2, a0, len2); filename2[len2] = 0; \
    n0 = rename(filename2, filename); n0 = n0 ? errno : 0) \
  X("WRITE-FILE", WRITE_FILE, cell_t fd = n0; DROP; cell_t len = n0; DROP; \
    n0 = write(fd, a0, len); n0 = n0 != len ? errno : 0) \
  X("READ-FILE", READ_FILE, cell_t fd = n0; DROP; cell_t len = n0; DROP; \
    n0 = read(fd, a0, len); PUSH n0 < 0 ? errno : 0) \
  X("FILE-POSITION", FILE_POSITION, \
    n0 = (cell_t) lseek(n0, 0, SEEK_CUR); PUSH n0 < 0 ? errno : 0) \
  X("REPOSITION-FILE", REPOSITION_FILE, cell_t fd = n0; DROP; \
    n0 = (cell_t) lseek(fd, tos, SEEK_SET); n0 = n0 < 0 ? errno : 0) \
  X("FILE-SIZE", FILE_SIZE, struct stat st; w = fstat(n0, &st); \
    n0 = (cell_t) st.st_size; PUSH w < 0 ? errno : 0) \
  X("NON-BLOCK", NON_BLOCK, n0 = fcntl(n0, F_SETFL, O_NONBLOCK); \
    n0 = n0 < 0 ? errno : 0)

#ifdef UEFORTH_SIM
# define OPTIONAL_CRAM_SUPPORT
#else
# define OPTIONAL_CRAM_SUPPORT \
   YV(ice, ice_cram_open, ice_cram_open()) \
   YV(ice, ice_cram_write, ice_cram_write(b1, n0); DROPn(2)) \
   YV(ice, ice_cram_close, ice_cram_close())
#endif

#ifdef UEFORTH_SIM
# define OPTIONAL_FLASH_SUPPORT
#else
# define OPTIONAL_FLASH_SUPPORT \
   YV(ice, ICE_FLASH_PAGE_SIZE, PUSH ICE_FLASH_PAGE_SIZE) \
   YV(ice, ice_flash_init, ice_flash_init()) \
   YV(ice, ice_flash_read, ice_flash_read(n2, b1, n0); DROPn(3)) \
   YV(ice, ice_flash_erase_sector, ice_flash_erase_sector(n0); DROP) \
   YV(ice, ice_flash_program_page, ice_flash_program_page(n1, b0); DROPn(2)) \
   YV(ice, ice_flash_erase_chip, ice_flash_erase_chip()) \
   YV(ice, ice_flash_wakeup, ice_flash_wakeup()) \
   YV(ice, ice_flash_sleep, ice_flash_sleep())
#endif

#ifdef UEFORTH_SIM
# define OPTIONAL_FPGA_SUPPORT
#else
# define OPTIONAL_FPGA_SUPPORT \
   YV(ice, ice_fpga_init, ice_fpga_init(n0); DROP) \
   YV(ice, ice_fpga_stop, ice_fpga_stop(); DROP) \
   YV(ice, ice_fpga_start, PUSH (ice_fpga_start() ? -1 :0))
#endif

#ifdef UEFORTH_SIM
# define OPTIONAL_LED_SUPPORT
#else
# define OPTIONAL_LED_SUPPORT \
   YV(ice, ice_led_init, ice_led_init()) \
   YV(ice, ice_led_red, ice_led_red(n0); DROP) \
   YV(ice, ice_led_green, ice_led_green(n0); DROP) \
   YV(ice, ice_led_blue, ice_led_blue(n0); DROP)
#endif

#ifdef UEFORTH_SIM
# define OPTIONAL_SPI_SUPPORT
#else
# define OPTIONAL_SPI_SUPPORT \
   YV(ice, ice_spi_init, ice_spi_init()) \
   YV(ice, ice_spi_chip_select, ice_spi_chip_select(n0); DROP) \
   YV(ice, ice_spi_chip_deselect, ice_spi_chip_deselect(n0); DROP) \
   YV(ice, ice_spi_read_blocking, ice_spi_read_blocking(b1, n0); DROPn(2)) \
   YV(ice, ice_spi_write_blocking, ice_spi_write_blocking(b1, n0); DROPn(2))
#endif

#ifdef UEFORTH_SIM
# define OPTIONAL_SRAM_SUPPORT
#else
# define OPTIONAL_SRAM_SUPPORT \
   YV(ice, ice_sram_init, ice_sram_init()) \
   YV(ice, ice_sram_get_id, ice_sram_get_id(b0); DROP) \
   YV(ice, ice_sram_read_blocking, ice_sram_read_blocking(n2, b1, n0); DROPn(3)) \
   YV(ice, ice_sram_write_blocking, ice_sram_write_blocking(n2, b1, n0); DROPn(3))
#endif

#define VOCABULARY_LIST V(forth) V(internals) V(ice)

#define PATH_MAX 256
static char filename[PATH_MAX];
static char filename2[PATH_MAX];
