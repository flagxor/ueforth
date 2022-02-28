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

also windows
s" \\.\COM3" r/w open-file throw constant remote
: remote-type ( a n -- ) remote write-file throw ;
: remote-emit ( ch -- ) >r rp@ 1 remote-type rdrop ;
: remote-key ( -- ch|0 ) 0 >r rp@ 1 remote read-file throw 0= throw r> ;
: remote-key? remote 0 WaitForSingleObject 0= ;
: terminal begin remote-key? if remote-key emit then
                 key? if key remote-emit then again ;
