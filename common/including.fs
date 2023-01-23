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

( Including Files )

internals definitions

: ends/ ( a n -- f ) 1- + c@ [char] / = ;
: dirname ( a n -- )
   dup if
     2dup ends/ if 1- then
   then
   begin dup while
     2dup ends/ if exit then 1-
   repeat ;

: starts./ ( a n -- f )
   2 < if drop 0 exit then
   2 s" ./" str= ;

: starts../ ( a n -- f )
   3 < if drop 0 exit then
   3 s" ../" str= ;

0 value sourcefilename&
0 value sourcefilename#
: sourcefilename ( -- a n ) sourcefilename& sourcefilename# ;
: sourcefilename! ( a n -- ) to sourcefilename# to sourcefilename& ;
: sourcedirname ( -- a n ) sourcefilename dirname ;

: include-file ( fh -- )
   dup file-size throw
   dup allocate throw
   swap over >r
   rot read-file throw
   r@ swap evaluate
   r> free throw ;

: raw-included ( a n -- )
   r/o open-file throw
   dup >r include-file
   r> close-file throw ;

0 value included-files

: path-join { a a# b b# -- a n }
  a# b# + { r# } r# cell+ cell+ allocate throw { r }
  2 cells +to r
  b c@ [char] / = if 0 to a# then
  begin b b# starts./ while
    2 +to b -2 +to b#
    a# b# + to r#
  repeat
  begin b b# starts../ a# 0<> and while
    3 +to b -3 +to b#
    a a# dirname to a# to a
    a# b# + to r#
  repeat
  a r a# cmove b r a# + b# cmove
  r# r cell - !
  r r# ;
: include+ 2 cells - { a }
  included-files a ! a to included-files ;

forth definitions internals

: included ( a n -- )
   sourcefilename >r >r
   >r >r sourcedirname r> r> path-join 2dup sourcefilename!
   ['] raw-included catch if
      ." Error including: " sourcefilename type cr
      -38 throw
   then
   sourcefilename& include+
   r> r> sourcefilename! ;

: include ( "name" -- ) bl parse included ;

: included? { a n -- f }
  sourcedirname a n path-join to n to a
  included-files begin dup while
    dup cell+ cell+ over cell+ @ a n str= if
      a 2 cells - free throw drop -1 exit
    then @
  repeat
  a 2 cells - free throw ;

: required ( a n -- ) 2dup included? if 2drop else included then ;
: needs ( "name" -- ) bl parse required ;

: file-exists? ( "name" -- f ) r/o open-file if drop 0 else close-file throw -1 then ;

forth
