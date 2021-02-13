#! /usr/bin/env nodejs

var fs = require('fs');

var source = fs.readFileSync(process.stdin.fd).toString();
var name = process.argv[2];
var version = process.argv[3];
var revision = process.argv[4];

source = source.replace('{{VERSION}}', version);
source = source.replace('{{REVISION}}', revision);
source = source.replace(/["]/g, '\\"');
source = '"' + source.split('\n').join('\\n"\n"') + '\\n"';
source = source.replace(/["]  ["]/g, '');
source = source.replace(/["] [(] ([^)]*)[)] ["]/g, '// $1');

source = 'const char ' + name + '[] =\n' + source + ';\n';

process.stdout.write(source);
