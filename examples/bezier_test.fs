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

DEFINED? windows [IF] windows dpi-aware [THEN]
also graphics
also structures

struct ScanSpan
  ptr field ->next
  ptr field ->edge

2048 constant max-scanlines
create scanlines max-scanlines cells allot
scanlines max-scanlines cells erase
0 value free-edges

: new-edge ( n next -- a )
  free-edges if
    free-edges dup ->next @ to free-edges
  else
    ScanSpan allocate throw
  then
  { np }
  np ->next !
  np ->edge !
  np
;
: row-span ( y -- a ) cells scanlines + ;
: add-edge { x y }
  y 0< y max-scanlines >= or if exit then
  y row-span { yp }
  begin yp @ while
    x yp @ ->edge @ <= if
      x yp @ new-edge yp !
      exit
    then
    yp @ ->next to yp
  repeat
  x yp @ new-edge yp !
;
: remove-edge ( y -- x )
  row-span { yp }
  yp @ 0= if 10000 exit then
  yp @ { old }
  yp @ ->edge @
  yp @ ->next @ yp !
  free-edges old ->next !
  old to free-edges
;
: draw-row { y }
  begin y row-span @ while
    y remove-edge y remove-edge over - y swap 1 box
  repeat
;
: draw-spans max-scanlines 0 do i draw-row loop ;

: 0.<< ( n -- n ) 16 lshift ;
: 0.>> ( n -- n ) 16 arshift ;

: line ( x1 y1 x2 y2 )
  0 0 0 0 { x1 y1 x2 y2 dx dy sy ey }
  y1 y2 > if
    y1 y2 to y1 to y2
    x1 x2 to x1 to x2
  then
  x2 x1 - to dx
  y1 0.>> to sy
  y2 0.>> to ey
  ey sy - to dy
  dy 0 ?do
    x1 dx i dy */ + 0.>> sy i + add-edge
  loop
;

: distance2 { x1 y1 x2 y2 } x1 x2 - dup * y1 y2 - dup * + ;

: raw-bezier 0 0 { x1 y1 x2 y2 x3 y3 d xn yn }
  d 0< if
    x1 y1 x3 y3 line
  else
    x1 x2 2* + x3 + 2/ 2/ to xn
    y1 y2 2* + y3 + 2/ 2/ to yn
    x1 y1   x1 x2 + 2/ y1 y2 + 2/  xn yn   d 1- recurse
    xn yn   x2 x3 + 2/ y2 y3 + 2/  x3 y3   d 1- recurse
  then
;

: bezier 10 raw-bezier ;

0 value pen-x   0 value pen-y
: move-to { x y } x 0.<< to pen-x   y 0.<< to pen-y ;
: line-to { x y } pen-x pen-y x 0.<< y 0.<< line   x y move-to ;
: bezier-to { x' y' x y } pen-x pen-y x' 0.<< y' 0.<< x 0.<< y 0.<< bezier  x y move-to ;

-1 -1 window

$ff0000 constant red
$ffff00 constant yellow
$ffffff constant white
$000000 constant black
$0000bb constant blue

: draw
  black to color  0 0 width height box
  yellow to color  0 0 width 2/ height 2/ box

  red to color
  width 3 / 100 move-to
  mouse-x 1 mouse-x mouse-y bezier-to
  mouse-x 2/ 1 line-to
  300 200 200 400 bezier-to
  1 mouse-y 2 3 */ line-to
  width 3 / 100 line-to
  blue to color   draw-spans

  flip
;

: run
  begin
    poll
    IDLE event = if draw then
  event FINISHED = until
  bye
;
run
