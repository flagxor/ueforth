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
