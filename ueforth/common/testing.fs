( Testing Framework )
( run-tests runs all words starting with "test-", use assert to assert things. )
variable tests-found   variable tests-run    variable tests-passed
: assert ( f -- ) 0= throw ;
: startswith? ( a n a n -- f )
   >r swap r@ < if rdrop 2drop 0 exit then
   r> for aft 2dup c@ swap c@ <> if 2drop rdrop 0 exit then 1+ swap 1+ then next 2drop -1 ;
: test? ( xt -- f ) >name s" test-" startswith? ;
: for-tests ( xt -- )
   last @ begin dup while dup test? if 2dup >r >r swap execute r> r> then >link repeat 2drop ;
: reset-test-counters   0 tests-found !   0 tests-run !   0 tests-passed ! ;
: count-test ( xt -- ) drop 1 tests-found +! ;
: check-fresh   depth if ."  DEPTH LEAK! " depth . 1 throw then ;
: wrap-test ( xt -- ) >r check-fresh r> execute check-fresh ;
: red   1 fg ;   : green   2 fg ;   : hr   40 for [char] - emit next cr ;
: run-test ( xt -- ) dup >name type ['] wrap-test catch
   if drop ( cause xt restored on throw ) red ."  FAILED" normal cr
   else green ."  OK" normal cr 1 tests-passed +! then 1 tests-run +! ;
: pre-test-run   cr hr tests-found @ . ." Tests found." cr hr ;
: show-test-results
   hr ."   PASSED: " green tests-passed @ . normal ."   RUN: " tests-run @ .
   ."   FOUND: " tests-found @ . cr
   tests-passed @ tests-found @ = if
     green ."   ALL TESTS PASSED" normal cr
   else
     ."   FAILED: " red tests-run @ tests-passed @ - . normal cr
   then hr ;
: run-tests
   reset-test-counters ['] count-test for-tests pre-test-run
   ['] run-test for-tests show-test-results
   tests-passed @ tests-found @ <> sysexit ;
