#! /usr/bin/env nodejs

var fs = require('fs');

var code = fs.readFileSync(process.argv[2]).toString();
var opcodes = fs.readFileSync(process.argv[3]).toString();
var calling = fs.readFileSync(process.argv[4]).toString();
var core = fs.readFileSync(process.argv[5]).toString();
var interp = fs.readFileSync(process.argv[6]).toString();
var boot = fs.readFileSync(process.argv[7]).toString();

code = code.replace('{{opcodes}}', function() { return opcodes; });
code = code.replace('{{calling}}', function() { return calling; });
code = code.replace('{{boot}}', function() { return boot; });
code = code.replace('{{core}}', function() { return core; });
code = code.replace('{{interp}}', function() { return interp; });

process.stdout.write(code);
