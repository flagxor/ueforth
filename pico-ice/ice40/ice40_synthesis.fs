\ Copyright 2025 Bradley D. Nelson
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

needs ice40_layout.fs
needs ice40_allocation.fs

ice40 synthesis definitions

39 constant LED_G
40 constant LED_B
41 constant LED_R

: XOR1 ( a b -- o ) NotConnected NotConnected $6666 LUT4 ;
: OR1 ( a b -- o ) NotConnected NotConnected $eeee LUT4 ;
: AND1 ( a b -- o ) NotConnected NotConnected $8888 LUT4 ;
: INVERT1 ( a b -- o ) NotConnected NotConnected NotConnected $5555 LUT4 ;
: BUFFER1 ( a b -- o ) NotConnected NotConnected NotConnected $aaaa LUT4 ;

: HA ( x y -- so co ) 2dup AND1 >r XOR1 r> ;
: FA ( x y z -- so co ) HA >r HA r> OR1 ;

: BUS ( v bus -- bus ) here >r , , r> ;
: UNBUS ( bus -- v bus ) dup cell+ @ swap @ ;
: nBUS ( v* n -- bus ) 0 swap 0 ?do BUS loop ;
: nUNBUS ( bus -- v* ) begin dup while UNBUS repeat drop ;

: NEARBY { wire -- wire } wire .getXY dup 2 < if 1+ else 1- then
                          2dup CramCell .create .isRam? if >r 1+ r> then
                          PLACE wire ;

: IN1PIN ( n -- bus ) pin .makeInput NEARBY BUFFER1 ;
: OUT1PIN! ( wire n -- ) pin .makeOutput NEARBY >r BUFFER1 r> route! ;

: INPINS ( p1..pn n -- wire ) 0 swap 0 ?do swap IN1PIN swap BUS loop ;
: OUTPINS! ( bus p1..pn n -- )
   nBUS begin dup while
     UNBUS >r swap UNBUS >r swap OUT1PIN! r> r>
   repeat 2drop ;

: REVERSE ( a -- a' )
  0 swap begin dup while
    UNBUS >r swap BUS r>
  repeat drop
;

: INVERT ( a -- a' )
   dup 0= if exit then
   UNBUS >r INVERT1 r> recurse BUS
;

: BUFFER ( a -- a' )
   dup 0= if exit then
   UNBUS >r BUFFER1 r> recurse BUS
;

: AND ( a b -- c )
   dup 0= if nip exit then
   UNBUS >r >r UNBUS r> swap >r AND1
   r> r> recurse BUS
;

: OR ( a b -- c )
   dup 0= if nip exit then
   UNBUS >r >r UNBUS r> swap >r OR1
   r> r> recurse BUS
;

: XOR ( a b -- c )
   dup 0= if nip exit then
   UNBUS >r >r UNBUS r> swap >r XOR1
   r> r> recurse BUS
;

: +c ( a b ci -- c )
   >r dup 0= if 2drop r> 0 BUS exit then r>
   -rot
   UNBUS >r >r UNBUS r> swap >r FA
   r> r> rot recurse BUS
;

: + ( a b -- c ) NotConnected +c ;

: REGISTER ( n -- bus )
   0 swap 0 ?do FFL swap BUS loop REVERSE ;

: REG! ( v a -- )
   dup 0= if 2drop exit then
   UNBUS >r >r UNBUS r> swap >r FF!
   r> r> recurse
;

forth definitions

