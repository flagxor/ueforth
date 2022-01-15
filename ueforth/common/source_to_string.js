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

var name = process.argv[2];
var version = process.argv[3];
var revision = process.argv[4];
var source = '';
for (var i = 5; i < process.argv.length; i++) {
  source += DropCopyright(fs.readFileSync(process.argv[i]).toString());
}

source = source.replace('{{VERSION}}', version);
source = source.replace('{{REVISION}}', revision);

source = 'const char ' + name + '[] = R"""(\n' + source + ')""";\n';

process.stdout.write(source);
