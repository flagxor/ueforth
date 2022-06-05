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

( Graphics Utilities )
\ Pen:
\   ( $rrggbb ) to color
\ Drawing:
\   box ( x y w h -- )
\ Transforms:
\   g{ ( -- ) Preserve transform
\   }g ( -- ) Restore transform
\   translate ( x y -- )
\   scale ( nx dx ny dy -- )
\   viewport ( w h -- )
\   vertical-flip ( -- ) Use math style viewport.
\ Conversions:
\   screen>g ( x y -- x' y' ) Transform screen to viewport

also internals
graphics definitions

0 value color

internals definitions

( Scale to be divided by $10000 )
$10000 value sx   $10000 value sy
( Translation )
0 value tx   0 value ty

: hline { x y w }
  \ x y pixel w 1- for color over l! 4 + next drop ;
  x y pixel w color fill32 ;

create gstack 1024 cells allot
gstack value gp
: >g ( n -- ) gp ! gp cell+ to gp ;
: g> ( -- n ) gp cell - to gp gp @ ;

: raw-box { left top w h }
  left w + top h + { right bottom }
  left right 2dup min to left max to right
  top bottom 2dup min to top max to bottom
  left 0 max to left
  top 0 max to top
  right width min to right
  bottom height min to bottom
  left right >= top bottom >= or if exit then
  right left - to w
  bottom top - to h
  top h 1- for left over w hline 1+ next drop
;

graphics definitions also internals

: box { left top w h }
  left sx * tx + 16 arshift
  top sy * ty + 16 arshift
  w sx * 16 arshift
  h sy * 16 arshift
  raw-box
;

: screen>g ( x y -- x' y' ) 16 lshift ty - sy / swap
                            16 lshift tx - sx / swap ;

: g{   sx >g   sy >g   tx >g   ty >g ;
: }g   g> to ty   g> to tx   g> to sy   g> to sx ;
: translate ( x y -- ) sy * +to ty   sx * +to tx ;
: scale ( nx dx ny dy -- )
  sy -rot */ to sy
  sx -rot */ to sx ;
: viewport { w h }
  width 2/ height 2/ translate
  10000 width height */ 10000 w h */ < if
    width w  width h w */ 1 max h scale
  else
    height w h */ 1 max w  height h scale
  then
  w 2/ negate h 2/ negate translate
;

: vertical-flip
  0 height 2/ translate
  1 1 -1 1 scale
  0 height 2/ negate translate
;

only forth definitions
