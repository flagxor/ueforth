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
variable last-typer
: typer ( xt@ xt! sz "name" -- )
   create dup , 1 max cell min , , ,
   does> dup last-typer !  dup cell+ @ last-align !  @ ;
: sc@ ( a -- c ) c@ dup 127 > if 256 - then ;
' sc@ ' c! 1 typer i8
' c@  ' c! 1 typer u8
' sw@ ' w! 2 typer i16
' uw@ ' w! 2 typer u16
' sl@ ' l! 4 typer i32
' ul@ ' l! 4 typer u32
' @   ' !  8 typer i64  ( Wrong on 32-bit! )
' @   ' !  cell typer ptr
long-size cell = [IF]
  : long   ptr ;
[ELSE]
  : long   i32 ;
[THEN]

variable last-struct
: struct ( "name" ) 0 0 0 typer latestxt >body last-struct !
                    1 last-align ! ;
: align-by ( a n -- a ) 1- dup >r + r> invert and ;
: max! ( n a -- ) swap over @ max swap ! ;
: struct-align ( n -- )
  dup last-struct @ cell+ max!
  last-struct @ @ swap align-by last-struct @ ! ;
: field ( n "name" )
  last-align @ struct-align
  create last-struct @ @ ,   last-struct @ +!  last-typer @ ,
  does> @ + ;

: field-op ( n "name" -- )
  >r ' dup >body cell+ @ r> cells + @
  state @ if >r , r> , else >r execute r> execute then ;
: !field ( n "name" -- ) 2 field-op ; immediate
: @field ( "name" -- n ) 3 field-op ; immediate

forth definitions
