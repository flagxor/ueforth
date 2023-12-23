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

import argparse
import os
import sys

parser = argparse.ArgumentParser(
  prog='importation',
  description='Imports header / fs files')
parser.add_argument('-i', required=True)
parser.add_argument('-o', required=True)
parser.add_argument('-I', action='append')
parser.add_argument('-D', action='append')
parser.add_argument('--depsout')
parser.add_argument('--no-out', action='store_true')
parser.add_argument('--keep-first-comment', action='store_true')
parser.add_argument('--name')
parser.add_argument('--header')
args = parser.parse_args()
bases = args.I or []
replacements = args.D or []

results = []
imported = set([__file__])

def Import(filename):
  filename = os.path.abspath(filename)
  if filename in imported:
    return
  imported.add(filename)
  with open(filename, 'r') as fh:
    data = fh.read().splitlines()
    for line in data:
      if filename.endswith('.fs') and line.startswith('needs '):
        sfilename = line.split(' ')[1]
        sfilename = os.path.join(os.path.dirname(filename), sfilename)
        Import(sfilename)
      elif (filename.endswith('.h') or
            filename.endswith('.html') or
            filename.endswith('.ino') or
            filename.endswith('.cc') or
            filename.endswith('.cpp') or
            filename.endswith('.c')) and line.startswith('#include "'):
        sfilename = line.split('"')[1]
        done = False
        for base in bases:
          sfilename = os.path.join(base, sfilename)
          if os.path.exists(sfilename):
            Import(sfilename)
            done = True
            break
        if not done:
          results.append(line)
      else:
        results.append(line)

def Process():
  Import(args.i)
  # Conversion version tags.
  output = []
  for line in results:
    for r in replacements:
      name, value = r.split('=', 1)
      line = line.replace('{{' + name + '}}', value)
    output.append(line)
  # Drop comments.
  comment1 = False
  comment2 = False
  counter = 0
  old_output = output
  output = []
  for line in old_output:
    if line == '<!--':
      comment1 = True
    elif comment1 and line == '-->':
      comment1 = False
    elif 'Copyright' in line:
      if counter != 0 or not args.keep_first_comment:
        comment2 = True
      counter += 1
    elif comment2 and line == '':
      comment2 = False
    elif not comment1 and not comment2:
      output.append(line)
  # Emit deps.
  if args.depsout:
    with open(args.depsout, 'w') as fh:
      fh.write(args.o + ': ' +
               ' '.join([os.path.relpath(i) for i in imported]) + '\n')
  # Emit expanded file.
  if not args.no_out:
    with open(args.o, 'w') as fh:
      if args.header == 'web':
        fh.write('const ' + args.name + ' = `\n' +
                 '\n'.join(output) + '\n`;\n')
      elif args.header == 'cpp':
        fh.write('const char ' + args.name + '[] = R"""(\n' +
                 '\n'.join(output) + '\n)""";\n')
      elif args.header == 'win':
        fixed = []
        for line in output:
          line = line.replace('\\', '\\\\')
          line = line.replace('"', '\\"')
          line = '"' + line + '\\n"'
          if line.startswith('"(') and line.endswith(')\\n"'):
            line = '// ' + line
          if line:
            fixed.append(line)
        fh.write('const char ' + args.name + '[] =\n' +
                 '\n'.join(fixed) + '\n;\n')
      else:
        fh.write('\n'.join(output) + '\n')

Process()
