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

needs hashing.fs

internals hashing definitions

: init   $67452301 TO h0  $EFCDAB89 TO h1
         $98BADCFE TO h2  $10325476 TO h3  $C3D2E1F0 TO h4 ;

: extend
  80 16 DO
    I 3 - @w  I 8 - @w XOR  I 14 - @w XOR  I 16 - @w XOR  1 <<< I !w
  LOOP
;
: step ( n i -- ) @w +  a 5 <<< + e L+ ( to temp )
                  d TO e  c TO d  b 30 <<< TO c  a TO b  ( from temp ) TO a ;
: start   h0 TO A  h1 TO b  h2 TO c  h3 TO d  h4 TO e ;
: chunk1   20  0 DO b c AND b INVERT d AND XOR      $5A827999 +  I step LOOP ;
: chunk2   40 20 DO b c XOR d XOR                   $6ED9EBA1 +  I step LOOP ;
: chunk3   60 40 DO b c AND b d AND XOR c d AND XOR $8F1BBCDC +  I step LOOP ;
: chunk4   80 60 DO b c XOR d XOR                   $CA62C1D6 +  I step LOOP ;
: finish   a h0 L+ TO h0  b h1 L+ TO h1  c h2 L+ TO h2
           d h3 L+ TO h3  e h4 L+ TO h4 ;
: chunk   extend start chunk1 chunk2 chunk3 chunk4 finish ;

: >w { msg n }
  w 64 ERASE  msg w n CMOVE  $80 w n + c!  w 64 <->* ;

40 constant sha1-size
create sha1-hash  sha1-size allot

: format
  sha1-hash  h0 >dig  h1 >dig  h2 >dig  h3 >dig  h4 >dig  DROP ;

: sha1 { msg n -- hash n } n 64 /mod { edge wholes }
  init
  wholes 0 ?DO msg 64 >w chunk 64 +TO msg LOOP
  edge 0= IF
    0 0 >w
  ELSE
    msg edge >w
    edge 56 >= IF chunk w 64 ERASE THEN
  THEN
  n 8 * 16 RSHIFT 16 RSHIFT 14 !w
  n 8 * 15 !w chunk
  format sha1-hash sha1-size
;

forth definitions
