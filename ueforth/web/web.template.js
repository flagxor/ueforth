'use strict';

(function() {

const HEAP_SIZE = (1024 * 1024);
const DSTACK_SIZE = 4096;
const RSTACK_SIZE = 4096;

const boot = `
{{boot}}
`;

var buffer = new ArrayBuffer(HEAP_SIZE);
var i32 = new Int32Array(buffer);
var u8 = new Uint8Array(buffer);
var objects = [SetEval];

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

function create(name, opcode) {
}

function InitDictionary() {
{{dict}}
}

function Interpreter(stdlib, foreign, heap) {
  "use asm";

  var imul = stdlib.Math.imul;

  var Call = foreign.Call;
  var COMMA = foreign.COMMA;
  var IMMEDIATE = foreign.IMMEDIATE;
  var find = foreign.parse;
  var parse = foreign.parse;
  var convert = foreign.convert;
  var evaluate1 = foreign.evaluate1;

  var u8 = new stdlib.Uint8Array(heap);
  var i32 = new stdlib.Int32Array(heap);

  function run(initrp) {
    initrp = initrp | 0;
    var tos = 0;
    var ip = 0;
    var sp = 0;
    var rp = 0;
    var w = 0;
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
          case 0:  // OP_DOCOLON
            rp = (rp + 4) | 0; i32[rp] = ip; ip = (w + 4) | 0;
            break;
          case 1:  // OP_DOCREATE
            sp = (sp + 4) | 0; i32[sp] = tos; tos = (w + 8) | 0;  // 4 * 2
            break;
          case 2:  // OP_DODOES
            sp = (sp + 4) | 0; i32[sp] = tos; tos = (w + 8) | 0;  // 4 * 2
            rp = (rp + 4) | 0; i32[rp] = ip; ip = i32[(w + 4) >> 2];
            break;
{{cases}}
        }
      }
    }
  }
}

})();
