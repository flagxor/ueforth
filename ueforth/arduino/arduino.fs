( Set up Basic I/O )
internals definitions
: arduino-bye   0 terminate ;
' arduino-bye is bye
: arduino-type ( a n -- ) Serial.write drop ;
' arduino-type is type
: arduino-key ( -- n )
   begin Serial.available if 0 >r rp@ 1 Serial.readBytes drop r> dup 13 <> if exit then then again ;
' arduino-key is key
: arduino-key? ( -- n ) Serial.available ;
' arduino-key? is key?
forth definitions

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

( Setup entry )
: ok   ." ESP32forth v{{VERSION}} - rev {{REVISION}}" cr prompt refill drop quit ;
