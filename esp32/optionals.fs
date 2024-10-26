\ Copyright 2023 Bradley D. Nelson
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

internals DEFINED? assembler-source [IF]
  assembler-source evaluate
[THEN] forth
internals DEFINED? xtensa-assembler-source [IF]
  xtensa-assembler-source evaluate
[THEN] forth
internals DEFINED? riscv-assembler-source [IF]
  riscv-assembler-source evaluate
[THEN] forth

internals DEFINED? camera-source [IF]
  camera-source evaluate
[THEN] forth

internals DEFINED? interrupts-source [IF]
  interrupts-source evaluate
[THEN] forth

internals DEFINED? oled-source [IF]
  oled-source evaluate
[THEN] forth

DEFINED? rmt-builtins [IF]
  vocabulary rmt   rmt definitions
  transfer rmt-builtins
  forth definitions
[THEN]

internals DEFINED? serial-bluetooth-source [IF]
  serial-bluetooth-source evaluate
[THEN] forth

internals DEFINED? spi-flash-source [IF]
  spi-flash-source evaluate
[THEN] forth

internals DEFINED? HTTPClient-builtins [IF]
  vocabulary HTTPClient   HTTPClient definitions
  transfer HTTPClient-builtins
  forth definitions
[THEN] forth
