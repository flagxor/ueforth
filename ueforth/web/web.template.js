'use strict';

(function() {

const HEAP_SIZE = (1024 * 1024);
const DSTACK_SIZE = 4096;
const RSTACK_SIZE = 4096;

const boot = `
{{boot}}
`;

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
        }
      }
    }
  }
}

})();
