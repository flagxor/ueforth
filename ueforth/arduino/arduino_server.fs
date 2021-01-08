( Server Terminal )
: n. ( n -- ) <# #s #> type ;  : ip# dup 255 and n. [char] . emit 256 / ;
: ip. ( n -- ) ip# ip# ip# 255 and . ;
: r| ( -- z ) [char] | parse s>z ;

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
<button onclick="ask('$100 init hush')">init</button>
<button onclick="ask('ride')">ride</button>
<button onclick="ask('blow')">blow</button>
<button onclick="ask('$50000 p0')">fore</button>
<button onclick="ask('$a0000 p0')">back</button>
<button onclick="ask('$10000 p0')">left</button>
<button onclick="ask('$40000 p0')">right</button>
<button onclick="ask('$90000 p0')">spin</button>
<button onclick="ask('0 p0')">stop</button>
<button onclick="ask('4 p0s')">LED</button>
<button onclick="ask('$24 ADC . $27 ADC . $22 ADC . $23 ADC .')">ADC</button>
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
| constant index-html

variable webserver

: handle-index
   ( ." Handling index.html content length" cr
     index-html z>s nip webserver @ WebServer.setContentLength )
   ." Handling index.html content length" cr
   200 z" text/html" index-html webserver @ WebServer.send
   ." Done! Handling index.html" cr
;

: handle-input
   ." Handling input" cr
   z" cmd" webserver @ WebServer.hasArg if
     ." hasarg" cr
     z" cmd" webserver @ WebServer.arg
     ." Got: " cr dup .
     2dup type cr
     ['] evaluate catch drop
     200 z" text/plain" z" nop" webserver @ WebServer.send
   else
     ." not hasarg" cr
     500 z" text/plain" z" Missing Input" webserver @ WebServer.send
   then
;

: serve
   80 WebServer.new webserver !
   z" /" ['] handle-index webserver @ WebServer.on
   z" /input" ['] handle-input webserver @ WebServer.on
   webserver @ WebServer.begin
   begin
     webserver @ WebServer.handleClient
     1 ms
     yield
   again
;

: wifi ( z z -- )
   WIFI_MODE_STA Wifi.mode
   WiFi.begin 1000 ms WiFi.localIP ip. cr
   z" ueforth" MDNS.begin if ." MDNS started" else ." MDNS failed" then cr ;
: webui ( z z -- ) wifi serve ; 
