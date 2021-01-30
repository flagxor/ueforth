s" \\.\COM3" r/w open-file throw constant remote
: remote-type ( a n -- ) remote write-file throw ;
: remote-emit ( ch -- ) >r rp@ 1 remote-type rdrop ;
: remote-key ( -- ch|0 ) 0 >r rp@ 1 remote read-file throw 0= throw r> ;
: remote-key? remote 0 WaitForSingleObject 0= ;
: terminal begin remote-key? if remote-key emit then
                 key? if key remote-emit then again ;
