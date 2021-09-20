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
  w/o create-file if drop ." failed create-file" exit then
  >r r@ write-file if r> drop ." failed write-file" exit then
  r> close-file drop
;

internals definitions
( Leave some room for growth of starting system. )
$8000 constant growth-gap
here growth-gap + growth-gap 1- + growth-gap 1- invert and constant saving-base
: park-heap ( -- a ) saving-base ;
: park-forth ( -- a ) saving-base cell+ ;
: 'cold ( -- a ) saving-base 2 cells + ;   0 'cold !

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
