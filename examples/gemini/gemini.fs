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

: askit ( a: a -- a )
   >a
   {{
     [[ _s" contents" [[
       {{
         [[ _s" parts" [[
           {{
             [[ _s" text" a> ]]
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
  z" POST" top range session HTTPClient.sendRequest adrop 200 <> throw
  session HTTPClient.getStreamPtr { result }
  _s" "
  begin result NetworkClient.available while
    result NetworkClient.available STRING array
    top range result NetworkClient.readBytes drop ,c
  repeat
  session HTTPClient.delete
  cr
;

: snipped ( a -- a ) top 3 + top >count @ 3 - _s anip ;

: ask
  nl parse _s askit >json doquery
  snipped reply-text a. cr ;

: askjson ( a -- a )
   >a {{
     [[ _s" contents" [[
       {{
         [[ _s" parts" [[
           {{
             [[ _s" text" a> ]]
           }}
         ]] ]]
       }}
     ]] ]]
     [[ _s" generationConfig" {{
       [[ _s" response_mime_type" _s" application/json" ]]
     }} ]]
   }}
;

: asklist ( a -- a )
  _s"  using this JSON schema: list[str]" ,c askjson >json doquery
  snipped reply-text json> ;

previous

[THEN]

previous previous previous forth definitions
