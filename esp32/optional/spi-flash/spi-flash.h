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

/*
 * ESP32forth SPI Flash v{{VERSION}}
 * Revision: {{REVISION}}
 */

#ifndef SIM_PRINT_ONLY
# include "esp_spi_flash.h"
# include "esp_partition.h"
#endif
#define OPTIONAL_SPI_FLASH_VOCABULARY V(spi_flash)
#define OPTIONAL_SPI_FLASH_SUPPORT \
  XV(internals, "spi-flash-source", SPI_FLASH_SOURCE, \
      PUSH spi_flash_source; PUSH sizeof(spi_flash_source) - 1) \
  YV(spi_flash, spi_flash_init, spi_flash_init()) \
  YV(spi_flash, spi_flash_get_chip_size, PUSH spi_flash_get_chip_size()) \
  YV(spi_flash, spi_flash_erase_sector, n0 = spi_flash_erase_sector(n0)) \
  YV(spi_flash, spi_flash_erase_range, n0 = spi_flash_erase_range(n1, n0); DROP) \
  YV(spi_flash, spi_flash_write, n0 = spi_flash_write(n2, a1, n0); NIPn(2)) \
  YV(spi_flash, spi_flash_write_encrypted, n0 = spi_flash_write_encrypted(n2, a1, n0); NIPn(2)) \
  YV(spi_flash, spi_flash_read, n0 = spi_flash_read(n2, a1, n0); NIPn(2)) \
  YV(spi_flash, spi_flash_read_encrypted, n0 = spi_flash_read_encrypted(n2, a1, n0); NIPn(2)) \
  YV(spi_flash, spi_flash_mmap, \
      n0 = spi_flash_mmap(n4, n3, (spi_flash_mmap_memory_t) n2, \
                          (const void **) a1, (spi_flash_mmap_handle_t *) a0); NIPn(4)) \
  YV(spi_flash, spi_flash_mmap_pages, \
      n0 = spi_flash_mmap_pages((const int *) a4, n3, (spi_flash_mmap_memory_t) n2, \
                                (const void **) a1, (spi_flash_mmap_handle_t *) a0); NIPn(4)) \
  YV(spi_flash, spi_flash_munmap, spi_flash_munmap((spi_flash_mmap_handle_t) a0); DROP) \
  YV(spi_flash, spi_flash_mmap_dump, spi_flash_mmap_dump()) \
  YV(spi_flash, spi_flash_mmap_get_free_pages, \
      n0 = spi_flash_mmap_get_free_pages((spi_flash_mmap_memory_t) n0)) \
  YV(spi_flash, spi_flash_cache2phys, n0 = spi_flash_cache2phys(a0)) \
  YV(spi_flash, spi_flash_phys2cache, \
      n0 = (cell_t) spi_flash_phys2cache(n1, (spi_flash_mmap_memory_t) n0); NIP) \
  YV(spi_flash, spi_flash_cache_enabled, PUSH spi_flash_cache_enabled()) \
  YV(spi_flash, esp_partition_find, \
      n0 = (cell_t) esp_partition_find((esp_partition_type_t) n2, \
                                       (esp_partition_subtype_t) n1, c0); NIPn(2)) \
  YV(spi_flash, esp_partition_find_first, \
      n0 = (cell_t) esp_partition_find_first((esp_partition_type_t) n2, \
                                             (esp_partition_subtype_t) n1, c0); NIPn(2)) \
  YV(spi_flash, esp_partition_t_size, PUSH sizeof(esp_partition_t)) \
  YV(spi_flash, esp_partition_get, \
      n0 = (cell_t) esp_partition_get((esp_partition_iterator_t) a0)) \
  YV(spi_flash, esp_partition_next, \
      n0 = (cell_t) esp_partition_next((esp_partition_iterator_t) a0)) \
  YV(spi_flash, esp_partition_iterator_release, \
      esp_partition_iterator_release((esp_partition_iterator_t) a0); DROP) \
  YV(spi_flash, esp_partition_verify, n0 = (cell_t) esp_partition_verify((esp_partition_t *) a0)) \
  YV(spi_flash, esp_partition_read, \
      n0 = esp_partition_read((const esp_partition_t *) a3, n2, a1, n0); NIPn(3)) \
  YV(spi_flash, esp_partition_write, \
      n0 = esp_partition_write((const esp_partition_t *) a3, n2, a1, n0); NIPn(3)) \
  YV(spi_flash, esp_partition_erase_range, \
      n0 = esp_partition_erase_range((const esp_partition_t *) a2, n1, n0); NIPn(2)) \
  YV(spi_flash, esp_partition_mmap, \
      n0 = esp_partition_mmap((const esp_partition_t *) a5, n4, n3, \
                              (spi_flash_mmap_memory_t) n2, \
                              (const void **) a1, \
                              (spi_flash_mmap_handle_t *) a0); NIPn(5)) \
  YV(spi_flash, esp_partition_get_sha256, \
      n0 = esp_partition_get_sha256((const esp_partition_t *) a1, b0); NIP) \
  YV(spi_flash, esp_partition_check_identity, \
      n0 = esp_partition_check_identity((const esp_partition_t *) a1, \
                                        (const esp_partition_t *) a0); NIP)

{{spi_flash}}
