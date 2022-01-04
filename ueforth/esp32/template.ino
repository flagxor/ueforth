/*
 * Copyright 2021 Bradley D. Nelson
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

/*
 * ESP32forth v{{VERSION}}
 * Revision: {{REVISION}}
 */

{{opcodes}}
{{floats}}
{{calling}}

// For now, default on several options.
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

// SD_MMC does not work on ESP32-S2 / ESP32-C3
#if !defined(CONFIG_IDF_TARGET_ESP32S2) && !defined(CONFIG_IDF_TARGET_ESP32C3)
# define ENABLE_SD_MMC_SUPPORT
#endif

// ESP32-C3 has no DACs.
#if !defined(CONFIG_IDF_TARGET_ESP32C3)
# define ENABLE_DAC_SUPPORT
#endif

// RMT support designed around v2.0.1 toolchain.
// While ESP32 also has RMT, for now only include for
// ESP32-S2 and ESP32-C3.
#if defined(CONFIG_IDF_TARGET_ESP32S2) || defined(CONFIG_IDF_TARGET_ESP32C3)
# define ENABLE_RMT_SUPPORT
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
// Some boards (e.g. ESP32-S2-WROVER) don't seem to have
// built the serial library, so check if its enabled as well.
#ifdef BOARD_HAS_PSRAM
# define ENABLE_CAMERA_SUPPORT
# if defined(CONFIG_BT_ENABLED) && defined(CONFIG_BLUEDROID_ENABLED)
#  define ENABLE_SERIAL_BLUETOOTH_SUPPORT
# endif
#endif

#ifdef ENABLE_WEBSERVER_SUPPORT
# include "WebServer.h"
#endif

#include <errno.h>
#include <unistd.h>
#include <fcntl.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <sys/select.h>

#define HEAP_SIZE (100 * 1024)
#define STACK_SIZE 512
#define INTERRUPT_STACK_CELLS 64

// Optional hook to pull in words for userwords.h
#if __has_include("userwords.h")
# include "userwords.h"
#else
# define USER_WORDS
#endif

#define PLATFORM_OPCODE_LIST \
  FLOATING_POINT_LIST \
  REQUIRED_MEMORY_SUPPORT \
  REQUIRED_SERIAL_SUPPORT \
  REQUIRED_ARDUINO_GPIO_SUPPORT \
  REQUIRED_SYSTEM_SUPPORT \
  REQUIRED_FILES_SUPPORT \
  OPTIONAL_LEDC_SUPPORT \
  OPTIONAL_DAC_SUPPORT \
  OPTIONAL_SPIFFS_SUPPORT \
  OPTIONAL_WIFI_SUPPORT \
  OPTIONAL_MDNS_SUPPORT \
  OPTIONAL_WEBSERVER_SUPPORT \
  OPTIONAL_SD_SUPPORT \
  OPTIONAL_SD_MMC_SUPPORT \
  OPTIONAL_I2C_SUPPORT \
  OPTIONAL_SERIAL_BLUETOOTH_SUPPORT \
  OPTIONAL_CAMERA_SUPPORT \
  OPTIONAL_SOCKETS_SUPPORT \
  OPTIONAL_FREERTOS_SUPPORT \
  OPTIONAL_INTERRUPTS_SUPPORT \
  OPTIONAL_RMT_SUPPORT \
  OPTIONAL_OLED_SUPPORT \
  USER_WORDS

#define REQUIRED_MEMORY_SUPPORT \
  Y(MALLOC, SET malloc(n0)) \
  Y(SYSFREE, free(a0); DROP) \
  Y(REALLOC, SET realloc(a1, n0); NIP) \
  Y(heap_caps_malloc, SET heap_caps_malloc(n1, n0); NIP) \
  Y(heap_caps_free, heap_caps_free(a0); DROP) \
  Y(heap_caps_realloc, \
      tos = (cell_t) heap_caps_realloc(a2, n1, n0); NIPn(2))

#define REQUIRED_SYSTEM_SUPPORT \
  X("MS-TICKS", MS_TICKS, PUSH millis()) \
  X("RAW-YIELD", RAW_YIELD, yield()) \
  Y(TERMINATE, exit(n0))

#define REQUIRED_SERIAL_SUPPORT \
  X("Serial.begin", SERIAL_BEGIN, Serial.begin(tos); DROP) \
  X("Serial.end", SERIAL_END, Serial.end()) \
  X("Serial.available", SERIAL_AVAILABLE, PUSH Serial.available()) \
  X("Serial.readBytes", SERIAL_READ_BYTES, n0 = Serial.readBytes(b1, n0); NIP) \
  X("Serial.write", SERIAL_WRITE, n0 = Serial.write(b1, n0); NIP) \
  X("Serial.flush", SERIAL_FLUSH, Serial.flush())

#define REQUIRED_ARDUINO_GPIO_SUPPORT \
  Y(pinMode, pinMode(n1, n0); DROPn(2)) \
  Y(digitalWrite, digitalWrite(n1, n0); DROPn(2)) \
  Y(digitalRead, n0 = digitalRead(n0)) \
  Y(analogRead, n0 = analogRead(n0)) \
  Y(pulseIn, n0 = pulseIn(n2, n1, n0); NIPn(2))

