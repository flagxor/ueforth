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

( Graphics Heart )
\ Drawing:
\   heart ( x y h -- )

graphics internals definitions

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
0 value heart-start
0 value heart-end

: cmin! ( n a ) dup >r c@ min r> c! ;
: cmax! ( n a ) dup >r c@ max r> c! ;

: heart-initialize
  heart-start if exit then
  heart-size allocate throw to heart-start
  heart-size allocate throw to heart-end
  heart-start heart-size 0 fill
  heart-end heart-size 0 fill
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

: raw-heart 0 { x y sx sy r }
  heart-initialize
  y sy 2/ - to y
  sy 0< if
    y sy + to y
    sy abs to sy
  then
  sy 0 do
    i heart-size sy */ to r
    x heart-start r + c@ sx heart-ratio */ +
      y i +
      heart-end r + c@ sx heart-ratio */
      1 raw-box
    x heart-start r + c@
      heart-end r + c@ + sx heart-ratio */ -
      y i +
      heart-end r + c@ sx heart-ratio */
      1 raw-box
  loop
;

graphics definitions also internals

: heart 0 { x y s r }
  x sx * tx + 16 arshift
  y sy * ty + 16 arshift
  s sx * 16 arshift
  s sy * 16 arshift
  raw-heart
;

only forth definitions
