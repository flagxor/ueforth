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

( Print spaces )
: spaces ( n -- ) for aft space then next ;

internals definitions

( Temporary for platforms without CALLCODE )
DEFINED? CALLCODE 0= [IF]
  create CALLCODE
[THEN]

( Safe memory access, i.e. aligned )
cell 1- constant cell-mask
: cell-base ( a -- a ) cell-mask invert and ;
: cell-shift ( a -- a ) cell-mask and 8 * ;
: ca@ ( a -- n ) dup cell-base @ swap cell-shift rshift 255 and ;

( Print address line leaving room )
: dump-line ( a -- a ) cr <# #s #> 20 over - >r type r> spaces ;

( Semi-dangerous word to trim down the system heap )
DEFINED? realloc [IF]
: relinquish ( n -- ) negate 'heap-size +! 'heap-start @ 'heap-size @ realloc drop ;
[THEN]

forth definitions internals

( Examine Memory )
: dump ( a n -- )
   over 15 and if over dump-line over 15 and 3 * spaces then
   for aft
     dup 15 and 0= if dup dump-line then
     dup ca@ <# # #s #> type space 1+
   then next drop cr ;

( Remove from Dictionary )
: forget ( "name" ) ' dup >link current @ !  >name drop here - allot ;

internals definitions
1 constant IMMEDIATE_MARK
2 constant SMUDGE
4 constant BUILTIN_FORK
16 constant NONAMED
32 constant +TAB
64 constant -TAB
128 constant ARGS_MARK
: mem= ( a a n -- f)
   for aft 2dup c@ swap c@ <> if 2drop rdrop 0 exit then 1+ swap 1+ then next 2drop -1 ;
forth definitions also internals
: :noname ( -- xt ) 0 , current @ @ , NONAMED SMUDGE or ,
                    here dup current @ ! ['] mem= @ , postpone ] ;
: str= ( a n a n -- f) >r swap r@ <> if rdrop 2drop 0 exit then r> mem= ;
: startswith? ( a n a n -- f ) >r swap r@ < if rdrop 2drop 0 exit then r> mem= ;
: .s   ." <" depth n. ." > " raw.s cr ;
only forth definitions

( Tweak indent on branches )
internals internalized definitions

: flags'or! ( n -- ) ' >flags& dup >r c@ or r> c! ;
+TAB flags'or! BEGIN
-TAB flags'or! AGAIN
-TAB flags'or! UNTIL
+TAB flags'or! AHEAD
-TAB flags'or! THEN
+TAB flags'or! IF
+TAB -TAB or flags'or! ELSE
+TAB -TAB or flags'or! WHILE
-TAB flags'or! REPEAT
+TAB flags'or! AFT
+TAB flags'or! FOR
-TAB flags'or! NEXT
+TAB flags'or! DO
ARGS_MARK +TAB or flags'or! ?DO
ARGS_MARK -TAB or flags'or! +LOOP
ARGS_MARK -TAB or flags'or! LOOP
ARGS_MARK flags'or! LEAVE

forth definitions 

( Definitions building to SEE and ORDER )
internals definitions
variable indent
: see. ( xt -- ) >name type space ;
: icr   cr indent @ 0 max 4* spaces ;
: indent+! ( n -- ) indent +! icr ;
: see-one ( xt -- xt+1 )
   dup cell+ swap @
   dup ['] DOLIT = if drop dup @ . cell+ exit then
   dup ['] DOSET = if drop ." TO " dup @ cell - see. cell+ icr exit then
   dup ['] DOFLIT = if drop dup sf@ <# [char] e hold #fs #> type space cell+ exit then
   dup ['] $@ = if drop ['] s" see.
                   dup @ dup >r >r dup cell+ r> type cell+ r> 1+ aligned +
                   [char] " emit space exit then
   dup ['] DOES> = if icr then
   dup >flags -TAB AND if -1 indent+! then
   dup see.
   dup >flags +TAB AND if
     1 indent+!
   else
     dup >flags -TAB AND if icr then
   then
   dup ['] ! = if icr then
   dup ['] +! = if icr then
   dup  @ ['] BRANCH @ =
   over @ ['] 0BRANCH @ = or
   over @ ['] DONEXT @ = or
   over >flags ARGS_MARK and or
       if swap cell+ swap then
   drop
;
: see-loop   dup >body swap >params 1- cells over +
             begin 2dup < while swap see-one swap repeat 2drop ;
: ?see-flags   >flags IMMEDIATE_MARK and if ." IMMEDIATE " then ;
: see-xt ( xt -- )
  dup @ ['] see-loop @ = if
    ['] : see.  dup see.
    1 indent ! icr
    dup see-loop
    -1 indent+! ['] ; see.
    ?see-flags cr
    exit
  then
  dup >flags BUILTIN_FORK and if ." Built-in-fork: " see. exit then
  dup @ ['] input-buffer @ = if ." CREATE/VARIABLE: " see. cr exit then
  dup @ ['] SMUDGE @ = if ." DOES>/CONSTANT: " see. cr exit then
  dup @ ['] callcode @ = if ." Code: " see. cr exit then
  dup >params 0= if ." Built-in: " see. cr exit then
  ." Unsupported: " see. cr ;

: nonvoc? ( xt -- f )
  dup 0= if exit then dup >name nip swap >flags NONAMED BUILTIN_FORK or and or ;
: see-vocabulary ( voc )
  @ begin dup nonvoc? while dup see-xt >link repeat drop cr ;
: >vocnext ( xt -- xt ) >body 2 cells + @ ;
: see-all
  last-vocabulary @ begin dup while
    ." VOCABULARY " dup see. cr ." ------------------------" cr
    dup >body see-vocabulary
    >vocnext
  repeat drop cr ;
: voclist-from ( voc -- ) begin dup while dup see. cr >vocnext repeat drop ;
: voclist   last-vocabulary @ voclist-from ;
: voc. ( voc -- ) 2 cells - see. ;
: vocs. ( voc -- ) dup voc. @ begin dup while
    dup nonvoc? 0= if ." >> " dup 2 cells - voc. then
    >link
  repeat drop cr ;

( Words to measure size of things )
: size-vocabulary ( voc )
  @ begin dup nonvoc? while
    dup >params . dup >size . dup . dup see. cr >link
  repeat drop ;
: size-all
  last-vocabulary @ begin dup while
    0 . 0 . 0 . dup see. cr
    dup >body size-vocabulary
    >vocnext
  repeat drop cr ;

forth definitions also internals
: see   ' see-xt ;
: order   context begin dup @ while dup @ vocs. cell+ repeat drop ;
only forth definitions

( List words in Dictionary / Vocabulary )
internals definitions
70 value line-width
0 value line-pos
: onlines ( xt -- xt )
   line-pos line-width > if cr 0 to line-pos then
   dup >name nip 1+ line-pos + to line-pos ;
: vins. ( voc -- )
  >r 'builtins begin dup >link while
    dup >params r@ = if dup onlines see. then
    3 cells +
  repeat drop rdrop ;
: ins. ( n xt -- n ) cell+ @ vins. ;
: ?ins. ( xt -- xt ) dup >flags BUILTIN_FORK and if dup ins. then ;
forth definitions also internals
: vlist   0 to line-pos context @ @
          begin dup nonvoc? while ?ins. dup onlines see. >link repeat drop cr ;
: words   0 to line-pos context @ @
          begin dup while ?ins. dup onlines see. >link repeat drop cr ;
only forth definitions
