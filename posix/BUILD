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

Importation('$dst/gen/posix_boot.h', '$src/posix/posix_boot.fs', name='boot')
Compile('$dst/posix/ueforth', '$src/posix/main.c',
        implicit=['$dst/gen/posix_boot.h'])

Alias('posix', ' '.join([
  ForthTest('$dst/tests/posix_all_tests.out',
            '$dst/posix/ueforth', '$src/common/all_tests.fs'),
  TestCommand('$dst/tests/posix_see_all_test.out',
              '$dst/posix/ueforth',
              'echo internals see-all bye | $dst/posix/ueforth '
              '>$dst/tests/posix_see_all_test.out 2>&1'),
  TestCommand(' '.join(['$dst/tests/posix_save_restore_test.out',
                        '$dst/tests/posix_save_restore.bin']),
              '$dst/posix/ueforth',
              'echo ": foosquare dup * ; save $dst/tests/posix_save_restore.bin bye" | '
              '$dst/posix/ueforth > $dst/tests/posix_save_restore_test.out && '
              'echo "restore $dst/tests/posix_save_restore.bin 4 foosquare 16 <> posix sysexit" | '
              '$dst/posix/ueforth >> $dst/tests/posix_save_restore_test.out'),
]))

OneShot('install',
        '$dst/posix/ueforth',
        'sudo cp $dst/posix/ueforth /usr/bin/ueforth',
        pool='console')

Default('posix')