#define REQUIRED_FILES_SUPPORT \
  X("R/O", R_O, PUSH O_RDONLY) \
  X("R/W", R_W, PUSH O_RDWR) \
  X("W/O", W_O, PUSH O_WRONLY) \
  Y(BIN, ) \
  X("CLOSE-FILE", CLOSE_FILE, tos = close(tos); tos = tos ? errno : 0) \
  X("FLUSH-FILE", FLUSH_FILE, fsync(tos); /* fsync has no impl and returns ENOSYS :-( */ tos = 0) \
  X("OPEN-FILE", OPEN_FILE, cell_t mode = n0; DROP; cell_t len = n0; DROP; \
    memcpy(filename, a0, len); filename[len] = 0; \
    n0 = open(filename, mode, 0777); PUSH n0 < 0 ? errno : 0) \
  X("CREATE-FILE", CREATE_FILE, cell_t mode = n0; DROP; cell_t len = n0; DROP; \
    memcpy(filename, a0, len); filename[len] = 0; \
    n0 = open(filename, mode | O_CREAT | O_TRUNC); PUSH n0 < 0 ? errno : 0) \
  X("DELETE-FILE", DELETE_FILE, cell_t len = n0; DROP; \
    memcpy(filename, a0, len); filename[len] = 0; \
    n0 = unlink(filename); n0 = n0 ? errno : 0) \
  X("WRITE-FILE", WRITE_FILE, cell_t fd = n0; DROP; cell_t len = n0; DROP; \
    n0 = write(fd, a0, len); n0 = n0 != len ? errno : 0) \
  X("READ-FILE", READ_FILE, cell_t fd = n0; DROP; cell_t len = n0; DROP; \
    n0 = read(fd, a0, len); PUSH n0 < 0 ? errno : 0) \
  X("FILE-POSITION", FILE_POSITION, \
    n0 = (cell_t) lseek(n0, 0, SEEK_CUR); PUSH n0 < 0 ? errno : 0) \
  X("REPOSITION-FILE", REPOSITION_FILE, cell_t fd = n0; DROP; \
    n0 = (cell_t) lseek(fd, tos, SEEK_SET); n0 = n0 < 0 ? errno : 0) \
  X("RESIZE-FILE", RESIZE_FILE, cell_t fd = n0; DROP; n0 = ResizeFile(fd, tos)) \
  X("FILE-SIZE", FILE_SIZE, struct stat st; w = fstat(n0, &st); \
    n0 = (cell_t) st.st_size; PUSH w < 0 ? errno : 0) \
  X("NON-BLOCK", NON_BLOCK, n0 = fcntl(n0, F_SETFL, O_NONBLOCK); \
    n0 = n0 < 0 ? errno : 0)

#ifndef ENABLE_LEDC_SUPPORT
# define OPTIONAL_LEDC_SUPPORT
#else
# define OPTIONAL_LEDC_SUPPORT \
  Y(ledcSetup, \
      n0 = (cell_t) (1000000 * ledcSetup(n2, n1 / 1000.0, n0)); NIPn(2)) \
  Y(ledcAttachPin, ledcAttachPin(n1, n0); DROPn(2)) \
  Y(ledcDetachPin, ledcDetachPin(n0); DROP) \
  Y(ledcRead, n0 = ledcRead(n0)) \
  Y(ledcReadFreq, n0 = (cell_t) (1000000 * ledcReadFreq(n0))) \
  Y(ledcWrite, ledcWrite(n1, n0); DROPn(2)) \
  Y(ledcWriteTone, \
      n0 = (cell_t) (1000000 * ledcWriteTone(n1, n0 / 1000.0)); NIP) \
  Y(ledcWriteNote, \
      tos = (cell_t) (1000000 * ledcWriteNote(n2, (note_t) n1, n0)); NIPn(2))
#endif

#ifndef ENABLE_DAC_SUPPORT
# define OPTIONAL_DAC_SUPPORT
# else
# define OPTIONAL_DAC_SUPPORT \
  Y(dacWrite, dacWrite(n1, n0); DROPn(2))
#endif

#ifndef ENABLE_SPIFFS_SUPPORT
// Provide a default failing SPIFFS.begin
# define OPTIONAL_SPIFFS_SUPPORT \
  X("SPIFFS.begin", SPIFFS_BEGIN, NIPn(2); n0 = 0)
#else
# include "SPIFFS.h"
# define OPTIONAL_SPIFFS_SUPPORT \
  X("SPIFFS.begin", SPIFFS_BEGIN, \
      tos = SPIFFS.begin(n2, c1, n0); NIPn(2)) \
  X("SPIFFS.end", SPIFFS_END, SPIFFS.end()) \
  X("SPIFFS.format", SPIFFS_FORMAT, PUSH SPIFFS.format()) \
  X("SPIFFS.totalBytes", SPIFFS_TOTAL_BYTES, PUSH SPIFFS.totalBytes()) \
  X("SPIFFS.usedBytes", SPIFFS_USED_BYTES, PUSH SPIFFS.usedBytes())
#endif

