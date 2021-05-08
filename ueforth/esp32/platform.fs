( Add a yielding task so pause yields )
internals definitions
transfer{ yield raw-yield }transfer
' raw-yield 100 100 task yield-task
yield-task start-task
forth definitions

( Set up Basic I/O )
internals definitions
: esp32-bye   0 terminate ;
' esp32-bye is bye
: serial-type ( a n -- ) Serial.write drop ;
' serial-type is type
: serial-key ( -- n )
   begin pause Serial.available until 0 >r rp@ 1 Serial.readBytes drop r> ;
' serial-key is key
: serial-key? ( -- n ) Serial.available ;
' serial-key? is key?
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
