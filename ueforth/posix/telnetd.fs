( Telnet )

vocabulary telnetd   telnetd definitions also sockets

-1 value sockfd   -1 value clientfd
sockaddr telnet-port   sockaddr client   variable client-len

defer broker

: telnet-emit' ( ch -- ) >r rp@ 1 clientfd write-file rdrop if broker then ;
: telnet-emit ( ch -- ) dup nl = if 13 telnet-emit' then telnet-emit' ;
: telnet-type ( a n -- ) for aft dup c@ telnet-emit 1+ then next drop ;
: telnet-key ( -- n ) 0 >r rp@ 1 clientfd read-file swap 1 <> or if rdrop broker then r> ;

: connection ( n -- )
  dup 0< if drop exit then to clientfd
  0 echo !
  ['] telnet-key is key
  ['] telnet-type is type quit ;

: broker-connection
  rp0 rp! sp0 sp!
  begin
    ['] default-key is key   ['] default-type is type
    -1 echo !
    ." Listening on port " telnet-port ->port@ . cr
    sockfd client client-len sockaccept
    ." Connected: " dup . cr connection
  again ;
' broker-connection is broker

: server ( port -- )
  telnet-port ->port!
  AF_INET SOCK_STREAM 0 socket to sockfd
  sockfd telnet-port sizeof(sockaddr_in) bind throw
  sockfd 1 listen throw   broker ;

only forth definitions
