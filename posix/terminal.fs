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

also posix also termios
s" /dev/ttyS3" r/w O_NONBLOCK or open-file throw constant remote
256 constant ICRNL
create remote-termios sizeof(termios) allot
remote remote-termios tcgetattr drop
remote-termios .c_lflag l@ ICRNL INVERT and remote-termios .c_lflag l!
remote TCSAFLUSH remote-termios tcsetattr drop
: remote-type ( a n -- ) remote write-file throw ;
: remote-emit ( ch -- ) >r rp@ 1 remote-type rdrop ;
: remote-key ( -- ch|0 )
   0 >r rp@ 1 remote read-file dup EAGAIN = if rdrop 2drop 0 exit then
   throw if r> else rdrop 0 then ;
: terminal begin remote-key dup if emit else drop then
                 key? if key remote-emit then again ;
