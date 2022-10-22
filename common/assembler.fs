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

( Lazy loaded assembler/disassembler framework )
: assembler r|

current @
also internals
also asm definitions

-1 1 rshift invert constant high-bit
: odd? ( n -- f ) 1 and ;
: >>1 ( n -- n ) 1 rshift ;
: enmask ( n m -- n )
  0 -rot cell 8 * 1- for
    rot >>1 -rot
    dup odd? if
      over odd? if rot high-bit or -rot then
      swap >>1 swap
    then
    >>1
  next
  2drop
;
: demask ( n m -- n )
  0 >r begin dup while
    dup 0< if over 0< if r> 2* 1+ >r else r> 2* >r then then
    2* swap 2* swap
  repeat 2drop r>
;

variable length   variable pattern   variable mask
: bit! ( n a -- ) dup @ 2* rot 1 and or swap ! ;

: >opmask& ( xt -- a ) >body ;
: >next ( xt -- xt ) >body cell+ @ ;
: >inop ( a -- a ) >body 2 cells + @ ;
: >printop ( a -- a ) >body 3 cells + @ ;

variable operands
: for-operands ( xt -- )
   >r operands @ begin dup while r> 2dup >r >r execute r> >next repeat rdrop drop ;

: reset-operand ( xt -- ) >opmask& 0 swap ! ;
: reset   0 length !  0 mask !  0 pattern !  ['] reset-operand for-operands ;
: advance-operand ( xt -- ) >opmask& 0 swap bit! ;
: advance   ['] advance-operand for-operands ;

: skip  1 length +!  0 mask bit!  0 pattern bit!  advance ;
: bit ( n -- ) 1 length +!  1 mask bit!  pattern bit!  advance ;
: bits ( val n ) 1- for dup r@ rshift bit next drop ;
: o   0 bit ;   : l   1 bit ;

( struct: pattern next inop printop )
: operand ( inop printop "name" )
   create 0 , operands @ , latestxt operands ! swap , ,
   does> skip 1 swap +! ;
: names ( n "names"*n --) 0 swap 1- for dup constant 1+ next drop ;

: coden, ( val n -- ) 8 / 1- for dup code1, 8 rshift next drop ;

( struct: length pattern mask [xt pattern]* 0 )
variable opcodes
: op-snap ( xt -- ) dup >opmask& @ if dup , >opmask& @ , else drop then ;
: >xt ( a -- xt ) 2 cells - ;
: >length ( xt -- a ) >body cell+ @ ;
: >pattern ( xt -- a ) >body 2 cells + @ ;
: >mask ( xt -- a ) >body 3 cells + @ ;
: >operands ( xt -- a ) >body 4 cells + ;
: op ( "name" )
   create opcodes @ , latestxt opcodes !
          length @ , pattern @ , mask @ ,
          ['] op-snap for-operands 0 , reset
   does> >xt >r
         r@ >pattern
         0 r@ >operands begin dup @ while >r 1+ r> 2 cells + repeat
         swap for aft
           2 cells - dup >r swap >r dup cell+ @ >r @ >inop execute r> enmask r> or r>
         then next
         drop
         r> >length coden,
;

: for-ops ( xt -- )
   >r opcodes @ begin dup while r> 2dup >r >r execute r> >body @ repeat rdrop drop ;

: m@ ( a -- n ) 0 swap cell 0 do dup ca@ i 8 * lshift swap >r or r> 1+ loop drop ;
: m. ( n n -- ) base @ hex >r >r <# r> 1- for # # next #> type r> base ! ;
: sextend ( n n -- n ) cell 8 * swap - dup >r lshift r> arshift ;

variable istep
variable address
: matchit ( a xt -- a )
  >r dup m@ r@ >mask and r@ >pattern = if
    r@ >operands begin dup @ while
      >r dup m@ r@ cell+ @ demask r@ @ >printop execute r> 2 cells +
    repeat drop
    r@ see.
    r@ >length 8 / istep !
  then rdrop ;
: disasm1 ( a -- a )
  dup address ! dup . ."  --  " 0 istep ! ['] matchit for-ops
  istep @ 0= if 1 istep ! ." UNKNOWN!!!" then
  9 emit 9 emit ." -- " dup m@ istep @ m.
  istep @ +
  cr
;
: disasm ( a n -- ) for aft disasm1 then next drop ;

previous previous
also forth definitions
: assembler asm ;
previous
assembler
current !

| evaluate ;
