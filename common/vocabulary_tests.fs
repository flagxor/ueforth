\ Copyright 2021 Bradley D. Nelson
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

e: test-vocabularies
  vocabulary foo
  vocabulary bar
  foo definitions : test ." AAAA" cr ;
  bar definitions : test ." BBBB" cr ;
  forth definitions
  foo test
  bar test
  foo test
  forth definitions
  out: AAAA
  out: BBBB
  out: AAAA
;e

e: test-vlist
  vocabulary foo
  foo definitions
  : pig ;   : cow ;   : sheep ;
  forth definitions
  foo vlist
  forth definitions
  out: sheep cow pig 
;e

e: test-vlist-empty
  vocabulary foo
  foo vlist
  forth definitions
  out:
;e

e: test-order
  vocabulary foo
  vocabulary bar
  vocabulary baz
  also foo also bar also baz
  order
  out: baz >> FORTH 
  out: bar >> FORTH 
  out: foo >> FORTH 
  out: FORTH 
  only forth definitions
;e

e: test-order-previous
  vocabulary foo
  vocabulary bar
  vocabulary baz
  also foo also bar also baz
  order
  out: baz >> FORTH 
  out: bar >> FORTH 
  out: foo >> FORTH 
  out: FORTH 
  previous order
  out: bar >> FORTH 
  out: foo >> FORTH 
  out: FORTH 
  previous order
  out: foo >> FORTH 
  out: FORTH 
  previous order
  out: FORTH 
;e

e: test-previous-throw
  only forth definitions
  ' previous catch 0<> assert
;e

e: test-vocab-define-order
  vocabulary foo
  foo definitions
  : a ." AAAAAA" cr ;
  forth definitions
  : a ." BAD" cr ;
  foo a
  out: AAAAAA
  only forth definitions
;e

e: test-vocabulary-chaining
  vocabulary foo
  foo definitions
  vocabulary bar
  bar definitions
  : a ." aaaa" cr ;
  foo definitions
  : b ." bbbb" cr ;
  forth definitions
  : a ." BAD" cr ;
  : b ." BAD" cr ;
  foo a b
  out: BAD
  out: bbbb
  bar a b
  out: aaaa
  out: bbbb
  only forth definitions
;e

e: test-sealed
  : aaa ." good" cr ;
  vocabulary foo
  foo definitions
  : aaa ." bad" cr ;
  vocabulary bar sealed
  also bar definitions
  : bbb ." b" cr ;
  only forth definitions
  also foo bar
  aaa
  bbb
  out: good
  out: b
  only forth definitions
;e

e: test-nested
  vocabulary foo
  foo definitions
  : hi ;
  : there ;
  vocabulary bar
  bar definitions
  : a ;
  : b ;
  vlist
  out: b a 
  only forth definitions
;e

e: test-fixed-does>-normal
  : adder create , does> @ + ;
  3 adder foo
  4 foo 7 =assert
  4 ' foo execute 7 =assert
;e

also internals
variable see-tally
: tally-type ( a n -- ) nip see-tally +! ;
: test-see-all
  0 see-tally !
  ['] tally-type is type
  see-all
  ['] default-type is type
  see-tally @ 25000 >assert
;

(
e: test-fixed-does>-interp
  create hi 123 , does> @ + ;
  7 hi 130 =assert
;e
)
