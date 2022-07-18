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

( Words in this file are typically implemented as opcodes in extra_opcodes.h )

( Useful Basic Compound Words )
: nip ( a b -- b ) swap drop ;
: rdrop ( r: n n -- ) r> r> drop >r ;
: */ ( n n n -- n ) */mod nip ;
: * ( n n -- n ) 1 */ ;
: /mod ( n n -- n n ) 1 swap */mod ;
: / ( n n -- n ) /mod nip ;
: mod ( n n -- n ) /mod drop ;
: invert ( n -- ~n ) -1 xor ;
: negate ( n -- -n ) invert 1 + ;
: - ( n n -- n ) negate + ;
: rot ( a b c -- c a b ) >r swap r> swap ;
: -rot ( a b c -- b c a ) swap >r swap r> ;
: < ( a b -- a<b ) - 0< ;
: > ( a b -- a>b ) swap - 0< ;
: <= ( a b -- a>b ) swap - 0< 0= ;
: >= ( a b -- a<b ) - 0< 0= ;
: = ( a b -- a!=b ) - 0= ;
: <> ( a b -- a!=b ) = 0= ;
: 0<> ( n -- n) 0= 0= ;
: bl 32 ;   : nl 10 ;
: 1+ 1 + ;   : 1- 1 - ;
: 2* 2 * ;   : 2/ 2 / ;
: 4* 4 * ;   : 4/ 4 / ;
: +! ( n a -- ) swap over @ + swap ! ;

( Cells )
: cell+ ( n -- n ) cell + ;
: cells ( n -- n ) cell * ;
: cell/ ( n -- n ) cell / ;

( Double Words )
: 2drop ( n n -- ) drop drop ;
: 2dup ( a b -- a b a b ) over over ;
: 2@ ( a -- lo hi ) dup @ swap cell+ @ ;
: 2! ( lo hi a -- ) dup >r cell+ ! r> ! ;

( Heap Words )
: here ( -- a ) 'sys @ ;
: allot ( n -- ) 'sys +! ;
: , ( n --  ) here ! cell allot ;
: c, ( ch -- ) here c! 1 allot ;

( System Variables )
: sys: ( a -- a' "name" ) dup constant cell+ ;
'sys   sys: 'heap         sys: current       sys: 'context
       sys: 'latestxt     sys: 'notfound
       sys: 'heap-start   sys: 'heap-size    sys: 'stack-cells
       sys: 'boot         sys: 'boot-size
       sys: 'tib          sys: #tib          sys: >in
       sys: state         sys: base
       sys: 'argc         sys: 'argv         sys: 'runner
drop
: context ( -- a ) 'context @ cell+ ;
: latestxt ( -- xt ) 'latestxt @ ;

( Compilation State )
: [ 0 state ! ; immediate
: ] -1 state ! ; immediate

: literal aliteral ; immediate
