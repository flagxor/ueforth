\ Copyright 2024 Bradley D. Nelson
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

needs arrays.fs
needs json.fs
needs slurp.fs

vocabulary gemini also json also arrays also gemini definitions

: askit { a n -- a }
   {{
     [[ _s" contents" [[
       {{
         [[ _s" parts" [[
           {{
             [[ _s" text" a n _s ]]
           }}
         ]] ]]
       }}
     ]] ]]
   }}
;

: reply-text ( a: a -- a )
  json> _s" candidates" dict@ 0 a@ _s" content" dict@ _s" parts" dict@ 0 a@ _s" text" dict@
;

: 0,c ( a -- a\0 ) 0 _c ,c ;

DEFINED? HTTPClient [IF]

_s" /spiffs/gemini_cert" slurp 0,c aconstant cacert

_s" https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key="
  _s" /spiffs/gemini_key" slurp json> _s" key" dict@ ,c 0,c aconstant url

also HTTPClient
NetworkClientSecure.new constant nclient
cacert top adrop nclient NetworkClientSecure.setCACert

: doquery ( a -- n )
  HTTPClient.new { session }
  url top adrop nclient session HTTPClient.beginNC 0= throw
  1 session HTTPClient.setFollowRedirects
  10 session HTTPClient.setRedirectLimit
  0 session HTTPClient.setReuse
  z" POST" top session HTTPClient.sendRequest adrop 200 <> throw
  session HTTPClient.getStreamPtr { result }
  begin result NetworkClient.available while
    pad 1024 result NetworkClient.available min result NetworkClient.readBytes
    pad swap type
  repeat
  session HTTPClient.delete
  cr
;

: ask
  nl parse askit >json doquery
  top 3 + top >count @ 3 - _s anip reply-text a. cr
;

previous

[THEN]

previous previous previous forth definitions
