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

OPTIONS = '-I $src/site'
UE_OPTIONS = OPTIONS + ' -DFORTH=uEForth'
ESP_OPTIONS = OPTIONS + ' -DFORTH=ESP32forth'

DEPLOY_ITEMS = []

DEPLOY_ITEMS += [
  Copy('$dst/deploy/.gcloudignore', '$src/site/.gcloudignore'),
  Copy('$dst/deploy/ueforth.js', '$dst/web/ueforth.js'),
  Importation('$dst/deploy/index.html', '$src/site/index.html', options=ESP_OPTIONS),
  Importation('$dst/deploy/linux.html', '$src/site/linux.html', options=UE_OPTIONS),
  Importation('$dst/deploy/windows.html', '$src/site/windows.html', options=UE_OPTIONS),
  Importation('$dst/deploy/web.html', '$src/site/web.html', options=UE_OPTIONS),
  Importation('$dst/deploy/ESP32forth.html', '$src/site/ESP32forth.html', options=ESP_OPTIONS),
  Importation('$dst/deploy/pico-ice.html', '$src/site/pico-ice.html', options=UE_OPTIONS),
  Importation('$dst/deploy/internals.html', '$src/site/internals.html', options=UE_OPTIONS),
  Importation('$dst/deploy/classic.html', '$src/site/classic.html', options=ESP_OPTIONS),
  Copy('$dst/deploy/eforth.go', '$src/site/eforth.go'),
  Copy('$dst/deploy/app.yaml', '$src/site/app.yaml'),
]

DEPLOY_ITEMS += [
  Copy('$dst/deploy/static/' + i, '$src/site/static/' + i) for i in [
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
] + [
  Copy('$dst/deploy/static/favicon.ico', '$dst/resources/eforth.ico'),
]

Alias('deploy', ' '.join(DEPLOY_ITEMS))

Alias('publish', ' '.join([
  Alias('publish-esp32', ' '.join([
    Publish('publish-esp32-rev',
           '$dst/esp32/ESP32forth.zip',
            'ESP32forth-{{VERSION}}-{{REVSHORT}}.zip'),
    Shortcut('publish-esp32-ver',
             '$dst/esp32/ESP32forth.zip',
             'ESP32forth-{{VERSION}}.zip'),
  ])),
  Alias('publish-pico-ice', ' '.join([
    Shortcut('publish-pico-ice-rev',
             '$dst/pico-ice/ueforth-pico-ice.zip',
             'ueforth-pico-ice-{{VERSION}}-{{REVSHORT}}.zip'),
    Shortcut('publish-pico-ice-ver',
             '$dst/pico-ice/ueforth-pico-ice.zip',
             'ueforth-pico-ice-{{VERSION}}.zip'),
  ])),
  Alias('publish-linux', ' '.join([
    Shortcut('publish-linux-rev',
             '$dst/posix/ueforth',
             'ueforth-{{VERSION}}-{{REVSHORT}}.linux'),
    Shortcut('publish-linux-ver',
             '$dst/posix/ueforth',
             'ueforth-{{VERSION}}.linux'),
  ])),
  Alias('publish-web', ' '.join([
    Shortcut('publish-web-rev',
             '$dst/web/ueforth.js',
             'ueforth-{{VERSION}}-{{REVSHORT}}.js'),
    Shortcut('publish-web-ver',
             '$dst/web/ueforth.js',
             'ueforth-{{VERSION}}.js'),
  ])),
  Alias('publish-win', ' '.join([
    Alias('publish-win32', ' '.join([
      Shortcut('publish-win32-rev',
               '$dst/windows/uEf32.exe',
               'uEf32-{{VERSION}}-{{REVSHORT}}.exe'),
      Shortcut('publish-win32-ver',
               '$dst/windows/uEf32.exe',
               'uEf32-{{VERSION}}.exe'),
    ])),
    Alias('publish-win64', ' '.join([
      Shortcut('publish-win64-rev',
               '$dst/windows/uEf64.exe',
               'uEf64-{{VERSION}}-{{REVSHORT}}.exe'),
      Shortcut('publish-win64-ver',
               '$dst/windows/uEf64.exe',
               'uEf64-{{VERSION}}.exe'),
    ])),
  ])),
]))
