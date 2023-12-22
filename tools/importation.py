#! /usr/bin/env python3

import argparse
import os
import sys

parser = argparse.ArgumentParser(
  prog='importation',
  description='Imports header / fs files')
parser.add_argument('filename')
parser.add_argument('-I', action='append')
parser.add_argument('--set-version')
parser.add_argument('--set-revision')
args = parser.parse_args()
bases = args.I or []

results = []
imported = set()

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
  Import(args.filename)
  for line in results:
    if args.set_version:
      line = line.replace('{{VERSION}}', args.set_version)
    if args.set_revision:
      line = line.replace('{{REVISION}}', args.set_revision)
  print('\n'.join(results))

Process()
