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

internals

: run
  begin
    wait
(
    PRESSED event = if
      ." DOWN: " last-key . cr
    then
    RELEASED event = if
      ." UP: " last-key . cr
    then
    TYPED event = if
      ." CHAR: " last-char . cr
    then
)
    MOTION event = EXPOSED event = or if
    0 to color 0 0 width height box
    g{
      vertical-flip
      640 480 viewport
      $ff0000 to color
      0 0 640 480 box
      $ff7700 to color
      0 0 400 300 box
      g{
        mouse-x mouse-y screen>g translate
        LEFT-BUTTON pressed? if $ccccff else $ffccff then to color
        g{ -100 -100 translate 0 0 100 heart }g
        g{ 100 -100 translate 0 0 100 heart }g
        g{ -100 100 translate 0 0 100 heart }g
        g{ 100 100 translate 0 0 100 heart }g
        g{ -50 -50 100 100 box }g
      }g
    }g
    flip
    then
  event FINISHED = until
  bye
;
run