#ifndef ENABLE_FREERTOS_SUPPORT
# define OPTIONAL_FREERTOS_SUPPORT
#else
# include "freertos/FreeRTOS.h"
# include "freertos/task.h"
# define OPTIONAL_FREERTOS_SUPPORT \
  Y(vTaskDelete, vTaskDelete((TaskHandle_t) n0); DROP) \
  Y(xTaskCreatePinnedToCore, n0 = xTaskCreatePinnedToCore((TaskFunction_t) a6, \
        c5, n4, a3, (UBaseType_t) n2, (TaskHandle_t *) a1, (BaseType_t) n0); NIPn(6)) \
  Y(xPortGetCoreID, PUSH xPortGetCoreID())
#endif

#ifndef ENABLE_INTERRUPTS_SUPPORT
# define OPTIONAL_INTERRUPTS_SUPPORT
#else
# include "esp_intr_alloc.h"
# include "driver/timer.h"
# include "driver/gpio.h"
# define OPTIONAL_INTERRUPTS_SUPPORT \
  Y(gpio_config, n0 = gpio_config((const gpio_config_t *) a0)) \
  Y(gpio_reset_pin, n0 = gpio_reset_pin((gpio_num_t) n0)) \
  Y(gpio_set_intr_type, n0 = gpio_set_intr_type((gpio_num_t) n1, (gpio_int_type_t) n0); NIP) \
  Y(gpio_intr_enable, n0 = gpio_intr_enable((gpio_num_t) n0)) \
  Y(gpio_intr_disable, n0 = gpio_intr_disable((gpio_num_t) n0)) \
  Y(gpio_set_level, n0 = gpio_set_level((gpio_num_t) n1, n0); NIP) \
  Y(gpio_get_level, n0 = gpio_get_level((gpio_num_t) n0)) \
  Y(gpio_set_direction, n0 = gpio_set_direction((gpio_num_t) n1, (gpio_mode_t) n0); NIP) \
  Y(gpio_set_pull_mode, n0 = gpio_set_pull_mode((gpio_num_t) n1, (gpio_pull_mode_t) n0); NIP) \
  Y(gpio_wakeup_enable, n0 = gpio_wakeup_enable((gpio_num_t) n1, (gpio_int_type_t) n0); NIP) \
  Y(gpio_wakeup_disable, n0 = gpio_wakeup_disable((gpio_num_t) n0)) \
  Y(gpio_pullup_en, n0 = gpio_pullup_en((gpio_num_t) n0)) \
  Y(gpio_pullup_dis, n0 = gpio_pullup_dis((gpio_num_t) n0)) \
  Y(gpio_pulldown_en, n0 = gpio_pulldown_en((gpio_num_t) n0)) \
  Y(gpio_pulldown_dis, n0 = gpio_pulldown_dis((gpio_num_t) n0)) \
  Y(gpio_hold_en, n0 = gpio_hold_en((gpio_num_t) n0)) \
  Y(gpio_hold_dis, n0 = gpio_hold_dis((gpio_num_t) n0)) \
  Y(gpio_deep_sleep_hold_en, gpio_deep_sleep_hold_en()) \
  Y(gpio_deep_sleep_hold_dis, gpio_deep_sleep_hold_dis()) \
  Y(gpio_install_isr_service, n0 = gpio_install_isr_service(n0)) \
  Y(gpio_uninstall_isr_service, gpio_uninstall_isr_service()) \
  Y(gpio_isr_handler_add, n0 = GpioIsrHandlerAdd(n2, n1, n0); NIPn(2)) \
  Y(gpio_isr_handler_remove, n0 = gpio_isr_handler_remove((gpio_num_t) n0)) \
  Y(gpio_set_drive_capability, n0 = gpio_set_drive_capability((gpio_num_t) n1, (gpio_drive_cap_t) n0); NIP) \
  Y(gpio_get_drive_capability, n0 = gpio_get_drive_capability((gpio_num_t) n1, (gpio_drive_cap_t *) a0); NIP) \
  Y(esp_intr_alloc, n0 = EspIntrAlloc(n4, n3, n2, n1, a0); NIPn(4)) \
  Y(esp_intr_free, n0 = esp_intr_free((intr_handle_t) n0)) \
  Y(timer_isr_register, n0 = TimerIsrRegister(n5, n4, n3, n2, n1, a0); NIPn(5))
#endif

