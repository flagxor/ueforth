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

: dump-file ( a n a n -- )
  w/o create-file throw
  >r r@ write-file throw
  r> close-file drop
;

: cp ( "src" "dst" -- )
  bl parse r/o bin open-file throw { inf }
  bl parse w/o bin create-file throw { outf }
  begin
    here 80 inf read-file throw
    dup 0= if drop outf close-file throw inf close-file throw exit then
    here swap outf write-file throw
  again
;

: mv ( "src" "dst" -- ) bl parse bl parse rename-file throw ;
: rm ( "path" -- ) bl parse delete-file throw ;

: touch ( "path" -- )
  bl parse 2dup w/o open-file
  if drop w/o create-file throw then
  close-file throw
;

internals definitions

: cremit ( ch -- ) dup nl = if drop cr else emit then ;
: crtype ( a n -- ) for aft dup c@ cremit 1+ then next drop ;

forth definitions internals

: cat ( "path" -- )
  bl parse r/o bin open-file throw { fh }
  begin
    here 80 fh read-file throw
    dup 0= if drop fh close-file throw exit then
    here swap crtype
  again
;

DEFINED? read-dir [IF]
: ls ( "path" -- )
  bl parse dup 0= if 2drop s" ." then
  open-dir throw { dh } begin
    dh read-dir dup 0= if
      2drop dh close-dir throw exit
    then type cr
  again
;
[THEN]

internals definitions
( Leave some room for growth of starting system. )
0 value saving-base
: park-heap ( -- a ) saving-base ;
: park-forth ( -- a ) saving-base cell+ ;
: 'cold ( -- a ) saving-base 2 cells + ;
: setup-saving-base
  here to saving-base  16 cells allot  0 'cold ! ;

' forth >body constant forth-wordlist

: save-name
  'heap @ park-heap !
  forth-wordlist @ park-forth !
  w/o create-file throw >r
  saving-base here over - r@ write-file throw
  r> close-file throw ;

: restore-name ( "name" -- )
  r/o open-file throw >r
  saving-base r@ file-size throw r@ read-file throw drop
  r> close-file throw
  park-heap @ 'heap !
  park-forth @ forth-wordlist !
  'cold @ dup if execute else drop then ;

defer remember-filename
: default-remember-filename   s" myforth" ;
' default-remember-filename is remember-filename

forth definitions also internals

: save ( "name" -- ) bl parse save-name ;
: restore ( "name" -- ) bl parse restore-name ;
: remember   remember-filename save-name ;
: startup: ( "name" ) ' 'cold ! remember ;
: revive   remember-filename restore-name ;
: reset   remember-filename delete-file throw ;

only forth definitions
