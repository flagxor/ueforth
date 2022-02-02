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

defer web-interface
:noname r~
httpd
also streams also httpd
vocabulary web-interface   also web-interface definitions

r|
<!html>
<head>
<title>esp32forth</title>
<style>
body {
  padding: 5px;
  background-color: #111;
  color: #2cf;
  overflow: hidden;
}
#prompt {
  width: 100%;
  padding: 5px;
  font-family: monospace;
  background-color: #ff8;
}
#output {
  width: 100%;
  height: 80%;
  resize: none;
  overflow-y: scroll;
  word-break: break-all;
}
</style>
<link rel="icon" href="data:,">
</head>
<body>
<h2>ESP32forth v7</h2>
Upload File: <input id="filepick" type="file" name="files[]"></input><br/>
<button onclick="ask('hex\n')">hex</button>
<button onclick="ask('decimal\n')">decimal</button>
<button onclick="ask('words\n')">words</button>
<button onclick="ask('low led pin\n')">LED OFF</button>
<button onclick="ask('high led pin\n')">LED ON</button>
<br/>
<textarea id="output" readonly></textarea>
<input id="prompt" type="prompt"></input><br/>
<script>
var prompt = document.getElementById('prompt');
var filepick = document.getElementById('filepick');
var output = document.getElementById('output');
function httpPost(url, data, callback) {
  var r = new XMLHttpRequest();
  r.onreadystatechange = function() {
    if (this.readyState == XMLHttpRequest.DONE) {
      if (this.status === 200) {
        callback(this.responseText);
      } else {
        callback(null);
      }
    }
  };
  r.open('POST', url);
  r.send(data);
}
setInterval(function() { ask(''); }, 300);
function ask(cmd, callback) {
  httpPost('/input', cmd, function(data) {
    if (data !== null) { output.value += data; }
    output.scrollTop = output.scrollHeight;  // Scroll to the bottom
    if (callback !== undefined) { callback(); }
  });
}
prompt.onkeyup = function(event) {
  if (event.keyCode === 13) {
    event.preventDefault();
    ask(prompt.value + '\n');
    prompt.value = '';
  }
};
filepick.onchange = function(event) {
  if (event.target.files.length > 0) {
    var reader = new FileReader();
    reader.onload = function(e) {
      var parts = e.target.result.replace(/[\r]/g, '').split('\n');
      function upload() {
        if (parts.length === 0) { filepick.value = ''; return; }
        ask(parts.shift(), upload);
      }
      upload();
    }
    reader.readAsText(event.target.files[0]);
  }
};
window.onload = function() {
  ask('\n');
  prompt.focus();
};
</script>
| constant index-html# constant index-html

variable webserver
2000 constant out-size
200 stream input-stream
out-size stream output-stream
create out-string out-size 1+ allot align

: handle-index
   s" text/html" ok-response
   index-html index-html# send
;

: handle-input
   body input-stream >stream pause
   out-string out-size output-stream stream>
   s" text/plain" ok-response
   out-string z>s send
;

: serve-type ( a n -- ) output-stream >stream ;
: serve-key ( -- n ) input-stream stream>ch ;

: handle1
  handleClient if
    s" /" path str= if handle-index exit then
    s" /input" path str= if handle-input exit then
    notfound-response
  then
;

: do-serve    begin handle1 pause again ;
' do-serve 1000 1000 task webserver-task

: server ( port -- )
   server
   ['] serve-key is key
   ['] serve-type is type
   webserver-task start-task
;

only forth definitions
web-interface
~ evaluate ; is web-interface

