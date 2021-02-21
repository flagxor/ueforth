( WebServer )
include posix/sockets.fs

vocabulary telnetd   telnetd definitions also posix

8080 constant port
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

telnetd server
