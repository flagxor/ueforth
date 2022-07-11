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

const HEAP_SIZE = (1024 * 1024);
const STACK_CELLS = 4096;
const VOCABULARY_DEPTH = 16;

{{boot}}

var heap = new ArrayBuffer(HEAP_SIZE);
var i32 = new Int32Array(heap);
var u8 = new Uint8Array(heap);
var objects = [SetEval];
var g_sys = 256;  // Placed past a gap.
var g_sys_heap = g_sys + 0 * 4;
var g_sys_current = g_sys + 1 * 4;
var g_sys_context = g_sys + 2 * 4;
var g_sys_latestxt = g_sys + 3 * 4;
var g_sys_notfound = g_sys + 4 * 4;
var g_sys_heap_start = g_sys + 5 * 4;
var g_sys_heap_size = g_sys + 6 * 4;
var g_sys_stack_cells = g_sys + 7 * 4;
var g_sys_boot = g_sys + 8 * 4;
var g_sys_boot_size = g_sys + 9 * 4;
var g_sys_tib = g_sys + 10 * 4;
var g_sys_ntib = g_sys + 11 * 4;
var g_sys_tin = g_sys + 12 * 4;
var g_sys_state = g_sys + 13 * 4;
var g_sys_base = g_sys + 14 * 4;
var g_sys_argc = g_sys + 15 * 4;
var g_sys_argv = g_sys + 16 * 4;
var g_sys_runner = g_sys + 17 * 4;
var g_sys_rp = g_sys + 18 * 4;
var g_sys_DOLIT_XT = g_sys + 19 * 4;
var g_sys_DOFLIT_XT = g_sys + 20 * 4;
var g_sys_DOEXIT_XT = g_sys + 21 * 4;
var g_sys_YIELD_XT = g_sys + 22 * 4;
var g_sys_DOCREATE_OP = g_sys + 23 * 4;
var g_sys_builtins = g_sys + 24 * 4;

function SetEval(sp) {
  var index = i32[sp--];
  var len = i32[sp--];
  var code_addr = i32[sp--];
  var code = '';
  for (var i = 0; i < len; ++i) {
    code += String.fromCharCode(u8[name_addr + i]);
  }
  objects[index] = eval(code);
}

function Call(sp, tos) {
  return objects[tos](sp);
}

function Load(addr, content) {
  for (var i = 0; i < content.length; ++i) {
    u8[addr++] = content.charCodeAt(i);
  }
  return addr;
}

function UPPER(a) {
  // a = 97, z = 122
  return a >= 97 && a <= 122 ? a & 95 : a;
}

function Same(a, b) {
  if (a.length != b.length) {
    return false;
  }
  for (var i = 0; i < a.length; ++i) {
    if (UPPER(a.charCodeAt(i)) != UPPER(b.charCodeAt(i))) {
      return false;
    }
  }
  return true;
}

function GetName(xt) {
  var clen = i32[(xt - 3*4)>>2];
  var ret = '';
  for (var i = 0; i < clen; ++i) {
    ret += String.fromCharCode(u8[xt - 3 * 4 - clen + i]);
  }
  return ret;
}

function Find(name) {
  var pos = i32[i32[g_sys_context>>2]>>2];
  while (pos) {
    if (Same(GetName(pos), name)) {
      return pos;
    }
    pos = i32[(pos - 2*4)>>2];
  }
  return 0;
}

function comma(value) {
  i32[i32[g_sys_heap>>2]>>2] = value;
  i32[g_sys_heap>>2] = (i32[g_sys_heap>>2] + 4) | 0;
}

function create(name, flags, opcode) {
  i32[g_sys_heap>>2] = Load(i32[g_sys_heap>>2], name);  // name
  g_sys_heap = (g_sys_heap + 3) & ~3;

  i32[i32[g_sys_heap>>2]>>2] = name.length;  // length
  i32[g_sys_heap>>2] += 4;

  i32[i32[g_sys_heap>>2]>>2] = i32[i32[i32[g_sys_current]>>2]>>2];  // link
  i32[g_sys_heap>>2] += 4;

  i32[i32[g_sys_heap>>2]>>2] = 0;  // flags
  i32[g_sys_heap>>2] += 4;

  i32[i32[g_sys_current>>2]>>2] = i32[g_sys_heap>>2];

  i32[i32[i32[g_sys_current>>2]>>2]>>2] = opcode;  // code
  i32[g_sys_heap>>2] += 4;
}

function builtin(name, flags, vocab, opcode) {
}

