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

'use strict';

(function() {

const HEAP_SIZE = (4 * 1024 * 1024);
const STACK_CELLS = 4096;
const VOCABULARY_DEPTH = 16;

const IMMEDIATE = 1;
const SMUDGE = 2;
const BUILTIN_FORK = 4;
const BUILTIN_MARK = 8;

const DEBUGGING = false;

{{boot}}

var heap = new ArrayBuffer(HEAP_SIZE);
var f32 = new Float32Array(heap);
var i32 = new Int32Array(heap);
var u16 = new Uint16Array(heap);
var u8 = new Uint8Array(heap);
var builtins = [];
var opcodes = {};
var objects = [SetEval];
var context = {};  // For later use by platform.

{{sys}}

function SetEval(sp) {
  var index = i32[sp>>2]; sp -= 4;
  var n = i32[sp>>2]; sp -= 4;
  var a = i32[sp>>2]; sp -= 4;
  objects[index] = eval(GetString(a, n));
  return sp;
}

function Call(sp) {
  var op = i32[sp>>2]; sp -= 4;
  return objects[op](sp);
}

function Load(addr, content) {
  var data = unescape(encodeURIComponent(content));
  for (var i = 0; i < data.length; ++i) {
    u8[addr++] = data.charCodeAt(i);
  }
  return data.length;
}

function GetString(a, n) {
  var data = '';
  for (var i = 0; i < n; ++i) {
    data += String.fromCharCode(u8[a + i]);
  }
  try {
    return decodeURIComponent(escape(data));
  } catch (e) {
    return data;
  }
}

function CELL_ALIGNED(n) { return (n + 3) & ~3; }
function UPPER(ch) {
  return ch >= 'a'.charCodeAt(0) && ch <= 'z'.charCodeAt(0) ? (ch & 0x5F) : ch;
}

function TOFLAGS(xt) { return xt - 1 * 4; }
function TONAMELEN(xt) { return TOFLAGS(xt) + 1; }
function TOPARAMS(xt) { return TOFLAGS(xt) + 2; }
function TOSIZE(xt) { return CELL_ALIGNED(u8[TONAMELEN(xt)>>2]) + 4 * u16[TOPARAMS(xt)>>1]; }
function TOLINK(xt) { return xt - 2 * 4; }
function TONAME(xt) {
  return (u8[TOFLAGS(xt)] & BUILTIN_MARK)
    ? i32[TOLINK(xt)] : TOLINK(xt) - CELL_ALIGNED(u8[TONAMELEN(xt)]);
}
function TOBODY(xt) {
  return xt + (i32[xt>>2] === OP_DOCREATE || i32[xt>>2] === OP_DODOES ? 2 : 1) * 4;
}

function DOES(ip) {
  i32[i32[i32[g_sys_current>>2]>>2]>>2] = OP_DODOES;
  i32[(i32[i32[g_sys_current>>2]>>2] + 4)>>2] = ip;
}

function BUILTIN_ITEM(i) {
  return i32[g_sys_builtins>>2] + 4 * 3 * i;
}
function BUILTIN_NAME(i) {
  return i32[(BUILTIN_ITEM(i) + 0 * 4)>>2];
}
function BUILTIN_FLAGS(i) {
  return u8[BUILTIN_ITEM(i) + 1 * 4 + 0];
}
function BUILTIN_NAMELEN(i) {
  return u8[BUILTIN_ITEM(i) + 1 * 4 + 1];
}
function BUILTIN_VOCAB(i) {
  return u16[(BUILTIN_ITEM(i) + 1 * 4 + 2)>>1];
}
function BUILTIN_CODE(i) {
  return BUILTIN_ITEM(i) + 2 * 4;
}

function Find(name) {
  if (name.length === 0) {
    return 0;
  }
  name = name.toUpperCase();
  var raw = unescape(encodeURIComponent(name));
  for (var voc = i32[g_sys_context>>2]; i32[voc>>2]; voc += 4) {
    var xt = i32[i32[voc>>2]>>2];
    while (xt) {
      if (u8[TOFLAGS(xt)] & BUILTIN_FORK) {
        var vocab = i32[(TOLINK(xt) + 4 * 3)>>2];
        for (var i = 0; BUILTIN_NAME(i); ++i) {
          if (BUILTIN_VOCAB(i) === vocab &&
              raw.length === BUILTIN_NAMELEN(i) &&
              name === GetString(BUILTIN_NAME(i), BUILTIN_NAMELEN(i)).toUpperCase()) {
            if (DEBUGGING) { console.log('FOUND: ' + name); }
            return BUILTIN_CODE(i);
          }
        }
      }
      if (!(u8[TOFLAGS(xt)] & SMUDGE) &&
          raw.length === u8[TONAMELEN(xt)] &&
          name.toUpperCase() === GetString(TONAME(xt), u8[TONAMELEN(xt)]).toUpperCase()) {
          if (DEBUGGING) { console.log('FOUND REGULAR: ' + name); }
        return xt;
      }
      xt = i32[TOLINK(xt)>>2];
    }
  }
  if (DEBUGGING) { console.log('NOT FOUND: ' + name); }
  return 0;
}

function trace(ip, sp, tos) {
  var line = '[';
  for (var i = 0; i < 3; i++) {
    line += i32[(sp + (i - 2) * 4)>>2] + ' ';
  }
  line += tos + '] ';
  while (line.length < 25) {
    line += ' ';
  }
  line += ip + ': ';
  for (var i = 0; i < 10; ++i) {
    var val = i32[(ip + i * 4)>>2];
    if (i32[val>>2] && opcodes[i32[val>>2]] !== undefined) {
      var op = opcodes[i32[val>>2]];
      if (op === 'DOCOL') {
        line += op + '(' + val + ') ';
      } else {
        line += op + ' ';
      }
    } else {
      line += val + ' ';
    }
  }
  console.log(line);
}

function COMMA(value) {
  i32[i32[g_sys_heap>>2]>>2] = value;
  i32[g_sys_heap>>2] += 4;
}

function CCOMMA(value) {
  u8[i32[g_sys_heap>>2]] = value;
  i32[g_sys_heap>>2]++;
}

function SSMOD(sp) {
  var a = i32[(sp - 8)>>2];
  var b = i32[(sp - 4)>>2];
  var c = i32[sp>>2];
  a *= b;
  var x = Math.floor(a / c);
  var m = a - x * c;
  i32[(sp - 8)>>2] = m;
  i32[sp>>2] = x;
}

function Finish() {
  if (i32[g_sys_latestxt>>2] && !u16[TOPARAMS(i32[g_sys_latestxt>>2])>>1]) {
    var sz = i32[g_sys_heap>>2] - (i32[g_sys_latestxt>>2] + 4);
    sz /= 4;
    if (sz < 0 || sz > 0xffff) { sz = 0xffff; }
    u16[TOPARAMS(i32[g_sys_latestxt>>2])>>1] = sz;
  }
}

function UNSMUDGE() {
  u8[TOFLAGS(i32[i32[g_sys_current>>2]>>2])] &= ~SMUDGE;
  Finish();
}

function DOIMMEDIATE() {
  u8[TOFLAGS(i32[i32[g_sys_current>>2]>>2])] |= IMMEDIATE;
}

function Create(name, flags, op) {
  if (DEBUGGING) { console.log('CREATE: ' + name); }
  Finish();
  i32[g_sys_heap>>2] = CELL_ALIGNED(i32[g_sys_heap>>2]);
  var name_len = Load(i32[g_sys_heap>>2], name);  // name
  i32[g_sys_heap>>2] += name_len;
  i32[g_sys_heap>>2] = CELL_ALIGNED(i32[g_sys_heap>>2]);
  COMMA(i32[i32[g_sys_current>>2]>>2]);  // link
  COMMA((name_len << 8) | flags);  // flags & length
  i32[i32[g_sys_current>>2]>>2] = i32[g_sys_heap>>2];
  i32[g_sys_latestxt>>2] = i32[g_sys_heap>>2];
  COMMA(op);
}

function Builtin(name, flags, vocab, opcode) {
  opcodes[opcode] = name;
  builtins.push([name, flags | BUILTIN_MARK, vocab, opcode]);
}

function LoadScripts() {
  if (globalObj.write) {
    return;
  }
  var text = '';
  var tags = document.getElementsByTagName('script');
  for (var i = 0; i < tags.length; ++i) {
    if (tags[i].type == 'text/forth') {
      text += tags[i].text + '\n';
    }
  }
  var encoder = new TextEncoder();
  context.scripts = encoder.encode(text);
}

function SetupBuiltins() {
  for (var i = 0; i < builtins.length; ++i) {
    var name = builtins[i][0];
    builtins[i][0] = i32[g_sys_heap>>2];
    var name_len = Load(i32[g_sys_heap>>2], name);  // name
    i32[g_sys_heap>>2] += name_len;
    i32[g_sys_heap>>2] = CELL_ALIGNED(i32[g_sys_heap>>2]);
    builtins[i][1] |= (name_len << 8);
  }
  i32[g_sys_builtins>>2] = i32[g_sys_heap>>2];
  for (var i = 0; i < builtins.length; ++i) {
    COMMA(builtins[i][0]);
    COMMA(builtins[i][1] | (builtins[i][2] << 16));
    COMMA(builtins[i][3]);
  }
  COMMA(0);
  COMMA(0);
  COMMA(0);
}

function Match(sep, ch) {
  return sep == ch || (sep == 32 && (ch == 9 || ch == 10 || ch == 13));
}

function Parse(sep, ret) {
  if (sep == 32) {
    while (i32[g_sys_tin>>2] < i32[g_sys_ntib>>2] &&
           Match(sep, u8[i32[g_sys_tib>>2] + i32[g_sys_tin>>2]])) { ++i32[g_sys_tin>>2]; }
  }
  var start = i32[g_sys_tin>>2];
  while (i32[g_sys_tin>>2] < i32[g_sys_ntib>>2] &&
         !Match(sep, u8[i32[g_sys_tib>>2] + i32[g_sys_tin>>2]])) { ++i32[g_sys_tin>>2]; }
  var len = i32[g_sys_tin>>2] - start;
  if (i32[g_sys_tin>>2] < i32[g_sys_ntib>>2]) { ++i32[g_sys_tin>>2]; }
  i32[ret>>2] = i32[g_sys_tib>>2] + start;
  if (DEBUGGING) { console.log('PARSE: [' + GetString(i32[ret>>2], len) + ']'); }
  return len;
}

function Convert(pos, n, base, ret) {
  i32[ret>>2] = 0;
  var negate = 0;
  if (!n) { return 0; }
  if (u8[pos] == '-'.charCodeAt(0)) { negate = -1; ++pos; --n; }
  if (u8[pos] == '$'.charCodeAt(0)) { base = 16; ++pos; --n; }
  for (; n; --n) {
    var d = UPPER(u8[pos]) - 48;
    if (d > 9) {
      d -= 7;
      if (d < 10) { return 0; }
    }
    if (d >= base) { return 0; }
    i32[ret>>2] = i32[ret>>2] * base + d;
    ++pos;
  }
  if (negate) { i32[ret>>2] = -i32[ret>>2]; }
  return -1;
}

function FConvert(pos, n, ret) {
  f32[ret>>2] = 0;
  var negate = 0;
  var has_dot = 0;
  var exp = 0;
  var shift = 1;
  if (!n) { return 0; }
  if (u8[pos] == '-'.charCodeAt(0)) { negate = -1; ++pos; --n; }
  for (; n; --n) {
    if (u8[pos] >= 48 && u8[pos] <= 48 + 9) {
      if (has_dot) {
        shift = shift * 0.1;
        f32[ret>>2] = f32[ret>>2] + (u8[pos] - 48) * shift;
      } else {
        f32[ret>>2] = f32[ret>>2] * 10 + (u8[pos] - 48);
      }
    } else if (u8[pos] == 'e'.charCodeAt(0) || u8[pos] == 'E'.charCodeAt(0)) {
      break;
    } else if (u8[pos] == '.'.charCodeAt(0)) {
      if (has_dot) { return 0; }
      has_dot = -1;
    } else {
      return 0;
    }
    ++pos;
  }
  if (!n) { return 0; }  // must have E
  ++pos; --n;
  if (n) {
    var tmp = f32[ret>>2];
    if (!Convert(pos, n, 10, ret)) { return 0; }
    exp = i32[ret>>2];
    f32[ret>>2] = tmp;
  }
  if (exp < -128 || exp > 128) { return 0; }
  for (; exp < 0; ++exp) { f32[ret>>2] *= 0.1; }
  for (; exp > 0; --exp) { f32[ret>>2] *= 10.0; }
  if (negate) { f32[ret>>2] = -f32[ret>>2]; }
  return -1;
}

function Evaluate1(rp) {
  var call = 0;
  var tos, sp, ip, fp;

  // UNPARK
  sp = i32[rp>>2]; rp -= 4;
  tos = i32[sp>>2]; sp -= 4;
  fp = i32[rp>>2]; rp -= 4;
  ip = i32[rp>>2]; rp -= 4;

  var name = sp + 8;
  var len = Parse(32, name);
  if (len == 0) {  // ignore empty
    sp += 4; i32[sp>>2] = tos; tos = 0;
    // PARK
    rp += 4; i32[rp>>2] = ip;
    rp += 4; i32[rp>>2] = fp;
    sp += 4; i32[sp>>2] = tos;
    rp += 4; i32[rp>>2] = sp;
    return rp;
  }
  name = i32[name>>2];
  var xt = Find(GetString(name, len));
  if (xt) {
    if (i32[g_sys_state>>2] && !(u8[TOFLAGS(xt)] & IMMEDIATE)) {
      COMMA(xt);
    } else {
      call = xt;
    }
  } else {
    if (DEBUGGING) { console.log('CONVERTING: ' + GetString(name, len)); }
    var n = sp + 16;
    if (Convert(name, len, i32[g_sys_base>>2], n)) {
      if (i32[g_sys_state>>2]) {
        COMMA(i32[g_sys_DOLIT_XT>>2]);
        COMMA(i32[n>>2]);
      } else {
        sp += 4; i32[sp>>2] = tos; tos = i32[n>>2];
      }
    } else {
      if (FConvert(name, len, n)) {
        if (i32[g_sys_state>>2]) {
          COMMA(i32[g_sys_DOFLIT_XT>>2]);
          f32[i32[g_sys_heap>>2]>>2] = f32[n>>2]; i32[g_sys_heap>>2] += 4;
        } else {
          fp += 4; f32[fp>>2] = f32[n>>2];
        }
      } else {
        if (DEBUGGING) { console.log('CANT FIND: ' + GetString(name, len)); }
        sp += 4; i32[sp>>2] = tos; tos = name;
        sp += 4; i32[sp>>2] = tos; tos = len;
        sp += 4; i32[sp>>2] = tos; tos = -1;
        call = i32[g_sys_notfound>>2];
      }
    }
  }
  sp += 4; i32[sp>>2] = tos; tos = call;
  // PARK
  rp += 4; i32[rp>>2] = ip;
  rp += 4; i32[rp>>2] = fp;
  sp += 4; i32[sp>>2] = tos;
  rp += 4; i32[rp>>2] = sp;

  return rp;
}

function InitDictionary() {
{{dict}}
  SetupBuiltins();
}

function Init() {
  i32[g_sys_heap_start>>2] = 0;
  i32[g_sys_heap_size>>2] = HEAP_SIZE;
  i32[g_sys_stack_cells>>2] = STACK_CELLS;

  // Start heap after G_SYS area.
  i32[g_sys_heap>>2] = i32[g_sys_heap_start>>2] + 256;
  i32[g_sys_heap>>2] += 4;

  // Allocate stacks.
  var fp = i32[g_sys_heap>>2] + 4; i32[g_sys_heap>>2] += STACK_CELLS * 4;
  var rp = i32[g_sys_heap>>2] + 4; i32[g_sys_heap>>2] += STACK_CELLS * 4;
  var sp = i32[g_sys_heap>>2] + 4; i32[g_sys_heap>>2] += STACK_CELLS * 4;

  // FORTH worldlist (relocated when vocabularies added).
  var forth_wordlist = i32[g_sys_heap>>2];
  COMMA(0);
  // Vocabulary stack.
  i32[g_sys_current>>2] = forth_wordlist;
  i32[g_sys_context>>2] = i32[g_sys_heap>>2];
  i32[g_sys_latestxt>>2] = 0;
  COMMA(forth_wordlist);
  for (var i = 0; i < VOCABULARY_DEPTH; ++i) { COMMA(0); }

  // setup boot text.
  var source = i32[g_sys_heap>>2];
  var len = Load(i32[g_sys_heap>>2], boot);
  i32[g_sys_heap>>2] += len;
  var source_len = i32[g_sys_heap>>2] - source;
  i32[g_sys_boot>>2] = source;
  i32[g_sys_boot_size>>2] = source_len;

  InitDictionary();

  i32[g_sys_latestxt>>2] = 0;  // So last builtin doesn't get wrong size.
  i32[g_sys_DOLIT_XT>>2] = Find("DOLIT");
  i32[g_sys_DOFLIT_XT>>2] = Find("DOFLIT");
  i32[g_sys_DOEXIT_XT>>2] = Find("EXIT");
  i32[g_sys_YIELD_XT>>2] = Find("YIELD");
  i32[g_sys_notfound>>2] = Find("DROP");

  // Init code.
  var start = i32[g_sys_heap>>2];
  COMMA(Find("EVALUATE1"));
  COMMA(Find("BRANCH"));
  COMMA(start);

  i32[g_sys_argc>>2] = 0;
  i32[g_sys_argv>>2] = 0;
  i32[g_sys_base>>2] = 10;
  i32[g_sys_tib>>2] = source;
  i32[g_sys_ntib>>2] = source_len;
  i32[g_sys_ntib>>2] = source_len;

  rp += 4; i32[rp>>2] = start;
  rp += 4; i32[rp>>2] = fp;
  rp += 4; i32[rp>>2] = sp;
  i32[g_sys_rp>>2] = rp;
}

function VM(stdlib, foreign, heap) {
  "use asm";

  var imul = stdlib.Math.imul;
  var fround = stdlib.Math.fround;

  var sqrt = stdlib.Math.sqrt;
  var sin = stdlib.Math.sin;
  var cos = stdlib.Math.cos;
  var atan2 = stdlib.Math.atan2;
  var floor = stdlib.Math.floor;
  var exp = stdlib.Math.exp;
  var log = stdlib.Math.log;
  var pow = stdlib.Math.pow;
  var fabs = stdlib.Math.abs;
  var fmin = stdlib.Math.min;
  var fmax = stdlib.Math.max;

  var SSMOD = foreign.SSMOD;
  var Call = foreign.Call;
  var COMMA = foreign.COMMA;
  var CCOMMA = foreign.CCOMMA;
  var DOES = foreign.DOES;
  var DOIMMEDIATE = foreign.DOIMMEDIATE;
  var UNSMUDGE = foreign.UNSMUDGE;
  var TOBODY = foreign.TOBODY;
  var create = foreign.create;
  var find = foreign.find;
  var parse = foreign.parse;
  var convert = foreign.convert;
  var fconvert = foreign.fconvert;
  var evaluate1 = foreign.evaluate1;
  var emitlog = foreign.log;
  var trace = foreign.trace;

  var u8 = new stdlib.Uint8Array(heap);
  var i16 = new stdlib.Int16Array(heap);
  var i32 = new stdlib.Int32Array(heap);
  var f32 = new stdlib.Float32Array(heap);

{{sys}}

  function memset(dst, ch, n) {
    dst = dst | 0;
    ch = ch | 0;
    n = n | 0;
    while (n | 0) {
      u8[dst] = ch;
      dst = (dst + 1) | 0;
      n = (n - 1) | 0;
    }
  }

  function memmove(dst, src, n) {
    dst = dst | 0;
    src = src | 0;
    n = n | 0;
    if ((src | 0) < (dst | 0)) {
      src = (src + n - 1) | 0;
      dst = (dst + n - 1) | 0;
      while (n | 0) {
        u8[dst] = u8[src];
        src = (src - 1) | 0;
        dst = (dst - 1) | 0;
        n = (n - 1) | 0;
      }
    } else {
      while (n | 0) {
        u8[dst] = u8[src];
        src = (src + 1) | 0;
        dst = (dst + 1) | 0;
        n = (n - 1) | 0;
      }
    }
  }

  function run() {
    var tos = 0;
    var ip = 0;
    var sp = 0;
    var rp = 0;
    var fp = 0;
    var w = 0;
    var ir = 0;
    var ft = fround(0.0);

    // UNPARK
    rp = i32[g_sys_rp>>2]|0;
    sp = i32[rp>>2]|0; rp = (rp - 4)|0;
    tos = i32[sp>>2]|0; sp = (sp - 4)|0;
    fp = i32[rp>>2]|0; rp = (rp - 4)|0;
    ip = i32[rp>>2]|0; rp = (rp - 4)|0;
    for (;;) {
      //trace(ip|0, sp|0, tos|0);
      w = i32[ip>>2]|0;
      ip = (ip + 4)|0;
      decode: for (;;) {
        ir = u8[w]|0;
        switch (ir&0xff) {
{{cases}}
          default:
            break;
        }
        break;
      }
    }
  }
  return {run: run};
}

var ffi = {
  'Call': Call,
  'create': function(name, len, flags, op) { Create(GetString(name, len), flags, op); },
  'parse': function(sep, ret) { return Parse(sep, ret); },
  'find': function(a, n) { return Find(GetString(a, n)); },
  'convert': function(pos, n, base, ret) { return Convert(pos, n, base, ret); },
  'fconvert': function(pos, n, ret) { return FConvert(pos, n, ret); },
  'evaluate1': function(rp) { return Evaluate1(rp); },
  'log': function(n) { console.log(n); },
  'trace': function(ip, sp, tos) { trace(ip, sp, tos); },
  'COMMA': function(n) { COMMA(n); },
  'CCOMMA': function(n) { CCOMMA(n); },
  'SSMOD': function(sp) { SSMOD(sp); },
  'TOBODY': function(tos) { return TOBODY(tos); },
  'DOES': function(ip) { DOES(ip); },
  'DOIMMEDIATE': function() { DOIMMEDIATE(); },
  'UNSMUDGE': function() { UNSMUDGE(); },
};

function getGlobalObj() {
  return (function(g) {
    return g;
  })(new Function('return this')());
}
var globalObj = getGlobalObj();

var module = VM(globalObj, ffi, heap);

function run() {
  module.run();
  setTimeout(run, 0);
}

function Start() {
  LoadScripts();
  Init();
  setTimeout(run, 0);
}

if (globalObj.write) {
  Start();
} else {
  if (globalObj.ueforth === null) {
    globalObj.ueforth = context;
    context.Start = Start;
  } else {
    window.addEventListener('load', function() {
      Start();
    });
  }
}

})();
