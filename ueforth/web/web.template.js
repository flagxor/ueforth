'use strict';

(function() {

const HEAP_SIZE = (1024 * 1024);
const DSTACK_SIZE = 4096;
const RSTACK_SIZE = 4096;

const boot = `
{{boot}}
`;

function create(name, opcode) {
}

function InitDictionary() {
{{dict}}
}

function Interpreter(stdlib, foreign, heap) {
  "use asm";

  var imul = stdlib.Math.imul;

  var exit = foreign.exit;
  var emit = foreign.emit;
  var qkey = foreign.qkey;
  var color = foreign.color;
  var print_decimal = foreign.print_decimal;
  var print_hexadecimal = foreign.print_hexadecimal;

  var u8 = new stdlib.Uint8Array(heap);
  var i32 = new stdlib.Int32Array(heap);

  function convert(pos, n, ret) {
    pos = pos | 0;
    n = n | 0;
    ret = ret | 0;
    var negate = 0;
    var d = 0;

    i32[ret>>2] = 0;
    if (!n) { return 0; }
    if (u8[pos] == '-') { negate = -1; pos = (pos + 1) | 0; n = (n - 1) | 0; }
    for (; n; n = (n - 1) | 0) {
      d = (u8[pos] - 48) | 0;
      if ((d >>> 0) > 9) {
        d = ((d & 59) - 7) | 0;
        if (d < 10) { return 0; }
      }
      if ((d >>> 0) >= (i32[g_base] >>> 0)) { return 0; }
      ret = ((imul(ret, i32[g_base]) >>> 0) + d) | 0;
      pos = (pos + 1) | 0;
    }
    if (negate) { i32[ret] = (-i32[ret]) | 0; }
    return -1;
  }
    
  function same(a, b, len) {
    a = a | 0;
    b = b | 0;
    len = len | 0;
    for (;len && (i32[a] & 95) == (i32[b] & 95);
         len = (len - 1) | 0, a = (a + 1) | 0, b = (b + 1) | 0);
    return len | 0;
  }

  function run(initrp) {
    initrp = initrp | 0;
    var tos = 0;
    var ip = 0;
    var sp = 0;
    var rp = 0;
    var w = 0;
    var t = 0;
    var ir = 0;
    rp = initrp;
    ip = i32[rp>>2]|0; rp = (rp - 4)|0;
    sp = i32[rp>>2]|0; rp = (rp - 4)|0;
    tos = i32[sp>>2]|0; sp = (sp - 4)|0;
    for (;;) {
      w = i32[ip>>2]|0;
      for (;;) {
        ir = i32[((ip + (w<<2))|0)>>2]|0;
        ip = (ip + 4)|0;
        switch (ir & 0xff) {
{{cases}}
        }
      }
    }
  }
}

})();
