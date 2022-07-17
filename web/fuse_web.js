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
var sys = fs.readFileSync(process.argv[6]).toString();

function ReplaceAll(haystack, needle, replacement) {
  for (;;) {
    var old = haystack;
    haystack = haystack.replace(needle, replacement);
    if (old === haystack) {
      return haystack;
    }
  }
}

boot = boot.replace(/[\\]/g, '\\\\');

cases = ReplaceAll(cases, 'DROP;', 'tos = *sp--;');
cases = ReplaceAll(cases, 'DUP;', '*++sp = tos;');

cases = ReplaceAll(cases, 'tos += sizeof(float)', 'tos = (tos + 4)|0');
cases = ReplaceAll(cases, 'tos *= sizeof(float)', 'tos = (tos * 4)|0');
cases = ReplaceAll(cases, 'tos += sizeof(cell_t)', 'tos = (tos + 4)|0');
cases = ReplaceAll(cases, 'tos *= sizeof(cell_t)', 'tos = (tos * 4)|0');

cases = ReplaceAll(cases, 'tos += *sp--', 'tos = (tos + *sp)|0; --sp');
cases = ReplaceAll(cases, 'tos = (*sp--) - tos', 'tos = (*sp - tos)|0; --sp');
cases = ReplaceAll(cases, 'tos *= *sp--', 'tos = imul(tos, *sp); --sp');
cases = ReplaceAll(cases, '*((cell_t *) tos) += *sp--',
                          'i32[tos>>2] = ((i32[tos>>2]|0) + (i32[sp>>2]|0))|0; --sp');
cases = ReplaceAll(cases, ' -tos', ' (-tos)|0');
cases = ReplaceAll(cases, '++tos', 'tos = (tos + 1)|0');
cases = ReplaceAll(cases, '--tos', 'tos = (tos - 1)|0');

cases = ReplaceAll(cases, /tos (.)= /, 'tos = tos $1 ');
cases = ReplaceAll(cases, '*((cell_t *) *ip) = ', 'i32[i32[ip>>2]>>2] = ');
cases = ReplaceAll(cases, /[*](.)p[+][+]/, '*$1p, ++$1p');
cases = ReplaceAll(cases, /[*](.)p[-][-]/, '*$1p, --$1p');
cases = ReplaceAll(cases, /[*][+][+](.)p/, '++$1p, *$1p');
cases = ReplaceAll(cases, '*(cell_t *) tos = ', 'i32[tos>>2] = ');
cases = ReplaceAll(cases, '((cell_t *) tos)[1] = ', 'i32[(tos + 4)>>2] = ');
cases = ReplaceAll(cases, '*(int32_t *) tos = ', 'i32[tos>>2] = ');
cases = ReplaceAll(cases, '*(int16_t *) tos = ', 'i16[tos>>1] = ');
cases = ReplaceAll(cases, '*(uint8_t *) tos = ', 'u8[tos] = ');
cases = ReplaceAll(cases, '*(float *) tos = ', 'f32[tos>>2] = ');
cases = ReplaceAll(cases, '*(cell_t *) tos', '(i32[tos>>2]|0)');
cases = ReplaceAll(cases, '((cell_t *) tos)[1]', '(i32[(tos + 4)>>2]|0)');
cases = ReplaceAll(cases, '*(int32_t *) tos', '(i32[tos>>2]|0)');
cases = ReplaceAll(cases, '*(uint32_t *) tos', '(i32[tos>>2]>>>0)');
cases = ReplaceAll(cases, '*(int16_t *) tos', '(i16[tos>>1]|0)');
cases = ReplaceAll(cases, '*(uint16_t *) tos', '(i16[tos>>1]>>>0)');
cases = ReplaceAll(cases, '*(uint8_t *) tos', '(u8[tos]|0)');
cases = ReplaceAll(cases, '*(float *) tos', 'f32[tos>>2]');
cases = ReplaceAll(cases, '*(float *) ip', 'f32[ip>>2]');

cases = ReplaceAll(cases, '(float) tos', 'fround(tos|0)');
cases = ReplaceAll(cases, 'tos = (cell_t) *fp', 'tos = ~~fround(f32[fp>>2])');

cases = ReplaceAll(cases, '*fp = ', 'f32[fp>>2] = ');
cases = ReplaceAll(cases, 'fp[-2] = ', 'f32[(fp - 8)>>2] = ');
cases = ReplaceAll(cases, 'fp[-1] = ', 'f32[(fp - 4)>>2] = ');
cases = ReplaceAll(cases, 'fp[1] = ', 'f32[(fp + 4)>>2] = ');
cases = ReplaceAll(cases, '*fp', 'fround(f32[fp>>2])');
cases = ReplaceAll(cases, 'fp[0]', 'fround(f32[fp>>2])');
cases = ReplaceAll(cases, 'fp[-1]', 'fround(f32[(fp - 4)>>2])');
cases = ReplaceAll(cases, 'fp[-2]', 'fround(f32[(fp - 8)>>2])');

