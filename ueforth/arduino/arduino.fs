( Map Arduino / ESP32 things to shorter names. )
: pin ( n n -- ) swap digitalWrite ;
: adc ( n -- n ) analogRead ;
: duty ( n n -- ) 255 ledcAnalogWrite ;
: freq ( n n -- ) 1000 * 13 ledcSetup drop ;
: tone ( n n -- ) 1000 * ledcWriteTone drop ;
