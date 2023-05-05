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

( Test DO LOOP Works )

e: test-0doloop
  : foo 0 do i . loop cr ;
  10 foo
  out: 0 1 2 3 4 5 6 7 8 9 
;e

e: test-0?do-loop
  : foo 0 ?do i . loop cr ;
  10 foo
  out: 0 1 2 3 4 5 6 7 8 9 
  0 foo
  out: 
;e

e: test-rev-doloop
  : foo 0 10 do i . -1 +loop cr ;
  foo
  out: 10 9 8 7 6 5 4 3 2 1 0 
;e

e: test-rev-?doloop
  : foo 0 10 ?do i . -1 +loop cr ;
  foo
  out: 10 9 8 7 6 5 4 3 2 1 0 
;e

e: test-do+loop
  : foo 0 do i . 2 +loop cr ;
  ." 9 foo " 9 foo
  out: 9 foo 0 2 4 6 8 
  ." 10 foo " 10 foo
  out: 10 foo 0 2 4 6 8 
  ." 11 foo " 11 foo
  out: 11 foo 0 2 4 6 8 10 
  ." 1 foo " 1 foo
  out: 1 foo 0 
;e

e: test-?do+loop
  : foo 0 ?do i . 2 +loop cr ;
  ." 9 foo " 9 foo
  out: 9 foo 0 2 4 6 8 
  ." 10 foo " 10 foo
  out: 10 foo 0 2 4 6 8 
  ." 11 foo " 11 foo
  out: 11 foo 0 2 4 6 8 10 
  ." 1 foo " 1 foo
  out: 1 foo 0 
  ." 0 foo " 0 foo
  out: 0 foo 
;e

e: test-doloop-leave
  : foo 0 do 42 emit i 7 = if ." left " leave ." nope" then i . loop cr ;
  ." 7 foo " 7 foo
  out: 7 foo *0 *1 *2 *3 *4 *5 *6 
  ." 8 foo " 8 foo
  out: 8 foo *0 *1 *2 *3 *4 *5 *6 *left 
  ." 9 foo " 9 foo
  out: 9 foo *0 *1 *2 *3 *4 *5 *6 *left 
;e

e: test-do+loop-leave
  : foo 0 do 42 emit i 8 = if ." left " leave ." nope" then i . 2 +loop cr ;
  ." 7 foo " 7 foo
  out: 7 foo *0 *2 *4 *6 
  ." 8 foo " 8 foo
  out: 8 foo *0 *2 *4 *6 
  ." 9 foo " 9 foo
  out: 9 foo *0 *2 *4 *6 *left 
  ." 0 foo " 0 foo
  out: 0 foo *0 *2 *4 *6 *left 
;e

e: test-?do+loop-leave
  : foo 0 ?do 42 emit i 8 = if ." left " leave ." nope" then i . 2 +loop cr ;
  ." 7 foo " 7 foo
  out: 7 foo *0 *2 *4 *6 
  ." 8 foo " 8 foo
  out: 8 foo *0 *2 *4 *6 
  ." 9 foo " 9 foo
  out: 9 foo *0 *2 *4 *6 *left 
  ." 0 foo " 0 foo
  out: 0 foo 
;e

e: test-do+loop-unloop
  : foo 0 do 42 emit i 8 = if ." left " cr unloop exit then i . 2 +loop ." done " cr ;
  ." 7 foo " 7 foo
  out: 7 foo *0 *2 *4 *6 done 
  ." 8 foo " 8 foo
  out: 8 foo *0 *2 *4 *6 done 
  ." 9 foo " 9 foo
  out: 9 foo *0 *2 *4 *6 *left 
  ." 0 foo " 0 foo
  out: 0 foo *0 *2 *4 *6 *left 
;e

e: test-?do+loop-unloop
  : foo 0 ?do 42 emit i 8 = if ." left " cr unloop exit then i . 2 +loop ." done " cr ;
  7 foo
  out: *0 *2 *4 *6 done 
  8 foo
  out: *0 *2 *4 *6 done 
  9 foo
  out: *0 *2 *4 *6 *left 
  0 foo
  out: done 
;e

e: test-doloop-j
  : foo 5 0 do 3 0 do j . loop loop cr ;
  foo
  out: 0 0 0 1 1 1 2 2 2 3 3 3 4 4 4 
;e

e: test-doloop-unsigned
  : foo 0 -1 1 rshift 1+ dup 10 + swap do 1+ loop . cr ;
  foo
  out: 10 
;e

e: test-do+loop-unsigned
  : foo 0 -1 1 rshift dup 10 + do 1+ -1 +loop . cr ;
  foo
  out: 11 
;e
