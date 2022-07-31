\ Copyright 2021 Bradley D. Nelson
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

( Stack Baseline )
sp@ constant sp0
rp@ constant rp0
fp@ constant fp0
: depth ( -- n ) sp@ sp0 - cell/ ;
: fdepth ( -- n ) fp@ fp0 - 4 / ;

( Useful heap size words )
: remaining ( -- n ) 'heap-start @ 'heap-size @ + 'heap @ - ;
: used ( -- n ) 'heap @ sp@ 'stack-cells @ cells + - 28 + ;

( Quoting Words )
: ' bl parse 2dup find dup >r -rot r> 0= 'notfound @ execute 2drop ;
: ['] ' aliteral ; immediate
: char bl parse drop c@ ;
: [char] char aliteral ; immediate

( Core Control Flow )
create BEGIN ' nop @ ' begin !        : begin   ['] begin , here ; immediate
create AGAIN ' branch @ ' again !     : again   ['] again , , ; immediate
create UNTIL ' 0branch @ ' until !    : until   ['] until , , ; immediate
create AHEAD ' branch @ ' ahead !     : ahead   ['] ahead , here 0 , ; immediate
create THEN ' nop @ ' then !          : then   ['] then , here swap ! ; immediate
create IF ' 0branch @ ' if !          : if   ['] if , here 0 , ; immediate
create ELSE ' branch @ ' else !       : else   ['] else , here 0 , swap here swap ! ; immediate
create WHILE ' 0branch @ ' while !    : while   ['] while , here 0 , swap ; immediate
create REPEAT ' branch @ ' repeat !   : repeat   ['] repeat , , here swap ! ; immediate
create AFT ' branch @ ' aft !         : aft   drop ['] aft , here 0 , here swap ; immediate

( Recursion )
: recurse   current @ @ aliteral ['] execute , ; immediate

( Postpone - done here so we have ['] and IF )
: immediate? ( xt -- f ) >flags 1 and 0= 0= ;
: postpone ' dup immediate? if , else aliteral ['] , , then ; immediate

( Rstack nest depth )
variable nest-depth

( FOR..NEXT )
create FOR ' >r @ ' for !         : for   1 nest-depth +! ['] for , here ; immediate
create NEXT ' donext @ ' next !   : next   -1 nest-depth +! ['] next , , ; immediate

( DO..LOOP )
variable leaving
: leaving,   here leaving @ , leaving ! ;
: leaving(   leaving @ 0 leaving !   2 nest-depth +! ;
: )leaving   leaving @ swap leaving !  -2 nest-depth +!
             begin dup while dup @ swap here swap ! repeat drop ;
: DO ( n n -- .. ) swap r> -rot >r >r >r ;
: do ( lim s -- ) leaving( postpone DO here ; immediate
: ?DO ( n n -- n n f .. )
   2dup = if 2drop r> @ >r else swap r> cell+ -rot >r >r >r then ;
: ?do ( lim s -- ) leaving( postpone ?DO leaving, here ; immediate
: UNLOOP   r> rdrop rdrop >r ;
: LEAVE   r> rdrop rdrop @ >r ;
: leave   postpone LEAVE leaving, ; immediate
: +LOOP ( n -- ) dup 0< swap r> r> rot + dup r@ < -rot >r >r xor 0=
                 if r> cell+ rdrop rdrop >r else r> @ >r then ;
: +loop ( n -- ) postpone +LOOP , )leaving ; immediate
: LOOP   r> r> 1+ dup r@ < -rot >r >r 0=
         if r> cell+ rdrop rdrop >r else r> @ >r then ;
: loop   postpone LOOP , )leaving ; immediate
create I ' r@ @ ' i !  ( i is same as r@ )
: J ( -- n ) rp@ 3 cells - @ ;
: K ( -- n ) rp@ 5 cells - @ ;

( Exceptions )
variable handler
: catch ( xt -- n )
  fp@ >r sp@ >r handler @ >r rp@ handler ! execute
  r> handler ! rdrop rdrop 0 ;
: throw ( n -- )
  dup if handler @ rp! r> handler !
         r> swap >r sp! drop r> r> fp! else drop then ;
' throw 'notfound !

( Values )
: value ( n -- ) constant ;
: value-bind ( xt-val xt )
   >r >body state @ if
     r@ ['] ! = if rdrop ['] doset , , else aliteral r> , then
   else r> execute then ;
: to ( n -- ) ' ['] ! value-bind ; immediate
: +to ( n -- ) ' ['] +! value-bind ; immediate

( Deferred Words )
: defer ( "name" -- ) create 0 , does> @ dup 0= throw execute ;
: is ( xt "name -- ) postpone to ; immediate
