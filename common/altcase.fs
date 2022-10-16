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

( A WIP exploration of an optimized version of CASE. )
( NOTE: NOT YET FUNCTIONAL )
( TODO: complete this )

internals definitions
create case-buffer 400 cells allot
variable cases
variable default-target  variable default-fixup
: case, ( n -- ) case-buffer cases @ cells + ! 1 cases +! ;
forth definitions internals

( default num target fixup )

: CASE ( n -- )
  0 cases !  0 default-target !  0 default-fixup !
  postpone ahead postpone [ ; immediate
: ENDCASE   postpone ] postpone then
            case-buffer cases @ 3 / for aft
               dup @ . cell+
               dup @ . cell+
               dup @ . cell+ cr
            then next drop ; immediate
: OTHERWISE    here default-target ! postpone ] ; immediate
: OF ( n -- ) case, here case, postpone ] ; immediate
: ENDOF   postpone [ postpone ahead
          default-target @ default-fixup @ 0= and
          if default-fixup ! else case, then ; immediate

forth definitions

: test
  case
    1 of ." one" endof
    2 of ." two" endof
    3 of ." three" endof
    otherwise ." other" endof
  endcase
;

see test
