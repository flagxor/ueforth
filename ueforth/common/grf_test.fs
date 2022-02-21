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

grf

$00ccff value color

: hline { x y w }
  x y pixel w 1- for color over l! 4 + next drop ;

: box { left top w h }
  left w + top h + { right bottom }
  left 0 max to left
  top 0 max to top
  right width min to right
  bottom height min to bottom
  left right >= top bottom >= or if exit then
  right left - to w
  bottom top - to h
  top h 1- for left over w hline 1+ next drop
;

0 value clicking

640 480 window
: run
  begin
    wait
    PRESSED event = if 1 to clicking then
    RELEASED event = if 0 to clicking then
    0 to color
    0 0 width height box
    clicking if $00cc00 else $ffcc00 then to color
    100 for
      mouse-x 100 - mouse-y 50 - i + 200 1 box
      color 2 + to color
    next
    flip
  event FINISHED = until
  bye
;
run
