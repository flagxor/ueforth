also posix
s" /dev/ttyS3" r/w O_NONBLOCK or open-file throw constant remote
: remote-type ( a n -- ) remote write-file throw ;
: remote-emit ( ch -- ) >r rp@ 1 remote-type rdrop ;
: remote-key ( -- ch|0 )
   0 >r rp@ 1 remote read-file dup EAGAIN = if rdrop 2drop 0 exit then
   throw if r> else rdrop 0 then ;
: terminal begin remote-key dup if emit else drop then
                 key? if key remote-emit then again ;
