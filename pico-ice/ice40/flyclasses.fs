\ Copyright 2025 Bradley D. Nelson
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

vocabulary flyclasses   flyclasses definitions

0 value classes
1 value methods
0 value dispatch
0 value implementing

: flyclass   create classes , 1 +to classes does> @ ;
: method& ( m cls -- a ) classes mod methods * + cells dispatch + ;
: accrued ( -- a ) 0 implementing method& ;
: method   create methods , 1 +to methods does> @ over method& @ execute ;
: implementation ( cls -- ) to implementing ;
: >min ( a -- n ) cell+ @ ;         : >max ( a -- a ) @ ;
: >below ( a -- a ) 2 cells + @ ;   : >above ( a -- a) 3 cells + @ ;
: field ( min max -- "name" )
   create 2dup , , accrued @ , swap - 1+ accrued @ * dup , accrued !
   does> >r r@ >above mod r@ >below / r> >min + ;
: doput ( n o -- o "name" ) >r dup r@ >below mod swap r@ >above / r@ >above * +
                            swap r@ >max min r@ >min - r> >below * + ;
: put ( n o -- o "name" ) ' >body postpone literal postpone doput ; immediate
: extension ( cls -- ) 0 swap method& accrued methods cells cmove ;
: initiate   here to dispatch
             classes 1- for classes , methods 1- 1- for ['] abort , next next ;
: do:: ( o cls m -- ) swap method& @ execute ;
: :: ( o cls "name" -- ) ' >body @ postpone literal postpone do:: ; immediate
: m:   ' >body @ :noname ;
: ;m   postpone ; swap implementing method& ! ; immediate

forth definitions