#ifndef ENABLE_RMT_SUPPORT
# define OPTIONAL_RMT_SUPPORT
#else
# include "driver/rmt.h"
# define OPTIONAL_RMT_SUPPORT \
  Y(rmt_set_clk_div, n0 = rmt_set_clk_div((rmt_channel_t) n1, n0); NIP) \
  Y(rmt_get_clk_div, n0 = rmt_get_clk_div((rmt_channel_t) n1, b0); NIP) \
  Y(rmt_set_rx_idle_thresh, n0 = rmt_set_rx_idle_thresh((rmt_channel_t) n1, n0); NIP) \
  Y(rmt_get_rx_idle_thresh, \
    n0 = rmt_get_rx_idle_thresh((rmt_channel_t) n1, (uint16_t *) a0); NIP) \
  Y(rmt_set_mem_block_num, n0 = rmt_set_mem_block_num((rmt_channel_t) n1, n0); NIP) \
  Y(rmt_get_mem_block_num, n0 = rmt_get_mem_block_num((rmt_channel_t) n1, b0); NIP) \
  Y(rmt_set_tx_carrier, n0 = rmt_set_tx_carrier((rmt_channel_t) n4, n3, n2, n1, \
                                                (rmt_carrier_level_t) n0); NIPn(4)) \
  Y(rmt_set_mem_pd, n0 = rmt_set_mem_pd((rmt_channel_t) n1, n0); NIP) \
  Y(rmt_get_mem_pd, n0 = rmt_get_mem_pd((rmt_channel_t) n1, (bool *) a0); NIP) \
  Y(rmt_tx_start, n0 = rmt_tx_start((rmt_channel_t) n1, n0); NIP) \
  Y(rmt_tx_stop, n0 = rmt_tx_stop((rmt_channel_t) n0)) \
  Y(rmt_rx_start, n0 = rmt_rx_start((rmt_channel_t) n1, n0); NIP) \
  Y(rmt_rx_stop, n0 = rmt_rx_stop((rmt_channel_t) n0)) \
  Y(rmt_tx_memory_reset, n0 = rmt_tx_memory_reset((rmt_channel_t) n0)) \
  Y(rmt_rx_memory_reset, n0 = rmt_rx_memory_reset((rmt_channel_t) n0)) \
  Y(rmt_set_memory_owner, n0 = rmt_set_memory_owner((rmt_channel_t) n1, (rmt_mem_owner_t) n0); NIP) \
  Y(rmt_get_memory_owner, n0 = rmt_get_memory_owner((rmt_channel_t) n1, (rmt_mem_owner_t *) a0); NIP) \
  Y(rmt_set_tx_loop_mode, n0 = rmt_set_tx_loop_mode((rmt_channel_t) n1, n0); NIP) \
  Y(rmt_get_tx_loop_mode, n0 = rmt_get_tx_loop_mode((rmt_channel_t) n1, (bool *) a0); NIP) \
  Y(rmt_set_rx_filter, n0 = rmt_set_rx_filter((rmt_channel_t) n2, n1, n0); NIPn(2)) \
  Y(rmt_set_source_clk, n0 = rmt_set_source_clk((rmt_channel_t) n1, (rmt_source_clk_t) n0); NIP) \
  Y(rmt_get_source_clk, n0 = rmt_get_source_clk((rmt_channel_t) n1, (rmt_source_clk_t * ) a0); NIP) \
  Y(rmt_set_idle_level, n0 = rmt_set_idle_level((rmt_channel_t) n2, n1, \
        (rmt_idle_level_t) n0); NIPn(2)) \
  Y(rmt_get_idle_level, n0 = rmt_get_idle_level((rmt_channel_t) n2, \
        (bool *) a1, (rmt_idle_level_t *) a0); NIPn(2)) \
  Y(rmt_get_status, n0 = rmt_get_status((rmt_channel_t) n1, (uint32_t *) a0); NIP) \
  Y(rmt_set_rx_intr_en, n0 = rmt_set_rx_intr_en((rmt_channel_t) n1, n0); NIP) \
  Y(rmt_set_err_intr_en, n0 = rmt_set_err_intr_en((rmt_channel_t) n1, (rmt_mode_t) n0); NIP) \
  Y(rmt_set_tx_intr_en, n0 = rmt_set_tx_intr_en((rmt_channel_t) n1, n0); NIP) \
  Y(rmt_set_tx_thr_intr_en, n0 = rmt_set_tx_thr_intr_en((rmt_channel_t) n2, n1, n0); NIPn(2)) \
  Y(rmt_set_gpio, n0 = rmt_set_gpio((rmt_channel_t) n3, (rmt_mode_t) n2, (gpio_num_t) n1, n0); NIPn(3)) \
  Y(rmt_config, n0 = rmt_config((const rmt_config_t *) a0)) \
  Y(rmt_isr_register, n0 = rmt_isr_register((void (*)(void*)) a3, a2, n1, \
        (rmt_isr_handle_t *) a0); NIPn(3)) \
  Y(rmt_isr_deregister, n0 = rmt_isr_deregister((rmt_isr_handle_t) n0)) \
  Y(rmt_fill_tx_items, n0 = rmt_fill_tx_items((rmt_channel_t) n3, \
        (rmt_item32_t *) a2, n1, n0); NIPn(3)) \
  Y(rmt_driver_install, n0 = rmt_driver_install((rmt_channel_t) n2, n1, n0); NIPn(2)) \
  Y(rmt_driver_uinstall, n0 = rmt_driver_uninstall((rmt_channel_t) n0)) \
  Y(rmt_get_channel_status, n0 = rmt_get_channel_status((rmt_channel_status_result_t *) a0)) \
  Y(rmt_get_counter_clock, n0 = rmt_get_counter_clock((rmt_channel_t) n1, (uint32_t *) a0); NIP) \
  Y(rmt_write_items, n0 = rmt_write_items((rmt_channel_t) n3, (rmt_item32_t *) a2, n1, n0); NIPn(3)) \
  Y(rmt_wait_tx_done, n0 = rmt_wait_tx_done((rmt_channel_t) n1, n0); NIP) \
  Y(rmt_get_ringbuf_handle, n0 = rmt_get_ringbuf_handle((rmt_channel_t) n1, (RingbufHandle_t *) a0); NIP) \
  Y(rmt_translator_init, n0 = rmt_translator_init((rmt_channel_t) n1, (sample_to_rmt_t) n0); NIP) \
  Y(rmt_translator_set_context, n0 = rmt_translator_set_context((rmt_channel_t) n1, a0); NIP) \
  Y(rmt_translator_get_context, n0 = rmt_translator_get_context((const size_t *) a1, (void **) a0); NIP) \
  Y(rmt_write_sample, n0 = rmt_write_sample((rmt_channel_t) n3, b2, n1, n0); NIPn(3))
