( Sockets )
vocabulary sockets   sockets definitions also posix

z" socket" 3 sysfunc socket
z" bind" 3 sysfunc bind
z" listen" 2 sysfunc listen
z" connect" 3 sysfunc connect
z" accept" 3 sysfunc sockaccept
z" poll" 3 sysfunc poll
z" setsockopt" 5 sysfunc setsockopt

1 constant SOCK_STREAM
2 constant AF_INET
16 constant sizeof(sockaddr_in)
1 constant SOL_SOCKET
2 constant SO_REUSEADDR

: bs, ( n -- ) dup 256 / c, c, ;
: s, ( n -- ) dup c, 256 / c, ;
: l, ( n -- ) dup s, 65536 / s, ;
: sockaddr   create AF_INET s, 0 bs, 0 l, 0 l, 0 l, ;
: ->port@ ( a -- n ) 2 + >r r@ c@ 256 * r> 1+ c@ + ;
: ->port! ( n a --  ) 2 + >r dup 256 / r@ c! r> 1+ c! ;

forth definitions
