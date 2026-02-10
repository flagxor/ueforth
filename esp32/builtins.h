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

#ifndef SIM_PRINT_ONLY

# include <dirent.h>
# include <errno.h>
# include <unistd.h>
# include <fcntl.h>
# include <sys/types.h>
# include <sys/stat.h>
# include <sys/select.h>

// Hook to pull in words from optional userwords.h
# if __has_include("userwords.h")
#  include "userwords.h"
# else
#  define USER_WORDS
# endif

// Hook to pull in words from optional assemblers.
# if __has_include("assemblers.h")
#  include "assemblers.h"
# else
#  define OPTIONAL_ASSEMBLERS_SUPPORT
# endif

// Hook to pull in optional interrupts and timers support.
# if __has_include("interrupts.h")
#  include "interrupts.h"
# else
#  define OPTIONAL_INTERRUPTS_VOCABULARIES
#  define OPTIONAL_INTERRUPTS_SUPPORT
# endif

// Hook to pull in optional Oled support.
# if __has_include("oled.h")
#  include "oled.h"
# else
#  define OPTIONAL_OLED_VOCABULARY
#  define OPTIONAL_OLED_SUPPORT
# endif

// Hook to pull in optional ESP32-CAM camera support.
# if __has_include("camera.h")
#  include "camera.h"
# else
#  define OPTIONAL_CAMERA_VOCABULARY
#  define OPTIONAL_CAMERA_SUPPORT
# endif

// Hook to pull in optional RMT (Remote Control) support.
# if __has_include("rmt.h")
#  include "rmt.h"
# else
#  define OPTIONAL_RMT_VOCABULARY
#  define OPTIONAL_RMT_SUPPORT
# endif

// Hook to pull in optional serial bluetooth support.
# if __has_include("serial-bluetooth.h")
#  include "serial-bluetooth.h"
# else
#  define OPTIONAL_BLUETOOTH_VOCABULARY
#  define OPTIONAL_SERIAL_BLUETOOTH_SUPPORT
# endif

// Hook to pull in optional SPI flash support.
# if __has_include("spi-flash.h")
#  include "spi-flash.h"
# else
#  define OPTIONAL_SPI_FLASH_VOCABULARY
#  define OPTIONAL_SPI_FLASH_SUPPORT
# endif

// Hook to pull in optional HTTPClient support.
# if __has_include("http-client.h")
#  include "http-client.h"
# else
#  define OPTIONAL_HTTP_CLIENT_VOCABULARY
#  define OPTIONAL_HTTP_CLIENT_SUPPORT
# endif

static cell_t ResizeFile(cell_t fd, cell_t size);

#endif

#define PLATFORM_OPCODE_LIST \
  USER_WORDS \
  EXTERNAL_OPTIONAL_MODULE_SUPPORT \
  REQUIRED_PLATFORM_SUPPORT \
  REQUIRED_ESP_SUPPORT \
  REQUIRED_MEMORY_SUPPORT \
  REQUIRED_SERIAL_SUPPORT \
  OPTIONAL_SERIAL2_SUPPORT \
  REQUIRED_ARDUINO_GPIO_SUPPORT \
  REQUIRED_SYSTEM_SUPPORT \
  REQUIRED_FILES_SUPPORT \
  OPTIONAL_LEDC_V2_SUPPORT \
  OPTIONAL_LEDC_V3_SUPPORT \
  OPTIONAL_DAC_SUPPORT \
  OPTIONAL_SPIFFS_SUPPORT \
  OPTIONAL_WIFI_SUPPORT \
  OPTIONAL_MDNS_SUPPORT \
  OPTIONAL_SD_SUPPORT \
  OPTIONAL_SD_MMC_SUPPORT \
  OPTIONAL_I2C_SUPPORT \
  OPTIONAL_SOCKETS_SUPPORT \
  OPTIONAL_FREERTOS_SUPPORT \
  CALLING_OPCODE_LIST \
  FLOATING_POINT_LIST

#define EXTERNAL_OPTIONAL_MODULE_SUPPORT \
  OPTIONAL_ASSEMBLERS_SUPPORT \
  OPTIONAL_CAMERA_SUPPORT \
  OPTIONAL_INTERRUPTS_SUPPORT \
  OPTIONAL_OLED_SUPPORT \
  OPTIONAL_RMT_SUPPORT \
  OPTIONAL_SERIAL_BLUETOOTH_SUPPORT \
  OPTIONAL_SPI_FLASH_SUPPORT \
  OPTIONAL_HTTP_CLIENT_SUPPORT

