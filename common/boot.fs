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

( Tools to build postpone later out of recognizers )
: immediate? ( xt -- f ) >flags 1 and 0= 0= ;
: postpone, ( xt -- ) aliteral ['] , , ;

( Rstack nest depth )
variable nest-depth

( FOR..NEXT )
create FOR ' >r @ ' for !         : for   1 nest-depth +! ['] for , here ; immediate
create NEXT ' donext @ ' next !   : next   -1 nest-depth +! ['] next , , ; immediate

( Define a data type for Recognizers. )
: RECTYPE: ( xt1 xt2 xt3 "name" -- ) CREATE , , , ;
: do-notfound ( a n -- ) -1 'notfound @ execute ;
' do-notfound ' do-notfound ' do-notfound  RECTYPE: RECTYPE-NONE
' execute     ' ,           ' postpone,    RECTYPE: RECTYPE-WORD
' execute     ' execute     ' ,            RECTYPE: RECTYPE-IMM
' drop        ' execute     ' execute      RECTYPE: RECTYPE-NUM

: RECOGNIZE ( c-addr len addr1 -- i*x addr2 )
  dup @ for aft
    cell+ 3dup >r >r >r @ execute
    dup RECTYPE-NONE <> if rdrop rdrop rdrop rdrop exit then
    drop r> r> r>
  then next
  drop RECTYPE-NONE
;

( Define a recognizer stack. )
create RECSTACK 0 , 10 cells allot
: +RECOGNIZER ( xt -- ) 1 RECSTACK +! RECSTACK dup @ cells + ! ;
: -RECOGNIZER ( -- ) -1 RECSTACK +! ;
: GET-RECOGNIZERS ( -- xtn..xt1 n )
   RECSTACK @ for RECSTACK r@ cells + @ next ;
: SET-RECOGNIZERS ( xtn..xt1 n -- )
   0 RECSTACK ! for aft +RECOGNIZER then next ;

( Create recognizer based words. )
: postpone ( "name" -- ) bl parse RECSTACK RECOGNIZE @ execute ; immediate
: +evaluate1
  bl parse dup 0= if 2drop exit then
  RECSTACK RECOGNIZE state @ 1+ 1+ cells + @ execute
;

( Setup recognizing words. )
: REC-FIND ( c-addr len -- xt addr1 | addr2 )
  find dup if
    dup immediate? if RECTYPE-IMM else RECTYPE-WORD then
  else
    drop RECTYPE-NONE
  then
;
' REC-FIND +RECOGNIZER

( Setup recognizing integers. )
: REC-NUM ( c-addr len -- n addr1 | addr2 )
  s>number? if
    ['] aliteral RECTYPE-NUM
  else
    RECTYPE-NONE
  then
;
' REC-NUM +RECOGNIZER

: interpret0 begin +evaluate1 again ; interpret0

( Useful stack/heap words )
: depth ( -- n ) sp@ sp0 - cell/ ;
: fdepth ( -- n ) fp@ fp0 - 4 / ;
: remaining ( -- n ) 'heap-start @ 'heap-size @ + 'heap @ - ;
: used ( -- n ) 'heap @ sp@ 'stack-cells @ cells + - 28 + ;

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
: +LOOP ( n -- ) r> r> dup r@ - >r rot + r> -rot
                       dup r@ - -rot >r >r xor 0<
                 if r> cell+ rdrop rdrop >r else r> @ >r then ;
: +loop ( n -- ) postpone +LOOP , )leaving ; immediate
: LOOP   r> r> dup r@ - >r 1+ r> -rot
               dup r@ - -rot >r >r xor 0<
         if r> cell+ rdrop rdrop >r else r> @ >r then ;
: loop   postpone LOOP , )leaving ; immediate
create I ' r@ @ ' i !  ( i is same as r@ )
: J ( -- n ) rp@ 3 cells - @ ;
: K ( -- n ) rp@ 5 cells - @ ;

( Exceptions )
variable handler
handler 'throw-handler !
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
