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

internals vocabulary hashing  hashing definitions

0 VALUE h0  0 VALUE h1  0 VALUE h2  0 VALUE h3
0 VALUE h4  0 VALUE h5  0 VALUE h6  0 VALUE h7
0 VALUE a   0 VALUE b   0 VALUE c   0 VALUE d
0 VALUE e   0 VALUE f   0 VALUE g   0 VALUE h
0 VALUE temp1           0 VALUE temp2

CREATE w 80 4* ALLOT
: @w ( n -- n ) 4* w + UL@ ;
: !w ( n n -- ) 4* w + L! ;

: 32-bit ( n -- n ) $ffffffff AND ;
: L+ ( n n -- n ) + 32-bit ;
: L, ( n -- ) HERE L! 4 ALLOT ;

: <<< ( n n -- n ) 2DUP LSHIFT -ROT 32 SWAP - RSHIFT OR 32-bit ;
: >>> ( n n -- n ) 2DUP RSHIFT -ROT 32 SWAP - LSHIFT OR 32-bit ;

VARIABLE ends
: <-> ( n - n ) ends ! 0 4 0 DO 8 LSHIFT ends I + C@ OR LOOP ;
: <->* ( a n -- ) 0 ?DO DUP UL@ <-> OVER L! 4 + LOOP DROP ;

: >dig ( a n -- a )
  BASE @ >R HEX <# # # # # # # # # #> R> BASE !
  ROT 2DUP + >R SWAP CMOVE R> ;

only forth definitions
