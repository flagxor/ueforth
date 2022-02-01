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

( Migrate various words to separate vocabularies, and constants )

vocabulary Wire   Wire definitions
transfer{
  Wire.begin Wire.setClock Wire.getClock
  Wire.setTimeout Wire.getTimeout
  Wire.beginTransmission Wire.endTransmission
  Wire.requestFrom Wire.write
  Wire.available Wire.read
  Wire.peek Wire.flush
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

DEFINED? SD.begin [IF]
vocabulary SD   SD definitions
transfer{
  SD.begin SD.end
  SD.beginFull SD.beginDefaults
  SD.totalBytes SD.usedBytes
  SD.cardType
}transfer
forth definitions
[THEN]

DEFINED? SD_MMC.begin [IF]
vocabulary SD_MMC   SD_MMC definitions
transfer{
  SD_MMC.begin SD_MMC.end
  SD_MMC.beginFull SD_MMC.beginDefaults
  SD_MMC.totalBytes SD_MMC.usedBytes
  SD_MMC.cardType
}transfer
forth definitions
[THEN]

DEFINED? spi_flash_init [IF]
vocabulary spi_flash   spi_flash definitions
transfer{
  spi_flash_init spi_flash_get_chip_size
  spi_flash_erase_sector spi_flash_erase_range
  spi_flash_write spi_flash_write_encrypted
  spi_flash_read spi_flash_read_encrypted
  spi_flash_mmap spi_flash_mmap_pages spi_flash_munmap
  spi_flash_mmap_dump spi_flash_mmap_get_free_pages
  spi_flash_cache2phys spi_flash_phys2cache spi_flash_cache_enabled
  esp_partition_find esp_partition_find_first esp_partition_get
  esp_partition_next esp_partition_iterator_release
  esp_partition_verify esp_partition_read esp_partition_write
  esp_partition_erase_range esp_partition_mmap
  esp_partition_get_sha256 esp_partition_check_identity
  esp_partition_t_size
}transfer
0 constant SPI_PARTITION_TYPE_APP
1 constant SPI_PARTITION_TYPE_DATA
$ff constant SPI_PARTITION_SUBTYPE_ANY
( Work around changing struct layout )
: p>common ( part -- part' ) esp_partition_t_size 40 >= if cell+ then ;
: p>type ( part -- n ) p>common @ ;
: p>subtype ( part -- n ) p>common cell+ @ ;
: p>address ( part -- n ) p>common 2 cells + @ ;
: p>size ( part -- n ) p>common 3 cells + @ ;
: p>label ( part -- a n ) p>common 4 cells + z>s ;
: p. ( part -- )
  base @ >r >r decimal
  ." TYPE: " r@ p>type . ." SUBTYPE: " r@ p>subtype .
  ." ADDR: " r@ hex p>address .  ." SIZE: " r@ p>size .
  ." LABEL: " r> p>label type cr r> base ! ;
: list-partition-type ( type -- )
  SPI_PARTITION_SUBTYPE_ANY 0 esp_partition_find
  begin dup esp_partition_get p. esp_partition_next dup 0= until drop ;
: list-partitions   SPI_PARTITION_TYPE_APP list-partition-type
                    SPI_PARTITION_TYPE_DATA list-partition-type ;
forth definitions
[THEN]

vocabulary SPIFFS   SPIFFS definitions
transfer{
  SPIFFS.begin SPIFFS.end
  SPIFFS.format
  SPIFFS.totalBytes SPIFFS.usedBytes
}transfer
forth definitions

DEFINED? ledcSetup [IF]
vocabulary ledc  ledc definitions
transfer{
  ledcSetup ledcAttachPin ledcDetachPin
  ledcRead ledcReadFreq
  ledcWrite ledcWriteTone ledcWriteNote
}transfer
forth definitions
[THEN]

vocabulary Serial   Serial definitions
transfer{
  Serial.begin Serial.end
  Serial.available Serial.readBytes
  Serial.write Serial.flush
}transfer
forth definitions

vocabulary sockets   sockets definitions
transfer{
  socket bind listen connect sockaccept select poll errno setsockopt
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

DEFINED? gpio_config [IF]
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
( Prefix these with # because GPIO_INTR_DISABLE conflicts with a function. )
0 constant #GPIO_INTR_DISABLE
1 constant #GPIO_INTR_POSEDGE
2 constant #GPIO_INTR_NEGEDGE
3 constant #GPIO_INTR_ANYEDGE
4 constant #GPIO_INTR_LOW_LEVEL
5 constant #GPIO_INTR_HIGH_LEVEL
( Easy word to trigger on any change to a pin )
ESP_INTR_FLAG_DEFAULT gpio_install_isr_service drop
: pinchange ( xt pin ) dup #GPIO_INTR_ANYEDGE gpio_set_intr_type throw
                       swap 0 gpio_isr_handler_add throw ;
forth definitions
[THEN]

DEFINED? rmt_set_clk_div [IF]
vocabulary rmt   rmt definitions
transfer{
  rmt_set_clk_div rmt_get_clk_div rmt_set_rx_idle_thresh rmt_get_rx_idle_thresh
  rmt_set_mem_block_num rmt_get_mem_block_num rmt_set_tx_carrier
  rmt_set_mem_pd rmt_get_mem_pd rmt_tx_start rmt_tx_stop rmt_rx_start rmt_rx_stop
  rmt_tx_memory_reset rmt_rx_memory_reset rmt_set_memory_owner rmt_get_memory_owner
  rmt_set_tx_loop_mode rmt_get_tx_loop_mode rmt_set_rx_filter
  rmt_set_source_clk rmt_get_source_clk rmt_set_idle_level rmt_get_idle_level
  rmt_get_status rmt_set_rx_intr_en rmt_set_err_intr_en rmt_set_tx_intr_en
  rmt_set_tx_thr_intr_en
  rmt_set_gpio rmt_config rmt_isr_register rmt_isr_deregister
  rmt_fill_tx_items rmt_driver_install rmt_driver_uinstall
  rmt_get_channel_status rmt_get_counter_clock rmt_write_items
  rmt_wait_tx_done rmt_get_ringbuf_handle rmt_translator_init
  rmt_translator_set_context rmt_translator_get_context rmt_write_sample
}transfer
forth definitions
[THEN]

DEFINED? xPortGetCoreID [IF]
vocabulary rtos   rtos definitions
transfer{
  xPortGetCoreID xTaskCreatePinnedToCore vTaskDelete
}transfer
forth definitions
[THEN]

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
  OledNew OledDelete OledBegin OledAddr
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
