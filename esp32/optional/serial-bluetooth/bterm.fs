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

DEFINED? bluetooth [IF]

( Lazy loaded Bluetooth Serial Terminal )
: bterm r|
vocabulary bterm  bterm definitions
also bluetooth also internals also esp
120000 getFreeHeap - 0 max relinquish ( must have 110k for bluetooth )
z" forth xx:xx:xx:xx:xx:xx" constant name
( Create unique name for device )
base @ hex getEfuseMac
<# # # char : hold # # #> name 6 + swap cmove
<# # # char : hold # # char : hold # # char : hold # # #> name c + swap cmove
base !
SerialBT.new constant bt
name 0 bt SerialBT.begin drop
." Bluetooth Serial Terminal on: " name z>s type cr
: bt-type ( a n -- ) bt SerialBT.write drop ;
: bt-key? ( -- f ) bt SerialBT.available 0<> pause ;
: bt-key ( -- ch ) begin bt-key? until 0 >r rp@ 1 bt SerialBT.readBytes drop r> ;
: bt-on  ['] bt-type is type      ['] bt-key is key      ['] bt-key? is key? ;
: bt-off ['] default-type is type ['] default-key is key ['] default-key? is key? ;
only forth definitions
bterm 500 ms bt-on
| evaluate ;

[THEN]
