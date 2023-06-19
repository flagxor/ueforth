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

web definitions

: web-type ( a n -- ) web-type-raw if pause then ;
' web-type is type
: web-key ( -- n ) begin pause web-key-raw dup if exit then drop again ;
' web-key is key
: web-key? ( -- f ) pause web-key?-raw ;
' web-key? is key?
' web-terminate is terminate

0 value session?

: upload-file ( a n -- )
   upload-start
   begin yield upload-done? until
   upload-success? assert
;

: upload ( "filename" ) bl parse dup assert upload-file ;

: include-file { a n -- }
  0 0 a n session? getItem { len }
  here { buf } len allot
  buf len a n session? getItem len = assert
  buf len evaluate
;

: cat ( "filename" )
  bl parse { name name# }
  0 0 name name# session? getItem { len }
  here len name name# session? getItem len = assert
  here len type
;

: download ( "filename" )
  bl parse { name name# }
  0 0 name name# session? getItem { len }
  here len name name# session? getItem len = assert
  here len s" application/octet-stream"
    name name# raw-download
;

: ls
  session? keyCount 0 ?do
    pad 80 i session? getKey pad swap type cr
  loop
;

: rm   bl parse session? removeItem ;

: import
  s" _temp.fs" 2dup upload-file
  2dup >r >r include-file
  r> r> session? removeItem
;

: yielding  begin 50 ms yield again ;
' yielding 10 10 task yielding-task
yielding-task start-task

forth definitions
