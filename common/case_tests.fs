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

( Test CASE Works )

e: test-case
  : foo
    case
      1 of ." one" cr endof
      2 of ." two" cr endof
      ." other: " dup . cr
    endcase
  ;
  1 foo
  out: one
  2 foo
  out: two
  3 foo
  out: other: 3 
;e

e: test-case-dup
  : foo
    case
      1 of ." one" cr endof
      2 of ." two" cr endof
      1 of ." onemore" cr endof
      ." other: " dup . cr
    endcase
  ;
  1 foo
  out: one
  2 foo
  out: two
  3 foo
  out: other: 3 
;e

e: test-case-string
  : foo
    case
      1 of s" one" endof
      2 of s" two" endof
      1 of s" onemore" endof
      >r s" other" r>
    endcase
  ;
  1 foo type cr
  out: one
  2 foo type cr
  out: two
  3 foo type cr
  out: other
;e
