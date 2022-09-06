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

( Lazy loaded visual editor. )

: visual r|

also DEFINED? termios [IF] termios [THEN]
also internals
also ansi
also forth
current @
vocabulary visual  visual definitions
vocabulary insides  insides definitions

256 constant max-path
create filename max-path allot 0 value filename#
0 value fileh

10 constant start-size
start-size allocate throw value text
start-size value capacity
0 value length
0 value caret

: up ( n -- n ) begin dup 0 > over text + c@ nl <> and while 1- repeat 1- 0 max ;
: nup ( n -- n ) 10 for up next ;
: down ( n -- n ) begin dup length < over text + c@ nl <> and while 1+ repeat 1+ length min ;
: ndown ( n -- n ) 10 for down next ;

: update
    caret nup dup 0<> if 1+ 1+ then { before }
    before ndown ndown { after }
    page
    text before + caret before - crtype
    caret length < text caret + c@ nl <> and if
      1 bg text caret + c@ emit normal
      text caret + 1+ after caret - 1- 0 max crtype
    else
      1 bg space normal
      text caret + after caret - crtype
    then normal
;

: insert ( ch -- )
  length capacity = if text capacity 1+ 2* >r r@ 1+ resize throw to text r> to capacity then
  text caret + dup 1+ length caret - cmove>
  text caret + c!
  1 +to caret
  1 +to length
  update
;

: handle-esc
    key
    dup [char] [ = if drop
       key
       dup [char] A = if drop caret up to caret update exit then
       dup [char] B = if drop caret down to caret update exit then
       dup [char] C = if drop caret 1+ length min to caret update exit then
       dup [char] D = if drop caret 1- 0 max to caret update exit then
       dup [char] 5 = if drop key drop caret 8 for up next to caret update exit then
       dup [char] 6 = if drop key drop caret 8 for down next to caret update exit then
       drop
       exit
    then
    drop
;

: delete
    length caret > if
      text caret + dup 1+ swap length caret - 1- 0 max cmove
      -1 +to length
      update
    then
;

: backspace
    caret 0 > if
        -1 +to caret
        delete
    then
;

: load ( a n -- )
     0 to caret
     dup to filename#
     filename swap cmove
     filename filename# r/o open-file 0= if
         to fileh
         fileh file-size throw to capacity
         text capacity 1+ resize throw to text
         capacity to length
         text length fileh read-file throw drop
         fileh close-file throw
     else
         drop
         0 to capacity
         0 to length
     then
;

: save
     filename filename# w/o create-file throw to fileh
     text length fileh write-file throw
     fileh close-file throw
;

: quit-edit
     page filename filename# type cr ." SAVE? "
     begin
         key 95 and
         dup [char] Y = if drop save 123 throw then
         dup [char] N = if drop 123 throw then
         drop
     again
;

: handle-key ( ch -- )
    dup 27 = if drop handle-esc exit then
    dup [char] D [char] @ - = if delete exit then
    dup [char] H [char] @ - = over 127 = or if drop backspace exit then
    dup [char] L [char] @ - = if drop update exit then
    dup [char] S [char] @ - = if drop save update exit then
    dup [char] X [char] @ - = if drop quit-edit then
    dup [char] Q [char] @ - = if drop quit-edit then
    dup 13 = if drop nl insert exit then
    dup bl >= if insert else drop then
;

: ground   depth 0<> throw ;
: step   *key handle-key ground ;

DEFINED? raw-mode 0= [IF]
    : raw-mode ;
    : normal-mode ;
[THEN]

: run
    raw-mode update
    begin
        ['] step catch
        dup 123 = if drop normal-mode page exit then
        if ." FAILURE!" then
    again
;

visual definitions insides

: edit ( <filename> ) bl parse load run ;

previous previous previous previous current ! visual
| evaluate ;
