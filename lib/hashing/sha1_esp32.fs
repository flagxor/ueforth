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

$3ff0001c constant peri_clk_en
$3ff00020 constant peri_rst_en
$3ff03000 constant sha_text
$3ff03080 constant sha1_start
$3ff03084 constant sha1_continue
$3ff03088 constant sha1_load
$3ff0308c constant sha1_busy

: wait   begin sha1_busy @ 0= until ;
variable started
: chunk
  started @ if
    1 sha1_continue !
  else
    1 sha1_start ! 1 started !
  then
  wait
;

: >w { msg n }
  sha_text 64 ERASE
  msg sha_text n CMOVE
  $80 sha_text n + c!
  sha_text 64 <->* ;

40 constant sha1-size
create sha1-hash  sha1-size allot

: >dig ( a n -- a )
  BASE @ >R HEX <# # # # # # # # # #> R> BASE !
  ROT 2DUP + >R SWAP CMOVE R> ;
: format
  sha1-hash
  5 0 DO i cells sha_text + @ >dig LOOP
  DROP ;

: sha1 { msg n -- hash n } n 64 /mod { edge wholes }
  0 started !
  wholes 0 ?DO msg 64 >w chunk 64 +TO msg LOOP
  edge 0= IF
    0 0 >w
  ELSE
    msg edge >w
    edge 56 >= IF chunk sha_text 64 ERASE THEN
  THEN
  n 8 * 16 RSHIFT 16 RSHIFT 14 4* sha_text + L!
  n 8 * 15 4* sha_text + L!
  chunk
  1 sha1_load ! wait
  format sha1-hash sha1-size
;

forth definitions
