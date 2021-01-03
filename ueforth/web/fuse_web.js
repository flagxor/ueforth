#! /usr/bin/env nodejs

var fs = require('fs');
var code = fs.readFileSync(process.argv[2]).toString();
var boot = fs.readFileSync(process.argv[3]).toString();
var dict = fs.readFileSync(process.argv[4]).toString();
var cases = fs.readFileSync(process.argv[5]).toString();

function ReplaceAll(haystack, needle, replacement) {
  for (;;) {
    var old = haystack;
    haystack = haystack.replace(needle, replacement);
    if (old === haystack) {
      return haystack;
    }
  }
}

code = code.replace('{{boot}}', function() { return boot; });
code = code.replace('{{dict}}', function() { return dict; });
code = code.replace('{{cases}}', function() { return cases; });

console.log(code);
