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

# Main files.
ESP32_ZIP_FILES += [
  Importation('$dst/esp32/ESP32forth/README.txt',
              '$src/esp32/README.txt'),
  Importation('$dst/esp32/ESP32forth/ESP32forth.ino',
              '$src/esp32/ESP32forth.ino',
              implicit=['$dst/gen/esp32_boot.h'], keep=True, options=OPTIONS),
]
Importation('$dst/gen/esp32_boot.h', '$src/esp32/esp32_boot.fs', name='boot')

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
  Command('$dst/esp32-sim/sizes.txt',
          ' '.join(['$dst/esp32-sim/Esp32forth-sim', '$src/tools/memuse.py']),
	        'echo internals size-all bye | $dst/esp32-sim/Esp32forth-sim | '
          '$src/tools/memuse.py >$dst/esp32-sim/sizes.txt'),
  ForthTest('$dst/tests/esp32_sim_all_tests.out',
            '$dst/esp32-sim/Esp32forth-sim', '$src/esp32/esp32_all_tests.fs',
            interp=('echo "include $src/esp32/esp32_all_tests.fs \\n1 terminate" | '
                    '$dst/esp32-sim/Esp32forth-sim')),
]
Compile('$dst/gen/print-esp32-builtins', '$src/esp32/print-builtins.cpp'),
Run('$dst/gen/esp32_sim_opcodes.h', '$dst/gen/print-esp32-builtins')

# Main Alias.
Alias('esp32', ' '.join(ESP32_FILES))
Default('esp32')

# Automate building / flashing.
BOARDS = {
  'esp32': '--fqbn=esp32:esp32:esp32:PSRAM=disabled,PartitionScheme=no_ota,CPUFreq=240,FlashMode=qio,FlashFreq=80,FlashSize=4M,UploadSpeed=921600,LoopCore=1,EventsCore=1,DebugLevel=none,EraseFlash=none',
  'esp32s2': '--fqbn=esp32:esp32:esp32s2:CDCOnBoot=default,MSCOnBoot=default,DFUOnBoot=default,UploadMode=default,PSRAM=disabled,PartitionScheme=default,CPUFreq=240,FlashMode=qio,FlashFreq=80,FlashSize=4M,UploadSpeed=921600,DebugLevel=none,EraseFlash=none',
  'esp32s3': '--fqbn=esp32:esp32:esp32s3:PSRAM=disabled,FlashMode=qio,FlashSize=4M,LoopCore=1,EventsCore=1,USBMode=hwcdc,CDCOnBoot=default,MSCOnBoot=default,DFUOnBoot=default,UploadMode=default,PartitionScheme=default,CPUFreq=240,UploadSpeed=921600,DebugLevel=none,EraseFlash=none',
  'esp32c3': '--fqbn=esp32:esp32:esp32c3:CDCOnBoot=default,PartitionScheme=default,CPUFreq=160,FlashMode=qio,FlashFreq=80,FlashSize=4M,UploadSpeed=921600,DebugLevel=none,EraseFlash=none',
  'esp32cam': '--fqbn=esp32:esp32:esp32cam:CPUFreq=240,FlashMode=qio,PartitionScheme=huge_app,FlashFreq=80,DebugLevel=none,EraseFlash=none',
}
for board in BOARDS:
  options = BOARDS[board]
  Shortcut(f'{board}-flash',
          '$dst/esp32/ESP32forth/ESP32forth.ino',
	        f"""\
          rm -rf {WINTMP}/ueforth_esp32/ESP32forth/ && \
          cp -r $dst/esp32/ESP32forth {WINTMP}/ueforth_esp32/ && \
          cd {WINTMP} && \
          {ARDUINO_CLI} compile """ + """\
          --port $${PORT:-com3} --upload """ + f"""\
          {options} \
          --build-path ueforth_esp32/{board}_build \
          --build-cache-path ueforth_esp32/{board}_cache \
          ueforth_esp32/ESP32forth/ESP32forth.ino""")
  Command(f'$dst/esp32/{board}_build/ESP32forth.ino.bin',
          '$dst/esp32/ESP32forth/ESP32forth.ino',
	        f"""\
          rm -rf {WINTMP}/ueforth_esp32/ESP32forth/ && \
          cp -r $dst/esp32/ESP32forth {WINTMP}/ueforth_esp32/ && \
          cd {WINTMP} && \
          {ARDUINO_CLI} compile {options} \
          --build-path ueforth_esp32/{board}_build \
          --build-cache-path ueforth_esp32/{board}_cache \
          ueforth_esp32/ESP32forth/ESP32forth.ino && \
          cd - && \
          cp {WINTMP}/ueforth_esp32/{board}_build/*.bin \
          $dst/esp32/{board}_build/ESP32forth.ino.bin""")
  Alias(f'{board}-build', f'$dst/esp32/{board}_build/ESP32forth.ino.bin')
Alias('esp32all', ' '.join([i + '-build' for i in BOARDS.keys()]))

Alias('putty', OneShot('$dst/gen/putty.not', '',
                       '$${HOME}/Desktop/putty.exe -serial $${PORT:-com3} -sercfg 115200'))
