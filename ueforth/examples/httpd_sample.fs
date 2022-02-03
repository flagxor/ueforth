#! /usr/bin/env ueforth

also httpd

: handle-index
  s" text/html" ok-response r|
<html>
  <body>Hi!</body>
</html>
| send ;

: handle1
  ." Got a request for: " path type cr
  path s" /" str= if handle-index exit then
  notfound-response
;

: run  8080 server
       begin handleClient if handle1 then pause again ;
run
