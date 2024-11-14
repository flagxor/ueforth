: slurp-file ( a n -- a n )
  r/o open-file throw >r
  r@ file-size throw ( sz )
  dup 1+ allocate throw swap ( data sz )
  2dup r@ read-file throw drop
  r> close-file throw
  2dup + 0 swap c!
;