#define REQUIRED_MEMORY_SUPPORT \
  YV(internals, MALLOC, SET malloc(n0)) \
  YV(internals, SYSFREE, free(a0); DROP) \
  YV(internals, REALLOC, SET realloc(a1, n0); NIP) \
  YV(internals, heap_caps_malloc, SET heap_caps_malloc(n1, n0); NIP) \
  YV(internals, heap_caps_free, heap_caps_free(a0); DROP) \
  YV(internals, heap_caps_realloc, \
      tos = (cell_t) heap_caps_realloc(a2, n1, n0); NIPn(2)) \
  YV(internals, heap_caps_get_total_size, n0 = heap_caps_get_total_size(n0)) \
  YV(internals, heap_caps_get_free_size, n0 = heap_caps_get_free_size(n0)) \
  YV(internals, heap_caps_get_minimum_free_size, \
      n0 = heap_caps_get_minimum_free_size(n0)) \
  YV(internals, heap_caps_get_largest_free_block, \
      n0 = heap_caps_get_largest_free_block(n0))

#define REQUIRED_PLATFORM_SUPPORT \
  X("ESP32?", IS_ESP32, PUSH UEFORTH_PLATFORM_IS_ESP32) \
  X("ESP32-S2?", IS_ESP32S2, PUSH UEFORTH_PLATFORM_IS_ESP32S2) \
  X("ESP32-S3?", IS_ESP32S3, PUSH UEFORTH_PLATFORM_IS_ESP32S3) \
  X("ESP32-C3?", IS_ESP32C3, PUSH UEFORTH_PLATFORM_IS_ESP32C3) \
  X("PSRAM?", HAS_PSRAM, PUSH UEFORTH_PLATFORM_HAS_PSRAM) \
  X("Xtensa?", IS_XTENSA, PUSH UEFORTH_PLATFORM_IS_XTENSA) \
  X("RISC-V?", IS_RISCV, PUSH UEFORTH_PLATFORM_IS_RISCV)

#define REQUIRED_ESP_SUPPORT \
  YV(ESP, getHeapSize, PUSH ESP.getHeapSize()) \
  YV(ESP, getFreeHeap, PUSH ESP.getFreeHeap()) \
  YV(ESP, getMaxAllocHeap, PUSH ESP.getMaxAllocHeap()) \
  YV(ESP, getChipModel, PUSH ESP.getChipModel()) \
  YV(ESP, getChipCores, PUSH ESP.getChipCores()) \
  YV(ESP, getFlashChipSize, PUSH ESP.getFlashChipSize()) \
  YV(ESP, getCpuFreqMHz, PUSH ESP.getCpuFreqMHz()) \
  YV(ESP, getSketchSize, PUSH ESP.getSketchSize()) \
  YV(ESP, deepSleep, ESP.deepSleep(tos); DROP) \
  YV(ESP, getEfuseMac, PUSH (cell_t) ESP.getEfuseMac(); PUSH (cell_t) (ESP.getEfuseMac() >> 32)) \
  YV(ESP, esp_log_level_set, esp_log_level_set(c1, (esp_log_level_t) n0); DROPn(2))

#define REQUIRED_SYSTEM_SUPPORT \
  X("MS-TICKS", MS_TICKS, PUSH millis()) \
  XV(internals, "RAW-YIELD", RAW_YIELD, yield()) \
  XV(internals, "RAW-TERMINATE", RAW_TERMINATE, ESP.restart())

#define REQUIRED_SERIAL_SUPPORT \
  XV(serial, "Serial.begin", SERIAL_BEGIN, Serial.begin(tos); DROP) \
  XV(serial, "Serial.end", SERIAL_END, Serial.end()) \
  XV(serial, "Serial.available", SERIAL_AVAILABLE, PUSH Serial.available()) \
  XV(serial, "Serial.readBytes", SERIAL_READ_BYTES, n0 = Serial.readBytes(b1, n0); NIP) \
  XV(serial, "Serial.write", SERIAL_WRITE, n0 = Serial.write(b1, n0); NIP) \
  XV(serial, "Serial.flush", SERIAL_FLUSH, Serial.flush()) \
  XV(serial, "Serial.setDebugOutput", SERIAL_DEBUG_OUTPUT, Serial.setDebugOutput(n0); DROP)

