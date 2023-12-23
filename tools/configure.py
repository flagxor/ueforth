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
  '-I', '../',
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
cflags = %(cflags)s
strip_args = %(strip_args)s
libs = %(libs)s

rule mkdir
  description = mkdir
  command = mkdir -p $out

rule importation
  description = importation
  depfile = $out.dd
  command = ../tools/importation.py -i $in -o $out -I . -I .. --name $name --header $header_mode --depsout $out.dd -DVERSION=$version -DREVSION=$revision

build gen: mkdir

""" % {
  'version': VERSION,
  'revision': REVISION,
  'cflags': ' '.join(CFLAGS),
  'strip_args': ' '.join(STRIP_ARGS),
  'libs': ' '.join(LIBS),
}

def ForthHeader(target, name, source, header_mode='cpp'):
  source = '../' + source
  global output
  output += f"""
build {target}: importation {source} | gen
  name = {name}
  header_mode = {header_mode}
"""

ForthHeader('gen/posix_boot.h', 'boot', 'posix/posix_boot.fs')
ForthHeader('gen/window_boot.h', 'boot', 'windows/windows_boot.fs', header_mode='win')
ForthHeader('gen/window_boot_extra.h', 'boot_extra', 'windows/windows_boot_extra.fs', header_mode='win')
ForthHeader('gen/pico_ice_boot.h', 'boot', 'pico-ice/pico_ice_boot.fs')
ForthHeader('gen/esp32_boot.h', 'boot', 'esp32/esp32_boot.fs')
ForthHeader('gen/web_boot.js', 'boot', 'esp32/esp32_boot.fs', header_mode='web')

print(output)
