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

PICO_ICE_FILES = []
PICO_ICE_ZIP_FILES = []

# Main .ino file.
PICO_ICE_ZIP_FILES += [
  Importation('$dst/pico-ice/ueforth-pico-ice/README.txt',
              '$src/pico-ice/README.txt'),
  # TODO: .uf2
  Copy('$dst/pico-ice/ueforth-pice-ice/LICENSE', '$src/LICENSE'),
  Copy('$dst/pico-ice/ueforth-pice-ice/pico-ice-sdk-LICENSE.md',
       '$src/pico-ice/pico-ice-sdk/LICENSE.md'),
  Copy('$dst/pico-ice/ueforth-pice-ice/pico-sdk-LICENSE.TXT',
       '$src/pico-ice/pico-sdk/LICENSE.TXT'),
]

# Zip it.
PICO_ICE_FILES += [
  Zip('$dst/pico-ice/ueforth-pico-ice.zip', PICO_ICE_ZIP_FILES,
      base='$dst/pico-ice/ueforth-pico-ice'),
]

# Simulator.
PICO_ICE_FILES += [
  CompileSim('$dst/pico-ice-sim/ueforth_pico_ice_sim', '$src/pico-ice/main.c',
             implicit=['$dst/gen/pico_ice_boot.h'])
]
Importation('$dst/gen/pico_ice_boot.h', '$src/pico-ice/pico_ice_boot.fs', name='boot')

# Main Alias.
Alias('pico', ' '.join(PICO_ICE_FILES))
