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
  '-I', '$src',
  '-I', './',
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
cxx = g++

rule mkdir
  description = mkdir
  command = mkdir -p $out

rule importation
  description = importation
  depfile = $out.dd
  command = ../tools/importation.py -i $in -o $out -I . -I $src $options --depsout $depfile -DVERSION=$version -DREVISION=$revision

rule compile
  description = CXX
  depfile = $out.dd
  command = $cxx $cflags $in -o $out $libs -MD -MF $depfile && strip $strip_args $out

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
  global output
  options = ''
  if keep:
    options += '--keep-first-comment'
  if name:
    options += ' --name ' + name + ' --header ' + header_mode
  outdir = os.path.dirname(target)
  implicit = ' '.join(implicit)
  output += f'build {target}: importation {source} | {outdir} {implicit}\n'
  if options:
    output += f'  options = {options}\n'
  if deps:
    output += f'  depfile = {deps}\n'

def Esp32Optional(main_name, main_source, parts):
  for name, source in parts:
    Importation('gen/esp32_' + name + '.h',
                source, name=name.replace('-', '_') + '_source')
  if not main_source:
    main_source = 'gen/esp32_' + main_name + '.h'
  Importation('esp32/ESP32forth/optional/' + main_name + '.h',
              main_source,
            keep=True,
            deps='gen/esp32_optional_' + main_name + '.h.dd',
            implicit=['gen/esp32_' + i + '.h' for i, _ in parts])

def Compile(target, source, implicit=[]):
  global output
  outdir = os.path.dirname(target)
  implicit = ' '.join(implicit)
  output += f'build {target}: compile {source} | {outdir} {implicit}\n'


Importation('esp32/ESP32forth/ESP32forth.ino',
            '$src/esp32/ESP32forth.ino',
            implicit=['gen/esp32_boot.h'], keep=True)
Importation('gen/esp32_boot.h', '$src/esp32/esp32_boot.fs', name='boot')
Importation('esp32/ESP32forth/README.txt',
            '$src/esp32/README.txt')
Importation('esp32/ESP32forth/optional/README-optional.txt',
            '$src/esp32/optional/README-optional.txt')
Esp32Optional('rmt', '$src/esp32/optional/rmt.h', [])
Esp32Optional('assemblers', '$src/esp32/optional/assemblers/assemblers.h',
              [('assembler', '$src/common/assembler.fs'),
               ('xtensa-assembler', '$src/esp32/optional/assemblers/xtensa-assembler.fs'),
               ('riscv-assembler', '$src/esp32/optional/assemblers/riscv-assembler.fs')])
Esp32Optional('camera', '$src/esp32/optional/camera/camera.h',
              [('camera', '$src/esp32/optional/camera/camera_server.fs')])
Esp32Optional('interrupts', '$src/esp32/optional/interrupts/interrupts.h',
              [('interrupts', '$src/esp32/optional/interrupts/timers.fs')])
Esp32Optional('oled', '$src/esp32/optional/oled/oled.h',
              [('oled', '$src/esp32/optional/oled/oled.fs')])
Esp32Optional('spi-flash', '$src/esp32/optional/spi-flash/spi-flash.h',
              [('spi-flash', '$src/esp32/optional/spi-flash/spi-flash.fs')])
Esp32Optional('serial-bluetooth', '$src/esp32/optional/serial-bluetooth/serial-bluetooth.h',
              [('serial-bluetooth', '$src/esp32/optional/serial-bluetooth/serial-bluetooth.fs')])

Importation('gen/posix_boot.h', '$src/posix/posix_boot.fs', name='boot')
Compile('posix/ueforth', '$src/posix/main.c',
        implicit=['gen/posix_boot.h'])

Importation('gen/window_boot.h', '$src/windows/windows_boot.fs', header_mode='win', name='boot')
Importation('gen/window_boot_extra.h', '$src/windows/windows_boot_extra.fs', header_mode='win', name='boot')
Importation('gen/pico_ice_boot.h', '$src/pico-ice/pico_ice_boot.fs', name='boot')
Importation('gen/web_boot.js', '$src/web/web_boot.fs', header_mode='web', name='boot')

print(output)
