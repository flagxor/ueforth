: dump-file ( a n a n -- )
  w/o create-file if drop ." failed create-file" exit then
  >r r@ write-file if r> drop ." failed write-file" exit then
  r> close-file drop
;
