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
\   last-char ( -- n )
\   pressed? ( k -- f )
\   event ( -- n )
\ Event constants:
\   IDLE RESIZED EXPOSED MOTION
\   PRESSED RELEASED TYPED FINISHED
\ Key/Button constants:
\   LEFT-BUTTON MIDDLE-BUTTON RIGHT-BUTTON

vocabulary graphics   graphics definitions
vocabulary internals

0 constant IDLE
1 constant RESIZED
2 constant EXPOSED
3 constant MOTION
4 constant PRESSED
5 constant RELEASED
6 constant TYPED
7 constant FINISHED

255 constant LEFT-BUTTON
254 constant MIDDLE-BUTTON
253 constant RIGHT-BUTTON

0 value mouse-x
0 value mouse-y
0 value last-key
0 value last-char
0 value event
0 value width
0 value height

internals definitions

0 value backbuffer

256 constant key-count
create key-state key-count allot
key-state key-count erase

: key-state! ( f k ) key-count mod key-state + c! ;

graphics definitions also internals

: pixel ( w h -- a ) width * + 4* backbuffer + ;

: pressed? ( k -- f ) key-state + c@ 0<> ;

( Rest of the core definitions per platform. )

only forth definitions
