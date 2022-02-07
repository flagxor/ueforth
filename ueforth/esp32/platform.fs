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

( Add a yielding task so pause yields )
internals definitions
: yield-step   raw-yield yield ;
' yield-step 100 100 task yield-task
yield-task start-task
forth definitions

( Set up Basic I/O )
internals definitions also serial
: esp32-bye   0 terminate ;
: serial-type ( a n -- ) Serial.write drop ;
: serial-key ( -- n )
   begin pause Serial.available until 0 >r rp@ 1 Serial.readBytes drop r> ;
: serial-key? ( -- n ) Serial.available ;
also forth definitions
: default-type serial-type ;
: default-key serial-key ;
: default-key? serial-key? ;
' default-type is type
' default-key is key
' default-key? is key?
' esp32-bye is bye
only forth definitions

also ledc also serial also SPIFFS

( Map Arduino / ESP32 things to shorter names. )
: pin ( n pin# -- ) swap digitalWrite ;
: adc ( n -- n ) analogRead ;
: duty ( n n -- ) 255 min 8191 255 */ ledcWrite ;
: freq ( n n -- ) 1000 * 13 ledcSetup drop ;
: tone ( n n -- ) 1000 * ledcWriteTone drop ;

( Utilities )
: page   30 for cr next ;

( Basic Ardiuno Constants )
0 constant LOW
1 constant HIGH
1 constant INPUT
2 constant OUTPUT
2 constant LED

( Startup Setup )
-1 echo !
115200 Serial.begin
100 ms
-1 z" /spiffs" 10 SPIFFS.begin drop
led OUTPUT pinMode
high led pin

internals definitions also ESP
: esp32-stats
  getChipModel z>s type ."    "
  getCpuFreqMHz . ." MHz   "
  getChipCores .  ." cores   "
  getFlashChipSize . ." bytes flash" cr
  ."      System Heap: " getFreeHeap getHeapSize free. cr
  ."                   " getMaxAllocHeap . ." bytes max contiguous" cr ;
' esp32-stats internals boot-prompt !
only forth definitions

( Setup entry )
internals : ok   ." ESP32forth" raw-ok ; forth
