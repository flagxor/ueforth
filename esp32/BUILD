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

OPTIONS = '-I $src/esp32'
ESP32_FILES = []
ESP32_ZIP_FILES = []
ESP32_OPTIONAL = []

# Main files.
ESP32_ZIP_FILES += [
  Importation('$dst/esp32/ESP32forth/README.txt',
              '$src/esp32/README.txt'),
  Importation('$dst/esp32/ESP32forth/ESP32forth.ino',
              '$src/esp32/ESP32forth.ino',
              implicit=['$dst/gen/esp32_boot.h'], keep=True, options=OPTIONS),
]
Importation('$dst/gen/esp32_boot.h', '$src/esp32/esp32_boot.fs', name='boot')


# Create a second copy of ESP32 where options will all be installed.
ESP32_OPTIONAL.append(
  Copy('$dst/esp32/with_optional/ESP32forth/ESP32forth.ino',
       '$dst/esp32/ESP32forth/ESP32forth.ino'))


def Esp32Optional(main_name, main_source, parts):
  global ESP32_OPTIONAL
  ESP32_OPTIONAL.append(
      Copy('$dst/esp32/with_optional/ESP32forth/' + main_name + '.h',
           '$dst/esp32/ESP32forth/optional/' + main_name + '.h'))
  implicit = []
  for name, source in parts:
    implicit.append(Importation('$dst/gen/esp32_' + name + '.h',
                                source, name=name.replace('-', '_') + '_source'))
  return Importation('$dst/esp32/ESP32forth/optional/' + main_name + '.h',
                     main_source,
                     keep=True,
                     deps='$dst/gen/esp32_optional_' + main_name + '.h.d',
                     implicit=implicit)


# Optional Compontents.
ESP32_ZIP_FILES += [
  Importation('$dst/esp32/ESP32forth/optional/README-optional.txt',
              '$src/esp32/optional/README-optional.txt'),
  Esp32Optional('rmt', '$src/esp32/optional/rmt.h', []),
  Esp32Optional('assemblers', '$src/esp32/optional/assemblers/assemblers.h',
                [('assembler', '$src/common/assembler.fs'),
                 ('xtensa-assembler', '$src/esp32/optional/assemblers/xtensa-assembler.fs'),
                 ('riscv-assembler', '$src/esp32/optional/assemblers/riscv-assembler.fs')]),
  Esp32Optional('camera', '$src/esp32/optional/camera/camera.h',
                [('camera', '$src/esp32/optional/camera/camera_server.fs')]),
  Esp32Optional('interrupts', '$src/esp32/optional/interrupts/interrupts.h',
                [('interrupts', '$src/esp32/optional/interrupts/timers.fs')]),
  Esp32Optional('oled', '$src/esp32/optional/oled/oled.h',
                [('oled', '$src/esp32/optional/oled/oled.fs')]),
  Esp32Optional('spi-flash', '$src/esp32/optional/spi-flash/spi-flash.h',
                [('spi-flash', '$src/esp32/optional/spi-flash/spi-flash.fs')]),
  Esp32Optional('serial-bluetooth', '$src/esp32/optional/serial-bluetooth/serial-bluetooth.h',
                [('serial-bluetooth', '$src/esp32/optional/serial-bluetooth/serial-bluetooth.fs')]),
]

# Zip it.
ESP32_FILES += [
  Zip('$dst/esp32/ESP32forth.zip', ESP32_ZIP_FILES, base='$dst/esp32/ESP32forth'),
]

# Simulator.
ESP32_FILES += [
  Compile('$dst/esp32-sim/Esp32forth-sim', '$src/esp32/sim_main.cpp',
          implicit=['$dst/gen/esp32_boot.h', '$dst/gen/esp32_sim_opcodes.h']),
  TestCommand('$dst/esp32-sim/sizes.txt',
              ' '.join(['$dst/esp32-sim/Esp32forth-sim', '$src/tools/memuse.py']),
	            'echo internals size-all bye | $dst/esp32-sim/Esp32forth-sim | '
              '$src/tools/memuse.py >$dst/esp32-sim/sizes.txt'),
  ForthTest('$dst/tests/esp32_sim_all_tests.out',
            '$dst/esp32-sim/Esp32forth-sim', '$src/esp32/esp32_all_tests.fs',
            interp=('echo "include $src/esp32/esp32_all_tests.fs \\n1 terminate" | '
                    '$dst/esp32-sim/Esp32forth-sim')),
  TestCommand('$dst/tests/esp32_sim_see_all_test.out',
              '$dst/esp32-sim/Esp32forth-sim',
              'echo internals see-all bye | $dst/esp32-sim/Esp32forth-sim '
              '>$dst/tests/esp32_sim_see_all_test.out 2>&1'),
]
Compile('$dst/gen/print-esp32-builtins', '$src/esp32/print-builtins.cpp'),
Run('$dst/gen/esp32_sim_opcodes.h', '$dst/gen/print-esp32-builtins')

