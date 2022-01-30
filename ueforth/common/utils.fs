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

( Words built after boot )

( For tests and asserts )
: assert ( f -- ) 0= throw ;

( Examine Memory )
: dump ( a n -- )
   cr 0 do i 16 mod 0= if cr then dup i + c@ . loop drop cr ;

( Remove from Dictionary )
: forget ( "name" ) ' dup >link current @ !  >name drop here - allot ;

internals definitions
2 constant SMUDGE
: mem= ( a a n -- f)
   for aft 2dup c@ swap c@ <> if 2drop rdrop 0 exit then 1+ swap 1+ then next 2drop -1 ;
forth definitions also internals
: :noname ( -- xt ) 0 , current @ @ , SMUDGE , here dup current @ ! ['] = @ , postpone ] ;
: str= ( a n a n -- f) >r swap r@ <> if rdrop 2drop 0 exit then r> mem= ;
: startswith? ( a n a n -- f ) >r swap r@ < if rdrop 2drop 0 exit then r> mem= ;
: .s   ." <" depth n. ." > " raw.s cr ;
only forth definitions

( Definitions building to SEE and ORDER )
internals definitions
: see. ( xt -- ) >name type space ;
: see-one ( xt -- xt+1 )
   dup cell+ swap @
   dup ['] DOLIT = if drop dup @ . cell+ exit then
   dup ['] DOFLIT = if drop dup sf@ <# [char] e hold #fs #> type space cell+ exit then
   dup ['] $@ = if drop ['] s" see.
                   dup @ dup >r >r dup cell+ r> type cell+ r> 1+ aligned +
                   [char] " emit space exit then
   dup  ['] BRANCH =
   over ['] 0BRANCH = or
   over ['] DONEXT = or
       if see. cell+ exit then
   see. ;
: exit= ( xt -- ) ['] exit = ;
: see-loop   >body begin dup @ exit= 0= while see-one repeat drop ;
: see-xt ( xt -- )
        dup @ ['] see-loop @ <>
        if ." Unsupported word type: " see. cr exit then
        ['] : see.  dup see.  space see-loop   ['] ; see. cr ;
: >name-length ( xt -- n ) dup 0= if exit then >name nip ;
: see-vocabulary ( voc ) @ begin dup >name-length while dup see-xt >link repeat drop cr ;
: >vocnext ( xt -- xt ) >body 2 cells + @ ;
: see-all
  last-vocabulary @ begin dup while
    ." VOCABULARY " dup see. cr ." ------------------------" cr
    dup >body see-vocabulary
    >vocnext
  repeat drop cr ;
: voclist   last-vocabulary @ begin dup while dup see. >vocnext repeat drop cr ;
: voc. ( voc -- ) 2 cells - see. ;
: vocs. ( voc -- ) dup voc. @ begin dup while
    dup >name-length 0= if ." >> " dup 2 cells - voc. then
    >link
  repeat drop cr ;
forth definitions also internals
: see   ' see-xt ;
: order   context begin dup @ while dup @ vocs. cell+ repeat drop ;
only forth definitions

( List words in Dictionary / Vocabulary )
internals definitions
75 value line-width
: onlines ( n xt -- n xt )
   swap dup line-width > if drop 0 cr then over >name nip + 1+ swap ;
forth definitions also internals
: vlist   0 context @ @ begin dup >name-length while onlines dup see. >link repeat 2drop cr ;
: words   0 context @ @ begin dup while onlines dup see. >link repeat 2drop cr ;
only forth definitions

( Extra Task Utils )
tasks definitions also internals
: .tasks   task-list @ begin dup 2 cells - see. @ dup task-list @ = until drop ;
only forth definitions
