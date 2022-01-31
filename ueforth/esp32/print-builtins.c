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

#define ENABLE_SPIFFS_SUPPORT
#define ENABLE_WIFI_SUPPORT
#define ENABLE_MDNS_SUPPORT
#define ENABLE_WEBSERVER_SUPPORT
#define ENABLE_I2C_SUPPORT
#define ENABLE_SOCKETS_SUPPORT
#define ENABLE_FREERTOS_SUPPORT
#define ENABLE_INTERRUPTS_SUPPORT
#define ENABLE_LEDC_SUPPORT
#define ENABLE_SD_SUPPORT
#define ENABLE_SPI_FLASH_SUPPORT

#define ENABLE_SD_MMC_SUPPORT
#define ENABLE_DAC_SUPPORT
#define ENABLE_RMT_SUPPORT
#define ENABLE_OLED_SUPPORT
#define ENABLE_SERIAL_BLUETOOTH_SUPPORT

#define FLOATING_POINT_LIST
#define USER_WORDS
#include "builtins.h"

#define Y(name, code) X(#name, name, code)

int main() {
#define X(str, name, code) printf("%s\n", str);
  PLATFORM_OPCODE_LIST
#undef X
  return 0;
}
