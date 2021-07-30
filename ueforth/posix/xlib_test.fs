\ Copyright 2021 Bradley D. Nelson
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

include posix/xlib.fs

also x11

0 XOpenDisplay constant display
display XDefaultScreen constant screen
display screen XBlackPixel constant black
display screen XWhitePixel constant white
display screen XRootWindow constant root-window
display root-window 0 0 640 480 0 black white XCreateSimpleWindow constant window
display window XMapWindow drop
display window 0 NULL XCreateGC constant gc

ExposureMask
ButtonPressMask or
ButtonReleaseMask or
KeyPressMask or
KeyReleaseMask or
PointerMotionMask or
StructureNotifyMask or constant event-mask
display window event-mask XSelectInput drop

variable width
variable height

create event xevent-size allot
: draw
  width @ . height @ .
  display gc black XSetForeground drop
  display gc black XSetBackground drop
  display window gc 0 0 width @ height @ XFillRectangle drop
  display gc white XSetForeground drop
  display gc white XSetBackground drop
  display window gc 0 0 width @ 2/ height @ 2/ XFillRectangle drop
;
: de event xevent-size
  event c@ .
  event c@ Expose = if
    draw
    ." Expose"
  then
  event c@ ButtonPress = if ." ButtonPress" then
  event c@ ButtonRelease = if ." ButtonRelease" then
  event c@ KeyPress = if ." KeyPress" then
  event c@ KeyRelease = if ." KeyRelease" then
  event c@ MotionNotify = if ." MotionNotify" then
  event c@ DestroyNotify = if ." DestroyNotify" then
  event c@ ConfigureNotify = if
    event 3 16 * 8 + + l@ width !
    event 3 16 * 12 + + l@ height !
    ." width & height: " width @ . height @ .
    ." ConfigureNotify"
  then
  event c@ MapNotify = if ." MapNotify" then
cr ;
: 1e display event XNextEvent drop de ;
: gg begin draw 1e again ;
