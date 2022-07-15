\ Copyright 2022 Bradley D. Nelson
\
\ Licensed under the Apache License, Version 2.0 (the "License");
\ you may not use this file except in compliance with the License.
\ You may obtain a copy of the License at
\
\     http://www.apache.org/licenses/LICENSE-2.0
\
\ Unless required by applicable law or agreed to in writing, software
\ distributed under the License is distributed on an "AS IS" BASIS,
\ WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
\ See the License for the specific language governing permissions and
\ limitations under the License.

internals definitions
( TODO: Figure out why this has to happen so late. )
transfer internals-builtins
forth definitions internals
( Bring a forth to the top of the vocabulary. )
: ok   ." uEforth" raw-ok ;

: jseval! ( a n index -- ) 0 call ;

r|
(function(sp) {
  var n = i32[sp>>2]; sp -= 4;
  var a = i32[sp>>2]; sp -= 4;
  var text = GetString(a, n);
  eval(text);
  return sp;
})
| 1 jseval!
: jseval ( a n -- ) 1 call ;

r"
  globalObj.inbuffer = [];
  if (!globalObj['write']) {
    var con = document.getElementById('console');
    if (con === null) {
      con = document.createElement('pre');
      con.id = 'console';
      document.body.appendChild(con);
    }
    window.outbuffer = '';
    window.onkeypress = function(e) {
      globalObj.inbuffer.push(e.keyCode);
    };
    window.onkeydown = function(e) {
      if (e.keyCode == 8) {
        globalObj.inbuffer.push(e.keyCode);
      }
    };
  }
" jseval

r|
(function(sp) {
  var n = i32[sp>>2]; sp -= 4;
  var a = i32[sp>>2]; sp -= 4;
  if (globalObj['write']) {
    var text = GetString(a, n);
    write(text);
  } else {
    var con = document.getElementById('console');
    for (var i = 0; i < n; ++i) {
      var ch = u8[a + i];
      if (ch == 12) {
        window.outbuffer = '';
      } else if (ch == 8) {
        window.outbuffer = window.outbuffer.slice(0, -1);
      } else if (ch == 13) {
      } else {
        window.outbuffer += String.fromCharCode(ch);
      }
    }
    con.innerText = window.outbuffer + String.fromCharCode(0x2592);
    window.scrollTo(0, document.body.scrollHeight);
  }
  return sp;
})
| 2 jseval!
: web-type ( a n -- ) 2 call yield ;
' web-type is type

r|
(function(sp) {
  if (globalObj['readline'] && !globalObj.inbuffer.length) {
    var line = readline();
    for (var i = 0; i < line.length; ++i) {
      globalObj.inbuffer.push(line.charCodeAt(i));
    }
    globalObj.inbuffer.push(13);
  }
  if (globalObj.inbuffer.length) {
    sp += 4; i32[sp>>2] = globalObj.inbuffer.shift();
  } else {
    sp += 4; i32[sp>>2] = 0;
  }
  return sp;
})
| 3 jseval!
: web-key ( -- n ) begin yield 3 call dup if exit then drop again ;
' web-key is key

r|
(function(sp) {
  if (globalObj['readline']) {
    return -1;
  }
  sp += 4; i32[sp>>2] = globalObj.inbuffer.length ? -1 : 0;
  return sp;
})
| 4 jseval!
: web-key? ( -- f ) yield 4 call ;
' web-key? is key?

r|
(function(sp) {
  var val = i32[sp>>2];  sp -= 4;
  if (globalObj['quit']) {
    quit(val);
  } else {
    Init();
  }
  return sp;
})
| 5 jseval!
: terminate ( n -- ) 5 call ;
: bye   0 terminate ;

r|
(function(sp) {
  if (globalObj['write']) {
    sp += 4; i32[sp>>2] = 0;  // Disable echo.
  } else {
    sp += 4; i32[sp>>2] = -1;  // Enable echo.
  }
  return sp;
})
| 6 jseval! 6 call echo !

: page   12 emit ;

transfer forth
forth
ok
