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

vocabulary web   web definitions

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
  context.inbuffer = [];
  if (!globalObj.write) {
    context.screen = document.getElementById('ueforth');
    if (context.screen === null) {
      context.screen = document.createElement('div');
      document.body.appendChild(context.screen);
    }
    context.terminal = document.createElement('pre');
    context.screen.appendChild(context.terminal);
    context.outbuffer = '';
    window.onkeypress = function(e) {
      context.inbuffer.push(e.keyCode);
    };
    window.onkeydown = function(e) {
      if (e.keyCode == 8) {
        context.inbuffer.push(e.keyCode);
      }
    };
  }
" jseval

r|
(function(sp) {
  var n = i32[sp>>2]; sp -= 4;
  var a = i32[sp>>2]; sp -= 4;
  if (globalObj.write) {
    var text = GetString(a, n);
    write(text);
  } else {
    for (var i = 0; i < n; ++i) {
      var ch = u8[a + i];
      if (ch == 12) {
        context.outbuffer = '';
      } else if (ch == 8) {
        context.outbuffer = context.outbuffer.slice(0, -1);
      } else if (ch == 13) {
      } else {
        context.outbuffer += String.fromCharCode(ch);
      }
    }
    context.terminal.innerText = context.outbuffer + String.fromCharCode(0x2592);
    window.scrollTo(0, document.body.scrollHeight);
  }
  return sp;
})
| 2 jseval!
: web-type ( a n -- ) 2 call yield ;
' web-type is type

r|
(function(sp) {
  if (globalObj.readline && !context.inbuffer.length) {
    var line = readline();
    for (var i = 0; i < line.length; ++i) {
      context.inbuffer.push(line.charCodeAt(i));
    }
    context.inbuffer.push(13);
  }
  if (context.inbuffer.length) {
    sp += 4; i32[sp>>2] = context.inbuffer.shift();
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
  if (globalObj.readline) {
    sp += 4; i32[sp>>2] = -1;
    return sp;
  }
  sp += 4; i32[sp>>2] = context.inbuffer.length ? -1 : 0;
  return sp;
})
| 4 jseval!
: web-key? ( -- f ) yield 4 call ;
' web-key? is key?

r|
(function(sp) {
  var val = i32[sp>>2];  sp -= 4;
  if (globalObj.quit) {
    quit(val);
  } else {
    Init();
  }
  return sp;
})
| 5 jseval!
: terminate ( n -- ) 5 call ;

r|
(function(sp) {
  if (globalObj.write) {
    sp += 4; i32[sp>>2] = 0;  // Disable echo.
  } else {
    sp += 4; i32[sp>>2] = -1;  // Enable echo.
  }
  return sp;
})
| 6 jseval! 6 call echo !

forth definitions web

: bye   0 terminate ;
: page   12 emit ;

forth definitions
