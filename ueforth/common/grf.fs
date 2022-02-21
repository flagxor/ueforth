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

( Generic Graphics Interface )
\ Startup:
\   window ( w h -- )
\ Drawing region:
\   pixel ( x y -- a ) (format [b g r x])
\   width ( -- n )
\   height ( -- n )
\   flip ( -- )
\ Getting events:
\   wait ( -- )
\   poll ( -- )
\ Event info:
\   mouse-x ( -- n )
\   mouse-y ( -- n )
\   last-key ( -- n )
\   last-keysym ( -- n )
\   last-keycode ( -- n )
\   pressed? ( k -- f )
\   event ( -- n )
\ Event constants:
\   UNKNOWN TIMEOUT RESIZED EXPOSED
\   MOTION PRESSED RELEASED FINISHED
\ Key/Button constants:
\   LEFT-BUTTON MIDDLE-BUTTON RIGHT-BUTTON

vocabulary grf   grf definitions
vocabulary internals

0 constant UNKNOWN
1 constant TIMEOUT
2 constant RESIZED
3 constant EXPOSED
4 constant MOTION
5 constant PRESSED
6 constant RELEASED
7 constant FINISHED

255 constant LEFT-BUTTON
254 constant MIDDLE-BUTTON
253 constant RIGHT-BUTTON

0 value mouse-x
0 value mouse-y
0 value last-key
0 value last-keysym
0 value last-keycode
0 value event
0 value width
0 value height

internals definitions

0 value backbuffer

256 constant key-count
create key-state key-count allot
key-state key-count erase

: key-state! ( f k ) key-state + c! ;

grf definitions also internals

: pixel ( w h -- a ) width * + 4* backbuffer + ;

: pressed? ( k -- f ) key-state + c@ 0<> ;

( Rest of the core definitions per platform. )

only forth definitions
