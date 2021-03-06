#! /usr/bin/env nodejs

var fs = require('fs');

var source = fs.readFileSync(process.stdin.fd).toString();
var replacements = [];
for (var i = 2; i < process.argv.length; ++i) {
  var item = process.argv[i];
  var m = item.match(/^([^=]+)=@(.+)$/);
  if (m) {
    replacements.push([m[1], fs.readFileSync(m[2]).toString()]);
    continue;
  }
  var m = item.match(/^([^=]+)=(.+)$/);
  if (m) {
    replacements.push([m[1], m[2]]);
    continue;
  }
  throw 'Bad replacement ' + item;
}
  
var version = process.argv[3];
var revision = process.argv[4];

for (;;) {
  var old_source = source;
  for (var i = 0; i < replacements.length; ++i) {
    source = source.replace('{{' + replacements[i][0] + '}}',
                            replacements[i][1]);
  }
  if (old_source == source) {
    break;
  }
}

process.stdout.write(source);