#ifndef ENABLE_SERIAL2_SUPPORT
# define OPTIONAL_SERIAL2_SUPPORT
#else
# define OPTIONAL_SERIAL2_SUPPORT \
  XV(serial, "Serial2.begin", SERIAL2_BEGIN, Serial2.begin(tos); DROP) \
  XV(serial, "Serial2.end", SERIAL2_END, Serial2.end()) \
  XV(serial, "Serial2.available", SERIAL2_AVAILABLE, PUSH Serial2.available()) \
  XV(serial, "Serial2.readBytes", SERIAL2_READ_BYTES, n0 = Serial2.readBytes(b1, n0); NIP) \
  XV(serial, "Serial2.write", SERIAL2_WRITE, n0 = Serial2.write(b1, n0); NIP) \
  XV(serial, "Serial2.flush", SERIAL2_FLUSH, Serial2.flush()) \
  XV(serial, "Serial2.setDebugOutput", SERIAL2_DEBUG_OUTPUT, Serial2.setDebugOutput(n0); DROP)
#endif

#define REQUIRED_ARDUINO_GPIO_SUPPORT \
  Y(pinMode, pinMode(n1, n0); DROPn(2)) \
  Y(digitalWrite, digitalWrite(n1, n0); DROPn(2)) \
  Y(digitalRead, n0 = digitalRead(n0)) \
  Y(analogRead, n0 = analogRead(n0)) \
  Y(pulseIn, n0 = pulseIn(n2, n1, n0); NIPn(2))

#define REQUIRED_FILES_SUPPORT \
  X("R/O", R_O, PUSH O_RDONLY) \
  X("W/O", W_O, PUSH O_WRONLY) \
  X("R/W", R_W, PUSH O_RDWR) \
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
  X("RESIZE-FILE", RESIZE_FILE, cell_t fd = n0; DROP; n0 = ResizeFile(fd, tos)) \
  X("FILE-SIZE", FILE_SIZE, struct stat st; w = fstat(n0, &st); \
    n0 = (cell_t) st.st_size; PUSH w < 0 ? errno : 0) \
  X("NON-BLOCK", NON_BLOCK, n0 = fcntl(n0, F_SETFL, O_NONBLOCK); \
    n0 = n0 < 0 ? errno : 0) \
  X("OPEN-DIR", OPEN_DIR, memcpy(filename, a1, n0); filename[n0] = 0; \
    n1 = (cell_t) opendir(filename); n0 = n1 ? 0 : errno) \
  X("CLOSE-DIR", CLOSE_DIR, n0 = closedir((DIR *) n0); n0 = n0 ? errno : 0) \
  YV(internals, READDIR, \
    struct dirent *ent = readdir((DIR *) n0); SET (ent ? ent->d_name: 0))

#ifndef ENABLE_LEDC_V2_SUPPORT
# define OPTIONAL_LEDC_V2_SUPPORT
#else
# define OPTIONAL_LEDC_V2_SUPPORT \
  YV(ledc, ledcSetup, \
      n0 = (cell_t) (1000000 * ledcSetup(n2, n1 / 1000.0, n0)); NIPn(2)) \
  YV(ledc, ledcAttachPin, ledcAttachPin(n1, n0); DROPn(2)) \
  YV(ledc, ledcDetachPin, ledcDetachPin(n0); DROP) \
  YV(ledc, ledcRead, n0 = ledcRead(n0)) \
  YV(ledc, ledcReadFreq, n0 = (cell_t) (1000000 * ledcReadFreq(n0))) \
  YV(ledc, ledcWrite, ledcWrite(n1, n0); DROPn(2)) \
  YV(ledc, ledcWriteTone, \
      n0 = (cell_t) (1000000 * ledcWriteTone(n1, n0 / 1000.0)); NIP) \
  YV(ledc, ledcWriteNote, \
      tos = (cell_t) (1000000 * ledcWriteNote(n2, (note_t) n1, n0)); NIPn(2))
#endif

