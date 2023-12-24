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

SCRIPT_DIR = os.path.dirname(os.path.abspath(__file__))
ROOT_DIR = os.path.dirname(SCRIPT_DIR)

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

WIN_CFLAGS = CFLAGS_COMMON + [
  '-I', '"c:/Program Files (x86)/Microsoft SDKs/Windows/v7.1A/Include"',
  '-I', '"c:/Program Files (x86)/Microsoft Visual Studio/2019/Community/VC/Tools/MSVC/14.28.29333/include"',
  '-I', '"c:/Program Files (x86)/Windows Kits/10/Include/10.0.19041.0/ucrt"',
]

WIN_LIBS = [
  'user32.lib',
]

WIN_LFLAGS32 = [
  '/LIBPATH:"c:/Program Files (x86)/Microsoft SDKs/Windows/v7.1A/Lib"',
  '/LIBPATH:"c:/Program Files (x86)/Microsoft Visual Studio/2019/Community/VC/Tools/MSVC/14.28.29333/lib/x86"',
  '/LIBPATH:"c:/Program Files (x86)/Windows Kits/10/Lib/10.0.19041.0/ucrt/x86"',
] + WIN_LIBS

WIN_LFLAGS64 = [
  '/LIBPATH:"c:/Program Files (x86)/Microsoft SDKs/Windows/v7.1A/Lib/x64"',
  '/LIBPATH:"c:/Program Files (x86)/Microsoft Visual Studio/2019/Community/VC/Tools/MSVC/14.28.29333/lib/x64"',
  '/LIBPATH:"c:/Program Files (x86)/Windows Kits/10/Lib/10.0.19041.0/ucrt/x64"',
] + WIN_LIBS

def LSQ(path):
  return subprocess.check_output(
      ['ls', path], stderr=subprocess.DEVNULL, shell=True).splitlines()[0]

PROGFILES = '/mnt/c/Program Files (x86)'
MSVS = PROGFILES + '/Microsoft Visual Studio'
MSKITS = PROGFILES + '/Windows Kits'
CL32 = LSQ(MSVS + '/*/*/VC/Tools/MSVC/*/bin/Hostx86/x86/cl.exe')
CL64 = LSQ(MSVS + '/*/*/VC/Tools/MSVC/*/bin/Hostx86/x64/cl.exe')
LINK32 = LSQ(MSVS + '/*/*/VC/Tools/MSVC/*/bin/Hostx86/x86/link.exe')
LINK64 = LSQ(MSVS + '/*/*/VC/Tools/MSVC/*/bin/Hostx86/x64/link.exe')
RC32 = LSQ(MSKITS + '/*/bin/*/x86/rc.exe')
RC64 = LSQ(MSKITS + '/*/bin/*/x64/rc.exe')

D8 = LSQ('${HOME}/src/v8/v8/out/x64.release/d8')
NODEJS = LSQ('/usr/bin/nodejs')

output = f"""
version = {VERSION}
revision = {REVISION}
src = ../
cflags = {' '.join(CFLAGS)}
strip_args = {' '.join(STRIP_ARGS)}
libs = {' '.join(LIBS)}
cxx = g++

CL32 = {CL32}
CL64 = {CL64}
LINK32 = {LINK32}
LINK64 = {LINK64}
RC32 = {RC32}
RC64 = {RC64}

D8 = {D8}
NODEJS = {NODEJS}

win_cflags = {' '.join(WIN_CFLAGS)}
win_lflags32 = {' '.join(WIN_LFLAGS32)}
win_lflags64 = {' '.join(WIN_LFLAGS64)}

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

rule run
  description = RUN
  command = $in >$out

build gen: mkdir

"""


def Mkdir(path):
  global output
  output += 'build ' + path + ': mkdir\n'


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


def Run(target, source):
  global output
  output += f'build {target}: run {source}\n'


def Include(path):
  Mkdir(path)
  path = os.path.join(ROOT_DIR, path, 'BUILD')
  data = open(path).read()
  exec(data)


Include('.')
print(output)
