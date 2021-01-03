#! /usr/bin/env nodejs

var fs = require('fs');

var source = fs.readFileSync(process.stdin.fd).toString();
var name = process.argv[2];

source = source.replace(/["]/g, '\\"');
source = '" ' + source.split('\n').join(' "\n" ') + ' "';
source = source.replace(/["]  ["]/g, '');
source = source.replace(/["] [(] ([^)]*)[)] ["]/g, '// $1');

source = 'const char ' + name + '[] =\n' + source + ';\n';

process.stdout.write(source);
