#! /usr/bin/env nodejs

var fs = require('fs');
var code = fs.readFileSync(process.argv[2]).toString();
var boot = fs.readFileSync(process.argv[3]).toString();

function ReplaceAll(haystack, needle, replacement) {
  for (;;) {
    var old = haystack;
    haystack = haystack.replace(needle, replacement);
    if (old === haystack) {
      return haystack;
    }
  }
}

boot = boot.replace(/["]/g, '\\"');
boot = '" ' + boot.split('\n').join(' "\n" ') + ' "';
boot = boot.replace(/["]  ["]/g, '');
boot = boot.replace(/["] [(] ([^)]*)[)] ["]/g, '// $1');
code = code.replace('{{boot}}', boot);

console.log(code);
