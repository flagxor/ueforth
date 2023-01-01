\ Copyright 2021 Bradley D. Nelson
\
\ Licensed under the Apache License, Version 2.0 (the "License");
\ you may not use this file except in compliance with the License.
\ You may obtain a copy of the License at
\
\     http://www.apache.org/licenses/LICENSE-2.0
\
\ Unless required by applicable law or agreed to in writing, software
\ distributed under the License is distributed on an "AS IS" BASIS,
\ WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
\ See the License for the specific language governing permissions and
\ limitations under the License.

( Implement Vocabularies )
( normal: link, flags&len, code )
( vocab:  link, flags&len, code | link , len=0, voclink )
variable last-vocabulary
: vocabulary ( "name" )
  create current @ 2 cells + , 0 , last-vocabulary @ ,
  current @ @ last-vocabulary !
  does> context ! ;
: definitions   context @ current ! ;
vocabulary FORTH
' forth >body @ >link ' forth >body !
forth definitions

( Make it easy to transfer words between vocabularies )
: xt-find& ( xt -- xt& ) context @ begin 2dup @ <> while @ >link& repeat nip ;
: xt-hide ( xt -- ) xt-find& dup @ >link swap ! ;
8 constant BUILTIN_MARK
: xt-transfer ( xt --  ) dup >flags BUILTIN_MARK and if drop exit then
  dup xt-hide   current @ @ over >link& !   current @ ! ;
: transfer ( "name" ) ' xt-transfer ;
: }transfer ;
: transfer{ begin ' dup ['] }transfer = if drop exit then xt-transfer again ;

( Watered down versions of these )
: only   forth 0 context cell+ ! ;
: voc-stack-end ( -- a ) context begin dup @ while cell+ repeat ;
: also   context context cell+ voc-stack-end over - 2 cells + cmove> ;
: previous
  voc-stack-end context cell+ = throw
  context cell+ context voc-stack-end over - cell+ cmove ;
: sealed   0 last-vocabulary @ >body ! ;

( Hide some words in an internals vocabulary )
vocabulary internals   internals definitions

( Vocabulary chain for current scope, place at the -1 position )
variable scope   scope context cell - !

transfer{
  xt-find& xt-hide xt-transfer
  voc-stack-end last-vocabulary notfound
  *key *emit wascr eat-till-cr
  immediate? input-buffer ?echo ?arrow. arrow
  evaluate-buffer evaluate&fill aliteral value-bind
  leaving( )leaving leaving leaving,
  parse-quote digit $@ raw.s
  tib-setup input-limit sp-limit ?stack
  [SKIP] [SKIP]' raw-ok boot-prompt free.
  $place zplace BUILTIN_MARK
}transfer

( Move branching opcodes to separate vocabulary )
vocabulary internalized  internalized definitions
: cleave   ' >link xt-transfer ;
cleave begin   cleave again   cleave until
cleave ahead   cleave then    cleave if
cleave else    cleave while   cleave repeat
cleave aft     cleave for     cleave next
cleave do      cleave ?do     cleave +loop
cleave loop    cleave leave

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
