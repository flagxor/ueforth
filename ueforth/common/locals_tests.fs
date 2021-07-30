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

( Testing Locals )

e: test-locals-one
  : test { a } a a * ;
  4 test 16 = assert
;e

e: test-locals-two
  : test { a b } a a a b b ;
  7 8 test .s
  out: <5> 7 7 7 8 8 
  sp0 sp!
;e

e: test-alignment
  30 allot
  : color24 { r g b } r 16 lshift g 8 lshift b or or ;
  1 2 3 color24 66051 = assert
;e

e: test-longname
  : setPixelColor { pixelNum } pixelNum ;
  1 setPixelColor 1 = assert
;e

e: test-dash
  : test { a b c -- a a b b c c } a a b b c c ;
  1 2 3 test * + * + * 23 = assert
;e

e: test-for-loop
  : test { a b } 5 for a . b . next cr ;
  1 2 test
  out: 1 2 1 2 1 2 1 2 1 2 1 2 
;e

e: test-do-loop
  : test { a b } 5 0 do a . b . loop cr ;
  1 2 test
  out: 1 2 1 2 1 2 1 2 1 2 
;e

e: test-do-+loop
  : test { a b } 10 0 do i . a . b . 2 +loop cr ;
  99 999 test
  out: 0 99 999 2 99 999 4 99 999 6 99 999 8 99 999 
;e

e: test-to
  : test 0 { a b } 123 to b a . b . cr ;
  3 test 
  out: 3 123 
;e

e: test-to-loop
  : test 0 { x } 5 0 do i i * to x x . loop cr ;
  test
  out: 0 1 4 9 16 
;e

e: test-multi
  : test { a b } 9 99 { c d } a . b . c . d . ;
  1 2 test cr
  out: 1 2 9 99 
;e

e: test-multi-to
  : test { a b } 9 99 { c d } 5 to c a . b . c . d . ;
  1 2 test cr
  out: 1 2 5 99 
;e