#endif

#ifndef ENABLE_CAMERA_SUPPORT
# define OPTIONAL_CAMERA_SUPPORT
#else
# include "esp_camera.h"
# define OPTIONAL_CAMERA_SUPPORT \
  Y(esp_camera_init, n0 = esp_camera_init((camera_config_t *) a0)) \
  Y(esp_camera_deinit, PUSH esp_camera_deinit()) \
  Y(esp_camera_fb_get, PUSH esp_camera_fb_get()) \
  Y(esp_camera_fb_return, esp_camera_fb_return((camera_fb_t *) a0); DROP) \
  Y(esp_camera_sensor_get, PUSH esp_camera_sensor_get())
#endif

#ifndef ENABLE_SOCKETS_SUPPORT
# define OPTIONAL_SOCKETS_SUPPORT
#else
# include <errno.h>
# include <sys/select.h>
# include <sys/socket.h>
# include <sys/time.h>
# include <sys/types.h>
# include <sys/un.h>
# include <sys/poll.h>
# define OPTIONAL_SOCKETS_SUPPORT \
  Y(socket, n0 = socket(n2, n1, n0); NIPn(2)) \
  Y(setsockopt, n0 = setsockopt(n4, n3, n2, a1, n0); NIPn(4)) \
  Y(bind, n0 = bind(n2, (struct sockaddr *) a1, n0); NIPn(2)) \
  Y(listen, n0 = listen(n1, n0); NIP) \
  Y(connect, n0 = connect(n2, (struct sockaddr *) a1, n0); NIPn(2)) \
  Y(sockaccept, n0 = accept(n2, (struct sockaddr *) a1, (socklen_t *) a0); NIPn(2)) \
  Y(select, n0 = select(n4, (fd_set *) a3, (fd_set *) a2, (fd_set *) a1, (struct timeval *) a0); NIPn(4)) \
  Y(poll, n0 = poll((struct pollfd *) a2, (nfds_t) n1, n0); NIPn(2)) \
  Y(errno, PUSH errno)
#endif

#ifndef ENABLE_SD_SUPPORT
# define OPTIONAL_SD_SUPPORT
#else
# include "SD.h"
# define OPTIONAL_SD_SUPPORT \
  X("SD.begin", SD_BEGIN, PUSH SD.begin()) \
  X("SD.beginFull", SD_BEGIN_FULL, \
      tos = SD.begin(n5, *(SPIClass*)a4, n3, c2, n1, n0); NIPn(5)) \
  X("SD.beginDefaults", SD_BEGIN_DEFAULTS, \
      PUSH SS; PUSH &SPI; PUSH 4000000; PUSH "/sd"; PUSH 5; PUSH false) \
  X("SD.end", SD_END, SD.end()) \
  X("SD.cardType", SD_CARD_TYPE, PUSH SD.cardType()) \
  X("SD.totalBytes", SD_TOTAL_BYTES, PUSH SD.totalBytes()) \
  X("SD.usedBytes", SD_USED_BYTES, PUSH SD.usedBytes())
#endif

#ifndef ENABLE_SD_MMC_SUPPORT
# define OPTIONAL_SD_MMC_SUPPORT
#else
# include "SD_MMC.h"
# define OPTIONAL_SD_MMC_SUPPORT \
  X("SD_MMC.begin", SD_MMC_BEGIN, PUSH SD_MMC.begin()) \
  X("SD_MMC.beginFull", SD_MMC_BEGIN_FULL, tos = SD_MMC.begin(c2, n1, n0); NIPn(2)) \
  X("SD_MMC.beginDefaults", SD_MMC_BEGIN_DEFAULTS, \
      PUSH "/sdcard"; PUSH false; PUSH false) \
  X("SD_MMC.end", SD_MMC_END, SD_MMC.end()) \
  X("SD_MMC.cardType", SD_MMC_CARD_TYPE, PUSH SD_MMC.cardType()) \
  X("SD_MMC.totalBytes", SD_MMC_TOTAL_BYTES, PUSH SD_MMC.totalBytes()) \
  X("SD_MMC.usedBytes", SD_MMC_USED_BYTES, PUSH SD_MMC.usedBytes())
#endif

