( Sockets )
posix definitions

z" socket" 3 sysfunc socket
z" bind" 3 sysfunc bind
z" listen" 2 sysfunc listen
z" connect" 3 sysfunc connect
z" accept" 3 sysfunc accept
z" poll" 3 sysfunc poll
z" setsockopt" 5 sysfunc setsockopt

1 constant SOCK_STREAM
2 constant AF_INET
16 constant sizeof(sockaddr_in)
1 constant SOL_SOCKET
2 constant SO_REUSEADDR

forth definitions
