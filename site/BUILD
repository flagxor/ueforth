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

if not WEB_ENABLED:
  Return()

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
]

if WINDOWS_ENABLED:
  DEPLOY_ITEMS += [
    Copy('$dst/deploy/static/favicon.ico', 'images/eforth.ico'),
  ]

Alias('site', ' '.join(DEPLOY_ITEMS))
Default('site')

Alias('deploy', ' '.join([
  Command('deploy-esp32', 'site',
	        'cd $dst/deploy && gcloud app deploy -q --project esp32forth *.yaml', pool='console'),
  Command('deploy-eforth', 'site',
	        'cd $dst/deploy && gcloud app deploy -q --project eforth *.yaml', pool='console'),
]))

PUBLISH_PARTS = []

PUBLISH_PARTS += [
  Alias('publish-esp32', ' '.join([
    Publish('publish-esp32-rev',
           '$dst/esp32/ESP32forth.zip',
            'ESP32forth-{{VERSION}}-{{REVSHORT}}.zip'),
    Publish('publish-esp32-ver',
             '$dst/esp32/ESP32forth.zip',
             'ESP32forth-{{VERSION}}.zip'),
  ])),
  Alias('publish-linux', ' '.join([
    Publish('publish-linux-rev',
             '$dst/posix/ueforth',
             'ueforth-{{VERSION}}-{{REVSHORT}}.linux'),
    Publish('publish-linux-ver',
             '$dst/posix/ueforth',
             'ueforth-{{VERSION}}.linux'),
  ])),
  Alias('publish-web', ' '.join([
    Publish('publish-web-rev',
             '$dst/web/ueforth.js',
             'ueforth-{{VERSION}}-{{REVSHORT}}.js'),
    Publish('publish-web-ver',
             '$dst/web/ueforth.js',
             'ueforth-{{VERSION}}.js'),
  ])),
]

if PICO_ICE_ENABLED:
  PUBLISH_PARTS += [
    Alias('publish-pico-ice', ' '.join([
      Publish('publish-pico-ice-rev',
               '$dst/pico-ice/ueforth-pico-ice.zip',
               'ueforth-pico-ice-{{VERSION}}-{{REVSHORT}}.zip'),
      Publish('publish-pico-ice-ver',
               '$dst/pico-ice/ueforth-pico-ice.zip',
               'ueforth-pico-ice-{{VERSION}}.zip'),
    ])),
  ]

if WINDOWS_ENABLED:
  PUBLISH_PARTS += [
    Alias('publish-win', ' '.join([
      Alias('publish-win32', ' '.join([
        Publish('publish-win32-rev',
                 '$dst/windows/uEf32.exe',
                 'uEf32-{{VERSION}}-{{REVSHORT}}.exe'),
        Publish('publish-win32-ver',
                 '$dst/windows/uEf32.exe',
                 'uEf32-{{VERSION}}.exe'),
      ])),
      Alias('publish-win64', ' '.join([
        Publish('publish-win64-rev',
                 '$dst/windows/uEf64.exe',
                 'uEf64-{{VERSION}}-{{REVSHORT}}.exe'),
        Publish('publish-win64-ver',
                 '$dst/windows/uEf64.exe',
                 'uEf64-{{VERSION}}.exe'),
      ])),
    ])),
  ]

Alias('publish', ' '.join([
  Command('publish-index', ' '.join([
      '$src/tools/webindex.py',
      '$src/tools/publish.py',
    ] + PUBLISH_PARTS),
    '$src/tools/webindex.py >$dst/gen/archive.html && ' +
    '$src/tools/publish.py --src $dst/gen/archive.html --dst archive.html >/dev/null 2>&1'),
]))
