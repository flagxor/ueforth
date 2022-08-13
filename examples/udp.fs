#! /usr/bin/env ueforth

also sockets
also tasks

1024 constant max-msg
create msg max-msg allot
variable len max-msg len !
-1 value sockfd

sockaddr incoming
sockaddr outgoing

sockaddr received
variable received-len sizeof(sockaddr_in) received-len !

: reader
    begin
        sockfd msg len 0 received received-len recvfrom
        dup 0 >= if
          received ->addr@ ip. ." :" received ->port@ . space space msg swap type cr
        else drop then
        pause
    again
;
' reader 10 10 task reader-task

: udp ( port -- )
  incoming ->port!
  AF_INET SOCK_DGRAM 0 socket to sockfd
  sockfd non-block throw
  sockfd incoming sizeof(sockaddr_in) bind throw
  reader-task start-task
;

: say ( port -- "host" )
  bl parse s>z gethostbyname ->h_addr outgoing ->addr!
  outgoing ->port!
  sockfd tib >in @ + #tib @ >in @ - 
    0 outgoing sizeof(sockaddr_in) sendto drop
  #tib @ >in !
;

: hear begin pause again ;

: help
  ." USAGE INSTRUCTIONS" cr
  ." ------------------" cr
  ." <port> udp ( open UDP connection on port )" cr
  ." hear ( wait for messages on udp port and print then )" cr
  ." <port> say <host> <message text> ( send a message to a port )" cr
  cr
  ." Example: 9999 udp hear ( listener )" cr
  ." Example: 9998 udp 9999 say localhost Can you hear me? ( sender )" cr
;
help quit
