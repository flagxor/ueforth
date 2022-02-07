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

( Fill, Move )
: cmove ( a a n -- ) for aft >r dup c@ r@ c! 1+ r> 1+ then next 2drop ;
: cmove> ( a a n -- ) for aft 2dup swap r@ + c@ swap r@ + c! then next 2drop ;
: fill ( a n ch -- ) swap for swap aft 2dup c! 1 + then next 2drop ;
: erase ( a n -- ) 0 fill ;   : blank ( a n -- ) bl fill ;

( Compound words requiring conditionals )
: min 2dup < if drop else nip then ;
: max 2dup < if nip else drop then ;
: abs ( n -- +n ) dup 0< if negate then ;

( Dictionary )
: here ( -- a ) 'sys @ ;
: allot ( n -- ) 'sys +! ;
: aligned ( a -- a ) cell 1 - dup >r + r> invert and ;
: align   here aligned here - allot ;
: , ( n --  ) here ! cell allot ;
: c, ( ch -- ) here c! 1 allot ;

( Dictionary Format )
: >flags& ( xt -- a ) cell - ; : >flags ( xt -- flags ) >flags& c@ ;
: >name-length ( xt -- n ) >flags& 1+ c@ ;
: >params ( xt -- n ) >flags& 2 + sw@ $ffff and ;
: >size ( xt -- n ) dup >params cells swap >name-length aligned + 3 cells + ;
: >link& ( xt -- a ) 2 cells - ;   : >link ( xt -- a ) >link& @ ;
: >name ( xt -- a n ) dup >name-length swap >link& over aligned - swap ;
: >body ( xt -- a ) dup @ [ ' >flags @ ] literal = 2 + cells + ;

( System Variables )
: sys: ( a -- a' "name" ) dup constant cell+ ;
'sys   sys: 'heap         sys: current       sys: 'context
       sys: 'latestxt     sys: 'notfound
       sys: 'heap-start   sys: 'heap-size    sys: 'stack-cells
       sys: 'boot         sys: 'boot-size
       sys: 'tib          sys: #tib          sys: >in
       sys: state         sys: base
       sys: 'argc         sys: 'argv         sys: 'runner
: context ( -- a ) 'context @ cell+ ;
: latestxt ( -- xt ) 'latestxt @ ;

: f= ( r r -- f ) f- f0= ;
: f< ( r r -- f ) f- f0< ;
: f> ( r r -- f ) fswap f< ;
: f<> ( r r -- f ) f= 0= ;
: f<= ( r r -- f ) f> 0= ;
: f>= ( r r -- f ) f< 0= ;

4 constant sfloat
: sfloats ( n -- n*4 ) sfloat * ;
: sfloat+ ( a -- a ) sfloat + ;

3.14159265359e fconstant pi

: fsqrt ( r -- r ) 1e 20 0 do fover fover f/ f+ 0.5e f* loop fnip ;

