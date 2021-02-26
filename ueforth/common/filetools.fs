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
: bootword ( -- a ) saving-base 2 cells + ;   0 bootword !

: save-name
  'heap @ park-heap !
  forth-wordlist @ park-forth !
  w/o create-file throw >r
  saving-base here over - r@ write-file throw
  r> close-file throw ;

: restore-name ( "name" -- )
  r/o open-file throw >r
  saving-base r@ file-size throw r@ read-file throw drop
  r> close-file throw
  park-heap @ 'heap !
  park-forth @ forth-wordlist !
  bootword @ dup if execute else drop then ;

defer remember-filename
: default-remember-filename   s" myforth" ;
' default-remember-filename is remember-filename

forth definitions also internals

: save ( "name" -- ) bl parse save-name ;
: restore ( "name" -- ) bl parse restore-name ;
: remember   remember-filename save-name ;
: startup: ( "name" ) ' bootword ! remember ;
: revive   remember-filename restore-name ;
: reset   remember-filename delete-file throw ;

only forth definitions