#ifndef ENABLE_I2C_SUPPORT
# define OPTIONAL_I2C_SUPPORT
#else
# include <Wire.h>
# define OPTIONAL_I2C_SUPPORT \
  X("Wire.begin", WIRE_BEGIN, n0 = Wire.begin(n1, n0); NIP) \
  X("Wire.setClock", WIRE_SET_CLOCK, Wire.setClock(n0); DROP) \
  X("Wire.getClock", WIRE_GET_CLOCK, PUSH Wire.getClock()) \
  X("Wire.setTimeout", WIRE_SET_TIMEOUT, Wire.setTimeout(n0); DROP) \
  X("Wire.getTimeout", WIRE_GET_TIMEOUT, PUSH Wire.getTimeout()) \
  X("Wire.beginTransmission", WIRE_BEGIN_TRANSMISSION, Wire.beginTransmission(n0); DROP) \
  X("Wire.endTransmission", WIRE_END_TRANSMISSION, SET Wire.endTransmission(n0)) \
  X("Wire.requestFrom", WIRE_REQUEST_FROM, n0 = Wire.requestFrom(n2, n1, n0); NIPn(2)) \
  X("Wire.write", WIRE_WRITE, n0 = Wire.write(b1, n0); NIP) \
  X("Wire.available", WIRE_AVAILABLE, PUSH Wire.available()) \
  X("Wire.read", WIRE_READ, PUSH Wire.read()) \
  X("Wire.peek", WIRE_PEEK, PUSH Wire.peek()) \
  X("Wire.flush", WIRE_FLUSH, Wire.flush())
#endif

#ifndef ENABLE_SERIAL_BLUETOOTH_SUPPORT
# define OPTIONAL_SERIAL_BLUETOOTH_SUPPORT
#else
# include "esp_bt_device.h"
# include "BluetoothSerial.h"
# define bt0 ((BluetoothSerial *) a0)
# define OPTIONAL_SERIAL_BLUETOOTH_SUPPORT \
  X("SerialBT.new", SERIALBT_NEW, PUSH new BluetoothSerial()) \
  X("SerialBT.delete", SERIALBT_DELETE, delete bt0; DROP) \
  X("SerialBT.begin", SERIALBT_BEGIN, n0 = bt0->begin(c2, n1); NIPn(2)) \
  X("SerialBT.end", SERIALBT_END, bt0->end(); DROP) \
  X("SerialBT.available", SERIALBT_AVAILABLE, n0 = bt0->available()) \
  X("SerialBT.readBytes", SERIALBT_READ_BYTES, n0 = bt0->readBytes(b2, n1); NIPn(2)) \
  X("SerialBT.write", SERIALBT_WRITE, n0 = bt0->write(b2, n1); NIPn(2)) \
  X("SerialBT.flush", SERIALBT_FLUSH, bt0->flush(); DROP) \
  X("SerialBT.hasClient", SERIALBT_HAS_CLIENT, n0 = bt0->hasClient()) \
  X("SerialBT.enableSSP", SERIALBT_ENABLE_SSP, bt0->enableSSP(); DROP) \
  X("SerialBT.setPin", SERIALBT_SET_PIN, n0 = bt0->setPin(c1); NIP) \
  X("SerialBT.unpairDevice", SERIALBT_UNPAIR_DEVICE, \
      n0 = bt0->unpairDevice(b1); NIP) \
  X("SerialBT.connect", SERIALBT_CONNECT, n0 = bt0->connect(c1); NIP) \
  X("SerialBT.connectAddr", SERIALBT_CONNECT_ADDR, n0 = bt0->connect(b1); NIP) \
  X("SerialBT.disconnect", SERIALBT_DISCONNECT, n0 = bt0->disconnect()) \
  X("SerialBT.connected", SERIALBT_CONNECTED, n0 = bt0->connected(n1); NIP) \
  X("SerialBT.isReady", SERIALBT_IS_READY, n0 = bt0->isReady(n2, n1); NIPn(2)) \
  /* Bluetooth */ \
  Y(esp_bt_dev_get_address, PUSH esp_bt_dev_get_address())
#endif

#ifndef ENABLE_WIFI_SUPPORT
# define OPTIONAL_WIFI_SUPPORT
#else
# include <WiFi.h>
# include <WiFiClient.h>

static IPAddress ToIP(cell_t ip) {
  return IPAddress(ip & 0xff, ((ip >> 8) & 0xff), ((ip >> 16) & 0xff), ((ip >> 24) & 0xff));
}

static cell_t FromIP(IPAddress ip) {
  cell_t ret = 0;
  ret = (ret << 8) | ip[3];
  ret = (ret << 8) | ip[2];
  ret = (ret << 8) | ip[1];
  ret = (ret << 8) | ip[0];
  return ret;
}

