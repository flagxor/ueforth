( Tests of utils.fs )
e: test-.s0
  .s
  out: <0> 
;e

e: test-.s
  1 2 3 .s
  out: <3> 1 2 3 
  128 .s
  out: <4> 1 2 3 128 
  2drop 2drop
  .s
  out: <0> 
;e

e: test-forget
  context @ @
  current @
  here
  : foo 123 ;
  : bar foo foo ;
  : baz bar bar * * ;
  forget foo
  here = assert
  current @ = assert
  context @ @ = assert
;e

e: test-see-number
  : test 123 456 ;
  see test
  out:cr
  out: : test  123 456 ; 
;e

e: test-see-string
  : test s" hello there" ;
  see test
  out:cr
  out: : test  s" hello there" ; 
;e

e: test-see-branch
  : test begin again ;
  see test
  out:cr
  out: : test  BRANCH ; 
;e

e: test-see-0branch
  : test begin until ;
  see test
  out:cr
  out: : test  0BRANCH ; 
;e

e: test-see-fornext
  : test for next ;
  see test
  out:cr
  out: : test  >R DONEXT ; 
;e
