( HTTP Daemon )
include posix/sockets.fs

vocabulary httpd   httpd definitions also posix

1 constant max-connections
2048 constant chunk-size
create chunk chunk-size allot
0 value chunk-filled

8080 constant port
-1 value sockfd   -1 value clientfd
: bs, ( n -- ) dup 256 / c, c, ;
: s, ( n -- ) dup c, 256 / c, ;
: l, ( n -- ) dup s, 65536 / s, ;
create httpd-port   AF_INET s, port bs, 0 l, 0 ,
create client   sizeof(sockaddr_in) allot   variable client-len

defer broker

: client-type ( a n -- ) clientfd -rot write 0< if 2drop broker then ;
: client-read ( -- n ) 0 >r clientfd rp@ 1 read 0< if rdrop broker then r> ;
: client-emit ( ch -- ) >r rp@ 1 client-type rdrop ;
: client-cr   13 client-emit nl client-emit ;

: connection ( n -- )
  dup 0< if drop exit then to clientfd
  clientfd chunk chunk-size read to chunk-filled
  chunk chunk-filled type cr
  s" HTTP/1.0 200 OK" client-type client-cr
  s" Content-type: text/html" client-type client-cr
  client-cr
  s" <!DOCTYPE html>" client-type client-cr
  s" <h1>Testing!</h1>" client-type client-cr
  s" <p>This is a test.</p>" client-type client-cr
  clientfd close drop
;

: broker-connection
  begin
    ." Listening on port " port . cr
    sockfd client client-len accept
    ." Connected: " dup . cr connection
  again ;
' broker-connection is broker

: server
  AF_INET SOCK_STREAM 0 socket to sockfd
  sockfd SOL_SOCKET SO_REUSEADDR 1 >r rp@ 4 setsockopt rdrop throw
  sockfd httpd-port sizeof(sockaddr_in) bind throw
  sockfd max-connections listen throw   broker ;

only forth definitions

httpd server