# define OPTIONAL_WIFI_SUPPORT \
  /* WiFi */ \
  X("WiFi.config", WIFI_CONFIG, \
      WiFi.config(ToIP(n3), ToIP(n2), ToIP(n1), ToIP(n0)); DROPn(4)) \
  X("WiFi.begin", WIFI_BEGIN, WiFi.begin(c1, c0); DROPn(2)) \
  X("WiFi.disconnect", WIFI_DISCONNECT, WiFi.disconnect()) \
  X("WiFi.status", WIFI_STATUS, PUSH WiFi.status()) \
  X("WiFi.macAddress", WIFI_MAC_ADDRESS, WiFi.macAddress(b0); DROP) \
  X("WiFi.localIP", WIFI_LOCAL_IPS, PUSH FromIP(WiFi.localIP())) \
  X("WiFi.mode", WIFI_MODE, WiFi.mode((wifi_mode_t) n0); DROP) \
  X("WiFi.setTxPower", WIFI_SET_TX_POWER, WiFi.setTxPower((wifi_power_t) n0); DROP) \
  X("WiFi.getTxPower", WIFI_GET_TX_POWER, PUSH WiFi.getTxPower())
#endif

#ifndef ENABLE_MDNS_SUPPORT
# define OPTIONAL_MDNS_SUPPORT
#else
# include <ESPmDNS.h>
# define OPTIONAL_MDNS_SUPPORT \
  /* mDNS */ \
  X("MDNS.begin", MDNS_BEGIN, n0 = MDNS.begin(c0))
#endif

#ifndef ENABLE_WEBSERVER_SUPPORT
# define OPTIONAL_WEBSERVER_SUPPORT
#else
# include <WebServer.h>
# define ws0 ((WebServer *) a0)
# define OPTIONAL_WEBSERVER_SUPPORT \
  /* WebServer */ \
  X("WebServer.new", WEBSERVER_NEW, PUSH new WebServer(tos)) \
  X("WebServer.delete", WEBSERVER_DELETE, delete ws0; DROP) \
  X("WebServer.begin", WEBSERVER_BEGIN, ws0->begin(n1); DROPn(2)) \
  X("WebServer.stop", WEBSERVER_STOP, ws0->stop(); DROP) \
  X("WebServer.on", WEBSERVER_ON, InvokeWebServerOn(ws0, c2, n1); DROPn(3)) \
  X("WebServer.hasArg", WEBSERVER_HAS_ARG, n0 = ws0->hasArg(c1); DROP) \
  X("WebServer.arg", WEBSERVER_ARG, \
      string_value = ws0->arg(c1); \
      c1 = &string_value[0]; n0 = string_value.length()) \
  X("WebServer.argi", WEBSERVER_ARGI, \
      string_value = ws0->arg(n1); \
      c1 = &string_value[0]; n0 = string_value.length()) \
  X("WebServer.argName", WEBSERVER_ARG_NAME, \
      string_value = ws0->argName(n1); \
      c1 = &string_value[0]; n0 = string_value.length()) \
  X("WebServer.args", WEBSERVER_ARGS, n0 = ws0->args()) \
  X("WebServer.setContentLength", WEBSERVER_SET_CONTENT_LENGTH, \
      ws0->setContentLength(n1); DROPn(2)) \
  X("WebServer.sendHeader", WEBSERVER_SEND_HEADER, \
      ws0->sendHeader(c3, c2, n1); DROPn(4)) \
  X("WebServer.send", WEBSERVER_SEND, ws0->send(n3, c2, c1); DROPn(4)) \
  X("WebServer.sendContent", WEBSERVER_SEND_CONTENT, \
      ws0->sendContent(c1); DROPn(2)) \
  X("WebServer.method", WEBSERVER_METHOD, n0 = ws0->method()) \
  X("WebServer.handleClient", WEBSERVER_HANDLE_CLIENT, ws0->handleClient(); DROP)
#endif

#ifndef ENABLE_OLED_SUPPORT
# define OPTIONAL_OLED_SUPPORT
#else
#  include <Adafruit_GFX.h>
#  include <Adafruit_SSD1306.h>
static Adafruit_SSD1306 *oled_display = 0;
# define OPTIONAL_OLED_SUPPORT \
  Y(OledAddr, PUSH &oled_display) \
  Y(OledNew, oled_display = new Adafruit_SSD1306(n2, n1, &Wire, n0); DROPn(3)) \
  Y(OledDelete, delete oled_display) \
  Y(OledBegin, n0 = oled_display->begin(n1, n0); NIP) \
  Y(OledHOME, oled_display->setCursor(0,0); DROP) \
  Y(OledCLS, oled_display->clearDisplay()) \
  Y(OledTextc, oled_display->setTextColor(n0); DROP) \
  Y(OledPrintln, oled_display->println(c0); DROP) \
  Y(OledNumln, oled_display->println(n0); DROP) \
  Y(OledNum, oled_display->print(n0); DROP) \
  Y(OledDisplay, oled_display->display()) \
  Y(OledPrint, oled_display->write(c0); DROP) \
  Y(OledInvert, oled_display->invertDisplay(n0); DROP) \
  Y(OledTextsize, oled_display->setTextSize(n0); DROP) \
  Y(OledSetCursor, oled_display->setCursor(n1,n0); DROPn(2)) \
  Y(OledPixel, oled_display->drawPixel(n2, n1, n0); DROPn(2)) \
  Y(OledDrawL, oled_display->drawLine(n4, n3, n2, n1, n0); DROPn(4)) \
  Y(OledCirc, oled_display->drawCircle(n3,n2, n1, n0); DROPn(3)) \
  Y(OledCircF, oled_display->fillCircle(n3, n2, n1, n0); DROPn(3)) \
  Y(OledRect, oled_display->drawRect(n4, n3, n2, n1, n0); DROPn(4)) \
  Y(OledRectF, oled_display->fillRect(n4, n3, n2, n1, n0); DROPn(3)) \
  Y(OledRectR, oled_display->drawRoundRect(n5, n4, n3, n2, n1, n0); DROPn(5)) \
  Y(OledRectRF, oled_display->fillRoundRect(n5, n4, n3, n2, n1, n0 ); DROPn(5))
