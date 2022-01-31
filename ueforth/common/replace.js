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
    if (lines[i] == '<!--') {
      while (lines[i] != '-->') {
        ++i;
      }
      i += 2;
    }
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

var source = fs.readFileSync(process.stdin.fd).toString();
var replacements = [];
for (var i = 2; i < process.argv.length; ++i) {
  var item = process.argv[i];
  var m = item.match(/^([^=]+)=@(.+)$/);
  if (m) {
    var content = DropCopyright(fs.readFileSync(m[2]).toString());
    replacements.push(['{{' + m[1] + '}}', content]);
    continue;
  }
  var m = item.match(/^([^=]+)=(.+)$/);
  if (m) {
    replacements.push(['{{' + m[1] + '}}', m[2]]);
    continue;
  }
  throw 'Bad replacement ' + item;
}

for (;;) {
  var old_source = source;
  for (var i = 0; i < replacements.length; ++i) {
    source = source.replace(
        replacements[i][0],
        function() { return replacements[i][1]});
  }
  if (old_source == source) {
    break;
  }
}

process.stdout.write(source);
