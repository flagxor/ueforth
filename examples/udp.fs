#! /usr/bin/env ueforth

also sockets
also tasks
also posix

1024 constant max-msg
create msg max-msg allot
-1 value sockfd

variable len max-msg len !
: reader
    begin
        sockfd msg len 0 0 0 recvfrom
        dup 0 >= if
          msg swap type cr
        else drop then
        pause
    again
;
' reader 10 10 task reader-task

sockaddr incoming
sockaddr outgoing

: udp ( port -- )
  incoming ->port!
  AF_INET SOCK_DGRAM 0 socket to sockfd
  sockfd non-block throw
  sockfd incoming sizeof(sockaddr_in) bind throw
  reader-task start-task
  stdin non-block throw
;

: say ( port -- )
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
  ." <port> say <message text> ( send a message to a port )" cr
  cr
  ." Example: 9999 udp hear ( listener )" cr
  ." Example: 9998 udp 9999 say Can you hear me? ( sender )" cr
;
help quit
