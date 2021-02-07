also bluetooth also internals
SerialBT.new constant bt
z" ueforthbt" 0 bt SerialBT.begin .
esp_bt_dev_get_address hex 6 dump cr
: bt-type bt SerialBT.write drop ;
: bt-key
   begin bt SerialBT.available until 0 >r rp@ 1 bt SerialBT.readBytes drop r> ;
: bt-on ['] bt-type is type ['] bt-key is key ;
: bt-off ['] arduino-type is type ['] arduino-key is key ;
