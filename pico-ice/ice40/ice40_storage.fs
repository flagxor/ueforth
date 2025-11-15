\ Copyright 2025 Bradley D. Nelson
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

needs ice40_config.fs

ice40 definitions
vocabulary storage   storage definitions

: ew! ( n a -- ) over 8 rshift over c! 1+ c! ;
: w, ( n -- ) here ew! 2 allot ;

create header1
  $ff c, $00 c, $00 c, $ff c, ( Init blop, maybe not needed? )
  $7e c, $aa c, $99 c, $7e c, ( Preamble )
  $51 c, $00 c, ( Frequency 0=low, 1=medium, 2=high )
  $01 c, $05 c, ( Reset CRC crc = 0xffff )
here header1 - constant header1#

create header2
  $92 c, $0020 w, ( Set flags bit0=nosleep bit5=warmboot )
  $62 c, cram-bank-width 1- w, ( Set bank width )
  $82 c, $0000 w, ( Set bank offset to 0 )
here header2 - constant header2#

0 value bank-id
0 value bank-height
create bank-header
  $72 c, here to bank-height 0 w, ( Set bank height, 5k only, upper vs lower )
  $11 c, here to bank-id 0 c, ( Set bank )
  $01 c, $01 c, ( Prefix )
here bank-header - constant bank-header#

create bank-footer
  $00 c, $00 c, ( Suffix )
here bank-footer - constant bank-footer#

create header3
  $72 c, bram-chunk-size w, ( Setting bank height? )
here header3 - constant header3#

0 value bram-bank
create bram-header1
  $11 c, here to bram-bank 0 c, ( Setting bank )
here bram-header1 - constant bram-header1#

0 value bram-offset
0 value bram-bank-width
create bram-header2
  $82 c, here to bram-offset 0 w, ( Setting bank offset )
  $62 c, here to bram-bank-width 0 w, ( Setting bank width )
here bram-header2 - constant bram-header2#

0 value footer-crc
create footer
  $22 c, here to footer-crc $0000 w, ( CRC )
  $01 c, $06 c, ( Wakeup )
  $00 c, ( Padding )
here footer - constant footer#

variable crc16
: +crc16 ( ch -- )
  7 for
    dup i rshift 1 and ( peel off one bit )
    crc16 uw@ 15 rshift xor if $1021 else 0 then
    crc16 uw@ 2* xor crc16 w!
  next
  drop
;
: *crc16 ( a n -- ) for aft dup c@ +crc16 1+ then next drop ;
: crc-write ( a n output -- ) >r 2dup *crc16 r> execute ;

: cram-write { output -- }
  ( write header )
  header1 header1# output execute
  $ffff crc16 w!
  header2 header2# output crc-write

  ( Bank 0 )
  0 bank-id c!
  cram-height-lower bank-height ew!
  bank-header bank-header# output crc-write
  cram cram-bank-width cram-height-lower * 8 / output crc-write
  bank-footer bank-footer# output crc-write

  ( Bank 1 )
  1 bank-id c!
  cram-height-upper bank-height ew!
  bank-header bank-header# output crc-write
  cram cram-bank-width cram-height-lower * 8 / +
    cram-bank-width cram-height-upper * 8 / output crc-write
  bank-footer bank-footer# output crc-write

  ( Bank 2 )
  2 bank-id c!
  cram-height-lower bank-height ew!
  bank-header bank-header# output crc-write
  cram cram-bank-width cram-height-lower cram-height-upper + * 8 / +
    cram-bank-width cram-height-lower * 8 / output crc-write
  bank-footer bank-footer# output crc-write

  ( Bank 3 )
  3 bank-id c!
  cram-height-upper bank-height ew!
  bank-header bank-header# output crc-write
  cram cram-bank-width cram-height-lower 2* cram-height-upper + * 8 / +
    cram-bank-width cram-height-upper * 8 / output crc-write
  bank-footer bank-footer# output crc-write

  ( BRAM Header )
  header3 header3# output crc-write

  ( BRAM Bank 0 )
  0 bram-bank c!
  bram-header1 bram-header1# output crc-write

  bram-chunk-size 0 * bram-offset ew!
  bram-width-lower 1- bram-bank-width ew!
  bram-header2 bram-header2# output crc-write

  bram-chunk-size 1 * bram-offset ew!
  bram-width-lower 1- bram-bank-width ew!
  bram-header2 bram-header2# output crc-write

  ( BRAM Bank 1 )
  1 bram-bank c!
  bram-header1 bram-header1# output crc-write

  bram-chunk-size 0 * bram-offset ew!
  bram-width-upper 1- bram-bank-width ew!
  bram-header2 bram-header2# output crc-write

  bram-chunk-size 1 * bram-offset ew!
  bram-width-upper 1- bram-bank-width ew!
  bram-header2 bram-header2# output crc-write

  ( BRAM Bank 2 )
  2 bram-bank c!
  bram-header1 bram-header1# output crc-write

  bram-chunk-size 0 * bram-offset ew!
  bram-width-lower 1- bram-bank-width ew!
  bram-header2 bram-header2# output crc-write

  bram-chunk-size 1 * bram-offset ew!
  bram-width-lower 1- bram-bank-width ew!
  bram-header2 bram-header2# output crc-write

  ( BRAM Bank 3 )
  3 bram-bank c!
  bram-header1 bram-header1# output crc-write

  bram-chunk-size 0 * bram-offset ew!
  bram-width-upper 1- bram-bank-width ew!
  bram-header2 bram-header2# output crc-write

  bram-chunk-size 1 * bram-offset ew!
  bram-width-upper 1- bram-bank-width ew!
  bram-header2 bram-header2# output crc-write

  ( write footer )
  $22 +crc16 ( Add first byte of footer )
  crc16 uw@ footer-crc ew! ( Update CRC )
  footer footer# output crc-write
;

0 value save-fh
: save-write ( a n -- ) save-fh write-file throw ;

ice40 definitions also storage

: save ( a n -- )
  w/o bin create-file throw to save-fh
  ['] save-write cram-write
  save-fh close-file throw
;

DEFINED? ice [IF]
also ice
  DEFINED? ice_cram_open [IF]

: deploy
  ice_cram_open
  ['] ice_cram_write cram-write
  ice_cram_close
;

  [THEN]
previous
[THEN]

previous
forth definitions
