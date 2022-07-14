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
| 2 jseval!
: jseval ( a n -- ) 2 call ;

r|
  if (!globalObj['write']) {
    var console = document.createElement('pre');
    console.id = 'console';
    document.body.appendChild(console);
  }
| jseval

r|
(function(sp) {
  var n = i32[sp>>2]; sp -= 4;
  var a = i32[sp>>2]; sp -= 4;
  var text = GetString(a, n);
  if (globalObj['write']) {
    write(text);
  } else {
    var console = document.getElementById('console');
    console.innerText += text.replace(/[\r]/g, '');
  }
  return sp;
})
| 1 jseval!

: web-type   1 call ;  ' web-type is type
: web-key   yield 0 ;  ' web-key is key
: web-key?   yield 0 ;  ' web-key? is key?

transfer forth
forth
ok
