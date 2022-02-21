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

-1 -1 window
: run
  begin
    wait
    0 to color
    0 0 width height box
    LEFT-BUTTON pressed? if $ccccff else $ffccff then to color
    mouse-x mouse-y height heart
    flip
  event FINISHED = until
  bye
;
run
