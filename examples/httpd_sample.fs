#! /usr/bin/env ueforth

\ Copyright 2022 Bradley D. Nelson
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
