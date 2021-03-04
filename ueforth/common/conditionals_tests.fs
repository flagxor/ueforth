( Interpret time conditionals )

e: test-1[if]
  1 [IF]
    : test ." hi" cr ;
  [THEN]
  test
  out: hi
;e

e: test-0[if]
  : test ." initial" cr ;
  0 [IF]
    : test ." hi" cr ;
  [THEN]
  test
  out: initial
;e

e: test-1[if][else]
  1 [IF]
    : test ." hi" cr ;
  [ELSE]
    : test ." there" cr ;
  [THEN]
  test
  out: hi
;e

e: test-0[if][else]
  0 [IF]
    : test ." hi" cr ;
  [ELSE]
    : test ." there" cr ;
  [THEN]
  test
  out: there
;e

e: test-1[if]-nesting
  1 [IF]
    : test ." foo" cr ;
  [ELSE]
    1 [IF]
      : test ." bar" cr ;
    [ELSE]
      : test ." baz" cr ;
    [THEN]
  [THEN]
  test
  out: foo
;e

e: test-0[if]-nesting
  0 [IF]
    1 [IF]
      : test ." foo" cr ;
    [ELSE]
      : test ." bar" cr ;
    [THEN]
  [ELSE]
    : test ." baz" cr ;
  [THEN]
  test
  out: baz
;e
