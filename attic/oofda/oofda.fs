\ Copyright 2023 Bradley D. Nelson
\
\ Licensed under the Apache License, Version 2.0 (the "License");
\ you may not use this file except in compliance with the License.
\ You may obtain a copy of the License at
\
\     http://www.apache.org/licenses/LICENSE-2.0
\
\  Unless required by applicable law or agreed to in writing, software
\  distributed under the License is distributed on an "AS IS" BASIS,
\  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
\  See the License for the specific language governing permissions and
\  limitations under the License.

defined? oofda-max-methods 0= [IF]
  100 constant oofda-max-methods
[THEN]

vocabulary classing also classing definitions also forth

variable 'this   : this ( -- o ) 'this @ ;
variable methods   variable last-method
: new-method ( ".name" -- xt )
   methods @ oofda-max-methods >= throw
   create methods @ , 1 methods +! latestxt
   does> this >r swap ( save this ) 'this ! ( switch it )
         dup last-method ! ( save last method )
         @ cells this @ + @ execute ( invoke method )
         r> 'this ! ( restore this ) ;
: method ( ".name" -- xt )
   current @ >r also forth definitions
   >in @ bl parse find dup if
     nip
   else
     drop >in !  new-method
   then
   previous r> current ! ;
: m# ( "name" -- n ) method >body @ ;
: m: ( "name" ) method drop ;   m: .construct  ( make this 0 )
: m! ( xt n class ) swap 3 + cells + ! ;
: field' ( "name" -- n ) ' >body @ ;

: nop-construct ;
m: .fallback
: undefined   last-method @ 2 cells - ( body> ) this .fallback ;
: error-fallback ( xt -- ) ." Undefined method: " >name type cr throw -1 ;
: blank-vtable   oofda-max-methods 0 do ['] undefined , loop ;
create ClassClass
  here 3 cells + ,              ( vtable* )
  0 ,                           ( parent )
  oofda-max-methods 3 + cells , ( size )
  blank-vtable                  ( vtable[] )

m: .size   m: .grow   m: .vtable   m: .parent   m: .getClass
:noname ( xt n ) this m! ; m# .setMethod ClassClass m!

: create ( "name" ) create this .size , does> @ this + ;
: variable ( "name" ) create this .size , cell this .grow does> @ this + ;
: value ( "name" ) create this .size , cell this .grow does> @ this + @ ;
: to ( n -- "name" ) field' postpone literal postpone this postpone +
                     postpone ! ; immediate
: +to ( n -- "name" ) field' postpone literal postpone this postpone +
                      postpone +! ; immediate
: dosuper ( n -- ) this ClassClass .getClass .parent .vtable + @ execute ;
: super ( "method" ) field' cells postpone literal postpone dosuper ; immediate

: : ( "name" ) m# :noname ;
: ;   postpone ; swap this .setMethod ; immediate

: defining ( cls -- ) 'this ! current @ also classing definitions ;

definitions

m: .new   m: .inherit
: class   create ClassClass .new defining ;
: end-class   previous current ! 0 'this ! ;
: extends   ' execute this .inherit ;
: extend   ' execute defining ;
: ClassClass ( -- cls ) ClassClass ;

previous previous definitions

extend ClassClass
  : .parent ( -- a ) this cell+ @ ;
  : .setParent ( a -- ) this cell+ ! ;
  : .size& ( -- a ) this 2 cells + ;
  : .size ( -- n ) this .size& @ ;
  : .setSize ( -- n ) this .size& ! ;
  : .grow ( n -- ) this .size + this .setSize ;
  : .vtable ( -- a ) this 3 cells + ;
  : .getClass ( o -- cls ) @ 3 cells - ;
  : .allocate ( n -- a ) here swap allot ;
  : .getName ( -- a n ) this 2 cells - >name ;
  : .getMethod ( n -- xt ) cells this .vtable + @ ;
  : .construct   0 this .setParent
                 cell this .setSize
                 oofda-max-methods 0 do ['] undefined i this .setMethod loop
                 ['] error-fallback [ m# .fallback ] literal this .setMethod
                 ['] nop-construct [ m# .construct ] literal this .setMethod ;
  : .setup ( -- cls ) this .size this .allocate
                      dup this .size 0 fill
                      this .vtable over ! ;
  : .new ( -- cls ) this .setup
                    dup >r .construct r> ;
  : .inherit ( cls -- ) dup this .setParent
                        .size& this .size& oofda-max-methods 1+ cells cmove ;
end-class
