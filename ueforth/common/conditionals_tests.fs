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

( Interpret time conditionals )

e: test-1[if]
  1 [IF]
    : test ." hi" cr ;
  [THEN]
  test
  out: hi
;e

e: test-0[if]
  : test ." initial" cr ;
  0 [IF]
    : test ." hi" cr ;
  [THEN]
  test
  out: initial
;e

e: test-1[if][else]
  1 [IF]
    : test ." hi" cr ;
  [ELSE]
    : test ." there" cr ;
  [THEN]
  test
  out: hi
;e

e: test-0[if][else]
  0 [IF]
    : test ." hi" cr ;
  [ELSE]
    : test ." there" cr ;
  [THEN]
  test
  out: there
;e

e: test-1[if]-nesting
  1 [IF]
    : test ." foo" cr ;
  [ELSE]
    1 [IF]
      : test ." bar" cr ;
    [ELSE]
      : test ." baz" cr ;
    [THEN]
  [THEN]
  test
  out: foo
;e

e: test-0[if]-nesting
  0 [IF]
    1 [IF]
      : test ." foo" cr ;
    [ELSE]
      : test ." bar" cr ;
    [THEN]
  [ELSE]
    : test ." baz" cr ;
  [THEN]
  test
  out: baz
;e
