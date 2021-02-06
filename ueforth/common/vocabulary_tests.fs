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
