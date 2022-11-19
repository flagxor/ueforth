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

: upload-file ( a n -- )
   upload-start
   begin yield upload-done? until
   upload-success? assert
;

: upload ( "filename" ) bl parse dup assert upload-file ;

: include-file { a n -- }
  0 0 a n 0 getItem { len }
  here { buf } len allot
  buf len a n 0 getItem len = assert
  buf len evaluate
; 

: import  s" _temp.fs" 2dup upload-file include-file ;

forth definitions