#ifndef ENABLE_LEDC_V3_SUPPORT
# define OPTIONAL_LEDC_V3_SUPPORT
#else
# define OPTIONAL_LEDC_V3_SUPPORT \
  YV(ledc, ledcAttach, n0 = ledcAttach(n2, n1, n0); NIPn(2)) \
  YV(ledc, ledcAttachChannel, n0 = ledcAttachChannel(n3, n2, n1, n0); NIPn(3)) \
  YV(ledc, ledcDetach, n0 = ledcDetach(n0)) \
  YV(ledc, ledcRead, n0 = ledcRead(n0)) \
  YV(ledc, ledcReadFreq, n0 = ledcReadFreq(n0)) \
  YV(ledc, ledcWrite, ledcWrite(n1, n0); DROPn(2)) \
  YV(ledc, ledcWriteTone, n0 = ledcWriteTone(n1, n0); NIP) \
  YV(ledc, ledcWriteNote, n0 = ledcWriteNote(n2, (note_t) n1, n0); NIPn(2)) \
  YV(ledc, ledcChangeFrequency, n0 = ledcChangeFrequency(n2, n1, n0); NIPn(2))
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
# ifndef SIM_PRINT_ONLY
#  include "SPIFFS.h"
# endif
# define OPTIONAL_SPIFFS_SUPPORT \
  XV(SPIFFS, "SPIFFS.begin", SPIFFS_BEGIN, \
      tos = SPIFFS.begin(n2, c1, n0); NIPn(2)) \
  XV(SPIFFS, "SPIFFS.end", SPIFFS_END, SPIFFS.end()) \
  XV(SPIFFS, "SPIFFS.format", SPIFFS_FORMAT, PUSH SPIFFS.format()) \
  XV(SPIFFS, "SPIFFS.totalBytes", SPIFFS_TOTAL_BYTES, PUSH SPIFFS.totalBytes()) \
  XV(SPIFFS, "SPIFFS.usedBytes", SPIFFS_USED_BYTES, PUSH SPIFFS.usedBytes())
#endif

#ifndef ENABLE_FREERTOS_SUPPORT
# define OPTIONAL_FREERTOS_SUPPORT
#else
# ifndef SIM_PRINT_ONLY
#  include "freertos/FreeRTOS.h"
#  include "freertos/task.h"
# endif
# define OPTIONAL_FREERTOS_SUPPORT \
  YV(rtos, vTaskDelete, vTaskDelete((TaskHandle_t) n0); DROP) \
  YV(rtos, xTaskCreatePinnedToCore, n0 = xTaskCreatePinnedToCore((TaskFunction_t) a6, \
        c5, n4, a3, (UBaseType_t) n2, (TaskHandle_t *) a1, (BaseType_t) n0); NIPn(6)) \
  YV(rtos, xPortGetCoreID, PUSH xPortGetCoreID())
#endif

#ifndef ENABLE_SOCKETS_SUPPORT
# define OPTIONAL_SOCKETS_SUPPORT
#else
# ifndef SIM_PRINT_ONLY
#  include <errno.h>
#  include <netdb.h>
#  include <sys/select.h>
#  include <sys/socket.h>
#  include <sys/time.h>
#  include <sys/types.h>
#  include <sys/un.h>
#  include <sys/poll.h>
# endif
# define OPTIONAL_SOCKETS_SUPPORT \
  YV(sockets, socket, n0 = socket(n2, n1, n0); NIPn(2)) \
  YV(sockets, setsockopt, n0 = setsockopt(n4, n3, n2, a1, n0); NIPn(4)) \
  YV(sockets, bind, n0 = bind(n2, (struct sockaddr *) a1, n0); NIPn(2)) \
  YV(sockets, listen, n0 = listen(n1, n0); NIP) \
  YV(sockets, connect, n0 = connect(n2, (struct sockaddr *) a1, n0); NIPn(2)) \
  YV(sockets, sockaccept, n0 = accept(n2, (struct sockaddr *) a1, (socklen_t *) a0); NIPn(2)) \
  YV(sockets, select, n0 = select(n4, (fd_set *) a3, (fd_set *) a2, (fd_set *) a1, (struct timeval *) a0); NIPn(4)) \
  YV(sockets, poll, n0 = poll((struct pollfd *) a2, (nfds_t) n1, n0); NIPn(2)) \
  YV(sockets, send, n0 = send(n3, a2, n1, n0); NIPn(3)) \
  YV(sockets, sendto, n0 = sendto(n5, a4, n3, n2, (const struct sockaddr *) a1, n0); NIPn(5)) \
  YV(sockets, sendmsg, n0 = sendmsg(n2, (const struct msghdr *) a1, n0); NIPn(2)) \
  YV(sockets, recv, n0 = recv(n3, a2, n1, n0); NIPn(3)) \
  YV(sockets, recvfrom, n0 = recvfrom(n5, a4, n3, n2, (struct sockaddr *) a1, (socklen_t *) a0); NIPn(5)) \
  YV(sockets, recvmsg, n0 = recvmsg(n2, (struct msghdr *) a1, n0); NIPn(2)) \
  YV(sockets, gethostbyname, n0 = (cell_t) gethostbyname(c0)) \
  XV(sockets, "errno", ERRNO, PUSH errno)
