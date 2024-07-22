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

#define STACK_CELLS 512
#define MINIMUM_FREE_SYSTEM_HEAP (64 * 1024)

// Default on several options.
#define ENABLE_SPIFFS_SUPPORT
#define ENABLE_WIFI_SUPPORT
#define ENABLE_MDNS_SUPPORT
#define ENABLE_I2C_SUPPORT
#define ENABLE_SOCKETS_SUPPORT
#define ENABLE_FREERTOS_SUPPORT
#define ENABLE_SD_SUPPORT
#define ENABLE_ESP32_FORTH_FAULT_HANDLING

// LEDC changed interface in v3.x+
#if ESP_ARDUINO_VERSION_MAJOR >= 3
# define ENABLE_LEDC_V3_SUPPORT
#else
# define ENABLE_LEDC_V2_SUPPORT
#endif

// SD_MMC does not work on ESP32-S2 / ESP32-C3
#if !defined(CONFIG_IDF_TARGET_ESP32S2) && !defined(CONFIG_IDF_TARGET_ESP32C3)
# define ENABLE_SD_MMC_SUPPORT
#endif

// Serial2 does not work on ESP32-S2 / ESP32-C3
#if !defined(CONFIG_IDF_TARGET_ESP32S2) && !defined(CONFIG_IDF_TARGET_ESP32C3)
# define ENABLE_SERIAL2_SUPPORT
#endif

// No DACS on ESP32-S3 and ESP32-C3.
#if !defined(CONFIG_IDF_TARGET_ESP32S3) && !defined(CONFIG_IDF_TARGET_ESP32C3)
# define ENABLE_DAC_SUPPORT
#endif

// ESP32-C3 doesn't support fault handling yet.
#if !defined(CONFIG_IDF_TARGET_ESP32C3)
#endif

#if !defined(USER_VOCABULARIES)
# define USER_VOCABULARIES
#endif

#if defined(CONFIG_IDF_TARGET_ESP32)
# define UEFORTH_PLATFORM_IS_ESP32 -1
#else
# define UEFORTH_PLATFORM_IS_ESP32 0
#endif

#if defined(CONFIG_IDF_TARGET_ESP32S2)
# define UEFORTH_PLATFORM_IS_ESP32S2 -1
#else
# define UEFORTH_PLATFORM_IS_ESP32S2 0
#endif

#if defined(CONFIG_IDF_TARGET_ESP32S3)
# define UEFORTH_PLATFORM_IS_ESP32S3 -1
#else
# define UEFORTH_PLATFORM_IS_ESP32S3 0
#endif

#if defined(CONFIG_IDF_TARGET_ESP32C3)
# define UEFORTH_PLATFORM_IS_ESP32C3 -1
#else
# define UEFORTH_PLATFORM_IS_ESP32C3 0
#endif

#if defined(BOARD_HAS_PSRAM)
# define UEFORTH_PLATFORM_HAS_PSRAM -1
#else
# define UEFORTH_PLATFORM_HAS_PSRAM 0
#endif

#if defined(BOARD_HAS_PSRAM)
# define UEFORTH_PLATFORM_HAS_PSRAM -1
#else
# define UEFORTH_PLATFORM_HAS_PSRAM 0
#endif

#define VOCABULARY_LIST \
  V(forth) V(internals) \
  V(rtos) V(SPIFFS) V(serial) V(SD) V(SD_MMC) V(ESP) \
  V(ledc) V(Wire) V(WiFi) V(sockets) \
  OPTIONAL_CAMERA_VOCABULARY \
  OPTIONAL_BLUETOOTH_VOCABULARY \
  OPTIONAL_INTERRUPTS_VOCABULARIES \
  OPTIONAL_OLED_VOCABULARY \
  OPTIONAL_RMT_VOCABULARY \
  OPTIONAL_SPI_FLASH_VOCABULARY \
  USER_VOCABULARIES
