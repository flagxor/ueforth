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

( Lazy loaded Server Terminal )

:noname [ ' web-interface >body @ ] literal execute
r|
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
web-interface
| evaluate ; is web-interface
: login web-interface forth r| login | evaluate ;
: webui web-interface forth r| webui | evaluate ;
