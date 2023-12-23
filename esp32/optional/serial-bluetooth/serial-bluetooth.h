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

/*
 * ESP32forth Serial Bluetooth v{{VERSION}}
 * Revision: {{REVISION}}
 */

#include "esp_bt_device.h"
#include "BluetoothSerial.h"

#define bt0 ((BluetoothSerial *) a0)

#define OPTIONAL_BLUETOOTH_VOCABULARY V(bluetooth)
#define OPTIONAL_SERIAL_BLUETOOTH_SUPPORT \
  XV(internals, "serial-bluetooth-source", SERIAL_BLUETOOTH_SOURCE, \
      PUSH serial_bluetooth_source; PUSH sizeof(serial_bluetooth_source) - 1) \
  XV(bluetooth, "SerialBT.new", SERIALBT_NEW, PUSH new BluetoothSerial()) \
  XV(bluetooth, "SerialBT.delete", SERIALBT_DELETE, delete bt0; DROP) \
  XV(bluetooth, "SerialBT.begin", SERIALBT_BEGIN, n0 = bt0->begin(c2, n1); NIPn(2)) \
  XV(bluetooth, "SerialBT.end", SERIALBT_END, bt0->end(); DROP) \
  XV(bluetooth, "SerialBT.available", SERIALBT_AVAILABLE, n0 = bt0->available()) \
  XV(bluetooth, "SerialBT.readBytes", SERIALBT_READ_BYTES, n0 = bt0->readBytes(b2, n1); NIPn(2)) \
  XV(bluetooth, "SerialBT.write", SERIALBT_WRITE, n0 = bt0->write(b2, n1); NIPn(2)) \
  XV(bluetooth, "SerialBT.flush", SERIALBT_FLUSH, bt0->flush(); DROP) \
  XV(bluetooth, "SerialBT.hasClient", SERIALBT_HAS_CLIENT, n0 = bt0->hasClient()) \
  XV(bluetooth, "SerialBT.enableSSP", SERIALBT_ENABLE_SSP, bt0->enableSSP(); DROP) \
  XV(bluetooth, "SerialBT.setPin", SERIALBT_SET_PIN, n0 = bt0->setPin(c1); NIP) \
  XV(bluetooth, "SerialBT.unpairDevice", SERIALBT_UNPAIR_DEVICE, \
      n0 = bt0->unpairDevice(b1); NIP) \
  XV(bluetooth, "SerialBT.connect", SERIALBT_CONNECT, n0 = bt0->connect(c1); NIP) \
  XV(bluetooth, "SerialBT.connectAddr", SERIALBT_CONNECT_ADDR, n0 = bt0->connect(b1); NIP) \
  XV(bluetooth, "SerialBT.disconnect", SERIALBT_DISCONNECT, n0 = bt0->disconnect()) \
  XV(bluetooth, "SerialBT.connected", SERIALBT_CONNECTED, n0 = bt0->connected(n1); NIP) \
  XV(bluetooth, "SerialBT.isReady", SERIALBT_IS_READY, n0 = bt0->isReady(n2, n1); NIPn(2)) \
  /* Bluetooth */ \
  YV(bluetooth, esp_bt_dev_get_address, PUSH esp_bt_dev_get_address())

#include "gen/esp32_serial-bluetooth.h"
