( Implement Vocabularies )
: forth   [ current @ ] literal context ! ;
: vocabulary ( "name" ) create 0 , current @ 2 cells + , does> cell+ context ! ;
: definitions   context @ current ! ;
: >name-length ( xt -- n ) dup 0= if exit then >name nip ;
: vlist   0 context @ @ begin dup >name-length while onlines dup see. >link repeat 2drop cr ;

( Make it easy to transfer words between vocabularies )
: transfer-xt ( xt --  ) context @ begin 2dup @ <> while @ >link& repeat nip
                         dup @ swap dup @ >link swap ! current @ @ over >link& !   current @ ! ;
: transfer ( "name" ) ' transfer-xt ;
: }transfer ;
: transfer{ begin ' dup ['] }transfer = if drop exit then transfer-xt again ;

( Hide some words in an internals vocabulary )
vocabulary internals   internals definitions
transfer{
  transfer-xt
  branch 0branch donext dolit
  'notfound notfound
  immediate? input-buffer ?echo ?echo-prompt
  evaluate1 evaluate-buffer
  'sys 'heap aliteral
  leaving( )leaving leaving leaving,
  (do) (?do) (+loop)
  parse-quote digit $@
  see. see-loop >name-length exit=
  see-one
  tib-setup input-limit
}transfer
forth definitions

