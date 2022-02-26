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
\   color! ( col -- )
\ Drawing:
\   box ( x y w h -- )

also internals
grf definitions
internals definitions

variable color

( Scale to be divided by $10000 )
variable sx   variable sy
$10000 sx !   $10000 sy !
( Translation )
variable tx   variable ty

: hline { x y w }
  \ x y pixel w 1- for color @ over l! 4 + next drop ;
  x y pixel w color @ fill32 ;

grf definitions also internals

: color! ( col -- ) color ! ;

: box { left top w h }
  left sx @ * tx @ + 16 rshift to left
  top sy @ * ty @ + 16 rshift to top
  w sx @ * 16 rshift to w
  h sy @ * 16 rshift to h

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

only forth definitions
