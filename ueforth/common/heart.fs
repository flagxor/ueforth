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

grf internals definitions

\ For t = 0 to 2pi
\     x = -16 to 16
\     y = -17 to 12
\ Goes around clockwise
\ x = 0 when t = pi
\ x = 0, y = 5 when t = 0
: heart-f ( f: t -- x y )
  fdup fsin 3e f** 16e f* fswap
  fdup fcos 13e f*
  fover 2e f* fcos 5e f* f-
  fover 3e f* fcos 2e f* f-
  fswap 4e f* fcos f-
;

4000 constant heart-steps
1024 constant heart-size
create heart-start heart-size allot
create heart-end heart-size allot
heart-start heart-size 0 fill
heart-end heart-size 0 fill

: cmin! ( n a ) dup >r c@ min r> c! ;
: cmax! ( n a ) dup >r c@ max r> c! ;

: heart-initialize
  heart-start heart-size 7 29 */ 128 fill
  heart-end heart-size 7 29 */ 128 fill
  heart-steps 0 do
    i s>f heart-steps s>f f/ pi f* heart-f
    fnegate 12e f+ 29.01e f/ heart-size s>f f* fswap 16e f* f>s f>s
    2dup heart-start + cmin!
    heart-end + cmax!
  loop
  heart-size 0 do
    heart-end i + c@ heart-start i + c@ - heart-end i + c!
  loop
;

512 29 32 */ constant heart-ratio

grf definitions also internals

: heart 0 { x y s r }
  heart-start c@ 0= if heart-initialize then
  y s 2/ - to y
  s 0 do
    i heart-size s */ to r
    x heart-start r + c@ s heart-ratio */ +
      y i +
      heart-end r + c@ s heart-ratio */
      1 box
    x heart-start r + c@
      heart-end r + c@ + s heart-ratio */ -
      y i +
      heart-end r + c@ s heart-ratio */
      1 box
  loop
;

only forth definitions
