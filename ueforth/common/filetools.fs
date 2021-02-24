: dump-file ( a n a n -- )
  w/o create-file if drop ." failed create-file" exit then
  >r r@ write-file if r> drop ." failed write-file" exit then
  r> close-file drop
;

internals definitions
( Leave some room for growth of starting system. )
$4000 constant growth-gap
here growth-gap + growth-gap 1- + growth-gap 1- invert and constant saving-base
: park-heap ( -- a ) saving-base ;
: park-forth ( -- a ) saving-base cell+ ;
forth definitions also internals

: save ( "name" -- )
  'heap @ park-heap !
  forth-wordlist @ park-forth !
  bl parse w/o create-file throw >r
  saving-base here over - r@ write-file throw
  r> close-file throw ;

: restore ( "name" -- )
  bl parse r/o open-file throw >r
  saving-base r@ file-size throw r@ read-file throw drop
  r> close-file throw
  park-heap @ 'heap !
  park-forth @ forth-wordlist ! ;

only forth definitions
