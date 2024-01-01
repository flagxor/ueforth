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

Importation('gen/web_boot.js', '$src/web/web_boot.fs', header_mode='web', name='boot')
Compile('gen/dump_web_opcodes', '$src/web/dump_web_opcodes.c')
GenRun('gen/web_cases.js', 'gen/dump_web_opcodes', 'cases', [])
GenRun('gen/web_dict.js', 'gen/dump_web_opcodes', 'dict', [])
GenRun('gen/web_sys.js', 'gen/dump_web_opcodes', 'sys', [])

WEB_ITEMS = [
  GenRun('web/ueforth.js', '$src/web/fuse_web.js', '', [
      '$src/web/web.template.js',
      'gen/web_boot.js',
      'gen/web_dict.js',
      'gen/web_cases.js',
      'gen/web_sys.js',
  ]),
  Copy('web/terminal.html', '$src/web/terminal.html'),
  Copy('web/lazy_terminal.html', '$src/web/lazy_terminal.html'),
  Copy('web/script_lite_test.html', '$src/web/script_lite_test.html'),
  Copy('web/script_test.fs', '$src/web/script_test.fs'),
]

Alias('web', ' '.join(WEB_ITEMS))