#endif

static char filename[PATH_MAX];

#ifdef ENABLE_WEBSERVER_SUPPORT
static String string_value;
#endif

#ifdef ENABLE_INTERRUPTS_SUPPORT
static cell_t EspIntrAlloc(cell_t source, cell_t flags, cell_t xt, cell_t arg, cell_t *ret);
static cell_t GpioIsrHandlerAdd(cell_t pin, cell_t xt, cell_t arg);
static cell_t TimerIsrRegister(cell_t group, cell_t timer, cell_t xt, cell_t arg, void *ret);
#endif

{{core}}
{{interp}}
{{boot}}

// Work around lack of ftruncate
static cell_t ResizeFile(cell_t fd, cell_t size) {
  struct stat st;
  char buf[256];
  cell_t t = fstat(fd, &st);
  if (t < 0) { return errno; }
  if (size < st.st_size) {
    // TODO: Implement truncation
    return ENOSYS;
  }
  cell_t oldpos = lseek(fd, 0, SEEK_CUR);
  if (oldpos < 0) { return errno; }
  t = lseek(fd, 0, SEEK_END);
  if (t < 0) { return errno; }
  memset(buf, 0, sizeof(buf));
  while (st.st_size < size) {
    cell_t len = sizeof(buf);
    if (size - st.st_size < len) {
      len = size - st.st_size;
    }
    t = write(fd, buf, len);
    if (t != len) {
      return errno;
    }
    st.st_size += t;
  }
  t = lseek(fd, oldpos, SEEK_SET);
  if (t < 0) { return errno; }
  return 0;
}

#ifdef ENABLE_WEBSERVER_SUPPORT
static void InvokeWebServerOn(WebServer *ws, const char *url, cell_t xt) {
  ws->on(url, [xt]() {
    cell_t code[2];
    code[0] = xt;
    code[1] = g_sys.YIELD_XT;
    cell_t stack[INTERRUPT_STACK_CELLS];
    cell_t rstack[INTERRUPT_STACK_CELLS];
    cell_t *rp = rstack;
    *++rp = (cell_t) (stack + 1);
    *++rp = (cell_t) code;
    forth_run(rp);
  });
}
#endif

#ifdef ENABLE_INTERRUPTS_SUPPORT
struct handle_interrupt_args {
  cell_t xt;
  cell_t arg;
};

static void IRAM_ATTR HandleInterrupt(void *arg) {
  struct handle_interrupt_args *args = (struct handle_interrupt_args *) arg;
  cell_t code[2];
  code[0] = args->xt;
  code[1] = g_sys.YIELD_XT;
  cell_t stack[INTERRUPT_STACK_CELLS];
  cell_t rstack[INTERRUPT_STACK_CELLS];
  stack[0] = args->arg;
  cell_t *rp = rstack;
  *++rp = (cell_t) (stack + 1);
  *++rp = (cell_t) code;
  forth_run(rp);
}

static cell_t EspIntrAlloc(cell_t source, cell_t flags, cell_t xt, cell_t arg, void *ret) {
  // NOTE: Leaks memory.
  struct handle_interrupt_args *args = (struct handle_interrupt_args *) malloc(sizeof(struct handle_interrupt_args));
  args->xt = xt;
  args->arg = arg;
  return esp_intr_alloc(source, flags, HandleInterrupt, args, (intr_handle_t *) ret);
}

static cell_t GpioIsrHandlerAdd(cell_t pin, cell_t xt, cell_t arg) {
  // NOTE: Leaks memory.
  struct handle_interrupt_args *args = (struct handle_interrupt_args *) malloc(sizeof(struct handle_interrupt_args));
  args->xt = xt;
  args->arg = arg;
  return gpio_isr_handler_add((gpio_num_t) pin, HandleInterrupt, args);
}

static cell_t TimerIsrRegister(cell_t group, cell_t timer, cell_t xt, cell_t arg, cell_t flags, void *ret) {
  // NOTE: Leaks memory.
  struct handle_interrupt_args *args = (struct handle_interrupt_args *) malloc(sizeof(struct handle_interrupt_args));
  args->xt = xt;
  args->arg = arg;
  return timer_isr_register((timer_group_t) group, (timer_idx_t) timer, HandleInterrupt, args, flags, (timer_isr_handle_t *) ret);
}
#endif

void setup() {
  cell_t *heap = (cell_t *) malloc(HEAP_SIZE);
  forth_init(0, 0, heap, boot, sizeof(boot));
}

void loop() {
  g_sys.rp = forth_run(g_sys.rp);
}
