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

e: test-structure
  also structures
  struct gappy
    ptr field ->first
     i8 field ->foo
    i16 field ->bar
    i32 field ->baz
  1000 ->first 1000 =assert
  1000 ->foo 1000 cell+ =assert
  1000 ->bar 1002 cell+ =assert
  1000 ->baz 1004 cell+ =assert
  i8 1 =assert
  i16 2 =assert
  i32 4 =assert
  i64 8 =assert
  ptr cell =assert
;e

e: test-nested-structure
  also structures
  struct rect
    i32 field ->left
    i32 field ->top
    i32 field ->right
    i32 field ->bottom
  struct gappy
     i16 field ->foo
    rect field ->bar
  1000 ->foo 1000 =assert
  1000 ->bar ->left 1004 =assert
  1000 ->bar ->top 1008 =assert
  1000 ->bar ->right 1012 =assert
  1000 ->bar ->bottom 1016 =assert
;e

e: test-forth-structure
  also structures
  0 last-align !
  struct foo
    1 field t1
    4 field t2
    9 field t3
   12 field t4
  0 t1 0 =assert
  0 t2 1 =assert
  0 t3 5 =assert
  0 t4 14 =assert
;e

e: test-structure-accessors
  also structures
  struct foo
    i8 field ->a
    u8 field ->b
    i16 field ->c
    u16 field ->d
    i32 field ->e
    u32 field ->f
    ptr field ->g
  pad foo erase

  127 pad !field ->a
  255 pad !field ->b
  32767 pad !field ->c
  65535 pad !field ->d
  2147483647 pad !field ->e
  4294967295 pad !field ->f
  1234 pad !field ->g

  127 pad @field ->a =assert
  255 pad @field ->b =assert
  32767 pad @field ->c =assert
  65535 pad @field ->d =assert
  2147483647 pad @field ->e =assert
  4294967295 pad @field ->f =assert
  1234 pad @field ->g =assert

  -128 pad !field ->a
  0 pad !field ->b
  -32768 pad !field ->c
  0 pad !field ->d
  -2147483648 pad !field ->e
  0 pad !field ->f
  1234 pad !field ->g

  -128 pad @field ->a =assert
  0 pad @field ->b =assert
  -32768 pad @field ->c =assert
  0 pad @field ->d =assert
  -2147483648 pad @field ->e =assert
  0 pad @field ->f =assert
  1234 pad @field ->g =assert
;e
