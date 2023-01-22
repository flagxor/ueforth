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

( Testing thrown values )

e: test-abort
  ' abort catch -1 =assert
;e

e: test-abort"
  : test abort" doh!" ;
  ' test catch -2 =assert
  out: doh!
;e

( Skip on ESP32 as not emulated. )
DEFINED? esp 0= [IF]

e: test-0/
  123 0 ' / catch -10 =assert
  0 =assert 123 =assert
;e

e: test-0mod
  123 0 ' mod catch -10 =assert
  0 =assert 123 =assert
;e

e: test-0*/
  123 456 0 ' */ catch -10 =assert
  0 =assert 456 =assert 123 =assert
;e

e: test-0*/mod
  123 456 0 ' */mod catch -10 =assert
  0 =assert 456 =assert 123 =assert
;e

e: test-0/mod
  123 0 ' /mod catch -10 =assert
  0 =assert 123 =assert
;e

e: test-bad-load
  0 ' @ catch -9 =assert
  0 =assert
;e

e: test-bad-store
  123 0 ' ! catch -9 =assert
  0 =assert 123 =assert
;e

( Skip on win64 because wine can't handle these, unsure why. )
DEFINED? windows 0= cell 4 = or [IF]

e: test-bad-execute
  internals
  0 ' call0 catch -9 =assert
  0 =assert
;e

[THEN]

[THEN]
