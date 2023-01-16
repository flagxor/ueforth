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

( Lazy loaded HTTP Daemon )
: httpd r|

vocabulary httpd   httpd definitions
also sockets
also internals

1 constant max-connections
2048 constant chunk-size
create chunk chunk-size allot
0 value chunk-filled
256 constant body-chunk-size
create body-chunk body-chunk-size allot
0 value body-1st-read
0 value body-read

-1 value sockfd   -1 value clientfd
sockaddr httpd-port   sockaddr client   variable client-len

: client-type ( a n -- ) clientfd write-file throw ;
: client-read ( -- n ) 0 >r rp@ 1 clientfd read-file throw 1 <> throw ;
: client-emit ( ch -- ) >r rp@ 1 client-type rdrop ;
: client-cr   13 client-emit nl client-emit ;

: server ( port -- )
  httpd-port ->port!  ." Listening on port " httpd-port ->port@ . cr
  AF_INET SOCK_STREAM 0 socket to sockfd
(  sockfd SOL_SOCKET SO_REUSEADDR 1 >r rp@ 4 setsockopt rdrop throw )
  sockfd non-block throw
  sockfd httpd-port sizeof(sockaddr_in) bind throw
  sockfd max-connections listen throw
;

: upper ( ch -- ch )
  dup [char] a >= over [char] z <= and if 95 and then ;
: strcase= ( a n a n -- f )
  >r swap r@ <> if rdrop 2drop 0 exit then r>
  for aft
    2dup c@ upper swap c@ upper <> if rdrop 2drop 0 exit then
    1+ swap 1+ swap
  then next
  2drop -1
;

variable goal   variable goal#
: end< ( n -- f ) chunk-filled < ;
: in@<> ( n ch -- f ) >r chunk + c@ r> <> ;
: skipto ( n ch -- n )
   >r begin dup r@ in@<> over end< and while 1+ repeat rdrop ;
: skipover ( n ch -- n ) skipto 1+ ;
: eat ( n ch -- n a n ) >r dup r> skipover swap 2dup - 1- >r chunk + r> ;
: crnl= ( n -- f ) dup chunk + c@ 13 = swap 1+ chunk + c@ nl = and ;
: header ( a n -- a n )
  goal# ! goal ! 0 nl skipover
  begin dup end< while
    dup crnl= if drop chunk 0 exit then
    [char] : eat goal @ goal# @ strcase= if 1+ 13 eat rot drop exit then
    nl skipover
  repeat drop chunk 0
;
: content-length ( -- n )
  s" Content-Length" header s>number? 0= if 0 then ;
: body ( -- a n ) ( reads a part of body )
  body-1st-read if
    body-read content-length >= if
      0 0 exit
    then
    body-chunk body-chunk-size clientfd read-file throw dup +to body-read
    body-chunk swap exit
  then
  -1 to body-1st-read
  0 to body-read
  0 nl skipover
  begin dup end< while
    dup crnl= if
      2 + chunk-filled over - swap chunk + swap
      dup +to body-read exit
    then
    nl skipover
  repeat drop chunk 0
;
: completed? ( -- f )
  0 begin dup end< while
    dup crnl= if drop -1 exit then
    nl skipover
  repeat drop 0
;
: read-headers
  0 to body-1st-read
  0 to chunk-filled
  begin completed? 0= while
    chunk chunk-filled + chunk-size chunk-filled -
      clientfd read-file throw +to chunk-filled
  repeat
;

: handleClient
  clientfd close-file drop
  -1 to clientfd
  sockfd client client-len sockaccept
  dup 0< if drop 0 exit then
  to clientfd
  chunk chunk-size erase
  read-headers
  -1
;

: hasHeader ( a n -- f ) 2drop header 0 0 strcase= 0= ;
: method ( -- a n ) 0 bl eat rot drop ;
: path ( -- a n ) 0 bl skipover bl eat rot drop ;
: send ( a n -- ) client-type ;

: response ( mime$ result$ status -- )
  s" HTTP/1.0 " client-type <# #s #> client-type
  bl client-emit client-type client-cr
  s" Content-type: " client-type client-type client-cr
  client-cr ;
: ok-response ( mime$ -- ) s" OK" 200 response ;
: bad-response ( -- ) s" text/plain" s" Bad Request" 400 response ;
: notfound-response ( -- ) s" text/plain" s" Not Found" 404 response ;

only forth definitions
httpd
| evaluate ;
