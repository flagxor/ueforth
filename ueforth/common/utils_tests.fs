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
