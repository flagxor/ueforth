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
 * ESP32forth ESP32-CAM Camera v{{VERSION}}
 * Revision: {{REVISION}}
 */

#include "esp_camera.h"

#define OPTIONAL_CAMERA_VOCABULARY V(camera)
#define OPTIONAL_CAMERA_SUPPORT \
  XV(internals, "camera-source", CAMERA_SOURCE, \
      PUSH camera_source; PUSH sizeof(camera_source) - 1) \
  YV(camera, esp_camera_init, n0 = esp_camera_init((camera_config_t *) a0)) \
  YV(camera, esp_camera_deinit, PUSH esp_camera_deinit()) \
  YV(camera, esp_camera_fb_get, PUSH esp_camera_fb_get()) \
  YV(camera, esp_camera_fb_return, esp_camera_fb_return((camera_fb_t *) a0); DROP) \
  YV(camera, esp_camera_sensor_get, PUSH esp_camera_sensor_get()) \
  YV(camera, esp_camera_save_to_nvs, n0 = esp_camera_save_to_nvs(c0)) \
  YV(camera, esp_camera_load_from_nvs, n0 = esp_camera_load_from_nvs(c0))

#include "esp32_camera.h"
