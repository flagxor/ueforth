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

( Testing Memory Faults )

( Skip on ESP32 as not emulated. )
DEFINED? esp 0= [IF]

e: test-read-null
  0 ' @ catch assert drop
;e

e: test-read-1023
  1023 ' @ catch assert drop
;e

e: test-read-1024
  1024 ' @ catch assert drop
;e

e: test-write-null
  123 0 ' ! catch assert 2drop
;e

e: test-write-1023
  123 1023 ' ! catch assert 2drop
;e

e: test-write-1024
  123 1024 ' ! catch assert 2drop
;e

( Skip on win64 because wine can't handle these, unsure why. )
DEFINED? windows 0= cell 4 = or [IF]

e: test-call-null
  internals
  0 ' call0 catch assert drop
;e

e: test-call-1023
  internals
  1023 ' call0 catch assert drop
;e

e: test-call-1024
  internals
  1024 ' call0 catch assert drop
;e

[THEN]

[THEN]
