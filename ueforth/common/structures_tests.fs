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