# Main Alias.
Alias('esp32', ' '.join(ESP32_FILES + ESP32_OPTIONAL))
Default('esp32')

# Automate building / flashing.
BOARDS = {
  'esp32': '--fqbn=esp32:esp32:esp32:PSRAM=disabled,PartitionScheme=no_ota,CPUFreq=240,FlashMode=qio,FlashFreq=80,FlashSize=4M,UploadSpeed=921600,LoopCore=1,EventsCore=1,DebugLevel=none,EraseFlash=none',
  'esp32s2': '--fqbn=esp32:esp32:esp32s2:CDCOnBoot=default,MSCOnBoot=default,DFUOnBoot=default,UploadMode=default,PSRAM=disabled,PartitionScheme=default,CPUFreq=240,FlashMode=qio,FlashFreq=80,FlashSize=4M,UploadSpeed=921600,DebugLevel=none,EraseFlash=none',
  'esp32s3': '--fqbn=esp32:esp32:esp32s3:PSRAM=disabled,FlashMode=qio,FlashSize=4M,LoopCore=1,EventsCore=1,USBMode=hwcdc,CDCOnBoot=default,MSCOnBoot=default,DFUOnBoot=default,UploadMode=default,PartitionScheme=default,CPUFreq=240,UploadSpeed=921600,DebugLevel=none,EraseFlash=none',
  'esp32c3': '--fqbn=esp32:esp32:esp32c3:CDCOnBoot=default,PartitionScheme=default,CPUFreq=160,FlashMode=qio,FlashFreq=80,FlashSize=4M,UploadSpeed=921600,DebugLevel=none,EraseFlash=none',
  'esp32cam': '--fqbn=esp32:esp32:esp32cam:CPUFreq=240,FlashMode=qio,PartitionScheme=huge_app,FlashFreq=80,DebugLevel=none,EraseFlash=none',
}
for board_base in BOARDS:
  options = BOARDS[board_base]
  for opt, optdir, deps in (('', '', ESP32_FILES),
                            ('opt', 'with_optional/', ESP32_OPTIONAL)):
    board = board_base + opt
    clobber = f'rm -rf {WINTMP}/ueforth_esp32/{board}_dir/ && '
    setup = (f'mkdir -p {WINTMP}/ueforth_esp32/{board}_dir/build && '
             f'mkdir -p {WINTMP}/ueforth_esp32/{board}_dir/cache && '
             f'cp -r $dst/esp32/ESP32forth {WINTMP}/ueforth_esp32/{board}_dir/ && '
             f'cd {WINTMP} && ')
    cmd = f' {ARDUINO_CLI} compile '
    upload = '--port $${PORT:-com3} --upload '
    args = (f'{options} '
            f'--build-path ueforth_esp32/{board}_dir/build '
            f'--build-cache-path ueforth_esp32/{board}_dir/cache '
            f'ueforth_esp32/{board}_dir/ESP32forth/ESP32forth.ino ')
    pipe = f' > ueforth_esp32/{board}_dir/build/build.out 2>&1 && '
    copyout = (f'cd - && '
               f'cp {WINTMP}/ueforth_esp32/{board}_dir/build/*.bin '
               f'$dst/esp32/{board}_build/ && '
               f'cp {WINTMP}/ueforth_esp32/{board}_dir/build/build.out '
               f'$dst/esp32/{board}_build/build.out')
    Shortcut(f'{board}-flash',
             ' '.join(deps),
             setup + cmd + upload + args, pool='console')
    Command(' '.join([f'$dst/esp32/{board}_build/ESP32forth.ino.bin',
                      f'$dst/esp32/{board}_build/build.out']),
            ' '.join(deps),
            clobber + setup + cmd + args + pipe + copyout, pool='serial')
    Alias(f'{board}-build', f'$dst/esp32/{board}_build/ESP32forth.ino.bin')

Alias('esp32all', ' '.join([i + '-build' for i in BOARDS.keys()]))

Alias('vet', ' '.join([
  'all',
  'esp32all',
  'esp32opt-build',
]))

Shortcut('clobber-esp32', '', f'rm -rf {WINTMP}/ueforth_esp32/')

Shortcut('putty', '', '$${HOME}/Desktop/putty.exe -serial $${PORT:-com3} -sercfg 115200')
