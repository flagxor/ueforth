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

( HTTP Daemon )

vocabulary httpd   httpd definitions also posix

1 constant max-connections
2048 constant chunk-size
create chunk chunk-size allot
0 value chunk-filled

-1 value sockfd   -1 value clientfd
: bs, ( n -- ) dup 256 / c, c, ;
: s, ( n -- ) dup c, 256 / c, ;
: l, ( n -- ) dup s, 65536 / s, ;
create httpd-port   AF_INET s, here 0 bs, 0 l, 0 , constant port
: port@ ( -- n ) port c@ 256 * port 1+ c@ + ;
: port! ( n --  ) dup 256 / port c! port 1+ c! ;
create client   sizeof(sockaddr_in) allot   variable client-len

: client-type ( a n -- ) clientfd -rot write 0< if 2drop 1 throw then ;
: client-read ( -- n ) 0 >r clientfd rp@ 1 read 0< if rdrop 1 throw then r> ;
: client-emit ( ch -- ) >r rp@ 1 client-type rdrop ;
: client-cr   13 client-emit nl client-emit ;

: handleClient
  clientfd close drop
  sockfd client client-len accept
  dup 0< if drop exit then to clientfd
  chunk chunk-size 0 fill
  clientfd chunk chunk-size read to chunk-filled
  ( chunk chunk-filled type cr )
;

: serve ( port -- )
  port!  ." Listening on port " port@ . cr
  AF_INET SOCK_STREAM 0 socket to sockfd
  sockfd SOL_SOCKET SO_REUSEADDR 1 >r rp@ 4 setsockopt rdrop throw
  sockfd httpd-port sizeof(sockaddr_in) bind throw
  sockfd max-connections listen throw
;

variable goal   variable goal#
: end< ( n -- f ) chunk-filled < ;
: in@<> ( n ch -- f ) >r chunk + c@ r> <> ;
: skipto ( n ch -- n )
   >r begin dup r@ in@<> over end< and while 1+ repeat rdrop ;
: skipover ( n ch -- n ) skipto 1+ ;
: eat ( n ch -- n a n ) >r dup r> skipover swap over over - 1- >r chunk + r> ;
: crnl= ( n -- f ) dup chunk + c@ 13 = swap 1+ chunk + c@ nl = and ;
: header ( a n -- a n )
  goal# ! goal ! 0 nl skipover
  begin dup end< while
    dup crnl= if drop chunk 0 exit then
    [char] : eat goal @ goal# @ str= if 2 + 13 eat rot drop exit then
    nl skipover
  repeat drop chunk 0
;
: body ( -- a n )
  0 nl skipover
  begin dup end< while
    dup crnl= if 2 + chunk-filled over - swap chunk + swap exit then
    nl skipover
  repeat drop chunk 0
;

: hasHeader ( a n -- f ) 2drop header 0 0 str= 0= ;
: method ( -- a n ) 0 bl eat rot drop ;
: path ( -- a n ) 0 bl skipover bl eat rot drop ;
: send ( a n -- ) client-type ;

: response ( mime$ result$ status mime$ -- )
  s" HTTP/1.0 " client-type <# #s #> client-type
  bl client-emit client-type client-cr
  s" Content-type: " client-type client-type client-cr
  client-cr ;
: ok-response ( mime$ -- ) s" OK" 200 response ;
: bad-response ( mime$ -- ) s" text/plain" s" Bad Request" 400 response ;
: notfound-response ( mime$ -- ) s" text/plain" s" Not Found" 404 response ;

only forth definitions
