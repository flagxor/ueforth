\ Copyright 2022 Bradley D. Nelson
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

( Defer I/O to platform specific )
defer type
defer key
defer key?
defer bye
: emit ( n -- ) >r rp@ 1 type rdrop ;
: space bl emit ;   : cr 13 emit nl emit ;

( Numeric Output )
variable hld
: pad ( -- a ) here 80 + ;
: digit ( u -- c ) 9 over < 7 and + 48 + ;
: extract ( n base -- n c ) u/mod swap digit ;
: <# ( -- ) pad hld ! ;
: hold ( c -- ) hld @ 1 - dup hld ! c! ;
: # ( u -- u ) base @ extract hold ;
: #s ( u -- 0 ) begin # dup while repeat ;
: sign ( n -- ) 0< if 45 hold then ;
: #> ( w -- b u ) drop hld @ pad over - ;
: str ( n -- b u ) dup >r abs <# #s r> sign #> ;
: hex ( -- ) 16 base ! ;   : octal ( -- ) 8 base ! ;
: decimal ( -- ) 10 base ! ;   : binary ( -- ) 2 base ! ;
: u. ( u -- ) <# #s #> type space ;
: . ( w -- ) base @ 10 xor if u. exit then str type space ;
: ? ( a -- ) @ . ;
: n. ( n -- ) base @ swap decimal <# #s #> type base ! ;

( Strings )
: parse-quote ( -- a n ) [char] " parse ;
: $place ( a n -- ) for aft dup c@ c, 1+ then next drop ;
: zplace ( a n -- ) $place 0 c, align ;
: $@   r@ dup cell+ swap @ r> dup @ 1+ aligned + cell+ >r ;
: s"   parse-quote state @ if postpone $@ dup , zplace
       else dup here swap >r >r zplace r> r> then ; immediate
: ."   postpone s" state @ if postpone type else type then ; immediate
: z"   postpone s" state @ if postpone drop else drop then ; immediate
: r"   parse-quote state @ if swap aliteral aliteral then ; immediate
: r|   [char] | parse state @ if swap aliteral aliteral then ; immediate
: r~   [char] ~ parse state @ if swap aliteral aliteral then ; immediate
: s>z ( a n -- z ) here >r zplace r> ;
: z>s ( z -- a n ) 0 over begin dup c@ while 1+ swap 1+ swap repeat drop ;

( Better Errors )
: notfound ( a n n -- )
   if cr ." ERROR: " type ."  NOT FOUND!" cr -1 throw then ;
' notfound 'notfound !

( Input )
: raw.s   depth 0 max for aft sp@ r@ cells - @ . then next ;
variable echo -1 echo !   variable arrow -1 arrow !  0 value wascr
: *emit ( n -- ) dup 13 = if drop cr else emit then ;
: ?echo ( n -- ) echo @ if *emit else drop then ;
: ?arrow.   arrow @ if >r >r raw.s r> r> ." --> " then ;
: *key ( -- n )
  begin
    key
    dup nl = if
      drop wascr if 0 else 13 exit then
    then
    dup 13 = to wascr
    dup if exit else drop then
  again ;
: eat-till-cr   begin *key dup 13 = if ?echo exit else drop then again ;
: accept ( a n -- n ) ?arrow. 0 swap begin 2dup < while
     *key
     dup 13 = if ?echo drop nip exit then
     dup 8 = over 127 = or if
       drop over if rot 1- rot 1- rot 8 ?echo bl ?echo 8 ?echo then
     else
       dup ?echo
       >r rot r> over c! 1+ -rot swap 1+ swap
     then
   repeat drop nip
   eat-till-cr
;
200 constant input-limit
: tib ( -- a ) 'tib @ ;
create input-buffer   input-limit allot
: tib-setup   input-buffer 'tib ! ;
: refill   tib-setup tib input-limit accept #tib ! 0 >in ! -1 ;

( Stack Guards )
sp0 'stack-cells @ 2 3 */ cells + constant sp-limit
: ?stack   sp@ sp0 < if ." STACK UNDERFLOW " -1 throw then
           sp-limit sp@ < if ." STACK OVERFLOW " -1 throw then ;

( REPL )
: prompt   ."  ok" cr ;
: evaluate-buffer   begin >in @ #tib @ < while evaluate1 ?stack repeat ;
: evaluate ( a n -- ) 'tib @ >r #tib @ >r >in @ >r
                      #tib ! 'tib ! 0 >in ! evaluate-buffer
                      r> >in ! r> #tib ! r> 'tib ! ;
: quit    begin ['] evaluate-buffer catch
          if 0 state ! sp0 sp! fp0 fp! rp0 rp! ." ERROR" cr then
          prompt refill drop again ;
variable boot-prompt
: free. ( nf nu -- ) 2dup swap . ." free + " . ." used = " 2dup + . ." total ("
                     over + 100 -rot */ n. ." % free)" ;
: raw-ok   ."  v{{VERSION}} - rev {{REVISION}}" cr
           boot-prompt @ if boot-prompt @ execute then
           ." Forth dictionary: " remaining used free. cr
           ." 3 x Forth stacks: " 'stack-cells @ cells . ." bytes each" cr
           prompt refill drop quit ;
