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

( Tests of utils.fs )
e: test-.s0
  .s
  out: <0> 
;e

e: test-.s
  1 2 3 .s
  out: <3> 1 2 3 
  128 .s
  out: <4> 1 2 3 128 
  2drop 2drop
  .s
  out: <0> 
;e

e: test-forget
  context @ @
  current @
  here
  : foo 123 ;
  : bar foo foo ;
  : baz bar bar * * ;
  forget foo
  here = assert
  current @ = assert
  context @ @ = assert
;e

e: test-see-number
  : test 123 456 ;
  see test
  out: : test  123 456 ; 
;e

e: test-see-string
  : test s" hello there" ;
  see test
  out: : test  s" hello there" ; 
;e

e: test-see-branch
  : test begin again ;
  see test
  out: : test  BRANCH ; 
;e

e: test-see-0branch
  : test begin until ;
  see test
  out: : test  0BRANCH ; 
;e

e: test-see-fornext
  : test for next ;
  see test
  out: : test  >R DONEXT ; 
;e
