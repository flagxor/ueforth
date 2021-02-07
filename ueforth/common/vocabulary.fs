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
: ?transfer ( "name" ) bl parse find dup if transfer-xt else drop then ;
: }transfer ;
: transfer{ begin ' dup ['] }transfer = if drop exit then transfer-xt again ;

( Watered down versions of these )
: only   forth 0 context cell+ ! ;
: last-voc ( -- a) context begin dup @ while cell+ repeat ;
: also   context context cell+ last-voc over - cell+ cmove> ;
: sealed   0 current @ ! ;

( Hide some words in an internals vocabulary )
vocabulary internals   internals definitions
transfer{
  transfer-xt last-voc
  branch 0branch donext dolit
  'context 'notfound notfound
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