cases = ReplaceAll(cases, /[*](.)p = /, 'i32[$1p>>2] = ');
cases = ReplaceAll(cases, 'sp[-1] = ', 'i32[(sp - 4)>>2] = ');
cases = ReplaceAll(cases, /[*](.)p/, '(i32[$1p>>2]|0)');
cases = ReplaceAll(cases, 'sp[-1]', '(i32[(sp - 4)>>2]|0)');

cases = ReplaceAll(cases, /([+-]).(.)p/, '$2p = ($2p $1 4) | 0');
cases = ReplaceAll(cases, 'sp -= (2-1)', 'sp = (sp - 4) | 0');
cases = ReplaceAll(cases, 'sp -= 2', 'sp = (sp - 8) | 0');
cases = ReplaceAll(cases, 'fp -= 2', 'fp = (fp - 8) | 0');
cases = ReplaceAll(cases, 'sizeof(cell_t)', '4');
cases = ReplaceAll(cases, 'sizeof(float)', '4');
cases = ReplaceAll(cases, 'sizeof(long)', '4');
cases = ReplaceAll(cases, '(void *) ', '');
cases = ReplaceAll(cases, '(const char *) ', '');
cases = ReplaceAll(cases, '(cell_t *) ', '');
cases = ReplaceAll(cases, '(cell_t) ', '');
cases = ReplaceAll(cases, '(float *) ', '');
cases = ReplaceAll(cases, '(float) ', '');
cases = ReplaceAll(cases, '0.0f', 'fround(0.0)');
cases = ReplaceAll(cases, '1.0f', 'fround(1.0)');
cases = ReplaceAll(cases, "' '", '32');
cases = ReplaceAll(cases, "'\\n'", '10');
cases = ReplaceAll(cases, /[(]ucell_t[)] ([^ ;)]+)/, '($1>>>0)');

cases = ReplaceAll(cases, '*(w + 4)', '(i32[(w + 4)>>2]|0)');
cases = ReplaceAll(cases, 'w + 4 * 2', '(w+8)|0');
cases = ReplaceAll(cases, 'w + 4', '(w+4)|0');

cases = ReplaceAll(cases, '(g_sys->context + 1)', '(i32[g_sys_context>>2]|0 + 4)|0');
cases = ReplaceAll(cases, '&g_sys->builtins->code', '((i32[g_sys_builtins>>2] + 8)|0)');
cases = ReplaceAll(cases, /[&]g_sys[-][>]([A-Za-z_]+)/, 'g_sys_$1');
cases = ReplaceAll(cases, /g_sys[-][>]([A-Za-z_]+) [=] /, 'i32[g_sys_$1>>2] = ');
cases = ReplaceAll(cases, /g_sys[-][>]([A-Za-z_]+)/, '(i32[g_sys_$1>>2]|0)');

cases = ReplaceAll(cases, /ADDROF[(]([^)]+)[)]/, 'OP_$1');

cases = ReplaceAll(cases, 'return rp', 'i32[g_sys_rp>>2] = rp | 0; return');

cases = ReplaceAll(cases, 'SSMOD_FUNC',
                   'sp = (sp + 4)|0; i32[sp>>2] = tos|0; ' +
                   'SSMOD(sp|0); tos = i32[sp>>2]|0; sp = (sp - 8)|0');

// Keep Together   vvv
cases = ReplaceAll(cases, /tos ([^=]?)= /, 'txx $1= ');
cases = ReplaceAll(cases, ' tos', ' (tos|0)');
cases = ReplaceAll(cases, /txx ([^=]?)= /, 'tos $1= ');
// Keep Together   ^^^
cases = ReplaceAll(cases, 'fp)', 'fp|0)');
cases = ReplaceAll(cases, ' (w>>>0) / (tos>>>0)', ' ((w>>>0) / (tos>>>0))|0');
cases = ReplaceAll(cases, 'COMMA(tos)', 'COMMA(tos|0)');
cases = ReplaceAll(cases, /find\(([^\n]+)\);/, 'find($1)|0;');
cases = ReplaceAll(cases, 'tos = parse(tos, sp)', 'tos = parse(tos|0, sp|0)|0');
cases = ReplaceAll(cases, 'tos = parse(32, sp)', 'tos = parse(32, sp|0)|0');
cases = ReplaceAll(cases, 'rp = evaluate1(rp)', 'rp = evaluate1(rp|0)|0');
cases = ReplaceAll(cases, /convert\(([^\n]+), sp\)/, 'convert($1, sp|0)|0');
cases = ReplaceAll(cases, 'DOES(ip)', 'DOES(ip|0)');
cases = ReplaceAll(cases, 'PARK;', '');  // TODO
cases = ReplaceAll(cases, '; ', ';\n            ');

cases = ReplaceAll(cases, 'tos = ((tos) + (*(tos) == OP_DOCREATE || *(tos) == OP_DODOES ? 2 : 1))',
                          'tos = TOBODY(tos|0)|0');

code = code.replace('{{boot}}', function() { return boot; });
code = code.replace('{{dict}}', function() { return dict; });
code = code.replace('{{cases}}', function() { return cases; });
code = code.replace(/[{][{]sys[}][}]/g, function() { return sys; });

console.log(code);
