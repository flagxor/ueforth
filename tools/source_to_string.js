#! /usr/bin/env nodejs
// Copyright 2021 Bradley D. Nelson
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

var fs = require('fs');

function DropCopyright(source) {
  var lines = source.split('\n');
  var cleaned = [];
  for (var i = 0; i < lines.length; ++i) {
    if (lines[i].search('Copyright') >= 0) {
      while (lines[i] != '') {
        ++i;
      }
    } else {
      cleaned.push(lines[i]);
    }
  }
  return cleaned.join('\n');
}

var is_windows = false;
var is_web = false;

var args = process.argv.slice(2);
if (args.length > 0 && args[0] == '-win') {
  is_windows = true;
  args.shift();
}
if (args.length > 0 && args[0] == '-web') {
  is_web = true;
  args.shift();
}
var name = args.shift();
var version = args.shift();
var revision = args.shift();
var source = '';
while (args.length > 0) {
  source += DropCopyright(fs.readFileSync(args.shift()).toString());
}

source = source.replace('{{VERSION}}', version);
source = source.replace('{{REVISION}}', revision);

if (is_windows) {
  source = source.replace(/\\/g, '\\\\');
  source = source.replace(/["]/g, '\\"');
  source = '"' + source.split('\n').join('\\n"\n"') + '\\n"';
  source = source.replace(/["]  ["]/g, '');
  source = source.replace(/["] [(] ([^)]*)[)] ["]/g, '// $1');
  source = 'const char ' + name + '[] =\n' + source + ';\n';
} else if (is_web) {
  source = 'const ' + name + ' = `\n' + source + '`;\n';
} else {
  source = 'const char ' + name + '[] = R"""(\n' + source + ')""";\n';
}

process.stdout.write(source);
