// Copyright 2025 Bradley D. Nelson
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
 * ESP32forth BLE Scanner v{{VERSION}}
 * Revision: {{REVISION}}
 *
 * Non-blocking BLE scan words for ESP32forth.
 *
 * Usage:
 *   ble-init                   ( -- )        initialize once at startup
 *   5 ble-scan-start           ( secs -- )   start scan, returns immediately
 *   ble-scanning?              ( -- flag )   -1 while scanning, 0 when done
 *   ble-scan-stop              ( -- )        stop scan early
 *   ble-count                  ( -- n )      number of devices found so far
 *   0 ble-addr                 ( i -- addr ) null-terminated MAC string
 *   0 ble-rssi                 ( i -- n )    RSSI in dBm
 *   0 ble-name                 ( i -- addr ) null-terminated name (empty if none)
 *   0 ble-new?                 ( i -- flag ) -1 if first time seen this boot
 *   ble-forget                 ( -- )        clear seen history
 *
 * Example — print all devices from a 5-second scan:
 *   : .ble-device ( i -- )
 *     dup ble-addr type ."  " dup ble-rssi . dup ble-new? if ." [NEW] " then
 *     ble-name dup if type else drop then cr ;
 *   : ble-scan
 *     5 ble-scan-start
 *     begin ble-scanning? while 100 ms repeat
 *     ble-count 0 do i .ble-device loop ;
 */

#include <BLEDevice.h>
#include <BLEScan.h>
#include <BLEAdvertisedDevice.h>
#include <set>
#include <string>

#define BLE_SCAN_MAX_RESULTS 64

struct BleScanResult {
  char addr[18];   // "aa:bb:cc:dd:ee:ff\0"
  char name[33];   // up to 32 chars + null
  int  rssi;
  bool isNew;
};

static BLEScan*         g_ble_scan      = nullptr;
static BleScanResult    g_ble_results[BLE_SCAN_MAX_RESULTS];
static volatile int     g_ble_count     = 0;
static TaskHandle_t     g_ble_task      = nullptr;
static uint32_t         g_ble_duration  = 5;
static std::set<std::string> g_ble_seen;

class BleScanCallbacks : public BLEAdvertisedDeviceCallbacks {
  void onResult(BLEAdvertisedDevice dev) override {
    int i = g_ble_count;
    if (i >= BLE_SCAN_MAX_RESULTS) return;
    String addrStr = dev.getAddress().toString();
    strncpy(g_ble_results[i].addr, addrStr.c_str(), 17);
    g_ble_results[i].addr[17] = '\0';
    if (dev.haveName()) {
      strncpy(g_ble_results[i].name, dev.getName().c_str(), 32);
      g_ble_results[i].name[32] = '\0';
    } else {
      g_ble_results[i].name[0] = '\0';
    }
    g_ble_results[i].rssi = dev.getRSSI();
    std::string addr(g_ble_results[i].addr);
    g_ble_results[i].isNew = (g_ble_seen.find(addr) == g_ble_seen.end());
    if (g_ble_results[i].isNew) g_ble_seen.insert(addr);
    g_ble_count = i + 1;  // write count last so readers see complete record
  }
};

static BleScanCallbacks* g_ble_callbacks = nullptr;

static void bleScanTask(void* param) {
  g_ble_scan->start(g_ble_duration, false);
  g_ble_task = nullptr;
  vTaskDelete(nullptr);
}

#define OPTIONAL_BLE_SCAN_VOCABULARY V(ble)

#define OPTIONAL_BLE_SCAN_SUPPORT \
  XV(ble, "ble-init", ble_init, { \
    BLEDevice::init(""); \
    g_ble_scan = BLEDevice::getScan(); \
    g_ble_callbacks = new BleScanCallbacks(); \
    g_ble_scan->setAdvertisedDeviceCallbacks(g_ble_callbacks, false); \
    g_ble_scan->setActiveScan(true); \
    g_ble_scan->setInterval(100); \
    g_ble_scan->setWindow(99); \
  }) \
  XV(ble, "ble-scan-start", ble_scan_start, { \
    g_ble_duration = (uint32_t) n0; DROP; \
    if (g_ble_task) { g_ble_scan->stop(); vTaskDelay(10); } \
    g_ble_count = 0; \
    g_ble_scan->clearResults(); \
    xTaskCreate(bleScanTask, "blescan", 4096, nullptr, 1, &g_ble_task); \
  }) \
  XV(ble, "ble-scanning?", ble_scanning, PUSH(g_ble_task != nullptr ? -1 : 0)) \
  XV(ble, "ble-scan-stop", ble_scan_stop, { \
    g_ble_scan->stop(); \
    vTaskDelay(pdMS_TO_TICKS(50)); \
  }) \
  XV(ble, "ble-count", ble_count, PUSH(g_ble_count)) \
  XV(ble, "ble-addr", ble_addr, { \
    int i = n0; \
    n0 = (i >= 0 && i < g_ble_count) ? (cell_t) g_ble_results[i].addr : (cell_t) ""; \
  }) \
  XV(ble, "ble-rssi", ble_rssi, { \
    int i = n0; \
    n0 = (i >= 0 && i < g_ble_count) ? g_ble_results[i].rssi : 0; \
  }) \
  XV(ble, "ble-name", ble_name, { \
    int i = n0; \
    n0 = (i >= 0 && i < g_ble_count) ? (cell_t) g_ble_results[i].name : (cell_t) ""; \
  }) \
  XV(ble, "ble-new?", ble_new, { \
    int i = n0; \
    n0 = (i >= 0 && i < g_ble_count && g_ble_results[i].isNew) ? -1 : 0; \
  }) \
  XV(ble, "ble-forget", ble_forget, g_ble_seen.clear())
