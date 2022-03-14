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

( Lazy load expand Graphics for Xlib )

graphics definitions

: window r|

also x11
forth graphics internals definitions
also posix also x11

0 value display
0 value screen
0 value colormap
0 value visual
0 value screen-depth
0 value black
0 value white
0 value root-window
0 value window-handle
0 value gc
0 value image
0 value xevent-type
create xevent xevent-size allot
256 constant keybuffer-size
create keybuffer keybuffer-size allot
0 value keybuffer-used

ExposureMask
ButtonPressMask or
ButtonReleaseMask or
KeyPressMask or
KeyReleaseMask or
PointerMotionMask or
StructureNotifyMask or constant EVENT-MASK

: image-resize { w h }
  w to width   h to height
  image if image XDestroyImage drop then
  w h * 4* malloc dup 0= throw to backbuffer
  display visual screen-depth ZPixmap 0 backbuffer
    width height 32 width 4* XCreateImage to image
;

: update-mouse
  [ xbutton ]
  xevent ->x sl@ to mouse-x
  xevent ->y sl@ to mouse-y
;

: update-key
  [ xkey ]
  xevent keybuffer keybuffer-size
    0 >r rp@ NULL XLookupString to keybuffer-used
    r> to last-key
  PRESSED event = negate last-key key-state!
;

: pending-key?
  keybuffer-used 0 <= if 0 exit then
  keybuffer c@ to last-char
  keybuffer 1+ keybuffer keybuffer-size 1- cmove>
  keybuffer-used 1- to keybuffer-used
  TYPED to event
  -1
;

: update-event
  IDLE to event
  xevent [ xany ] ->type sl@ to xevent-type
  Expose xevent-type = if
    [ xexposure ]
    xevent ->count @ 0= if
      EXPOSED to event
      exit
    then
  then
  ConfigureNotify xevent-type = if
    RESIZED to event
    [ xconfigure ]
    xevent ->width sl@ xevent ->height sl@ image-resize
    exit
  then
  KeyPress xevent-type = if
    PRESSED to event
    update-mouse
    update-key
    exit
  then
  KeyRelease xevent-type = if
    RELEASED to event
    update-mouse
    update-key
    exit
  then
  ButtonPress xevent-type = if
    PRESSED to event
    update-mouse
    ( uses carnal knowledge )
    [ xbutton ] 256 xevent ->button sl@ - to last-key
    1 last-key key-state!
    exit
  then
  ButtonRelease xevent-type = if
    RELEASED to event
    update-mouse
    ( uses carnal knowledge )
    [ xbutton ] 256 xevent ->button sl@ - to last-key
    0 last-key key-state!
    exit
  then
  MotionNotify xevent-type = if
    MOTION to event
    update-mouse
    exit
  then
;

also graphics definitions

: window { w h }
  w 0< if 640 to w 480 to h then
  NULL XOpenDisplay to display
  display XDefaultScreen to screen
  display screen XDefaultColorMap to colormap
  display screen XDefaultVisual to visual
  display screen XDefaultDepth to screen-depth
  display screen XBlackPixel to black
  display screen XWhitePixel to white
  display screen XRootWindow to root-window
  display root-window 0 0 w h 0 black white
    XCreateSimpleWindow to window-handle
  display window-handle XMapWindow drop
  display window-handle 0 NULL XCreateGC to gc
  display window-handle EVENT-MASK XSelectInput drop
  1 1 image-resize
;

: flip
  display window-handle gc image
    0 0 0 0 width height XPutImage drop
;

: wait
  pending-key? if exit then
  display xevent XNextEvent drop
  update-event
;

: poll
  pending-key? if exit then
  display event-mask xevent XCheckMaskEvent
    if update-event else IDLE to event then
;

forth definitions
previous previous previous previous
window
| evaluate ;

forth definitions
