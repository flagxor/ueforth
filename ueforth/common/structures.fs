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

( Vocabulary for building C-style structures )

vocabulary structures   structures definitions

variable last-align
: typer ( align sz "name" ) create , ,
                            does> dup cell+ @ last-align ! @ ;
1 1 typer i8
2 2 typer i16
4 4 typer i32
cell 8 typer i64
cell cell typer ptr
long-size long-size typer long

variable last-struct

: struct ( "name" ) 1 0 typer latestxt >body last-struct ! ;
: align-by ( a n -- a ) 1- dup >r + r> invert and ;
: struct-align ( n -- )
  dup last-struct @ cell+ @ max last-struct @ cell+ !
  last-struct @ @ swap align-by last-struct @ ! ;
: field ( n "name" )
  last-align @ struct-align
  create last-struct @ @ , last-struct @ +!
  does> @ + ;

forth definitions
