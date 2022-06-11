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

CREATE k HEX
  428a2f98 L, 71374491 L, b5c0fbcf L, e9b5dba5 L, 3956c25b L, 59f111f1 L, 923f82a4 L, ab1c5ed5 L,
  d807aa98 L, 12835b01 L, 243185be L, 550c7dc3 L, 72be5d74 L, 80deb1fe L, 9bdc06a7 L, c19bf174 L,
  e49b69c1 L, efbe4786 L, 0fc19dc6 L, 240ca1cc L, 2de92c6f L, 4a7484aa L, 5cb0a9dc L, 76f988da L,
  983e5152 L, a831c66d L, b00327c8 L, bf597fc7 L, c6e00bf3 L, d5a79147 L, 06ca6351 L, 14292967 L,
  27b70a85 L, 2e1b2138 L, 4d2c6dfc L, 53380d13 L, 650a7354 L, 766a0abb L, 81c2c92e L, 92722c85 L,
  a2bfe8a1 L, a81a664b L, c24b8b70 L, c76c51a3 L, d192e819 L, d6990624 L, f40e3585 L, 106aa070 L,
  19a4c116 L, 1e376c08 L, 2748774c L, 34b0bcb5 L, 391c0cb3 L, 4ed8aa4a L, 5b9cca4f L, 682e6ff3 L,
  748f82ee L, 78a5636f L, 84c87814 L, 8cc70208 L, 90befffa L, a4506ceb L, bef9a3f7 L, c67178f2 L,
DECIMAL
: k@ ( n -- n ) 4* k + UL@ ;

: init 
  $6a09e667 TO h0 $bb67ae85 TO h1 $3c6ef372 TO h2 $a54ff53a TO h3
  $510e527f TO h4 $9b05688c TO h5 $1f83d9ab TO h6 $5be0cd19 TO h7
;

: s0 { x } x 7 >>> x 18 >>> XOR x 3 RSHIFT XOR ;
: s1 { x } x 17 >>> x 19 >>> XOR x 10 RSHIFT XOR ;
: extend
  64 16 DO
    I 16 - @w  I 7 - @w +  I 15 - @w s0 +  I 2 - @w s1 + I !w
  LOOP
;

: maj { x y z -- n } x y AND x z AND XOR y z AND XOR ;
: ch { x y z -- n } x y AND x INVERT z AND XOR ;
: sh0 { x -- n } x 2 >>> x 13 >>> XOR x 22 >>> XOR ;
: sh1 { x -- n } x 6 >>> x 11 >>> XOR x 25 >>> XOR ;
: step { i }
  h  e sh1 +  e f g ch +  i k@ +  i @w L+ TO temp1
  a sh0  a b c maj L+ TO temp2
  g TO h  f TO g  e TO f  d temp1 L+ TO e
  c TO d  b TO c  a TO b  temp1 temp2 L+ TO a
;

: chunk
  extend
  h0 TO a  h1 TO b  h2 TO c  h3 TO d
  h4 TO e  h5 TO f  h6 TO g  h7 TO h
  64 0 DO I step LOOP
  a h0 L+ TO h0   b h1 L+ TO h1  c h2 L+ TO h2  d h3 L+ TO h3
  e h4 L+ TO h4   f h5 L+ TO h5  g h6 L+ TO h6  h h7 L+ TO h7
;

: >w { msg n }
  w 64 ERASE  msg w n CMOVE  $80 w n + c!  w 64 <->* ;

64 constant sha256-size
create sha256-hash  sha256-size allot

: format
  sha256-hash  h0 >dig  h1 >dig  h2 >dig  h3 >dig
               h4 >dig  h5 >dig  h6 >dig  h7 >dig DROP ;

: sha256 { msg n } n 64 /mod { edge wholes }
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
  format sha256-hash sha256-size
;

forth definitions
