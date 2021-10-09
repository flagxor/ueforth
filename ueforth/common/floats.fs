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

: f= ( r r -- f ) f- f0= ;
: f< ( r r -- f ) f- f0< ;
: f> ( r r -- f ) fswap f< ;
: f<> ( r r -- f ) f= 0= ;
: f<= ( r r -- f ) f> 0= ;
: f>= ( r r -- f ) f< 0= ;

4 constant sfloat
: sfloats ( n -- n*4 ) sfloat * ;
: sfloat+ ( a -- a ) sfloat + ;
: sf, ( r -- ) here sf! sfloat allot ;

: afliteral ( r -- ) ['] DOFLIT , sf, align ;
: fliteral   afliteral ; immediate

: fconstant ( r "name" ) create sf, align does> sf@ ;
: fvariable ( "name" ) create sfloat allot align ;

3.14159265359e fconstant pi

: fsqrt ( r -- r ) 1e 20 0 do fover fover f/ f+ 0.5e f* loop fnip ;

6 value precision
: set-precision ( n -- ) to precision ;

internals definitions
: #f+s ( r -- ) fdup precision 0 ?do 10e f* loop
                precision 0 ?do fdup f>s 10 mod [char] 0 + hold 0.1e f* loop
                [char] . hold fdrop f>s #s ;
transfer doflit
forth definitions internals

: #fs ( r -- ) fdup f0< if fnegate #f+s [char] - hold else #f+s then ;
: f. ( r -- ) <# #fs #> type space ;

forth definitions
