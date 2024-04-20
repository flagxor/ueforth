#! /usr/bin/env ueforth

also sockets

." GETTING HOST" cr

z" google.com" gethostbyname constant google.com

." SETTING UP ADDRESS" cr

sockaddr googleaddr

80 googleaddr ->port!
google.com ->h_addr googleaddr ->addr!
google.com ->h_addr ip. cr

." CREATING SOCKET" cr

AF_INET SOCK_STREAM 0 socket value sock

." CONNECTING..." cr

sock googleaddr sizeof(sockaddr_in) connect throw

." CONNECTED" cr

s" GET / HTTP/1.0" sock write-file throw

\ : semit ( ch s -- ) swap >r rp@ swap 1 swap write-file throw rdrop ;
: semit ( ch s -- ) swap >r rp@ 1 0 send 0< throw rdrop ;
: scr   13 sock semit 10 sock semit ;
scr
scr

." REQUESTED" cr

here 100000 sock read-file throw constant len
here len type
sock close-file throw

bye
