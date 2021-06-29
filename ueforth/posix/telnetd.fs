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

( Telnet )
include posix/sockets.fs

vocabulary telnetd   telnetd definitions also posix

5555 constant port
-1 value sockfd   -1 value clientfd
: bs, ( n -- ) dup 256 / c, c, ;
: s, ( n -- ) dup c, 256 / c, ;
: l, ( n -- ) dup s, 65536 / s, ;
create telnet-port   AF_INET s, port bs, 0 l, 0 ,
create client   sizeof(sockaddr_in) allot   variable client-len

defer broker

: telnet-type ( a n -- ) clientfd -rot write 0< if 2drop broker then ;
: telnet-key ( -- n ) 0 >r clientfd rp@ 1 read 0< if rdrop broker then r> ;

: connection ( n -- )
  dup 0< if drop exit then to clientfd
  ['] telnet-key is key
  ['] telnet-type is type quit ;

: broker-connection
  rp0 rp! sp0 sp!
  begin
    ['] stdin-key is key   ['] stdout-write is type
    ." Listening on port " port . cr
    sockfd client client-len accept
    ." Connected: " dup . cr connection
  again ;
' broker-connection is broker

: server
  AF_INET SOCK_STREAM 0 socket to sockfd
  sockfd telnet-port sizeof(sockaddr_in) bind throw
  sockfd 10 listen throw   broker ;

only forth definitions

