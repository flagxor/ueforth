( Testing Locals )

e: test-locals-one
  : test { a } a a * ;
  4 test 16 = assert
;e

e: test-locals-two
  : test { a b } a a a b b ;
  7 8 test .s
  out: <5> 7 7 7 8 8 
  sp0 sp!
;e

