\ Copyright 2025 Bradley D. Nelson
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

needs ice40_layout.fs

ice40 synthesis definitions

1 value tx
1 value ty
0 value tb

: PLACE ( x y -- ) to ty to tx 0 to tb ;
: PLACE-OUTPUT ( -- o ) tx ty tb Output .create ;
: USED? ( -- f ) place-output .isLogic? 0= if -1 exit then
                 place-output .getLogic 0<> ;

: ADVANCE
  tb 7 < if 1 +to tb exit then
  ty cells-height 1- < if 1 +to ty 0 to tb exit then
  tx cells-width 1- < assert
  1 +to tx 0 to ty 0 to tb
;

: allot-lut ( -- o )
  begin used? while
    advance
  repeat
  place-output
;

: ROUTE! { src dst -- }
  src dst route 0= if
    ." ERROR UNABLE TO ROUTE!!!" cr
    ." SOURCE: " src .print cr
    ." DESTINATION: " dst .print cr
    123 throw
  then
  src dst route.
;

: LUT4 { i0 i1 i2 i3 tbl -- o }
  allot-lut { o }
  tbl o .setLogic
  i0 0 o .getInput route!
  i1 1 o .getInput route!
  i2 2 o .getInput route!
  i3 3 o .getInput route!
  o
;

: FFL ( -- o )
  allot-lut { o }
  $aaaa o .setLogic
  -1 o .dffEnableBit .setBit
  o
;

: FF! ( v ff -- ) 0 swap .getInput route! ;

forth definitions

