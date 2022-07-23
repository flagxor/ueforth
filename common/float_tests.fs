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

e: test-f.
  123e f. cr
  out: 123.000000 
  123.123e f. cr
  out: 123.123000 
  -123.123e f. cr
  out: -123.123000 
;e

e: test-f+
  123e 11e f+ f. cr
  out: 134.000000 
;e

e: test-f*
  123e 10e f* f. cr
  out: 1230.000000 
;e

e: test-1/f
  100e 1/f f. cr
  out: 0.009999 
;e

e: test-f/
  1000e 4e f/ f. cr
  out: 250.000000 
;e

e: test-fsqrt
  256e fsqrt f. cr
  out: 16.000000 
;e

e: test-fswap
  123e 234e fswap f. f. cr
  out: 123.000000 234.000000 
;e

e: test-fover
  123e 234e fover f. f. f. cr
  out: 123.000000 234.000000 123.000000 
;e

e: test-throw
  : bar   123e 124e 125e 1 throw ;
  : foo   99e ['] bar catch . f. ;
  foo cr
  out: 1 99.000000 
;e

e: test-fconstant
  100e fconstant foo
  foo f. cr
  out: 100.000000 
;e

e: test-fvariable
  fvariable foo
  10e foo sf!
  foo sf@ fdup f* foo sf!
  foo sf@ f. cr
  out: 100.000000 
;e

e: test-fcompare
  123e 245e f< assert
  123e 66e f> assert
  123e 123e f>= assert
  124e 123e f>= assert
  123e 123e f<= assert
  123e 124e f<= assert
  123e 124e f<> assert
  123e 123e f= assert
;e

e: test-fliteral
  : foo [ 123e ] fliteral f. cr ;
  foo
  out: 123.000000 
;e

e: test-afliteral
  : foo [ 123e afliteral ] f. cr ;
  foo
  out: 123.000000 
;e

e: test-float-broken-parse
  internals
  s" teste" s>float? 0= assert
;e

e: test-sfloats
  10 sfloats 40 =assert
;e

e: test-sfloat+
  1 sfloat+ 5 =assert
;e

e: test-sfloat
  sfloat 4 =assert
;e
