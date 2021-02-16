e: test-vocabularies
  vocabulary foo
  vocabulary bar
  foo definitions : test ." AAAA" cr ;
  bar definitions : test ." BBBB" cr ;
  forth definitions
  foo test
  bar test
  foo test
  forth definitions
  out: AAAA
  out: BBBB
  out: AAAA
;e

e: test-vlist
  vocabulary foo
  foo definitions
  : pig ;   : cow ;   : sheep ;
  forth definitions
  foo vlist
  forth definitions
  out: sheep cow pig 
;e

e: test-vlist-empty
  vocabulary foo
  foo vlist
  forth definitions
  out:
;e

e: test-order
  vocabulary foo
  vocabulary bar
  vocabulary baz
  also foo also bar also baz
  order
  out: baz bar foo FORTH 
  only forth definitions
;e

e: test-vocab-define-order
  vocabulary foo
  foo definitions
  : a ." AAAAAA" cr ;
  forth definitions
  : a ." BAD" cr ;
  foo a
  out: AAAAAA
  only forth definitions
;e

e: test-vocabulary-chaining
  vocabulary foo
  foo definitions
  vocabulary bar
  bar definitions
  : a ." aaaa" cr ;
  foo definitions
  : b ." bbbb" cr ;
  forth definitions
  : a ." BAD" cr ;
  : b ." BAD" cr ;
  foo a b
  out: BAD
  out: bbbb
  bar a b
  out: aaaa
  out: bbbb
  only forth definitions
;e

e: test-sealed
  : aaa ." good" cr ;
  vocabulary foo
  foo definitions
  : aaa ." bad" cr ;
  vocabulary bar sealed
  also bar definitions
  : bbb ." b" cr ;
  only forth definitions
  also foo bar
  aaa
  bbb
  out: good
  out: b
  only forth definitions
;e

e: test-fixed-does>-normal
  : adder create , does> @ + ;
  3 adder foo
  4 foo 7 = assert
  4 ' foo execute 7 = assert
;e

(
e: test-fixed-does>-interp
  create hi 123 , does> @ + ;
  7 hi 130 = assert
;e
)
