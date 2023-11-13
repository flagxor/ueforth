#! /usr/bin/env ueforth

\ Copyright 2023 Daniel Nagy
\
\ Licensed under the Apache License, Version 2.0 (the "License");
\ you may not use this file except in compliance with the License.
\ You may obtain a copy of the License at
\
\     http://www.apache.org/licenses/LICENSE-2.0
\
\ Unless required by applicable law or agreed to in writing, software
\ distributed under the License is distributed on an "AS IS" BASIS,
\ WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
\ See the License for the specific language governing permissions and
\ limitations under the License.

also WiFi
also espnow

\ start wifi
WIFI_MODE_STA WiFi.mode
\ initialize espnow and check result
espnow_init ESP_OK <> throw

: espnow_register_example
  \ register the mac 12:34:56:78:9a:bc as a peer
  $12 c, $34 c, $56 c, $78 c, $9a c, $bc c,
  here 6 -
  ESPNOW_add_peer ESP_OK <> throw
  -6 allot
;
: espnow_send_some
  \ NULL a.k.a. send to peerlist
  \ send 10 bytes of data space pointer
  0 here 10 - 10
  espnow_send ESP_OK <> throw
;
