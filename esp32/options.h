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
#define INTERRUPT_STACK_CELLS 64
#define MINIMUM_FREE_SYSTEM_HEAP (64 * 1024)

// Default on several options.
#define ENABLE_SPIFFS_SUPPORT
#define ENABLE_WIFI_SUPPORT
#define ENABLE_MDNS_SUPPORT
#define ENABLE_I2C_SUPPORT
#define ENABLE_SOCKETS_SUPPORT
#define ENABLE_FREERTOS_SUPPORT
#define ENABLE_INTERRUPTS_SUPPORT
#define ENABLE_LEDC_SUPPORT
#define ENABLE_SD_SUPPORT
#define ENABLE_SPI_FLASH_SUPPORT
#define ENABLE_ESP32_FORTH_FAULT_HANDLING

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

// RMT support designed around v2.0.1 toolchain.
// While ESP32 also has RMT, for now only include for
// ESP32-S2 and ESP32-C3.
#if defined(CONFIG_IDF_TARGET_ESP32S2) || \
    defined(CONFIG_IDF_TARGET_ESP32C3) || \
    defined(SIM_PRINT_ONLY)
# define ENABLE_RMT_SUPPORT
#endif

// ESP32-C3 doesn't support fault handling yet.
#if !defined(CONFIG_IDF_TARGET_ESP32C3)
#endif

// Uncomment this #define for OLED Support.
// You will need to install these libraries from the Library Manager:
//   Adafruit SSD1306
//   Adafruit GFX Library
//   Adafruit BusIO
//#define ENABLE_OLED_SUPPORT

// For now assume only boards with PSRAM should enable
// camera support and BluetoothSerial.
// ESP32-CAM always have PSRAM, but so do WROVER boards,
// so this isn't an ideal indicator.
// Also limiting to ESP32 classic only, as these can't be ESP32-CAM.
// Some boards (e.g. ESP32-S2-WROVER) don't seem to have
// built the serial library, so check if its enabled as well.
#if (defined(CONFIG_IDF_TARGET_ESP32) && defined(BOARD_HAS_PSRAM)) || defined(SIM_PRINT_ONLY)
# define ENABLE_CAMERA_SUPPORT
# if (defined(CONFIG_BT_ENABLED) && \
      defined(CONFIG_BLUEDROID_ENABLED)) || \
     defined(SIM_PRINT_ONLY)
#  define ENABLE_SERIAL_BLUETOOTH_SUPPORT
# endif
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
  V(ledc) V(Wire) V(WiFi) V(bluetooth) V(sockets) V(oled) \
  V(rmt) V(interrupts) V(spi_flash) V(camera) V(timers) \
  USER_VOCABULARIES
