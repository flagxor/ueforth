( Implement Vocabularies )
variable last-vocabulary
current @ constant forth-wordlist
: forth   forth-wordlist context ! ;
: vocabulary ( "name" ) create 0 , current @ 2 cells + , current @ @ last-vocabulary !
                        does> cell+ context ! ;
: definitions   context @ current ! ;

( Make it easy to transfer words between vocabularies )
: xt-find& ( xt -- xt& ) context @ begin 2dup @ <> while @ >link& repeat nip ;
: xt-hide ( xt -- ) xt-find& dup @ >link swap ! ;
: xt-transfer ( xt --  ) dup xt-hide   current @ @ over >link& !   current @ ! ;
: transfer ( "name" ) ' xt-transfer ;
: ?transfer ( "name" ) bl parse find dup if xt-transfer else drop then ;
: }transfer ;
: transfer{ begin ' dup ['] }transfer = if drop exit then xt-transfer again ;

( Watered down versions of these )
: only   forth 0 context cell+ ! ;
: voc-stack-end ( -- a ) context begin dup @ while cell+ repeat ;
: also   context context cell+ voc-stack-end over - 2 cells + cmove> ;
: sealed   0 last-vocabulary @ >body cell+ ! ;

( Hide some words in an internals vocabulary )
vocabulary internals   internals definitions

( Vocabulary chain for current scope, place at the -1 position )
variable scope   scope context cell - !

transfer{
  xt-find& xt-hide xt-transfer
  voc-stack-end forth-wordlist
  last-vocabulary
  branch 0branch donext dolit
  'context 'notfound notfound
  immediate? input-buffer ?echo ?arrow. arrow
  evaluate1 evaluate-buffer
  'sys 'heap aliteral
  leaving( )leaving leaving leaving,
  (do) (?do) (+loop)
  parse-quote digit $@ raw.s
  tib-setup input-limit
}transfer
forth definitions

( Make DOES> switch to compile mode when interpreted )
(
forth definitions internals
' does>
: does>   state @ if postpone does> exit then
          ['] constant @ current @ @ dup >r !
          here r> cell+ ! postpone ] ; immediate
xt-hide
forth definitions
)
