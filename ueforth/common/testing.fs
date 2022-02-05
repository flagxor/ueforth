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

also ansi also internals

DEFINED? windows [IF]
  also windows
  : sysexit ( n -- ) ExitProcess ;
[ELSE]
  DEFINED? posix [IF]
    also posix
  [ELSE]
    : sysexit ( n -- ) terminate ;
  [THEN]
[THEN]

( Support for eval tests )
40000 constant expect-limit
create expect-buffer expect-limit allot
create result-buffer expect-limit allot
variable expect-used   variable result-used
: till;e ( -- n )
   begin >in @ bl parse dup 0= >r s" ;e" str= r> or if exit then drop again ;
: e: ( "name" -- ) create >in @
   till;e over - swap tib + swap dup , $place
   does> dup cell+ swap @ evaluate ;
: expect-emit ( ch -- ) expect-used @ expect-limit < assert
                        expect-buffer expect-used @ + c!
                        1 expect-used +! ;
: result-emit ( ch -- ) result-used @ expect-limit < assert
                        result-buffer result-used @ + c!
                        1 result-used +! ;
: expect-type ( a n -- ) for aft dup c@ expect-emit 1+ then next drop ;
: result-type ( a n -- ) for aft dup c@ result-emit 1+ then next drop ;
: expected ( -- a n ) expect-buffer expect-used @ ;
: resulted ( -- a n ) result-buffer result-used @ ;
: out:cr   nl expect-emit ;
: out:\ ( "line" -- ) nl parse expect-type ;
: out: ( "line" -- ) out:\ out:cr ;
variable confirm-old-type
: confirm{   ['] type >body @ confirm-old-type ! ['] result-type is type ;
: }confirm   confirm-old-type @ is type ;
: expect-reset   0 expect-used ! 0 result-used ! ;
: diverged ( a n a n -- a n )
   begin
      dup 0= if 2drop exit then
      >r dup c@ >r rot dup c@ >r -rot r> r> <> r> swap if 2drop exit then
      >r >r dup 0= if rdrop rdrop exit then r> r>
      >r >r >r 1+ r> 1- r> 1+ r> 1-
   again
;
: expect-finish   expected resulted str= if exit then }confirm
   cr ." Expected:" cr expected resulted diverged type cr
      ." Resulted:" cr resulted expected diverged type cr 1 throw ;

( Better error asserts )
: =assert ( actual expected -- )
  2dup <> if }confirm ."   FAILURE! EXPECTED: " .
                      ." ACTUAL: " . space 0 assert then 2drop ;
: <assert ( actual expected -- )
  2dup >= if }confirm ."   MUST BE LESS THAN: " .
                      ." ACTUAL: " . space 0 assert then 2drop ;
: >assert ( actual expected -- )
  2dup <= if }confirm ."   MUST BE GREATER THAN: " .
                      ." ACTUAL: " . space 0 assert then 2drop ;

( Input testing )
create in-buffer 1000 allot
variable in-head   variable in-tail
: >in ( c -- ) in-buffer in-head @ + c!  1 in-head +! ;
: in> ( -- c ) in-tail @ in-head @ <assert
               in-buffer in-tail @ + c@  1 in-tail +!
               in-head @ in-tail @ = if 0 in-head ! 0 in-tail ! then ;
: s>in ( a n -- ) for aft dup c@ >in 1+ then next drop ;
: in: ( "line" -- ) nl parse s>in nl >in ;
' in> is key

( Testing Framework )
( run-tests runs all words starting with "test-", use assert to assert things. )
variable tests-found   variable tests-run    variable tests-passed
: test? ( xt -- f ) >name s" test-" startswith? ;
: for-tests ( xt -- )
   context @ @ begin dup while dup test? if 2dup >r >r swap execute r> r> then >link repeat 2drop ;
: reset-test-counters   0 tests-found !   0 tests-run !   0 tests-passed ! ;
: count-test ( xt -- ) drop 1 tests-found +! ;
: check-fresh   depth if }confirm ."  DEPTH LEAK! " depth . 1 throw then
                fdepth if }confirm ."  FDEPTH LEAK! " fdepth . 1 throw then ;
: wrap-test ( xt -- ) expect-reset >r check-fresh r> execute check-fresh expect-finish ;
: red   1 fg ;   : green   2 fg ;   : hr   40 for [char] - emit next cr ;
: replace-line   13 emit clear-to-eol ;
: label-test ( xt -- ) replace-line >name type ;
: run-test ( xt -- ) dup label-test only forth confirm{ ['] wrap-test catch }confirm
   if drop ( cause xt restored on throw ) red ."  FAILED" normal cr
   else green ."  OK" normal 1 tests-passed +! then 1 tests-run +! ;
: show-test-results
   replace-line hr
   ."   PASSED: " green tests-passed @ . normal
   ."   RUN: " tests-run @ .
   ."   FOUND: " tests-found @ . cr
   tests-passed @ tests-found @ = if
     green ."   ALL TESTS PASSED" normal cr
   else
     ."   FAILED: " red tests-run @ tests-passed @ - . normal cr
   then hr ;
: run-tests
   reset-test-counters ['] count-test for-tests
   ['] run-test for-tests show-test-results
   tests-passed @ tests-found @ <> sysexit ;
only forth
