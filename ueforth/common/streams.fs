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

( Byte Stream / Ring Buffer )

vocabulary streams   streams definitions

: stream ( n "name" ) create 1+ dup , 0 , 0 , allot align ;
: >write ( st -- wr ) cell+ ;   : >read ( st -- rd ) 2 cells + ;
: >offset ( n st -- a ) 3 cells + + ;
: stream# ( sz -- n ) >r r@ >write @ r@ >read @ - r> @ mod ;
: full? ( st -- f ) dup stream# swap @ 1- = ;
: empty? ( st -- f ) stream# 0= ;
: wait-write ( st -- ) begin dup full? while pause repeat drop ;
: wait-read ( st -- ) begin dup empty? while pause repeat drop ;
: ch>stream ( ch st -- )
   dup wait-write
   >r r@ >write @ r@ >offset c!
   r@ >write @ 1+ r@ @ mod r> >write ! ;
: stream>ch ( st -- ch )
   dup wait-read
   >r r@ >read @ r@ >offset c@
   r@ >read @ 1+ r@ @ mod r> >read ! ;
: >stream ( a n st -- )
   swap for aft over c@ over ch>stream swap 1+ swap then next 2drop ;
: stream> ( a n st -- )
   begin over 1 > over empty? 0= and while
   dup stream>ch >r rot dup r> swap c! 1+ rot 1- rot repeat 2drop 0 swap c! ;

forth definitions
