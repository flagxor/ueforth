#! /usr/bin/env python

import sys

vocab = None
data = []
for line in sys.stdin.read().splitlines():
  parts = line.strip().split(' ')
  if (len(parts) != 4 or parts[0] == '-->' or
      parts[1] == 'bytes'):
    continue
  params = int(parts[0])
  size = int(parts[1])
  addr = int(parts[2])
  name = parts[3]
  if params == 0 and size == 0 and addr == 0:
    vocab = name
  else:
    data.append((addr, params, size, vocab, name))

data.sort()

base = None
layout = []
last_end = None
for addr, params, size, vocab, name in data:
  end = addr + (params - 1) * 4
  start = end - size
  if base is None:
    base = start
  if last_end is not None and last_end != start:
    layout.append((last_end - base, start - last_end, 0, 'none', '--GAP--'))
  layout.append((start - base, end - start, params, vocab, name))
  last_end = end

string_size = 0
params_size = 0
struct_size = 0
builtin_count = 0
builtin_size = 0
vocab_param_size = {}
vocab_size = {}
vocab_count = {}
for start, size, params, vocab, name in layout:
  string_size += size - params * 4 - 3 * 4
  params_size += (params * 4)
  struct_size += 3 * 4
  if vocab not in vocab_size:
    vocab_size[vocab] = 0
    vocab_param_size[vocab] = 0
    vocab_count[vocab] = 0
  vocab_size[vocab] += size
  vocab_param_size[vocab] += params * 4
  vocab_count[vocab] += 1
  if params == 0:
    builtin_count += 1
    builtin_size += size

vocab_table = []
for vocab in vocab_size:
  vocab_table.append(
      (vocab_size[vocab], vocab_param_size[vocab], vocab_count[vocab], vocab))
vocab_table.sort()

def Columns(data, widths, underline=False):
  result = []
  for i in range(len(data)):
    result.append((str(data[i]) + ' ' * widths[i])[:widths[i]])
  if underline:
    return (''.join(result) + '\n' +
            Columns(['-' * len(i) for i in data], widths))
  return ''.join(result)

items = len(layout)

columns = [7, 7, 7, 15, 30]
print(Columns(['START', 'SIZE', 'PARAMS', 'VOCABULARY', 'WORD'], columns, underline=True))
for item in layout:
  print(Columns(item, columns))
print('')
columns = [7, 12, 7, 15]
print(Columns(['SIZE', 'PARAM SIZE', 'COUNT', 'VOCABULARY'], columns, underline=True))
for item in vocab_table:
  print(Columns(item, columns))
print('')
columns = [7, 7, 15]
print(Columns(['SIZE', 'COUNT', 'CATEGORY'], columns, underline=True))
print(Columns([string_size, items, 'names'], columns))
print(Columns([params_size, items, 'params'], columns))
print(Columns([struct_size, items, 'structure'], columns))
print(Columns([builtin_size, builtin_count, 'builtins'], columns))
