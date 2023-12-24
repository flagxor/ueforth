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

ESP32_FILES = []

# Main .ino file.
ESP32_FILES += [
  Importation('esp32/ESP32forth/README.txt',
              '$src/esp32/README.txt'),
  Importation('esp32/ESP32forth/ESP32forth.ino',
              '$src/esp32/ESP32forth.ino',
              implicit=['gen/esp32_boot.h'], keep=True),
]
Importation('gen/esp32_boot.h', '$src/esp32/esp32_boot.fs', name='boot')

# Optional Compontents.
ESP32_FILES += [
  Importation('esp32/ESP32forth/optional/README-optional.txt',
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

# Simulator.
ESP32_FILES += [
  Compile('esp32-sim/esp32-sim', '$src/esp32/sim_main.cpp',
          implicit=['gen/esp32_boot.h', 'gen/esp32_sim_opcodes.h'])
]
Compile('gen/print-esp32-builtins', '$src/esp32/print-builtins.cpp'),
Run('gen/esp32_sim_opcodes.h', 'gen/print-esp32-builtins')

# Main Alias.
Alias('esp', ' '.join(ESP32_FILES))
