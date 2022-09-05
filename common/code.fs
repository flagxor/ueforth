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

( Lazy loaded code words )
: asm r|

also forth definitions
vocabulary asm
internals definitions

: ca! ( n a -- ) dup cell-base >r cell-shift swap over lshift
                 swap 255 swap lshift invert r@ @ and or r> ! ;

also asm definitions

variable code-start
variable code-at

DEFINED? posix [IF]
also posix
: reserve ( n -- )
  0 swap PROT_READ PROT_WRITE PROT_EXEC or or
  MAP_ANONYMOUS MAP_PRIVATE or -1 0 mmap code-start ! ;
previous
4096 reserve
[THEN]

DEFINED? esp [IF]
also esp
: reserve ( n -- ) MALLOC_CAP_EXEC heap_caps_malloc code-start ! ;
previous
1024 reserve
[THEN]

code-start @ code-at !

: chere ( -- a ) code-at @ ;
: callot ( n -- ) code-at +! ;
: code1, ( n -- ) chere ca! 1 callot ;
: code2, ( n -- ) dup code1, 8 rshift code1, ;
: code3, ( n -- ) dup code2, 16 rshift code1, ;
: code4, ( n -- ) dup code2, 16 rshift code2, ;
cell 8 = [IF]
: code,  dup code4, 32 rshift code4, ;
[ELSE]
: code,  code4, ;
[THEN]
: end-code   previous ;

also forth definitions

: code ( "name" ) create ['] callcode @ latestxt !
                  code-at @ latestxt cell+ ! also asm ;

previous previous previous
asm

| evaluate ;
