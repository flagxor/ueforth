#! /usr/bin/env ueforth

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

s" What's the time?" askit >json a.

0 [IF]
HTTPClient
NetworkClientSecure.new constant nclient

: 2constant create , , does> dup cell+ @ swap @ ;

: slurp-file ( a n -- a n )
  r/o open-file throw >r
  r@ file-size throw ( sz )
  dup 1+ allocate throw swap ( data sz )
  2dup r@ read-file throw drop
  r> close-file throw
  2dup + 0 swap c!
;

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
