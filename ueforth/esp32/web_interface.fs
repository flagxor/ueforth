( Server Terminal )

also streams also WiFi also web-interface definitions

: ip# dup 255 and n. [char] . emit 256 / ;
: ip. ( n -- ) ip# ip# ip# 255 and . ;

also forth definitions

: login ( z z -- )
   WIFI_MODE_STA Wifi.mode
   WiFi.begin begin WiFi.localIP 0= while 100 ms repeat WiFi.localIP ip. cr
   z" forth" MDNS.begin if ." MDNS started" else ." MDNS failed" then cr ;
: webui ( z z -- ) login 80 server ;

only forth definitions
