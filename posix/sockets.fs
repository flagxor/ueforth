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

( Sockets )
vocabulary sockets   sockets definitions also posix

z" socket" 3 sysfunc socket
z" bind" 3 sysfunc bind
z" listen" 2 sysfunc listen
z" connect" 3 sysfunc connect
z" accept" 3 sysfunc sockaccept
z" poll" 3 sysfunc poll
z" setsockopt" 5 sysfunc setsockopt

z" send" 4 sysfunc send
z" sendto" 6 sysfunc sendto
z" sendmsg" 3 sysfunc sendmsg

z" recv" 4 sysfunc recv
z" recvfrom" 6 sysfunc recvfrom
z" recvmsg" 3 sysfunc recvmsg

z" gethostbyname" 1 sysfunc gethostbyname

1 constant SOCK_STREAM
2 constant SOCK_DGRAM
3 constant SOCK_RAW

2 constant AF_INET
16 constant sizeof(sockaddr_in)
1 constant SOL_SOCKET
2 constant SO_REUSEADDR

: bs, ( n -- ) dup 8 rshift c, c, ;
: s, ( n -- ) dup c, 8 rshift c, ;
: l, ( n -- ) dup s, 16 rshift s, ;
: sockaddr   create AF_INET s, 0 bs, 0 l, 0 l, 0 l, ;
: ->port@ ( a -- n ) 2 + >r r@ c@ 8 lshift r> 1+ c@ + ;
: ->port! ( n a --  ) 2 + >r dup 8 rshift r@ c! r> 1+ c! ;
: ->addr@ ( a -- n ) 4 + ul@ ;
: ->addr! ( n a --  ) 4 + l! ;
: ->h_addr ( hostent -- n ) 2 cells + 8 + @ @ ul@ ;
: ip# ( n -- n ) dup 255 and n. [char] . emit 8 rshift ;
: ip. ( n -- ) ip# ip# ip# 255 and n. ;

( Fixup return )
: sockaccept sockaccept sign-extend ;

only forth definitions
