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

var version = process.argv[2];
var revision = process.argv[3];
var code = fs.readFileSync(process.argv[4]).toString();
var opcodes = DropCopyright(fs.readFileSync(process.argv[5]).toString());
var calling = DropCopyright(fs.readFileSync(process.argv[6]).toString());
var floats = DropCopyright(fs.readFileSync(process.argv[7]).toString());
var core = DropCopyright(fs.readFileSync(process.argv[8]).toString());
var interp = DropCopyright(fs.readFileSync(process.argv[9]).toString());
var boot = DropCopyright(fs.readFileSync(process.argv[10]).toString());

code = code.replace('{{VERSION}}', function() { return version; });
code = code.replace('{{REVISION}}', function() { return revision; });
code = code.replace('{{opcodes}}', function() { return opcodes; });
code = code.replace('{{floats}}', function() { return floats; });
code = code.replace('{{calling}}', function() { return calling; });
code = code.replace('{{boot}}', function() { return boot; });
code = code.replace('{{core}}', function() { return core; });
code = code.replace('{{interp}}', function() { return interp; });

process.stdout.write(code);
