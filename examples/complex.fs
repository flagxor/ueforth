#! /usr/bin/env ueforth

\ Copyright 2024 Bradley D. Nelson
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

also recognizers also internals

: azliteral   fswap afliteral afliteral ;
: find-char ( a n ch -- a )
  swap for aft over c@ over = if drop rdrop exit then >r 1+ r> then next
  2drop 0 ;
: iparts? { a n -- 0 | a n a n -1 }
  a n [char] i find-char dup 0= if exit then { m }
  m 1+ n 1- m a - -
  a m a - 
  -1
;
: rec-z ( a n -- z addr1 | addr2 )
  iparts? 0= if rectype-none exit then
  2dup s>number? if s>f 2drop else s>float? 0= if rectype-none exit then then
  2dup s>number? if s>f 2drop else s>float? 0= if rectype-none exit then then
  ['] azliteral rectype-num
;
' rec-z +recognizer

: z@ ( a -- z ) dup sf@ sfloat+ sf@ ;
: z! ( a -- z ) dup sfloat+ sf! sf! ;
: z, ( z -- ) fswap sf, sf, ;
: zconstant   create z, does> z@ ;
: zvariable   create 0i0 z, ;

: f>r   r> fp@ ul@ fdrop >r >r ;
: r>f   r> r> fdup fp@ l! >r ;
: -frot   frot frot ;
: zdup   fover fover ;
: zswap    f>r fswap f>r fswap r>f r>f fswap f>r fswap r>f ;
: zover    f>r f>r zdup r>f r>f zswap ;
: 2zdup   zover zover ;

: z. ( z -- ) fswap <# #fs #> type ." i" <# #fs #> type space ;

: z+ ( z z -- z ) f>r fswap f>r f+ r>f r>f f+ ;
: z- ( z z -- z ) f>r fswap f>r f- r>f r>f f- ;
: z* ( z z -- z ) 2zdup -frot f* f>r f* r>f f+ f>r
                        frot f* f>r f* r>f f- r>f ;
: zlen ( z -- f ) fdup f* fswap fdup f* f+ ;
: 1/z ( z -- z ) zdup zlen fdup f>r fswap f>r
                 f/ r>f fnegate r>f f/ ;
: z/ ( z z -- z ) 1/z z* ;

previous previous
