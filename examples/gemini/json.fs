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

needs arrays.fs

vocabulary json also internals also arrays also json definitions

defer getchar
-1 value token
: skip   getchar to token ;

variable insource
variable inlength
: in ( a n -- ) inlength ! insource ! skip ;
: ingetchar ( -- n )
   inlength @ 0= if -1 exit then
   insource @ c@ 1 insource +! -1 inlength +! ;
' ingetchar is getchar

s" DICTIONARY" _s aconstant DICT
: {{   [[ DICT ;
: }}   ]] ;

( Dictionary lookup )
: as= ( a: a a -- f )
  top >type @ STRING <> if a2drop 0 exit then
  under >type @ STRING <> if a2drop 0 exit then
  top range under range str= a2drop
;
: dict@ ( a: a key -- value )
  aswap
  top >count @ 1 ?do
    a2dup i a@ 0 a@ as= if i a@ 1 a@ anip unloop exit then
  loop
  a2drop
  _s" "
;

: space? ( ch -- f ) dup 8 = over 10 = or over 13 = or swap 32 = or ;
: <whitespace>    begin token space? while skip repeat ;

: expect ( a n -- ) for aft dup c@ token = assert 1+ skip then next drop ;
: sliteral ( a n -- ) postpone $@ dup , zplace ;
: e:   bl parse sliteral postpone expect ; immediate

: <escaped>
   e: \
   token skip case
     [char] " of [char] " _c catenate endof
     [char] \ of [char] \ _c catenate endof
     [char] / of [char] / _c catenate endof
     [char] b of 8 _c catenate endof
     [char] f of 12 _c catenate endof
     [char] n of nl _c catenate endof
     [char] r of 13 _c catenate endof
     [char] t of 9 _c catenate endof
     [char] u of 255 _c catenate skip skip skip skip endof
     -1 throw
   endcase
;

: <string>
   e: " s" " _s
   begin token [char] " <> while
     token [char] \ = if
       <escaped>
     else
       token _c catenate
       skip
     then
   repeat
   e: "
;

defer <value>

: <object>
   e: { <whitespace>
   DICT box
   begin
     token [char] } = if skip exit then
     <string> box <whitespace> e: : <whitespace> <value> box
     catenate box catenate
     token [char] } = if skip exit then
     e: , <whitespace>
   again
   e: }
;

: digit? ( -- f ) token [char] 0 >= token [char] 9 <= and ;
: <digit>   token [char] 0 - skip ;
: <integer>   digit? assert <digit>
              begin digit? while 10 * <digit> + repeat ;
: <fraction>   digit? assert 10 * <digit> + >r 1- r>
               begin digit? while 10 * <digit> + >r 1- r> repeat ;
: <number>
  token [char] - = if skip -1 else 1 then
  token [char] 0 = if skip 0 else <integer> then 0 swap
  token [char] . = if skip <fraction> then
  swap >r * r>
  token [char] e = token [char] E = or if
    skip
    token [char] - = if
      skip -1
    else
      token [char] + = if skip then 1
    then
    <integer> * +
  then
  dup if 10e s>f f** s>f f* _f else drop _i then
;

: <array>
   e: [ <whitespace>
   0 MIXED array
   begin
     token [char] ] = if skip exit then
     <value> box catenate <whitespace>
     token [char] ] = if skip exit then
     e: , <whitespace>
   again
;

s" null" _s aconstant null
s" true" _s aconstant true
s" false" _s aconstant false

:noname
   <whitespace>
   token case
     [char] " of <string> endof
     [char] { of <object> endof
     [char] [ of <array> endof
     [char] t of e: true true endof
     [char] f of e: false false endof
     [char] n of e: null null endof
     <number>
   endcase
   <whitespace>
; is <value>

: json> ( a -- a ) top range in <value> anip ;

: butlast? ( n -- f ) top >count @ 1- <> ;

: escaped ( a n -- a: a )
  _s" "
  0 ?do
    dup i + c@
    case
      [char] " of _s" \" ,c [char] " _c ,c endof
      [char] / of _s" \/" ,c endof
      [char] \ of _s" \\" ,c endof
      8 of _s" \b" ,c endof
      12 of _s" \f" ,c endof
      nl of _s" \n" ,c endof
      13 of _s" \r" ,c endof
      9 of _s" \t" ,c endof
      dup _c ,c
    endcase
  loop
  drop
;

: >json ( a: a -- a )
  top >type @ case
    MIXED of
      top >count @ 1 > if top @ DICT top adrop = else 0 then if
        _s" {" >a top >count @ 1 ?do
          adup i a@ 0 a@ recurse _s" :" ,c a> aswap ,c >a
          adup i a@ 1 a@ recurse a> aswap ,c >a
          i butlast? if a> _s" ," ,c >a then
        loop a> _s" }" ,c
      else
        _s" [" >a top >count @ 0 ?do
          adup i a@ recurse a> aswap ,c >a
          i butlast? if a> _s" ," ,c >a then
        loop a> _s" ]" ,c
      then
    endof
    STRING of
      top null top adrop = if exit then
      top true top adrop = if exit then
      top false top adrop = if exit then
      [char] " _c >a top range a> escaped ,c [char] " _c ,c
    endof
    INTEGER of
      _s" " >a
      top >count @ 0 ?do
        top i cells + @ <# #s #> a> _s"  " ,c _s ,c >a
      loop a> endof
    REAL of
      _s" " >a top >count @ 0 ?do
        top i sfloats + sf@ <# #fs #> a> _s" " ,c _s ,c >a
      loop a> endof
  endcase
  anip
;

previous previous previous forth definitions
