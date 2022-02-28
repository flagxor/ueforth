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

( pthreads )
posix definitions also internals

z" libpthread.so" shared-library pthread
  z" pthread_create" 4 pthread pthread_create
  z" pthread_join" 2 pthread pthread_join
  z" pthread_exit" 1 pthread pthread_exit

'sys 11 cells + @ constant forth_run

: ++! ( n a ) cell+ dup >r ! r> ;
: >entry ( xt sp rp -- rp ) here >r rot , ['] yield , ++! r> swap ++! ;

: thread ( xt dstack rstack -- tid )
  here >r cells allot here cell+ >r cells allot r> r> >entry ( rinit )
  0 >r rp@ ( rinit tid )
  0 rot forth_run swap ( tid attr forth_run rinit )
  pthread_create throw r> ;
: join ( tid -- ) 0 pthread_join throw ;

only forth definitions

