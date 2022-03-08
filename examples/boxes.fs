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

graphics

-1 -1 window

$ff0000 constant red
$ffff00 constant yellow
$00ff00 constant green
$0000ff constant blue
$000000 constant black
$ffffff constant white
$cccccc constant gray
$777777 constant dark-gray

: run
  begin
    poll
    IDLE event = if
      black to color 0 0 width height box
      g{
        vertical-flip
        640 480 viewport
        gray to color
        0 0 640 480 box
        dark-gray to color
        0 0 400 300 box
        g{
          mouse-x mouse-y screen>g translate
          LEFT-BUTTON pressed? if
            g{ -100 -100 translate  red to color     -50 -50 100 100 box }g
            g{ 100 -100 translate   yellow to color  -50 -50 100 100 box }g
            g{ -100 100 translate   green to color   -50 -50 100 100 box }g
            g{ 100 100 translate    blue to color    -50 -50 100 100 box }g
          then
          g{ white to color   -50 -50 100 100 box }g
        }g
      }g
      flip
    then
  event FINISHED = until
  bye
;
run
