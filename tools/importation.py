#! /usr/bin/env python3

import argparse
import os
import sys

parser = argparse.ArgumentParser(
  prog='importation',
  description='Imports header / fs files')
parser.add_argument('input')
parser.add_argument('output')
parser.add_argument('-I', action='append')
parser.add_argument('--set-version')
parser.add_argument('--set-revision')
parser.add_argument('--depsout')
args = parser.parse_args()
bases = args.I or []

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
            filename.endswith('.cc') or
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
  Import(args.input)
  for line in results:
    if args.set_version:
      line = line.replace('{{VERSION}}', args.set_version)
    if args.set_revision:
      line = line.replace('{{REVISION}}', args.set_revision)
  if args.depsout:
    with open(args.depsout, 'w') as fh:
      fh.write(args.output + ': ' +
               ' '.join([os.path.relpath(i) for i in imported]) + '\n')
  with open(args.output, 'w') as fh:
    fh.write('\n'.join(results) + '\n')

Process()
