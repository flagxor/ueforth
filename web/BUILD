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

Importation('$dst/gen/web_boot.js', '$src/web/web_boot.fs', header_mode='web', name='boot')
Compile('$dst/gen/dump_web_opcodes', '$src/web/dump_web_opcodes.c')
GenRun('$dst/gen/web_cases.js', '$dst/gen/dump_web_opcodes', 'cases', [])
GenRun('$dst/gen/web_dict.js', '$dst/gen/dump_web_opcodes', 'dict', [])
GenRun('$dst/gen/web_sys.js', '$dst/gen/dump_web_opcodes', 'sys', [])

WEB_ITEMS = [
  GenRun('$dst/web/ueforth.js', '$src/web/fuse_web.js', '', [
      '$src/web/web.template.js',
      '$dst/gen/web_boot.js',
      '$dst/gen/web_dict.js',
      '$dst/gen/web_cases.js',
      '$dst/gen/web_sys.js',
  ]),
  Copy('$dst/web/terminal.html', '$src/web/terminal.html'),
  Copy('$dst/web/lazy_terminal.html', '$src/web/lazy_terminal.html'),
  Copy('$dst/web/script_lite_test.html', '$src/web/script_lite_test.html'),
  Copy('$dst/web/script_test.html', '$src/web/script_test.html'),
  Copy('$dst/web/script_test.fs', '$src/web/script_test.fs'),
  Copy('$dst/web/serial.html', '$src/web/serial.html'),
]

if D8_AVAILABLE:
  WEB_ITEMS += [
    TestCommand('$dst/tests/web_sanity_test.out',
                ' '.join([
                    '$dst/web/ueforth.js',
                    '$src/tools/check_web_sanity.py',
                ]),
                'echo "120 3 + . cr bye" | ' + D8 + ' $dst/web/ueforth.js | '
                '$src/tools/check_web_sanity.py ' +
                '$dst/tests/web_sanity_test.out'),
  ]

Alias('web', ' '.join(WEB_ITEMS))
Default('web')

if D8_AVAILABLE:
  OneShot('d8',
          '$dst/web/ueforth.js',
          D8 + ' $dst/web/ueforth.js $in',
          pool='console')
