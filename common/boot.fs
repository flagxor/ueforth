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

: (   41 parse drop drop ; immediate
: \   10 parse drop drop ; immediate
: #!   10 parse drop drop ; immediate  ( shebang for scripts )
( Now can do comments! )

( Stack Baseline )
sp@ constant sp0
rp@ constant rp0
fp@ constant fp0
: depth ( -- n ) sp@ sp0 - cell/ ;
: fdepth ( -- n ) fp@ fp0 - 4 / ;

( Useful heap size words )
: remaining ( -- n ) 'heap-start @ 'heap-size @ + 'heap @ - ;
: used ( -- n ) 'heap @ sp@ 'stack-cells @ cells + - 28 + ;

( Compilation State )
: [ 0 state ! ; immediate
: ] -1 state ! ; immediate

( Quoting Words )
: ' bl parse 2dup find dup >r -rot r> 0= 'notfound @ execute 2drop ;
: ['] ' aliteral ; immediate
: char bl parse drop c@ ;
: [char] char aliteral ; immediate
: literal aliteral ; immediate

( Core Control Flow )
: begin   here ; immediate
: again   ['] branch , , ; immediate
: until   ['] 0branch , , ; immediate
: ahead   ['] branch , here 0 , ; immediate
: then   here swap ! ; immediate
: if   ['] 0branch , here 0 , ; immediate
: else   ['] branch , here 0 , swap here swap ! ; immediate
: while   ['] 0branch , here 0 , swap ; immediate
: repeat   ['] branch , , here swap ! ; immediate
: aft   drop ['] branch , here 0 , here swap ; immediate

( Recursion )
: recurse   current @ @ aliteral ['] execute , ; immediate

( Postpone - done here so we have ['] and IF )
: immediate? ( xt -- f ) >flags 1 and 0= 0= ;
: postpone ' dup immediate? if , else aliteral ['] , , then ; immediate

( Rstack nest depth )
variable nest-depth

( FOR..NEXT )
: for   1 nest-depth +! postpone >r postpone begin ; immediate
: next   -1 nest-depth +! postpone donext , ; immediate

( DO..LOOP )
variable leaving
: leaving,   here leaving @ , leaving ! ;
: leaving(   leaving @ 0 leaving !   2 nest-depth +! ;
: )leaving   leaving @ swap leaving !  -2 nest-depth +!
             begin dup while dup @ swap here swap ! repeat drop ;
: (do) ( n n -- .. ) swap r> -rot >r >r >r ;
: do ( lim s -- ) leaving( postpone (do) here ; immediate
: (?do) ( n n -- n n f .. ) 2dup = if 2drop 0 else -1 then ;
: ?do ( lim s -- ) leaving( postpone (?do) postpone 0branch leaving,
                   postpone (do) here ; immediate
: unloop   postpone rdrop postpone rdrop ; immediate
: leave   postpone unloop postpone branch leaving, ; immediate
: (+loop) ( n -- f .. ) dup 0< swap r> r> rot + dup r@ < -rot >r >r xor 0= ;
: +loop ( n -- ) postpone (+loop) postpone until
                 postpone unloop )leaving ; immediate
: loop   1 aliteral postpone +loop ; immediate
: i ( -- n ) postpone r@ ; immediate
: j ( -- n ) rp@ 3 cells - @ ;

( Exceptions )
variable handler
: catch ( xt -- n )
  fp@ >r sp@ >r handler @ >r rp@ handler ! execute
  r> handler ! rdrop rdrop 0 ;
: throw ( n -- )
  dup if handler @ rp! r> handler !
         r> swap >r sp! drop r> r> fp! else drop then ;
' throw 'notfound !

( Values )
: value ( n -- ) constant ;
: value-bind ( xt-val xt )
   >r >body state @ if
     r@ ['] ! = if rdrop ['] doset , , else aliteral r> , then
   else r> execute then ;
: to ( n -- ) ' ['] ! value-bind ; immediate
: +to ( n -- ) ' ['] +! value-bind ; immediate

( Deferred Words )
: defer ( "name" -- ) create 0 , does> @ dup 0= throw execute ;
: is ( xt "name -- ) postpone to ; immediate

( Defer I/O to platform specific )
defer type
defer key
defer key?
defer bye
: emit ( n -- ) >r rp@ 1 type rdrop ;
: space bl emit ;   : cr nl emit ;

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
variable echo -1 echo !   variable arrow -1 arrow !
: ?echo ( n -- ) echo @ if emit else drop then ;
: ?arrow.   arrow @ if >r >r raw.s r> r> ." --> " then ;
: accept ( a n -- n ) ?arrow. 0 swap begin 2dup < while
     key
     dup nl = over 13 = or if ?echo drop nip exit then
     dup 8 = over 127 = or if
       drop over if rot 1- rot 1- rot 8 ?echo bl ?echo 8 ?echo then
     else
       dup ?echo
       >r rot r> over c! 1+ -rot swap 1+ swap
     then
   repeat drop nip
   ( Eat rest of the line if buffer too small )
   begin key dup nl = over 13 = or if ?echo exit else drop then again
;
200 constant input-limit
: tib ( -- a ) 'tib @ ;
create input-buffer   input-limit allot
: tib-setup   input-buffer 'tib ! ;
: refill   tib-setup tib input-limit accept #tib ! 0 >in ! -1 ;

( REPL )
: prompt   ."  ok" cr ;
: evaluate-buffer   begin >in @ #tib @ < while evaluate1 repeat ;
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
