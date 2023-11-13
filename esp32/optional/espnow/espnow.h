// Copyright 2023 Daniel Nagy
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
 * ESP32forth ESPNOW v{{VERSION}}
 * Revision: {{REVISION}}
 */

// More documentation
// https://docs.espressif.com/projects/esp-idf/en/latest/esp32/api-reference/network/esp_now.html

#include <esp_now.h>

static esp_err_t ESPNOW_add_peer (const uint8_t *broadcastAddress) {
  esp_now_peer_info_t peerInfo = {0};
  memcpy(peerInfo.peer_addr, broadcastAddress, 6);
  peerInfo.channel = 0; // use current wifi channel
  peerInfo.encrypt = false;
  return esp_now_add_peer(&peerInfo);
}

#define OPTIONAL_ESPNOW_VOCABULARY V(espnow)
#define OPTIONAL_ESPNOW_SUPPORT \
  XV(internals, "espnow-source", ESPNOW_SOURCE, \
      PUSH espnow_source; PUSH sizeof(espnow_source) - 1) \
  YV(espnow, espnow_init    , PUSH esp_now_init()) \
  YV(espnow, espnow_deinit  , PUSH esp_now_deinit()) \
  YV(espnow, espnow_add_peer, n0 = ESPNOW_add_peer(b0)) \
  YV(espnow, espnow_send    , n0 = esp_now_send(b2, b1, n0); NIPn(2)) \


{{espnow}}
