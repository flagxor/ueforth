: (   41 parse drop drop ; immediate

( Useful Basic Compound Words )
: 2drop ( n n -- ) drop drop ;
: 2dup ( a b -- a b a b ) over over ;
: nip ( a b -- b ) swap drop ;
: rdrop ( r: n n -- ) r> r> drop >r ;
: */ ( n n n -- n ) */mod nip ;
: * ( n n -- n ) 1 */ ;
: /mod ( n n -- n n ) 1 swap */mod ;
: / ( n n -- n ) /mod nip ;
: mod ( n n -- n ) /mod drop ;
: invert ( n -- ~n ) -1 xor ;
: negate ( n -- -n ) invert 1 + ;
: - ( n n -- n ) negate + ;
: rot ( a b c -- c a b ) >r swap r> swap ;
: -rot ( a b c -- b c a ) swap >r swap r> ;
: < ( a b -- a<b ) - 0< ;
: > ( a b -- a>b ) swap - 0< ;
: = ( a b -- a!=b ) - 0= ;
: <> ( a b -- a!=b ) = 0= ;
: emit ( n -- ) >r rp@ 1 type rdrop ;
: bl 32 ;   : space bl emit ;
: nl 10 ;   : cr nl emit ;
: 1+ 1 + ;   : 1- 1 - ;
: 2* 2 * ;   : 2/ 2 / ;
: +! ( n a -- ) swap over @ + swap ! ;
: bye   0 sysexit ;

( Dictionary and Cells )
: here ( -- a ) 'heap @ ;
: allot ( n -- ) 'heap +! ;
: cell+ ( n -- n ) cell + ;
: cells ( n -- n ) cell * ;
: cell/ ( n -- n ) cell / ;
: aligned ( a -- a ) cell 1 - dup >r + r> invert and ;
: align   here aligned here - allot ;
: , ( n --  ) here ! cell allot ;
: c, ( ch -- ) here c! 1 allot ;

( Compilation State )
: [ 0 state ! ; immediate
: ] -1 state ! ; immediate

( Quoting Words )
: ' bl parse find ;
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

( Compound words requiring conditionals )
: min 2dup < if drop else nip then ;
: max 2dup < if nip else drop then ;
: abs ( n -- +n ) dup 0< if negate then ;

( Postpone - done here so we have ['] and IF )
: >flags ( xt -- flags ) cell - ;
: immediate? ( xt -- f ) >flags @ 1 and 0= 0= ;
: postpone ' dup immediate? if , else aliteral ['] , , then ; immediate

( Counted Loops )
: for   postpone >r postpone begin ; immediate
: next   postpone donext , ; immediate
: do   postpone swap postpone >r postpone >r here ; immediate
: i   postpone r@ ; immediate
: j   rp@ 3 cells - @ ;
: unloop   postpone rdrop postpone rdrop ; immediate
: +loop   postpone r> postpone + postpone r>
          postpone 2dup postpone >r postpone >r
          postpone < postpone 0= postpone until
          postpone unloop ; immediate
: loop   1 aliteral postpone +loop ; immediate

( Constants and Variables )
: constant create , does> @ ;
: variable create 0 , ;

( Stack Convience )
sp@ constant sp0
rp@ constant rp0
: depth ( -- n ) sp@ sp0 - cell/ ;

( Exceptions )
variable handler
: catch   sp@ >r handler @ >r rp@ handler ! execute r> handler ! r> drop 0 ;
: throw   handler @ rp! r> handler ! r> swap >r sp! drop r> ;
' throw 'throw !

( Numeric Output )
variable hld
: pad ( -- a ) here 80 + ;
: digit ( u -- c ) 9 over < 7 and + 48 + ;
: extract ( n base -- n c ) 0 swap um/mod swap digit ;
: <# ( -- ) pad hld ! ;
: hold ( c -- ) hld @ 1 - dup hld ! c! ;
: # ( u -- u ) base @ extract hold ;
: #s ( u -- 0 ) begin # dup while repeat ;
: sign ( n -- ) 0< if 45 hold then ;
: #> ( w -- b u ) drop hld @ pad over - ;
: str ( n -- b u ) dup >r abs <# #s r> sign #> ;
: hex ( -- ) 16 base ! ;
: decimal ( -- ) 10 base ! ;
: u. ( u -- ) <# #s #> space type ;
: . ( w -- ) base @ 10 xor if u. exit then str space type ;
: ? ( a -- ) @ . ;

( Strings )
: $.   r@ dup cell+ swap @ type r> dup @ aligned + cell+ >r ;
: ."   [char] " parse postpone $. dup , 0 do dup c@ c, 1+ loop drop align ; immediate
: $@   r@ dup cell+ swap @ r> dup @ aligned + cell+ >r ;
: s"   [char] " parse postpone $@ dup , 0 do dup c@ c, 1+ loop drop align ; immediate
: z$@   r@ cell+ r> dup @ aligned + cell+ >r ;
: z"   [char] " parse postpone z$@ dup 1+ , 0 do dup c@ c, 1+ loop drop 0 c, align ; immediate

( Examine Dictionary )
: >name ( xt -- a n ) 3 cells - dup @ swap over aligned - swap ;
: >link ( xt -- a ) 2 cells - @ ;
: >body ( xt -- a ) cell+ ;
: see. ( xt -- ) >name type space ;
: see-one ( xt -- xt+1 )
   dup @ dup ['] DOLIT = if drop cell+ dup @ . else see. then cell+ ;
: exit= ( xt -- ) ['] exit = ;
: see-loop   >body begin see-one dup @ exit= until ;
: see   cr ['] : see.  ' dup see.  see-loop drop  ['] ; see.  cr ;
: words   last @ begin dup see. >link dup 0= until drop cr ;

( Input )
: accept ( a n -- n ) 0 swap begin 2dup < while
   key dup nl = if 2drop nip exit then
   >r rot r> over c! 1+ -rot swap 1+ swap repeat drop nip ;
200 constant input-limit
: tib ( -- a ) 'tib @ ;
create input-buffer   input-limit allot
: tib-setup   input-buffer 'tib ! ;
: refill   tib-setup tib input-limit accept #tib ! 0 >in ! -1 ;

( REPL )
: prompt   ."  ok" cr ;
: eval-line   begin >in @ #tib @ < while eval1 repeat ;
: query   begin ['] eval-line catch if ." ERROR" cr then prompt refill drop again ;
: ok   ." uEForth" cr prompt refill drop query ;
ok
