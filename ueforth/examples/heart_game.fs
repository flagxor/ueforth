#! /usr/bin/env ueforth

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

grf also structures
640 480 window

1 31 lshift 1- constant max-random
0 value seed
: random ( n -- )
  seed max-random */
  seed 7127 + 7919 * max-random mod to seed
;

10000 constant entity-limit
struct EntityStruct
  ptr field ->x
  ptr field ->y
  ptr field ->vx
  ptr field ->vy
  ptr field ->kind
  ptr field ->step

0 constant DEAD
1 constant HEART-GOAL
2 constant FIRE
3 constant SPARK
4 constant ARROW

create entity-array entity-limit EntityStruct * allot
: entity ( n -- a ) EntityStruct * entity-array + ;
0 value entity-count

: nix ( n -- )
  entity entity-count 1- entity swap EntityStruct cmove
  entity-count 1- to entity-count
;

: cleanup
  0 begin dup entity-count < while
    dup entity ->kind @ DEAD = if
      dup nix
    else
      1+
    then
  repeat
;

: new ( -- n )
  entity-count entity EntityStruct erase
  entity-count dup 1+ to entity-count
;

create arrow-table
1 c, 1 c, 2 c, 2 c, 3 c, 3 c, 1 c, 1 c, 1 c, 1 c,
1 c, 1 c, 1 c, 1 c, 1 c, 1 c, 1 c, 1 c, 1 c, 1 c,
1 c, 1 c, 1 c, 1 c, 1 c, 1 c, 1 c, 1 c, 1 c, 1 c,
2 c, 3 c, 3 c, 3 c, 4 c, 4 c, 5 c, 5 c, 5 c, 5 c,
: arrow-- ( n -- ) 39 swap - arrow-table + c@ ;

: draw-one { e } e ->kind @ { kind }
  HEART-GOAL kind = if
    $ff0000 128 random dup 8 lshift + + to color
    e ->x @ 100 / e ->y @ 100 / e ->step @ heart
    exit
  then
  FIRE kind = if
    $222222 to color
    e ->x @ 100 / 4 - e ->y @ 100 / 4 - 8 8 box
    exit
  then
  SPARK kind = if
    $ff7700 128 random 8 lshift + to color
    e ->x @ 100 / 4 - e ->y @ 100 / 4 - 8 8 box
    exit
  then
  ARROW kind = if
    $ffff00 256 random + to color
    39 for
      e ->x @ 100 / e ->vx @ i 200 */ + i arrow-- 2/ -
      e ->y @ 100 / e ->vy @ i 200 */ + i arrow-- 2/ -
      i arrow-- dup box
    next
    exit
  then
;

: volcano
  height 2/ for
    $334400 i 100 height */ + to color
    width 2/ i 2/ - i height 2/ + i height 8 / + 1 box
  next
  0 to color
  width 2/ height 2/
    height 8 / 20 box
;

: draw
  $003300 to color 0 0 width height box
  volcano
  entity-count 0 ?do i entity draw-one loop
  flip
;

: random-spark { e }
  new entity { s }
  SPARK s ->kind !
  e ->x @ s ->x !
  e ->y @ s ->y !
  e ->vx @ 200 random 100 - + s ->vx !
  e ->vy @ 200 random 200 - + s ->vy !
  0 s ->step !
;

: tick-one { e }
  e ->vy @ 4 + e ->vy !
  e ->vx @ e ->x @ + e ->x !
  e ->vy @ e ->y @ + e ->y !
  e ->y @ height 100 * > if DEAD e ->kind ! then
  e ->kind @ { kind }
  FIRE kind = if
    e random-spark
  then
  SPARK kind = if
    1 e ->step +!
    e ->step @ 10 > if DEAD e ->kind ! then
  then
;

: tick   entity-count 0 ?do i entity tick-one loop ;

: random-heart { e }
  HEART-GOAL e ->kind !
  width 100 * random e ->x !
  height 100 * random e ->y !
  2000 random 1000 - e ->vx !
  2000 random 2000 - e ->vy !
  20 random 20 + e ->step !
;

: random-fire { e }
  FIRE e ->kind !
  width 10 * random width 50 * + e ->x !
  height 1 * random height 50 * + 100 + e ->y !
  200 random 100 - e ->vx !
  200 random 400 - e ->vy !
;

: random-arrow { e }
  ARROW e ->kind !
  width 100 * random e ->x !
  height 100 * random e ->y !
  200 random 100 - e ->vx !
  200 random 200 - e ->vy !
;

: mouse-direction ( -- x y )
  mouse-x 3 2 */
  height mouse-y - negate 3 2 */ ;

: targeted-arrow
  new entity { e }
  ARROW e ->kind !
  0 e ->x !
  height 100 * e ->y !
  mouse-direction e ->vy ! e ->vx !
;

: init
  10 for new entity random-heart next
  10 for new entity random-fire next
  10 for new entity random-arrow next
;

: volcano-spew
  1 for new entity random-fire next
;

: fire
  targeted-arrow
;

0 value last-tm
0 value next-tm

: run
  begin
    poll
    PRESSED event = if
      65 last-key = if
        init
      then
      LEFT-BUTTON last-key = if
        fire
      then
    then
    event UNKNOWN = if
      begin ms-ticks to next-tm next-tm last-tm - 10 < while 1 ms repeat
      next-tm to last-tm
      100 random 0= if volcano-spew then
      draw
      tick
      cleanup
    then
  FINISHED event = until
  bye
;
run
