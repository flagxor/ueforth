#! /usr/bin/env nodejs

var fs = require('fs');

var code = fs.readFileSync(process.argv[2]).toString();
var opcodes = fs.readFileSync(process.argv[3]).toString();
var core = fs.readFileSync(process.argv[4]).toString();
var boot = fs.readFileSync(process.argv[5]).toString();

code = code.replace('{{opcodes}}', opcodes);
code = code.replace('{{boot}}', boot);
code = code.replace('{{core}}', core);

process.stdout.write(code);
