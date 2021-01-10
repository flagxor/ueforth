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

cases = ReplaceAll(cases, 'DROP', 'tos = i32[sp>>2] | 0; sp = (sp - 4) | 0');
cases = ReplaceAll(cases, 'DUP', 'sp = (sp + 4) | 0; i32[sp>>2] = tos');
cases = ReplaceAll(cases, '*(cell_t *) tos', 'i32[tos>>2]');
cases = ReplaceAll(cases, '*(int32_t *) tos', 'i32[tos>>2]');
cases = ReplaceAll(cases, '*(uint8_t *) tos', 'u8[tos]');
cases = ReplaceAll(cases, '*sp', 'i32[sp>>2]');
cases = ReplaceAll(cases, '*rp', 'i32[rp>>2]');
cases = ReplaceAll(cases, '*ip', 'i32[ip>>2]');
cases = ReplaceAll(cases, 'sp[-1]', 'i32[(sp - 4)>>2]');
cases = ReplaceAll(cases, '++ip', 'ip = (ip + 4) | 0');
cases = ReplaceAll(cases, '++sp', 'sp = (sp + 4) | 0');
cases = ReplaceAll(cases, '++rp', 'rp = (rp + 4) | 0');
cases = ReplaceAll(cases, '--sp', 'sp = (sp - 4) | 0');
cases = ReplaceAll(cases, '--rp', 'rp = (rp - 4) | 0');
cases = ReplaceAll(cases, 'sp -= 2', 'sp = (sp - 8) | 0');
cases = ReplaceAll(cases, 'sizeof(cell_t)', '4');
cases = ReplaceAll(cases, '(void *) ', '');
cases = ReplaceAll(cases, '(const char *) ', '');
cases = ReplaceAll(cases, '(cell_t *) ', '');
cases = ReplaceAll(cases, '(cell_t) ', '');
cases = ReplaceAll(cases, 'g_sys.state', 'i32[(i32[g_sys>>2] + (3 * 4))>>2]');
cases = ReplaceAll(cases, 'g_sys.DOLIT_XT', 'i32[(i32[g_sys>>2] + (10 * 4))>>2]');
cases = ReplaceAll(cases, 'g_sys.DOEXIT_XT', 'i32[(i32[g_sys>>2] + (11 * 4))>>2]');
cases = ReplaceAll(cases, '&g_sys', 'g_sys');
cases = ReplaceAll(cases, '&& OP_DOCOLON', '0');
cases = ReplaceAll(cases, '&& OP_DOCREATE', '1');
cases = ReplaceAll(cases, 'goto **(void **) w', 'break decode');
cases = ReplaceAll(cases, 'SSMOD', '');

code = code.replace('{{boot}}', function() { return boot; });
code = code.replace('{{dict}}', function() { return dict; });
code = code.replace('{{cases}}', function() { return cases; });

console.log(code);
