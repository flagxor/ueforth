( Set up Basic I/O )
internals definitions
: arduino-bye   0 terminate ;
' arduino-bye is bye
: arduino-type ( a n -- ) Serial.write drop ;
' arduino-type is type
: arduino-key ( -- n )
   begin Serial.available until 0 >r rp@ 1 Serial.readBytes drop r> ;
' arduino-key is key
forth definitions
: key? ( -- n ) Serial.available ;

( Map Arduino / ESP32 things to shorter names. )
: pin ( n pin# -- ) swap digitalWrite ;
: adc ( n -- n ) analogRead ;
: duty ( n n -- ) 255 min 8191 255 */ ledcWrite ;
: freq ( n n -- ) 1000 * 13 ledcSetup drop ;
: tone ( n n -- ) 1000 * ledcWriteTone drop ;

( Basic Ardiuno Constants )
0 constant LOW
1 constant HIGH
1 constant INPUT
2 constant OUTPUT
2 constant LED

( WiFi Modes )
0 constant WIFI_MODE_NULL
1 constant WIFI_MODE_STA
2 constant WIFI_MODE_AP
3 constant WIFI_MODE_APSTA

( Startup Setup )
-1 echo !
115200 Serial.begin
100 ms
-1 z" /spiffs" 10 SPIFFS.begin drop
led OUTPUT pinMode
high led pin
