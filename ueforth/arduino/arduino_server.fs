( Server Terminal )

also streams also WebServer also WiFi
vocabulary web-interface   also web-interface definitions

: ip# dup 255 and n. [char] . emit 256 / ;
: ip. ( n -- ) ip# ip# ip# 255 and . ;

r|
<!html>
<head>
<title>esp32forth</title>
<style>
body {
  padding: 5px;
  background-color: #111;
  color: #2cf;
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
}
</style>
</head>
<h2>uEforth</h2>
<link rel="icon" href="data:,">
<body>
Upload File: <input id="filepick" type="file" name="files[]"></input><br/>
<button onclick="ask('hex')">hex</button>
<button onclick="ask('decimal')">decimal</button>
<button onclick="ask('words')">words</button>
<button onclick="ask('low led pin')">LED OFF</button>
<button onclick="ask('high led pin')">LED ON</button>
<br/>
<textarea id="output" readonly></textarea>
<input id="prompt" type="prompt"></input><br/>
<script>
var prompt = document.getElementById('prompt');
var filepick = document.getElementById('filepick');
var output = document.getElementById('output');
function httpPost(url, items, callback) {
  var fd = new FormData();
  for (k in items) {
    fd.append(k, items[k]);
  }
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
  r.send(fd);
}
function ask(cmd, callback) {
  httpPost('/input',
           {cmd: cmd + '\\n'}, function(data) {
    if (data !== null) { output.value += data; }
    output.scrollTop = output.scrollHeight;  // Scroll to the bottom
    if (callback !== undefined) { callback(); }
  });
}
prompt.onkeyup = function(event) {
  if (event.keyCode === 13) {
    event.preventDefault();
    ask(prompt.value);
    prompt.value = '';
  }
};
filepick.onchange = function(event) {
  if (event.target.files.length > 0) {
    var reader = new FileReader();
    reader.onload = function(e) {
      var parts = e.target.result.split('\\n');
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
  ask('');
  prompt.focus();
};
</script>
| s>z constant index-html

variable webserver
20000 constant out-size
200 stream input-stream
out-size stream output-stream
create out-string out-size 1+ allot align

: handle-index
   index-html z>s nip webserver @ WebServer.setContentLength
   200 z" text/html" index-html webserver @ WebServer.send
;

: handle-input
   z" cmd" webserver @ WebServer.hasArg if
     z" cmd" webserver @ WebServer.arg input-stream >stream pause
     out-string out-size output-stream stream>
     200 z" text/plain" out-string webserver @ WebServer.send
   else
     500 z" text/plain" z" Missing Input" webserver @ WebServer.send
   then
;

: serve-type ( a n -- ) output-stream >stream ;
: serve-key ( -- n ) input-stream stream>ch ;

: do-serve
   80 WebServer.new webserver !
   z" /webui" ['] handle-index webserver @ WebServer.on
   z" /" ['] handle-index webserver @ WebServer.on
   z" /input" ['] handle-input webserver @ WebServer.on
   webserver @ WebServer.begin
   begin
     webserver @ WebServer.handleClient
     1 ms
     yield
   again
;

' do-serve 1000 1000 task webserver-task

: serve
   ['] serve-type is type
   ['] serve-key is key
   webserver-task start-task
;

: login ( z z -- )
   WIFI_MODE_STA Wifi.mode
   WiFi.begin begin WiFi.localIP 0= while 100 ms repeat WiFi.localIP ip. cr
   z" forth" MDNS.begin if ." MDNS started" else ." MDNS failed" then cr ;

also forth definitions

: webui ( z z -- ) login serve ;

only forth definitions
