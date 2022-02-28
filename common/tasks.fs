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

( Cooperative Tasks )

vocabulary tasks   tasks definitions

variable task-list

forth definitions tasks also internals

: pause
  rp@ sp@ task-list @ cell+ !
  task-list @ @ task-list !
  task-list @ cell+ @ sp! rp!
;

: task ( xt dsz rsz "name" )
   create here >r 0 , 0 , ( link, sp )
   swap here cell+ r@ cell+ ! cells allot
   here r@ cell+ @ ! cells allot
   dup 0= if drop else
     here r@ cell+ @ @ ! ( set rp to point here )
     , postpone pause ['] branch , here 3 cells - ,
   then rdrop ;

: start-task ( t -- )
   task-list @ if
     task-list @ @ over !
     task-list @ !
   else
     dup task-list !
     dup !
   then
;

DEFINED? ms-ticks [IF]
  : ms ( n -- ) ms-ticks >r begin pause ms-ticks r@ - over >= until rdrop drop ;
[THEN]

tasks definitions
0 0 0 task main-task   main-task start-task
forth definitions