#endif

#ifndef ENABLE_SD_SUPPORT
# define OPTIONAL_SD_SUPPORT
#else
# ifndef SIM_PRINT_ONLY
#  include "SD.h"
# endif
# define OPTIONAL_SD_SUPPORT \
  XV(SD, "SD.begin", SD_BEGIN, PUSH SD.begin()) \
  XV(SD, "SD.beginFull", SD_BEGIN_FULL, \
      tos = SD.begin(n5, *(SPIClass*)a4, n3, c2, n1, n0); NIPn(5)) \
  XV(SD, "SD.beginDefaults", SD_BEGIN_DEFAULTS, \
      PUSH SS; PUSH &SPI; PUSH 4000000; PUSH "/sd"; PUSH 5; PUSH false) \
  XV(SD, "SD.end", SD_END, SD.end()) \
  XV(SD, "SD.cardType", SD_CARD_TYPE, PUSH SD.cardType()) \
  XV(SD, "SD.totalBytes", SD_TOTAL_BYTES, PUSH SD.totalBytes()) \
  XV(SD, "SD.usedBytes", SD_USED_BYTES, PUSH SD.usedBytes())
#endif

#ifndef ENABLE_SD_MMC_SUPPORT
# define OPTIONAL_SD_MMC_SUPPORT
#else
# ifndef SIM_PRINT_ONLY
#  include "SD_MMC.h"
# endif
# define OPTIONAL_SD_MMC_SUPPORT \
  XV(SD_MMC, "SD_MMC.begin", SD_MMC_BEGIN, PUSH SD_MMC.begin()) \
  XV(SD_MMC, "SD_MMC.beginFull", SD_MMC_BEGIN_FULL, tos = SD_MMC.begin(c2, n1, n0); NIPn(2)) \
  XV(SD_MMC, "SD_MMC.beginDefaults", SD_MMC_BEGIN_DEFAULTS, \
      PUSH "/sdcard"; PUSH false; PUSH false) \
  XV(SD_MMC, "SD_MMC.end", SD_MMC_END, SD_MMC.end()) \
  XV(SD_MMC, "SD_MMC.cardType", SD_MMC_CARD_TYPE, PUSH SD_MMC.cardType()) \
  XV(SD_MMC, "SD_MMC.totalBytes", SD_MMC_TOTAL_BYTES, PUSH SD_MMC.totalBytes()) \
  XV(SD_MMC, "SD_MMC.usedBytes", SD_MMC_USED_BYTES, PUSH SD_MMC.usedBytes()) \
  XV(SD_MMC, "SD_MMC.setPins", SD_MMC_SET_PINS, n0 = SD_MMC.setPins(n5, n4, n3, n2, n1, n0); NIPn(5))
#endif

#ifndef ENABLE_I2C_SUPPORT
# define OPTIONAL_I2C_SUPPORT
#else
# ifndef SIM_PRINT_ONLY
#  include <Wire.h>
# endif
# define OPTIONAL_I2C_SUPPORT \
  XV(Wire, "Wire.begin", WIRE_BEGIN, n0 = Wire.begin(n1, n0); NIP) \
  XV(Wire, "Wire.setClock", WIRE_SET_CLOCK, Wire.setClock(n0); DROP) \
  XV(Wire, "Wire.getClock", WIRE_GET_CLOCK, PUSH Wire.getClock()) \
  XV(Wire, "Wire.setTimeout", WIRE_SET_TIMEOUT, Wire.setTimeout(n0); DROP) \
  XV(Wire, "Wire.getTimeout", WIRE_GET_TIMEOUT, PUSH Wire.getTimeout()) \
  XV(Wire, "Wire.beginTransmission", WIRE_BEGIN_TRANSMISSION, Wire.beginTransmission(n0); DROP) \
  XV(Wire, "Wire.endTransmission", WIRE_END_TRANSMISSION, SET Wire.endTransmission(n0)) \
  XV(Wire, "Wire.requestFrom", WIRE_REQUEST_FROM, n0 = Wire.requestFrom(n2, n1, n0); NIPn(2)) \
  XV(Wire, "Wire.write", WIRE_WRITE, n0 = Wire.write(b1, n0); NIP) \
  XV(Wire, "Wire.available", WIRE_AVAILABLE, PUSH Wire.available()) \
  XV(Wire, "Wire.read", WIRE_READ, PUSH Wire.read()) \
  XV(Wire, "Wire.peek", WIRE_PEEK, PUSH Wire.peek()) \
  XV(Wire, "Wire.flush", WIRE_FLUSH, Wire.flush())
