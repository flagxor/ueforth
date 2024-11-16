\ Copyright 2024 Bradley D. Nelson
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

vocabulary arrays also internals also arrays definitions

128 constant stack-depth

( Stack for arrays )
create astack    stack-depth cells allot
variable ap    astack ap !
: apush ( a -- ) cell ap +! ap @ ! ;
: apop ( -- a ) ap @ @  cell negate ap +! ;
: top ( -- a ) ap @ @ ;
: under ( -- a ) ap @ cell - @ ;

( Secondary stack for arrays )
create arstack   stack-depth cells allot
variable arp   arstack arp !
: >a   apop   cell arp +! arp @ ! ;
: a>   arp @ @  cell negate arp +! apush ;

( Array types )
0 constant MIXED
1 constant STRING
2 constant INTEGER
3 constant REAL
create array-sizes   cell , 1 , cell , sfloat ,
: >esize ( type -- n ) cells array-sizes + @ ;

( ref n type ^data... )
3 cells constant header-size
: >type ( a -- a ) -1 cells + ;
: >count ( a -- a ) -2 cells + ;
: >ref ( a -- a ) -3 cells + ;

( Size of array data in bytes )
: bytes ( a -- n ) dup >type @ >esize swap >count @ * ;
( To string / array )
: range ( a -- a n ) dup >count @ ;

( Create an uninitialized array )
: array ( n type -- a: a )
   2dup >esize * header-size + allocate throw header-size + apush
   top >type !   top >count !   0 top >ref ! ;

( Reference counting for arrays )
: ref ( a -- ) 1 over >ref +! ;
: unref ( a -- )
   dup 0= if drop exit then
   -1 over >ref +!
   dup >ref @ 0< if
     dup >type @ MIXED = if
       dup dup >count @ 0 ?do
         dup @ recurse cell+
       loop
       drop 
     then
     header-size - free throw exit
   then drop ;

( Stack manipulation )
: adrop ( a: a -- ) apop unref ;
: a2drop ( a: a a -- ) adrop adrop ;
: anip ( a: a b -- b ) apop apop unref apush ;
: adup ( a: a -- a a ) top ref apush ;
: aswap ( a: a b -- b a ) apop apop swap apush apush ;
: aover ( a: a b -- a b a ) apop apop ref dup apush swap apush apush ;
: a2dup ( a: a b -- a b a b ) aover aover ;

( Index into the top of the stack )
: a@ ( n a: a -- a: a ) cells top + @ ref adrop apush ;

( Raw array creation words )
: empty ( -- a: a ) 0 MIXED array ;
: box ( a: a -- a ) apop 1 MIXED array top ! ;
: _s ( a n -- a: a ) dup STRING array top swap cmove ;
: _c ( ch -- a: a ) 1 STRING array top c! ;
: _i ( n -- a: a ) 1 INTEGER array top ! ;
: _f ( f: n -- a: a ) 1 REAL array top sf! ;
: _s"   postpone s" state @ if postpone _s else _s then ; immediate

: aconstant   create apop , does> @ ref apush ;

( Convert integer array to floats )
: n>f
   top >count @ REAL array
   under top range 0 ?do over @ s>f dup sf! sfloat+ >r cell+ r> loop 2drop anip ;

( Force integers to real. )
: binuminal
   top >type @ INTEGER = under >type @ REAL = and if n>f then
   under >type @ INTEGER = top >type @ REAL = and if apop n>f apush then
;

0 value layer
: lst ( a -- )
   layer spaces
   dup >type @ case
     MIXED of
       ." [" cr
       2 +to layer
       dup >count @ 0 ?do
         dup @ recurse cell+ cr
       loop
       drop
       -2 +to layer
       layer spaces ." ]"
     endof
     STRING of dup >count @ type endof
     INTEGER of dup >count @ 0 ?do dup @ . cell+ loop drop endof
     REAL of dup >count @ 0 ?do dup sf@ f. sfloat+ loop drop endof
   endcase
;
: a. ( a -- ) top lst adrop ;

: catenate ( a: a a -- a ) ( catenate )
  binuminal
  top >type @ under >type @ = if
    under >count @ top >count @ + top >type @ array apop >r
    under r@ under bytes cmove
    top r@ under bytes + top bytes cmove
    under under bytes 0 fill
    top top bytes 0 fill
    r> apush anip anip
    exit
  then
  top >type @ MIXED = if apop box apush recurse exit then
  under >type @ MIXED = if box recurse exit then
  apop apop 2 MIXED array top cell+ ! top !
;
: ,c   catenate ;

( Building arrays on the stack. )
: [[   ap @ ;
: ]]   ap @ swap - cell/ empty for aft aswap box aswap ,c then next ;

previous previous forth definitions
