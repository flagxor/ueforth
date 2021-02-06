e: test-vocabularies
  vocabulary foo
  vocabulary bar
  foo definitions : test ." AAAA" cr ;
  bar definitions : test ." BBBB" cr ;
  forth definitions
  foo test
  bar test
  foo test
  out: AAAA
  out: BBBB
  out: AAAA
;e