#endif

#ifndef ENABLE_WIFI_SUPPORT
# define OPTIONAL_WIFI_SUPPORT
#else
# ifndef SIM_PRINT_ONLY
#  include <WiFi.h>
#  include <WiFiClient.h>

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
# endif

# define OPTIONAL_WIFI_SUPPORT \
  /* WiFi */ \
  XV(WiFi, "WiFi.config", WIFI_CONFIG, \
      WiFi.config(ToIP(n3), ToIP(n2), ToIP(n1), ToIP(n0)); DROPn(4)) \
  XV(WiFi, "WiFi.begin", WIFI_BEGIN, WiFi.begin(c1, c0); DROPn(2)) \
  XV(WiFi, "WiFi.disconnect", WIFI_DISCONNECT, WiFi.disconnect()) \
  XV(WiFi, "WiFi.status", WIFI_STATUS, PUSH WiFi.status()) \
  XV(WiFi, "WiFi.macAddress", WIFI_MAC_ADDRESS, WiFi.macAddress(b0); DROP) \
  XV(WiFi, "WiFi.localIP", WIFI_LOCAL_IPS, PUSH FromIP(WiFi.localIP())) \
  XV(WiFi, "WiFi.gatewayIP", WIFI_GATEWAY_IPS, PUSH FromIP(WiFi.gatewayIP())) \
  XV(WiFi, "WiFi.subnetMask", WIFI_SUBNET_MASKS, PUSH FromIP(WiFi.subnetMask())) \
  XV(WiFi, "WiFi.mode", WIFI_MODE, WiFi.mode((wifi_mode_t) n0); DROP) \
  XV(WiFi, "WiFi.setTxPower", WIFI_SET_TX_POWER, WiFi.setTxPower((wifi_power_t) n0); DROP) \
  XV(WiFi, "WiFi.getTxPower", WIFI_GET_TX_POWER, PUSH WiFi.getTxPower()) \
  XV(WiFi, "WiFi.softAP", WIFI_SOFTAP, n0 = WiFi.softAP(c1, c0); NIP) \
  XV(WiFi, "WiFi.softAPIP", WIFI_SOFTAP_IP, PUSH FromIP(WiFi.softAPIP())) \
  XV(WiFi, "WiFi.softAPBroadcastIP", WIFI_SOFTAP_BROADCASTIP, PUSH FromIP(WiFi.softAPBroadcastIP())) \
  XV(WiFi, "WiFi.softAPNetworkID", WIFI_SOFTAP_NETWORKID, PUSH FromIP(WiFi.softAPNetworkID())) \
  XV(WiFi, "WiFi.softAPConfig", WIFI_SOFTAP_CONFIG, n0 = WiFi.softAPConfig(ToIP(n2), ToIP(n1), ToIP(n0))) \
  XV(WiFi, "WiFi.softAPdisconnect", WIFI_SOFTAP_DISCONNECT, n0 = WiFi.softAPdisconnect(n0)) \
  XV(WiFi, "WiFi.softAPgetStationNum", WIFI_SOFTAP_GET_STATION_NUM, PUSH WiFi.softAPgetStationNum())
#endif

#ifndef ENABLE_MDNS_SUPPORT
# define OPTIONAL_MDNS_SUPPORT
#else
# ifndef SIM_PRINT_ONLY
#  include <ESPmDNS.h>
# endif
# define OPTIONAL_MDNS_SUPPORT \
  /* mDNS */ \
  X("MDNS.begin", MDNS_BEGIN, n0 = MDNS.begin(c0) ? -1 : 0) \
  X("MDNS.addService", MDNS_ADD_SERVICE, n0 = MDNS.addService(c2, c1, n0) ? -1 : 0; NIPn(2)) \
  X("MDNS.setInstanceName", MDNS_SET_INSTANCE_NAME, MDNS.setInstanceName(c0); DROP) \
  X("MDNS.addServiceTxt", MDNS_ADD_SERVICE_TXT, MDNS.addServiceTxt(c3, c2, c1, c0); DROPn(4))
  /* alx */
#endif
