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

Alias('windows', 'win32 win64')

# Create boot.
Importation('gen/windows_boot.h', '$src/windows/windows_boot.fs',
            header_mode='win', name='boot')
Importation('gen/windows_boot_extra.h', '$src/windows/windows_boot_extra.fs',
            header_mode='win', name='boot_extra')

# Compile Win32.
Alias('win32', 'windows/uEf32.exe')
CompileW32('windows/uEf32.obj', '$src/windows/main.c',
           implicit=['gen/windows_boot.h', 'gen/windows_boot_extra.h'])
LinkW32('windows/uEf32.exe',
        ' '.join(['windows/uEf32.obj', 'resources/ueforth_res32.res']))

# Compile Win64.
Alias('win64', 'windows/uEf64.exe')
CompileW64('windows/uEf64.obj', '$src/windows/main.c',
           implicit=['gen/windows_boot.h', 'gen/windows_boot_extra.h'])
LinkW64('windows/uEf64.exe',
        ' '.join(['windows/uEf64.obj', 'resources/ueforth_res64.res']))

# Create icon.
ICON_SIZES = [
  ResizeImage('resources/eforth16x16.png', '$src/images/eforth.png', '16x16'),
  ResizeImage('resources/eforth32x32.png', '$src/images/eforth.png', '32x32'),
  ResizeImage('resources/eforth48x48.png', '$src/images/eforth.png', '48x48'),
  ResizeImage('resources/eforth256x256.png', '$src/images/eforth.png', '256x256'),
]
ConvertImage('resources/eforth.ico', ' '.join(ICON_SIZES))
CompileResource32('resources/ueforth_res32.res', '$src/windows/ueforth.rc',
                  implicit=['resources/eforth.ico'])
CompileResource64('resources/ueforth_res64.res', '$src/windows/ueforth.rc',
                  implicit=['resources/eforth.ico'])
