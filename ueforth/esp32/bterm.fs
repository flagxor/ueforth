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

( Lazy loaded Bluetooth Serial Terminal )

: bterm r|
vocabulary bterm  bterm definitions
also bluetooth also internals
SerialBT.new constant bt
z" forth" 0 bt SerialBT.begin drop
esp_bt_dev_get_address hex 6 dump cr
: bt-type bt SerialBT.write drop ;
: bt-key
   begin bt SerialBT.available until 0 >r rp@ 1 bt SerialBT.readBytes drop r> ;
: bt-on ['] bt-type is type ['] bt-key is key ;
: bt-off ['] serial-type is type ['] serial-key is key ;
only forth definitions
bterm 500 ms bt-on
| evaluate ;
