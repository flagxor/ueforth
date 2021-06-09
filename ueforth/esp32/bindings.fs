( Migrate various words to separate vocabularies, and constants )

vocabulary Wire   Wire definitions
transfer{
  Wire.begin Wire.setClock Wire.getClock
  Wire.setTimeout Wire.getTimeout
  Wire.lastError Wire.getErrorText
  Wire.beginTransmission Wire.endTransmission
  Wire.requestFrom Wire.writeTransmission
  Wire.readTransmission Wire.write
  Wire.available Wire.read
  Wire.peek Wire.busy Wire.flush
}transfer
forth definitions

vocabulary WebServer   WebServer definitions
transfer{
  WebServer.arg WebServer.argi WebServer.argName
  WebServer.new WebServer.delete
  WebServer.begin WebServer.stop
  WebServer.on WebServer.hasArg
  WebServer.sendHeader WebServer.send WebServer.sendContent
  WebServer.method WebServer.handleClient
  WebServer.args WebServer.setContentLength
}transfer
forth definitions

vocabulary WiFi   WiFi definitions

transfer{
  WiFi.config
  WiFi.begin WiFi.disconnect
  WiFi.status
  WiFi.macAddress WiFi.localIP
  WiFi.mode
  WiFi.setTxPower WiFi.getTxPower
}transfer

( WiFi Modes )
0 constant WIFI_MODE_NULL
1 constant WIFI_MODE_STA
2 constant WIFI_MODE_AP
3 constant WIFI_MODE_APSTA

forth definitions

vocabulary SD_MMC   SD_MMC definitions
( SD_MMC.begin - TODO: causing issues pulled in )
transfer{
  SD_MMC.cardType
  SD_MMC.end
  SD_MMC.totalBytes SD_MMC.usedBytes
}transfer
forth definitions

vocabulary SPIFFS   SPIFFS definitions
transfer{
  SPIFFS.begin SPIFFS.end
  SPIFFS.format
  SPIFFS.totalBytes SPIFFS.usedBytes
}transfer
forth definitions

vocabulary ledc  ledc definitions
transfer{
  ledcSetup ledcAttachPin ledcDetachPin
  ledcRead ledcReadFreq
  ledcWrite ledcWriteTone ledcWriteNote
}transfer
forth definitions

vocabulary Serial   Serial definitions
transfer{
  Serial.begin Serial.end
  Serial.available Serial.readBytes
  Serial.write Serial.flush
}transfer
forth definitions

vocabulary sockets   sockets definitions
transfer{
  socket bind listen connect sockaccept select poll errno
}transfer
1 constant SOCK_STREAM
2 constant AF_INET
16 constant sizeof(sockaddr_in)
1 constant SOL_SOCKET
2 constant SO_REUSEADDR

: bs, ( n -- ) dup 256 / c, c, ;
: s, ( n -- ) dup c, 256 / c, ;
: l, ( n -- ) dup s, 65536 / s, ;
: sockaddr   create 16 c, AF_INET c, 0 bs, 0 l, 0 l, 0 l, ;
: ->port@ ( a -- n ) 2 + >r r@ c@ 256 * r> 1+ c@ + ;
: ->port! ( n a --  ) 2 + >r dup 256 / r@ c! r> 1+ c! ;

forth definitions

vocabulary interrupts   interrupts definitions
transfer{
  gpio_config
  gpio_reset_pin gpio_set_intr_type
  gpio_intr_enable gpio_intr_disable
  gpio_set_level gpio_get_level
  gpio_set_direction
  gpio_set_pull_mode
  gpio_wakeup_enable gpio_wakeup_disable
  gpio_pullup_en gpio_pullup_dis
  gpio_pulldown_en gpio_pulldown_dis
  gpio_hold_en gpio_hold_dis
  gpio_deep_sleep_hold_en gpio_deep_sleep_hold_dis
  gpio_install_isr_service gpio_uninstall_isr_service
  gpio_isr_handler_add gpio_isr_handler_remove
  gpio_set_drive_capability gpio_get_drive_capability
  esp_intr_alloc esp_intr_free
}transfer

