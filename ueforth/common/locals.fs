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

( Local Variables )

( NOTE: These are not yet gforth compatible )

internals definitions

( Leave a region for locals definitions )
1024 constant locals-capacity  128 constant locals-gap
create locals-area locals-capacity allot
variable locals-here  locals-area locals-here !
: <>locals   locals-here @ here locals-here ! here - allot ;

variable scope-depth
: scope-clear
   begin scope-depth @ while postpone rdrop cell scope-depth +! repeat
   0 scope !   locals-area locals-here ! ;
: local@ ( n -- ) rp@ + @ ;
: do-local ( n -- ) nest-depth @ 1+ cells - aliteral ['] local@ , ;
: scope-create ( a n -- )
   dup >r $place align r> , ( name )
   scope @ , 1 , ( IMMEDIATE ) here scope ! ( link, flags )
   ['] scope-clear @ ( docol) ,
   scope-depth @ aliteral postpone do-local ['] exit ,
   cell negate scope-depth +!
;

: ?room   locals-here @ locals-area - locals-capacity locals-gap - >
          if scope-clear -1 throw then ;

( NOTE: This is not ANSForth compatible )
: (local) ( a n -- ) ?room <>locals scope-create <>locals postpone >r ;
: }? ( a n -- ) 1 <> if drop 0 exit then c@ [char] } = ;
: --? ( a n -- ) s" --" str= ;

also forth definitions

: {   begin bl parse
        dup 0= if scope-clear -1 throw then
        2dup --? if 2drop [char] } parse 2drop exit then
        2dup }? if 2drop exit then
      (local) again ; immediate
: ;   scope-clear postpone ; ; immediate

only forth definitions
