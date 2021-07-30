#! /usr/bin/env gforth

\ Copyright 2021 Bradley D. Nelson
\
\ Licensed under the Apache License, Version 2.0 (the "License");
\ you may not use this file except in compliance with the License.
\ You may obtain a copy of the License at
\
\     http://www.apache.org/licenses/LICENSE-2.0
\
\  Unless required by applicable law or agreed to in writing, software
\  distributed under the License is distributed on an "AS IS" BASIS,
\  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
\  See the License for the specific language governing permissions and
\  limitations under the License.

( CircleForth )
vocabulary circleforth   circleforth definitions

( Internal rstack )
create rstack 1000 cells allot   variable rp rstack rp !
: rp@ rp @ ;   : rp! rp ! ;  : r@ rp@ @ ;
: >r cell rp +! rp@ ! ;   : r> r@ -1 cells rp +! ;
( Internal IP & W )
variable ip   variable w
: run   0 >r begin ip @ @ cell ip +! dup w ! @ execute ip @ 0= until ;

variable last
( Create dictionary entry: { name-bytes name-len flags link code } )
: splace ( a n -- ) dup >r 0 do dup c@ c, 1+ loop drop r> , ;
: create-name ( a n -- ) splace 0 , last @ , here 0 , last ! ;
: code!   last @ ! ;
: p:   ' dup >name name>string create-name code! ;
: >p   create-name ' code! ;
( Access dictionary entry )
: >link ( xt -- a ) 1 cells - @ ;   : >flags 2 cells - ;
: >name ( xt -- a n ) dup 3 cells - @ swap over - 3 cells - swap ;
: or! ( n a -- ) dup @ rot or swap ! ;
: immediate   1 last @ >flags or! ;   : immediate? >flags @ 1 and 0<> ;

( Interpreter branching, calling, and literals )
: docreate: w @ cell+ cell+ ;
: dodoes: docreate: ip @ >r w @ cell+ @ ip ! ;
: docol: ip @ >r w @ cell+ ip ! ;
: dolit: ip @ @ cell ip +! ;
: branch ip @ @ ip ! ;
: 0branch if cell ip +! else ip @ @ ip ! then ;

( CREATE DOES> )
: create   parse-name create-name ['] docreate: code! 0 , ;
: does>   ['] dodoes: code! ip @ last @ cell+ ! r> ip ! ;

( Words that traverse the dictionary )
: find ( a n -- xt )
   last @ begin >r 2dup r@ >name str= if 2drop r> exit then
                r> >link dup 0= until drop 2drop 0 ;

( Literal handling )
p: dolit:   s" dolit:" find constant dolit:-xt
: aliteral dolit:-xt , , ;   p: aliteral
( Exit & Execute )
: 'exit r> ip ! ;   s" exit" >p 'exit   s" exit" find constant exit-xt
: execute ( xt -- ) >r exit-xt >r rp @ 1 cells - ip ! ;   p: execute
( Compiling words )
variable state
: colon parse-name create-name ['] docol: code! -1 state ! ;   s" :" >p colon
: semicolon exit-xt , 0 state ! ;   s" ;" >p semicolon immediate

( Pass thru primitives )
p: 0=   p: 0<   p: +   p: */mod   p: and   p: or   p: xor
p: dup   p: swap   p: over   p: drop   p: sp@   p: sp!
p: .   p: type   p: key
p: @   p: !   p: c@   p: c!
p: parse-name   p: parse   p: here   p: ,   p: allot
p: base   p: depth   p: cell
( Reimplemented primitives )
p: r@   p: >r   p: r>   p: rp@   p: rp!
p: branch   p: 0branch   p: find
p: immediate   p: create   p: does>
p: last   p: state

( Evaluate source )
: one-word dup immediate? 0= state @ and if , else execute run then ;
: one-number' state @ if aliteral then ;
: one-number s>number? 0= throw drop one-number' ;
: one-name 2dup find dup if nip nip one-word else drop one-number then ;
: prompt source-id 0= if ."  ok" cr then ;
: eval-line begin parse-name dup if one-name else 2drop exit then again ;
: boot begin ['] eval-line catch if ." ERROR" cr then prompt refill drop again ;
: include parse-name slurp-file ['] eval-line execute-parsing ;   p: include
: ok ." CircleForth" cr ."   ok" cr query ;   p: ok   : bye cr bye ;   p: bye

( Bootstrap )
boot
: (   41 parse drop drop ; immediate
: \   10 parse drop drop ; immediate
( And now we have comments! )
include compound.fs
ok
