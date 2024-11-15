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

r| What's the "time"?| askit >json a. cr cr
r| What's the "time"?| askit _s" contents" dict@ 0 a@ _s" parts" dict@ 0 a@ _s" text" dict@ >json a. cr

0 [IF]
HTTPClient
NetworkClientSecure.new constant nclient

: 2constant create , , does> dup cell+ @ swap @ ;

s" /spiffs/gemini_cert" slurp-file drop constant cacert
s" /spiffs/gemini_url" slurp-file drop constant url
s" /spiffs/question" slurp-file 2constant question

cacert nclient NetworkClientSecure.setCACert
." loaded cert:" cr
cacert z>s type cr

HTTPClient.new constant session
." created session" cr
." URL: " url z>s type cr
url nclient session HTTPClient.beginNC ." beginNC: " . cr
1 session HTTPClient.setFollowRedirects
10 session HTTPClient.setRedirectLimit
." set follow redirects and limit of 10" cr
." question: " question type
z" POST" question session HTTPClient.sendRequest ." POSTED: " . cr

session HTTPClient.getStreamPtr constant result
result NetworkClient.available ." available: " dup . cr

[THEN]

previous previous previous forth definitions
