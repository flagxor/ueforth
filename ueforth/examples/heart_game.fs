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
: arrow-- ( n -- ) 39 swap - arrow-table + c@ 100 * ;

: draw-one { e } e ->kind @ { kind }
  HEART-GOAL kind = if
    $ff0000 128 random dup 8 lshift + + to color
    e ->x @ e ->y @ e ->step @ heart
    exit
  then
  FIRE kind = if
    $222222 to color
    e ->x @ 400 - e ->y @ 400 - 800 800 box
    exit
  then
  SPARK kind = if
    $ff7700 128 random 8 lshift + to color
    e ->x @ 400 - e ->y @ 400 - 800 800 box
    exit
  then
  ARROW kind = if
    $ffff00 256 random + to color
    39 for
      e ->x @ e ->vx @ i 10 */ + i arrow-- 2/ -
      e ->y @ e ->vy @ i 10 */ + i arrow-- 2/ -
      i arrow-- dup box
    next
    exit
  then
;

: volcano
  240 for
    $334400 i 100 240 */ + to color
    32000 i 50 * - 5000 - 24000 i 100 * -
    i 100 * 10000 + 100 box
  next
  0 to color
  32000 4000 - 24000 3000 - 8000 3000 box
;

: draw
  0 to color 0 0 width height box
  g{
    vertical-flip
    100000 48000 viewport
    $003300 to color 0 0 100000 48000 box
    $0044cc to color 0 30000 100000 48000 box
    volcano
    entity-count 0 ?do i entity draw-one loop
  }g
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
  e ->vy @ 4 - e ->vy !
  e ->vx @ e ->x @ + e ->x !
  e ->vy @ e ->y @ + e ->y !
  e ->y @ 0< if DEAD e ->kind ! then
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
  64000 random e ->x !
  48000 random e ->y !
  2000 random 1000 - e ->vx !
  2000 random e ->vy !
  40 random 40 + 100 * e ->step !
;

: random-fire { e }
  FIRE e ->kind !
  32000 8000 random + 4000 - e ->x !
  24000 1000 random + 2000 - e ->y !
  200 random 100 - e ->vx !
  200 random 200 + e ->vy !
;

: random-arrow { e }
  ARROW e ->kind !
  64000 random e ->x !
  48000 random e ->y !
  200 random 100 - e ->vx !
  200 random e ->vy !
;

: mouse-direction ( -- x y ) mouse-x mouse-y screen>g ;

: shoot-arrow
  new entity { e }
  ARROW e ->kind !
  0 e ->x !
  0 e ->y !
  g{
    vertical-flip
    64000 48000 viewport
    g{
      70 1 70 1 scale
      mouse-direction e ->vy ! e ->vx !
    }g
  }g
;

: init
  10 for new entity random-heart next
  10 for new entity random-fire next
  10 for new entity random-arrow next
;

: volcano-spew
  2 for new entity random-fire next
;

: fire shoot-arrow ;

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
    event IDLE = if
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
