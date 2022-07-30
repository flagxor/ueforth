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
  here =assert
  current @ =assert
  context @ @ =assert
;e

e: test-see-number
  : test 123 456 ;
  see test
  out: : test 
  out:     123 456 
  out: ; 
;e

e: test-see-string
  : test s" hello there" ;
  see test
  out: : test 
  out:     s" hello there" 
  out: ; 
;e

e: test-see-begin-again
  : test begin . again ;
  see test
  out: : test 
  out:     BEGIN 
  out:         . 
  out:     AGAIN 
  out:     
  out: ; 
;e

e: test-see-begin-until
  : test begin . until ;
  see test
  out: : test 
  out:     BEGIN 
  out:         . 
  out:     UNTIL 
  out:     
  out: ;  
;e

e: test-see-begin-while-repeat
  : test begin . while . repeat ;
  see test
  out: : test 
  out:     BEGIN 
  out:         . 
  out:     WHILE 
  out:         . 
  out:     REPEAT 
  out:     
  out: ;  
;e

e: test-see-ahead-then
  : test ahead . then ;
  see test
  out: : test 
  out:     AHEAD 
  out:         . 
  out:     THEN 
  out:     
  out: ; 
;e

e: test-see-for-next
  : test for i . next ;
  see test
  out: : test 
  out:     FOR 
  out:         I . 
  out:     NEXT 
  out:     
  out: ; 
;e

e: test-see-for-aft-next
  : test for aft i . then . next . ;
  see test
  out: : test 
  out:     FOR 
  out:         AFT 
  out:             I . 
  out:         THEN 
  out:         . 
  out:     NEXT 
  out:     . 
  out: ; 
;e

e: test-see-string-strides
  : test0 1 if ." " then ;
  : test1 1 if ." >" then ;
  : test2 1 if ." ->" then ;
  : test3 1 if ." -->" then ;
  : test4 1 if ." --->" then ;
  : test5 1 if ." ---->" then ;
  : test6 1 if ." ----->" then ;
  : test7 1 if ." ------>" then ;
  : test8 1 if ." ------->" then ;
  see test0
  out: : test0 
  out:     1 IF 
  out:         s" " type 
  out:     THEN 
  out:     
  out: ; 
  see test1
  out: : test1 
  out:     1 IF 
  out:         s" >" type 
  out:     THEN 
  out:     
  out: ; 
  see test2
  out: : test2 
  out:     1 IF 
  out:         s" ->" type 
  out:     THEN 
  out:     
  out: ; 
  see test3
  out: : test3 
  out:     1 IF 
  out:         s" -->" type 
  out:     THEN 
  out:     
  out: ; 
  see test4
  out: : test4 
  out:     1 IF 
  out:         s" --->" type 
  out:     THEN 
  out:     
  out: ; 
  see test5
  out: : test5 
  out:     1 IF 
  out:         s" ---->" type 
  out:     THEN 
  out:     
  out: ; 
  see test6
  out: : test6 
  out:     1 IF 
  out:         s" ----->" type 
  out:     THEN 
  out:     
  out: ; 
  see test7
  out: : test7 
  out:     1 IF 
  out:         s" ------>" type 
  out:     THEN 
  out:     
  out: ; 
  see test8
  out: : test8 
  out:     1 IF 
  out:         s" ------->" type 
  out:     THEN 
  out:     
  out: ; 
;e

e: test-noname
  :noname dup * ;
  2 over execute
  swap execute 
  . cr
  out: 16 
;e

e: test-see-variable
  variable foo
  : bar foo @ . ;
  see bar
  out: : bar 
  out:     foo @ . 
  out: ; 
;e

e: test-see-create
  create foo
  : bar foo @ . ;
  see bar
  out: : bar 
  out:     foo @ . 
  out: ; 
;e

e: test-see-value
  0 value foo
  : bar foo . ;
  see bar
  out: : bar 
  out:     foo . 
  out: ; 
;e

e: test-see-to
  0 value foo
  : bar 123 to foo . ;
  see bar
  out: : bar 
  out:     123 TO foo 
  out:     . 
  out: ; 
;e

e: test-see-immediate
  : foo 123 ; immediate
  see foo
  out: : foo 
  out:     123 
  out: ; IMMEDIATE 
;e
