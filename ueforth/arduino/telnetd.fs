( Telnet )
vocabulary telnetd   telnetd definitions also sockets also internals

23 constant port
-1 value sockfd   -1 value clientfd
: bs, ( n -- ) dup 256 / c, c, ;
: s, ( n -- ) dup c, 256 / c, ;
: l, ( n -- ) dup s, 65536 / s, ;
create telnet-port  16 c, AF_INET c, port bs, 0 l, 0 l, 0 l,
create client   sizeof(sockaddr_in) allot   variable client-len

defer broker

: telnet-emit' ( ch -- ) >r rp@ 1 clientfd write-file rdrop if broker then ;
: telnet-emit ( ch -- ) dup nl = if 13 telnet-emit' then telnet-emit' ;
: telnet-type ( a n -- ) for aft dup c@ telnet-emit 1+ then next drop ;
: telnet-key ( -- n ) 0 >r rp@ 1 clientfd read-file if drop rdrop broker else drop then r> ;

: connection ( n -- )
  dup 0< if drop exit then to clientfd
  0 echo !
  ['] telnet-key is key
  ['] telnet-type is type quit ;

: broker-connection
  rp0 rp! sp0 sp!
  begin
    ['] arduino-key is key
    ['] arduino-type is type
    -1 echo !
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
