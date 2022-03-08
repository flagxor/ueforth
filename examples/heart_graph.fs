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
640 480 window

: run
  begin
    wait
    0 to color 0 0 width height box
    g{
      vertical-flip
      3200 3200 viewport
      $ffcccc to color
      1600 1600 2900 heart
      g{
        -5 -5 translate
        $ffffff to color
        32 for
            i 100 * 0 10 3200 box
            0 i 100 * 3200 10 box
        next
        $ff0000 to color
        1600 0 10 3200 box
        0 2400 3200 10 box
      }g
    }g
    flip
  FINISHED event = until
  bye
;
run
