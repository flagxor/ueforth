#! /usr/bin/env ueforth
\ Copyright 2023 Bradley D. Nelson
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

." test5.fs" cr

needs ../lib/logging.fs

: @Inject ( "name" -- x )
   bl parse
   2dup s" Range" str= if 2drop 10 postpone literal exit then
   2dup s" Logger" str= if 2drop postpone ConsoleLogger postpone .new exit then
   -1 throw
; immediate

class Counter
  value log
  value range
  : .construct   @Inject Range to range
                 @Inject Logger to log ;
  : .doit ( n -- ) s" Counter at: " log .logString log
                   .logNumber log .cr ;
  : .run   range 0 do i 1+ this .doit loop ;
end-class

Counter .new .run

bye
