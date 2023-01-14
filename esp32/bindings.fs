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

forth definitions internals
: read-dir ( dh -- a n ) readdir dup if z>s else 0 then ;
forth definitions

vocabulary ESP   ESP definitions
transfer ESP-builtins
only forth definitions

vocabulary Wire   Wire definitions
transfer wire-builtins
forth definitions

vocabulary WiFi   WiFi definitions
transfer WiFi-builtins
( WiFi Modes )
0 constant WIFI_MODE_NULL
1 constant WIFI_MODE_STA
2 constant WIFI_MODE_AP
3 constant WIFI_MODE_APSTA
forth definitions

vocabulary SD   SD definitions
transfer SD-builtins
forth definitions

vocabulary SD_MMC   SD_MMC definitions
transfer SD_MMC-builtins
forth definitions

vocabulary spi_flash   spi_flash definitions
transfer spi_flash-builtins
DEFINED? spi_flash_init [IF]
0 constant SPI_PARTITION_TYPE_APP
1 constant SPI_PARTITION_TYPE_DATA
$ff constant SPI_PARTITION_SUBTYPE_ANY

also structures
struct esp_partition_t
  ( Work around changing struct layout )
  esp_partition_t_size 40 >= [IF]
    ptr field p>gap
  [THEN]
  ptr field p>type
  ptr field p>subtype
  ptr field p>address
  ptr field p>size
  ptr field p>label

: p. ( part -- )
  base @ >r >r decimal
  ." TYPE: " r@ p>type @ . ." SUBTYPE: " r@ p>subtype @ .
  ." ADDR: " r@ hex p>address @ .  ." SIZE: " r@ p>size @ .
  ." LABEL: " r> p>label @ z>s type cr r> base ! ;
: list-partition-type ( type -- )
  SPI_PARTITION_SUBTYPE_ANY 0 esp_partition_find
  begin dup esp_partition_get p. esp_partition_next dup 0= until drop ;
: list-partitions   SPI_PARTITION_TYPE_APP list-partition-type
                    SPI_PARTITION_TYPE_DATA list-partition-type ;
[THEN]
only forth definitions

vocabulary SPIFFS   SPIFFS definitions
transfer SPIFFS-builtins
forth definitions

vocabulary ledc  ledc definitions
transfer ledc-builtins
forth definitions

vocabulary Serial   Serial definitions
transfer Serial-builtins
forth definitions

vocabulary sockets   sockets definitions
transfer sockets-builtins
1 constant SOCK_STREAM
2 constant SOCK_DGRAM
3 constant SOCK_RAW

2 constant AF_INET
16 constant sizeof(sockaddr_in)
1 constant SOL_SOCKET
2 constant SO_REUSEADDR

: bs, ( n -- ) dup 8 rshift c, c, ;
: s, ( n -- ) dup c, 8 rshift c, ;
: l, ( n -- ) dup s, 16 rshift s, ;
: sockaddr   create 16 c, AF_INET c, 0 bs, 0 l, 0 l, 0 l, ;
: ->port@ ( a -- n ) 2 + >r r@ c@ 8 lshift r> 1+ c@ + ;
: ->port! ( n a --  ) 2 + >r dup 8 rshift r@ c! r> 1+ c! ;
: ->addr@ ( a -- n ) 4 + ul@ ;
: ->addr! ( n a --  ) 4 + l! ;
: ->h_addr ( hostent -- n ) 2 cells + 8 + @ @ ul@ ;
: ip# ( n -- n ) dup 255 and n. [char] . emit 8 rshift ;
: ip. ( n -- ) ip# ip# ip# 255 and n. ;
forth definitions

vocabulary interrupts   interrupts definitions
transfer interrupts-builtins
DEFINED? gpio_config [IF]
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
[THEN]
forth definitions

vocabulary rmt   rmt definitions
transfer rmt-builtins
forth definitions

vocabulary rtos   rtos definitions
transfer rtos-builtins
forth definitions

DEFINED? SerialBT.new [IF]
  vocabulary bluetooth   bluetooth definitions
  transfer bluetooth-builtins
  forth definitions
[ELSE]
  internals definitions
  transfer bluetooth-builtins
  forth definitions
[THEN]

vocabulary oled   oled definitions
transfer oled-builtins
DEFINED? OledNew [IF]
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
[THEN]
forth definitions

internals definitions
( Heap Capabilities )
1 0 lshift constant MALLOC_CAP_EXEC
1 1 lshift constant MALLOC_CAP_32BIT
1 2 lshift constant MALLOC_CAP_8BIT
1 3 lshift constant MALLOC_CAP_DMA
1 10 lshift constant MALLOC_CAP_SPIRAM
1 11 lshift constant MALLOC_CAP_INTERNAL
1 12 lshift constant MALLOC_CAP_DEFAULT
1 13 lshift constant MALLOC_CAP_IRAM_8BIT
1 14 lshift constant MALLOC_CAP_RETENTION
1 15 lshift constant MALLOC_CAP_RTCRAM
forth definitions
