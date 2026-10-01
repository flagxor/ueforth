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

internals definitions

( Change default block source on arduino )
: arduino-default-use s" /spiffs/blocks.fb" open-blocks ;
' arduino-default-use is default-use

( Setup remember file )
: arduino-remember-filename   s" /spiffs/myforth" ;
' arduino-remember-filename is remember-filename

( Check for autoexec.fs and run if present.
  Failing that, try to revive save image. )
: autoexec
   ( Allow skip start files if key hit within 100 ms )
   10 for key? if rdrop exit then 10 ms next
   s" /spiffs/autoexec.fs" 2dup file-exists?
     if included else 2drop then
   ['] revive catch drop ;
' autoexec ( leave on the stack for fini.fs )

forth definitions

forth definitions
internals
\ dump tool
: dump ( addr len -- )
    cr cr ." --addr---  "
    ." 00 01 02 03 04 05 06 07 08 09 0A 0B 0C 0D 0E 0F  ------chars-----"
    2dup + { END_ADDR }             \ store latest address to dump
    swap { START_ADDR }             \ store START address to dump
    START_ADDR 16 / 16 * { 0START_ADDR } \ calc. addr for loop start
    16 / 1+ { LINES }
    base @ { myBASE }               \ save current base
    hex
    \ outer loop
    LINES 0 do
        0START_ADDR i 16 * +        \ calc start address for current line
        cr <# # # # #  [char] - hold # # # # #> type
        space space     \ and display address
        \ first inner loop, display bytes
        16 0 do
            \ calculate real address
            0START_ADDR j 16 * i + +
            ca@ <# # # #> type space \ display byte in format: NN
        loop 
        space
        \ second inner loop, display chars
        16 0 do
            \ calculate real address
            0START_ADDR j 16 * i + +
            \ display char if code in interval 32-127
            ca@     dup 32 < over 127 > or
            if      drop [char] . emit
            else    emit
            then
        loop 
    loop
    myBASE base !               \ restore current base
    cr cr
  ;

create crlf 13 C, 10 C,
: RECORDFILE  ( "filename" "filecontents" "<EOF>" -- )
    bl parse
    W/O CREATE-FILE throw >R
    BEGIN
        tib #tib accept
        tib over
        S" <EOF>" startswith?
        DUP IF
            swap drop
        ELSE
            swap
            tib swap
            R@ WRITE-FILE throw
            crlf 1+ 1 R@ WRITE-FILE throw
        THEN
    UNTIL
    R> CLOSE-FILE throw
;
: MAIN ( -- )
    s" /spiffs/main.fs"       included
  ;

