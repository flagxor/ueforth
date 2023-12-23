#! /usr/bin/env python3
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

import os
import sys
import subprocess

VERSION = '7.0.7.16'
STABLE_VERSION = '7.0.6.19'
OLD_STABLE_VERSION = '7.0.5.4'

REVISION = 'TODO'
#REVISION=$(shell git rev-parse HEAD | head -c 20)
#REVSHORT=$(shell echo $(REVISION) | head -c 7)

CFLAGS_COMMON = [
  '-O2',
  '-I', './',
  '-I', '$src',
]

CFLAGS_MINIMIZE = [
  '-s',
  '-DUEFORTH_MINIMAL',
  '-fno-exceptions',
  '-ffreestanding',
  '-fno-stack-protector',
  '-fomit-frame-pointer',
  '-fno-ident',
  '-ffunction-sections', '-fdata-sections',
  '-fmerge-all-constants',
]

if sys.platform == 'linux':
  CFLAGS_MINIMIZE.append('-Wl,--build-id=none')

CFLAGS = CFLAGS_COMMON + CFLAGS_MINIMIZE + [
  '-std=c++11',
  '-Wall',
  '-Werror',
  '-no-pie',
  '-Wl,--gc-sections',
]

if sys.platform == 'darwin':
  CFLAGS += [
    '-Wl,-dead_strip',
    '-D_GNU_SOURCE',
  ]
elif sys.platform == 'linux':
  CFLAGS += [
    '-s',
    '-Wl,--gc-sections',
    '-no-pie',
    '-Wl,--build-id=none',
  ]

STRIP_ARGS = ['-S']
if sys.platform == 'darwin':
  STRIP_ARGS += ['-x']
elif sys.platform == 'linux':
  STRIP_ARGS += [
    '--strip-unneeded',
    '--remove-section=.note.gnu.gold-version',
    '--remove-section=.comment',
    '--remove-section=.note',
    '--remove-section=.note.gnu.build-id',
    '--remove-section=.note.ABI-tag',
  ]

LIBS = ['-ldl']

output = """

version = %(version)s
revision = %(revision)s
src = ../
cflags = %(cflags)s
strip_args = %(strip_args)s
libs = %(libs)s

rule mkdir
  description = mkdir
  command = mkdir -p $out

rule importation
  description = importation
  depfile = $out.dd
  command = ../tools/importation.py -i $in -o $out -I . -I $src $options --depsout $depfile -DVERSION=$version -DREVSION=$revision

build gen: mkdir
build posix: mkdir
build windows: mkdir
build esp32: mkdir
build esp32/ESP32forth: mkdir
build esp32/ESP32forth/optional: mkdir

""" % {
  'version': VERSION,
  'revision': REVISION,
  'cflags': ' '.join(CFLAGS),
  'strip_args': ' '.join(STRIP_ARGS),
  'libs': ' '.join(LIBS),
}

def Importation(target, source, header_mode='cpp', name=None, keep=False, deps=None, implicit=[]):
  options = ''
  if keep:
    options += '--keep-first-comment'
  if name:
    options += ' --name ' + name + ' --header ' + header_mode
  outdir = os.path.dirname(target)
  implicit = ' '.join(implicit)
  global output
  output += f'build {target}: importation {source} | {outdir} {implicit}\n'
  if options:
    output += f'  options = {options}\n'
  if deps:
    output += f'  depfile = {deps}\n'

def Esp32Optional(target, c_source, header, name, forth_source):
  Importation(target, name, forth_source)
  Importation('gen/' + header, name, forth_source)

Importation('gen/esp32_assembler.h',
            '$src/common/assembler.fs', name='assembler_source')
Importation('gen/esp32_xtensa-assembler.h',
            '$src/esp32/optional/assemblers/xtensa-assembler.fs', name='xtensa_assembler_source')
Importation('gen/esp32_riscv-assembler.h',
            '$src/esp32/optional/assemblers/riscv-assembler.fs', name='riscv_assembler_source')
Importation('esp32/ESP32forth/optional/assemblers.h',
            '$src/esp32/optional/assemblers/assemblers.h',
            deps='gen/esp32_optional_assemblers.h.dd',
            implicit=[
              'gen/esp32_assembler.h',
              'gen/esp32_xtensa-assembler.h',
              'gen/esp32_riscv-assembler.h',
            ])

Importation('gen/esp32_camera.h',
            '$src/esp32/optional/camera/camera_server.fs', name='camera_source')
Importation('esp32/ESP32forth/optional/camera.h',
            'gen/esp32_camera.h',
            deps='gen/esp32_optional_camera.h.dd',
            implicit=['gen/esp32_camera.h'])

Importation('gen/esp32_interrupts.h',
            '$src/esp32/optional/interrupts/timers.fs', name='interrupts_source')
Importation('esp32/ESP32forth/optional/interrupts.h',
            'gen/esp32_interrupts.h',
            deps='gen/esp32_optional_interrupts.h.dd',
            implicit=['gen/esp32_interrupts.h'])

Importation('gen/esp32_oled.h',
            '$src/esp32/optional/oled/oled.fs', name='oled_source')
Importation('esp32/ESP32forth/optional/oled.h',
            'gen/esp32_oled.h',
            deps='gen/esp32_optional_oled.h.dd',
            implicit=['gen/esp32_oled.h'])

Importation('gen/esp32_spi-flash.h',
            '$src/esp32/optional/spi-flash/spi-flash.fs',
            name='spi_flash_source')
Importation('esp32/ESP32forth/optional/spi-flash.h',
            'gen/esp32_spi-flash.h',
            deps='gen/esp32_optional_spi-flash.h.dd',
            implicit=['gen/esp32_spi-flash.h'])

Importation('gen/esp32_serial-bluetooth.h',
            '$src/esp32/optional/serial-bluetooth/serial-bluetooth.fs',
            name='serial_bluetooth_source')
Importation('esp32/ESP32forth/optional/serial-bluetooth.h',
            'gen/esp32_serial-bluetooth.h',
            deps='gen/esp32_optional_serial-bluetooth.h.dd',
            implicit=['gen/esp32_serial-bluetooth.h'])

Importation('gen/posix_boot.h', '$src/posix/posix_boot.fs', name='boot')
Importation('gen/window_boot.h', '$src/windows/windows_boot.fs', header_mode='win', name='boot')
Importation('gen/window_boot_extra.h', '$src/windows/windows_boot_extra.fs', header_mode='win', name='boot')
Importation('gen/pico_ice_boot.h', '$src/pico-ice/pico_ice_boot.fs', name='boot')
Importation('gen/esp32_boot.h', '$src/esp32/esp32_boot.fs', name='boot')
Importation('gen/web_boot.js', '$src/web/web_boot.fs', header_mode='web', name='boot')

print(output)