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

cases = ReplaceAll(cases, 'DROP', 'tos = *sp--');
cases = ReplaceAll(cases, 'DUP', '*++sp = tos');
cases = ReplaceAll(cases, 'tos += *sp--', 'tos = (tos + *sp)|0; --sp');
cases = ReplaceAll(cases, /tos (.)= /, 'tos = tos $1 ');
cases = ReplaceAll(cases, /[*](.)p[+][+]/, '*$1p; ++$1p');
cases = ReplaceAll(cases, /[*](.)p[-][-]/, '*$1p; --$1p');
cases = ReplaceAll(cases, /[*][+][+](.)p/, '++$1p; *$1p');
cases = ReplaceAll(cases, '*(cell_t *) tos = ', 'i32[tos>>2] = ');
cases = ReplaceAll(cases, '*(int32_t *) tos = ', 'i32[tos>>2] = ');
cases = ReplaceAll(cases, '*(uint8_t *) tos = ', 'u8[tos] = ');
cases = ReplaceAll(cases, '*(cell_t *) tos', '(i32[tos>>2]|0)');
cases = ReplaceAll(cases, '*(int32_t *) tos', '(i32[tos>>2]|0)');
cases = ReplaceAll(cases, '*(uint8_t *) tos', '(u8[tos]|0)');
cases = ReplaceAll(cases, /[*](.)p = /, 'i32[$1p>>2] = ');
cases = ReplaceAll(cases, 'sp[-1] = ', 'i32[(sp - 4)>>2] = ');
cases = ReplaceAll(cases, /[*](.)p/, '(i32[$1p>>2]|0)');
cases = ReplaceAll(cases, 'sp[-1]', '(i32[(sp - 4)>>2]|0)');
cases = ReplaceAll(cases, /([+-]).(.)p/, '$2p = ($2p $1 4) | 0');
cases = ReplaceAll(cases, 'sp -= 2', 'sp = (sp - 8) | 0');
cases = ReplaceAll(cases, 'sizeof(cell_t)', '4');
cases = ReplaceAll(cases, '(void *) ', '');
cases = ReplaceAll(cases, '(const char *) ', '');
cases = ReplaceAll(cases, '(cell_t *) ', '');
cases = ReplaceAll(cases, '(cell_t) ', '');
cases = ReplaceAll(cases, /[(]ucell_t[)] ([^ ;]+)/, '($1>>>0)');
cases = ReplaceAll(cases, 'g_sys.state', 'i32[(i32[g_sys>>2] + (3 * 4))>>2]');
cases = ReplaceAll(cases, 'g_sys.DOLIT_XT', 'i32[(i32[g_sys>>2] + (10 * 4))>>2]|0');
cases = ReplaceAll(cases, 'g_sys.DOEXIT_XT', 'i32[(i32[g_sys>>2] + (11 * 4))>>2]|0');
cases = ReplaceAll(cases, '&g_sys', 'g_sys');
cases = ReplaceAll(cases, '&& OP_DOCOL', '0');
cases = ReplaceAll(cases, '&& OP_DOVAR', '1');
cases = ReplaceAll(cases, 'goto **(void **) w', 'break decode');
cases = ReplaceAll(cases, 'SSMOD_FUNC', '');
// Keep Together   vvv
cases = ReplaceAll(cases, /tos ([^=]?)= /, 'txx $1= ');
cases = ReplaceAll(cases, ' tos', ' (tos|0)');
cases = ReplaceAll(cases, /txx ([^=]?)= /, 'tos $1= ');
// Keep Together   ^^^
cases = ReplaceAll(cases, ' (w>>>0) / (tos>>>0)', ' ((w>>>0) / (tos>>>0))|0');
cases = ReplaceAll(cases, 'COMMA(tos)', 'COMMA(tos|0)');
cases = ReplaceAll(cases, /find\(([^\n]+)\);/, 'find($1)|0;');
cases = ReplaceAll(cases, 'tos = parse(tos, sp)', 'tos = parse(tos|0, sp|0)|0');
cases = ReplaceAll(cases, 'tos = parse(32, sp)', 'tos = parse(32, sp|0)|0');
cases = ReplaceAll(cases, 'sp = evaluate1(sp)', 'sp = evaluate1(sp|0)|0');
cases = ReplaceAll(cases, /convert\(([^\n]+), sp\)/, 'convert($1, sp|0)|0');
cases = ReplaceAll(cases, 'DOES(ip)', 'DOES(ip|0)');
cases = ReplaceAll(cases, 'PARK;', '');  // TODO
cases = ReplaceAll(cases, '; ', ';\n            ');

code = code.replace('{{boot}}', function() { return boot; });
code = code.replace('{{dict}}', function() { return dict; });
code = code.replace('{{cases}}', function() { return cases; });

console.log(code);