0 constant ESP_INTR_FLAG_DEFAULT
: ESP_INTR_FLAG_LEVELn ( n=1-6 -- n ) 1 swap lshift ;
1 7 lshift constant ESP_INTR_FLAG_NMI
1 8 lshift constant ESP_INTR_FLAG_SHARED
1 9 lshift constant ESP_INTR_FLAG_EDGE
1 10 lshift constant ESP_INTR_FLAG_IRAM
1 11 lshift constant ESP_INTR_FLAG_INTRDISABLED

0 constant GPIO_INTR_DISABLE
1 constant GPIO_INTR_POSEDGE
2 constant GPIO_INTR_NEGEDGE
3 constant GPIO_INTR_ANYEDGE
4 constant GPIO_INTR_LOW_LEVEL
5 constant GPIO_INTR_HIGH_LEVEL

( Easy word to trigger on any change to a pin )
ESP_INTR_FLAG_DEFAULT gpio_install_isr_service drop
: pinchange ( xt pin ) dup GPIO_INTR_ANYEDGE gpio_set_intr_type throw
                       swap 0 gpio_isr_handler_add throw ;

forth definitions

vocabulary rtos   rtos definitions
transfer{
  xPortGetCoreID xTaskCreatePinnedToCore vTaskDelete
}transfer
forth definitions

DEFINED? SerialBT.new [IF]
vocabulary bluetooth   bluetooth definitions
transfer{
  SerialBT.new SerialBT.delete SerialBT.begin SerialBT.end
  SerialBT.available SerialBT.readBytes SerialBT.write
  SerialBT.flush SerialBT.hasClient
  SerialBT.enableSSP SerialBT.setPin SerialBT.unpairDevice
  SerialBT.connect SerialBT.connectAddr SerialBT.disconnect SerialBT.connected
  SerialBT.isReady esp_bt_dev_get_address
}transfer
forth definitions
[THEN]

DEFINED? OledNew [IF]
vocabulary oled   oled definitions
transfer{
  OledNew OledDelete
  OledHOME OledCLS
  OledTextc OledPrintln OledNumln OledNum
  OledDisplay OledPrint
  OledInvert OledTextsize OledSetCursor
  OledPixel OledDrawL OledCirc OledCircF
  OledRect OledRectF OledRectR OledRectrf
}transfer

128 constant WIDTH
64 constant HEIGHT
-1 constant OledReset
0 constant BLACK
1 constant WHITE
1 constant SSD1306_EXTERNALVCC
2 constant SSD1306_SWITCHCAPVCC
: OledInit
  OledAddr @ 0= if
    WIDTH HEIGHT OledReset OledNew
    SSD1306_SWITCHCAPVCC $3C OledBegin drop
  then
  OledCLS
  2 OledTextsize  ( Draw 2x Scale Text )
  WHITE OledTextc  ( Draw white text )
  0 0 OledSetCursor  ( Start at top-left corner )
  z" *Esp32forth*" OledPrintln OledDisplay
;
forth definitions
[THEN]

internals definitions
transfer{
  malloc sysfree realloc
  heap_caps_malloc heap_caps_free heap_caps_realloc
}transfer

( Heap Capabilities )
binary
0001 constant MALLOC_CAP_EXEC
0010 constant MALLOC_CAP_32BIT
0100 constant MALLOC_CAP_8BIT
1000 constant MALLOC_CAP_DMA
: MALLOC_CAP_PID ( n -- ) 10000 over 11 ( 3 ) - for 2* next ;
000010000000000 constant MALLOC_CAP_SPIRAM
000100000000000 constant MALLOC_CAP_INTERNAL
001000000000000 constant MALLOC_CAP_DEFAULT
010000000000000 constant MALLOC_CAP_IRAM_8BIT
010000000000000 constant MALLOC_CAP_RETENTION
decimal
forth definitions

