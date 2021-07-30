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

( Camera Server )
DEFINED? camera [IF]

vocabulary camera-server   camera-server definitions
also camera also httpd

r|
<!DOCTYPE html>
<body>
<img id="pic">
<script>
var pic = document.getElementById('pic');
function httpPost(url, callback) {
  var r = new XMLHttpRequest();
  r.responseType = 'blob';
  r.onreadystatechange = function() {
    if (this.readyState == XMLHttpRequest.DONE) {
      if (this.status === 200) {
        callback(this.response);
      } else {
        callback(null);
      }
    }
  };
  r.open('POST', url);
  r.send();
}
function Frame() {
  httpPost('./image', function(r) {
    if (r !== null) {
      try {
        pic.src = URL.createObjectURL(r);
      } catch (e) {
      }
    }
    setTimeout(Frame, 30);
  });
}
Frame();
</script>
| constant index-html# constant index-html

: handle-index
   s" text/html" ok-response
   index-html index-html# send
;

: handle-image
  s" image/jpeg" ok-response
  esp_camera_fb_get dup dup @ swap cell+ @ send
  esp_camera_fb_return
;

: handle1
  handleClient
  s" /" path str= if handle-index exit then
  s" /image" path str= if handle-image exit then
  notfound-response
;

: do-serve    begin ['] handle1 catch drop pause again ;

: server ( port -- )
   server
   camera-config esp_camera_init throw
   do-serve
;

only forth definitions

[THEN]
