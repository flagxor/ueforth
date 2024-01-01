# Copyright 2023 Bradley D. Nelson
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

REPLACE = [
  './tools/importation.py',
  '-I', 'site',
  '-DVERSION=' + VERSION,
  '-DSTABLE_VERSION' + STABLE_VERSION,
  '-DOLD_STABLE_VERSION=' + OLD_STABLE_VERSION,
]

UE_REPLACE = REPLACE + ['-DFORTH=uEForth']
ESP_REPLACE = REPLACE + ['-DFORTH=ESP32forth']

DEPLOY_ITEMS = []

DEPLOY_ITEMS += [
  Copy('deploy/.gcloudignore', '$src/site/.gcloudignore'),
  Copy('deploy/ueforth.js', 'web/ueforth.js'),
  Importation('deploy/index.html', '$src/site/index.html'),
  Importation('deploy/linux.html', '$src/site/linux.html'),
  Importation('deploy/windows.html', '$src/site/windows.html'),
  Importation('deploy/web.html', '$src/site/web.html'),
  Importation('deploy/ESP32forth.html', '$src/site/ESP32forth.html'),
  Importation('deploy/pico-ice.html', '$src/site/pico-ice.html'),
  Importation('deploy/internals.html', '$src/site/internals.html'),
  Importation('deploy/classic.html', '$src/site/classic.html'),
  Copy('deploy/eforth.go', '$src/site/eforth.go'),
  Copy('deploy/app.yaml', '$src/site/app.yaml'),
]

DEPLOY_ITEMS += [
  Copy('deploy/static/' + i, '$src/site/static/' + i) for i in [
    'eforth.css',
    'robots.txt',
    'esp32-c3-wroom-02.jpg',
    'esp32-s2-wroom.jpg',
    'esp32-wroom.jpg',
    'esp-cam-front.jpg',
    'esp32-c3-mini-1.jpg',
    'esp32-mini-1.jpg',
    'esp32-s2-wrover.jpg',
    'esp-cam-back.jpg',
    'esp-cam-wiring.jpg',
    'serial_bridge.jpg',
  ]
]

Alias('site', ' '.join(DEPLOY_ITEMS))
