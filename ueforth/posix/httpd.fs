( HTTP Daemon )
vocabulary httpd   httpd definitions also sockets

1 constant max-connections
2048 constant chunk-size
create chunk chunk-size allot
0 value chunk-filled

-1 value sockfd   -1 value clientfd
sockaddr httpd-port   sockaddr client   variable client-len

: client-type ( a n -- ) clientfd write-file throw ;
: client-read ( -- n ) 0 >r rp@ 1 clientfd read-file throw 1 <> throw ;
: client-emit ( ch -- ) >r rp@ 1 client-type rdrop ;
: client-cr   13 client-emit nl client-emit ;

: handleClient
  clientfd close-file drop
  sockfd client client-len sockaccept
  dup 0< if drop exit then to clientfd
  chunk chunk-size 0 fill
  chunk chunk-size clientfd read-file throw to chunk-filled
;

: server ( port -- )
  httpd-port ->port!  ." Listening on port " httpd-port ->port@ . cr
  AF_INET SOCK_STREAM 0 socket to sockfd
  ( sockfd SOL_SOCKET SO_REUSEADDR 1 >r rp@ 4 setsockopt rdrop throw )
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