function InitDictionary() {
{{dict}}
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
  comma(0);
  // Vocabulary stack.
  i32[g_sys_current>>2] = forth_wordlist;
  i32[g_sys_context>>2] = i32[g_sys_heap>>2];
  i32[g_sys_latestxt>>2] = 0;
  comma(forth_wordlist);
  for (var i = 0; i < VOCABULARY_DEPTH; ++i) { comma(0); }

  // setup boot text.
  var source = g_sys_heap;
  i32[g_sys_heap>>2] = Load(i32[g_sys_heap>>2], boot);
  var source_len = g_sys_heap - source;
  i32[g_sys_boot>>2] = source;
  i32[g_sys_boot_size>>2] = source_len;

  InitDictionary();

  i32[g_sys_latestxt>>2] = 0;  // So last builtin doesn't get wrong size.
  i32[g_sys_DOLIT_XT>>2] = Find("DOLIT");
  i32[g_sys_DOFLIT_XT>>2] = Find("DOFLIT");
  i32[g_sys_DOEXIT_XT>>2] = Find("EXIT");
  i32[g_sys_YIELD_XT>>2] = Find("YIELD");

  // Init code.
  var start = i32[g_sys_heap>>2];
  comma(Find("EVALUATE1"));
  comma(Find("BRANCH"));
  comma(start);

  i32[g_sys_argc>>2] = 0;
  i32[g_sys_argv>>2] = 0;
  i32[g_sys_base>>2] = 10;
  i32[g_sys_tib>>2] = source;
  i32[g_sys_ntib>>2] = source_len;

  i32[rp>>2] = fp; rp += 4;
  i32[rp>>2] = sp; rp += 4;
  i32[rp>>2] = start; rp += 4;
  i32[g_sys_rp] = rp;
}

function VM(stdlib, foreign, heap) {
  "use asm";

  var imul = stdlib.Math.imul;

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
  var create = foreign.create;
  var find = foreign.find;
  var parse = foreign.parse;
  var memset = foreign.memset;
  var memmove = foreign.memmove;
  var convert = foreign.convert;
  var evaluate1 = foreign.evaluate1;
  var log = foreign.log;

  var u8 = new stdlib.Uint8Array(heap);
  var i16 = new stdlib.Int16Array(heap);
  var i32 = new stdlib.Int32Array(heap);
  var f32 = new stdlib.Float32Array(heap);

  const g_sys = 256;
  var g_sys_heap = g_sys + 0 * 4;
  var g_sys_current = g_sys + 1 * 4;
  var g_sys_context = g_sys + 2 * 4;
  var g_sys_latestxt = g_sys + 3 * 4;
  var g_sys_notfound = g_sys + 4 * 4;
  var g_sys_heap_start = g_sys + 5 * 4;
  var g_sys_heap_size = g_sys + 6 * 4;
  var g_sys_stack_cells = g_sys + 7 * 4;
  var g_sys_boot = g_sys + 8 * 4;
  var g_sys_boot_size = g_sys + 9 * 4;
  var g_sys_tib = g_sys + 10 * 4;
  var g_sys_ntib = g_sys + 11 * 4;
  var g_sys_tin = g_sys + 12 * 4;
  var g_sys_state = g_sys + 13 * 4;
  var g_sys_base = g_sys + 14 * 4;
  var g_sys_argc = g_sys + 15 * 4;
  var g_sys_argv = g_sys + 16 * 4;
  var g_sys_runner = g_sys + 17 * 4;
  var g_sys_rp = g_sys + 18 * 4;
  var g_sys_DOLIT_XT = g_sys + 19 * 4;
  var g_sys_DOFLIT_XT = g_sys + 20 * 4;
  var g_sys_DOEXIT_XT = g_sys + 21 * 4;
  var g_sys_YIELD_XT = g_sys + 22 * 4;
  var g_sys_DOCREATE_OP = g_sys + 23 * 4;
  var g_sys_builtins = g_sys + 24 * 4;

  function run() {
    var tos = 0;
    var ip = 0;
    var sp = 0;
    var rp = 0;
    var fp = 0;
    var w = 0;
    var ir = 0;

    // UNPARK
    rp = i32[g_sys_rp>>2]|0;
    ip = i32[rp>>2]|0; rp = (rp - 4)|0;
    sp = i32[rp>>2]|0; rp = (rp - 4)|0;
    fp = i32[rp>>2]|0; rp = (rp - 4)|0;
    tos = i32[sp>>2]|0; sp = (sp - 4)|0;
    for (;;) {
      w = i32[ip>>2]|0;
      log(ip|0);
      ip = (ip + 4)|0;
      decode: for (;;) {
        ir = u8[w]|0;
        log(ir|0);
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
  Call: Call,
  create: function() { console.log('create'); },
  parse: function() { console.log('parse'); },
  COMMA: function(n) { i32[i32[g_sys_heap>>2]] = n; i32[g_sys_heap>>2] += 4; console.log('comma'); },
  CCOMMA: function(n) { u8[i32[g_sys_heap>>2]] = n; i32[g_sys_heap>>2] += 1; console.log('ccomma'); },
  SSMOD: function() { console.log('ssmod'); },
  DOES: function() { console.log('does'); },
  DOIMMEDIATE: function() { console.log('immediate'); },
  UNSMUDGE: function() { console.log('unsmudge'); },
  parse: function() { console.log('parse'); },
  find: function() { console.log('find'); },
  convert: function() { console.log('convert'); },
  evaluate1: function() { console.log('evaluate1'); },
  log: function(n) { console.log(n); }
};

heap[128 + 6] = 256 * 4;  // set g_sys.heap = 256 * 4;

var globalObj;
if (typeof window === 'undefined') {
  globalObj = global;
} else {
  globalObj = window;
}
var module = VM(globalObj, ffi, heap);
Init();
setTimeout(function() {
  module.run();
}, 10);

})();
