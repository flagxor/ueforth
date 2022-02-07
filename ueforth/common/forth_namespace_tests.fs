\ Copyright 2022 Bradley D. Nelson
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

also internals
: list-builtins ( voc )
  >r 'builtins begin dup >link while
    dup >params r@ = if dup see. cr then
    3 cells +
  repeat drop rdrop ;
: list-from ( xt ) begin dup nonvoc? while
    dup >flags BUILTIN_FORK and if
      dup cell+ @ list-builtins
    then
    dup see. cr
    >link
  repeat drop ;

e: check-locals
  out: +to 
  out: to 
  out: ; 
  out: { 
  out: (local) 
;e

e: check-highlevel-floats
  out: f.s 
  out: f. 
  out: #fs 
  out: set-precision 
  out: precision 
  out: fvariable 
  out: fconstant 
  out: fliteral 
  out: afliteral 
  out: sf, 
;e

e: check-boot
  out: quit 
  out: evaluate 
  out: prompt 
  out: refill 
  out: tib 
  out: accept 
  out: echo 
  out: z>s 
  out: s>z 
  out: r~ 
  out: r| 
  out: r" 
  out: z" 
  out: ." 
  out: s" 
  out: n. 
  out: ? 
  out: . 
  out: u. 
  out: binary 
  out: decimal 
  out: octal 
  out: hex 
  out: str 
  out: #> 
  out: sign 
  out: #s 
  out: # 
  out: hold 
  out: <# 
  out: extract 
  out: pad 
  out: hld 
  out: cr 
  out: space 
  out: emit 
  out: bye 
  out: key? 
  out: key 
  out: type 
  out: is 
  out: defer 
  out: +to 
  out: to 
  out: value-bind 
  out: value 
  out: throw 
  out: catch 
  out: handler 
  out: j 
  out: i 
  out: loop 
  out: +loop 
  out: leave 
  out: unloop 
  out: ?do 
  out: do 
  out: next 
  out: for 
  out: nest-depth 
  out: postpone 
  out: recurse 
  out: aft 
  out: repeat 
  out: while 
  out: else 
  out: if 
  out: then 
  out: ahead 
  out: until 
  out: again 
  out: begin 
  out: literal 
  out: [char] 
  out: char 
  out: ['] 
  out: ' 
  out: ] 
  out: [ 
  out: used 
  out: remaining 
  out: fdepth 
  out: depth 
  out: fp0 
  out: rp0 
  out: sp0 
  out: variable 
  out: constant 
  out: \ 
  out: ( 
;e

e: check-extra-opcodes
  out: nip 
  out: rdrop 
  out: */ 
  out: * 
  out: /mod 
  out: / 
  out: mod 
  out: invert 
  out: negate 
  out: - 
  out: rot 
  out: -rot 
  out: < 
  out: > 
  out: <= 
  out: >= 
  out: = 
  out: <> 
  out: 0<> 
  out: bl 
  out: nl 
  out: 1+ 
  out: 1- 
  out: 2* 
  out: 2/ 
  out: 4* 
  out: 4/ 
  out: +! 
  out: cell+ 
  out: cells 
  out: cell/ 
  out: 2drop 
  out: 2dup 
  out: 2@ 
  out: 2! 

  out: cmove 
  out: cmove> 
  out: fill 
  out: erase 
  out: blank 

  out: min 
  out: max 
  out: abs 

  out: here 
  out: allot 
  out: aligned 
  out: align 
  out: , 
  out: c, 

  out: >flags 
  out: >params 
  out: >size 
  out: >link& 
  out: >link 
  out: >name 
  out: >body 

  out: current 
  out: #tib 
  out: >in 
  out: state 
  out: base 
  out: context 
  out: latestxt 
;e

e: check-core-opcodes
  out: 0= 
  out: 0< 
  out: + 
  out: U/MOD 
  out: */MOD 
  out: LSHIFT 
  out: RSHIFT 
  out: AND 
  out: OR 
  out: XOR 
  out: DUP 
  out: SWAP 
  out: OVER 
  out: DROP 
  out: @ 
  out: SL@ 
  out: SW@ 
  out: C@ 
  out: ! 
  out: L! 
  out: W! 
  out: C! 
  out: SP@ 
  out: SP! 
  out: RP@ 
  out: RP! 
  out: >R 
  out: R> 
  out: R@ 
  out: EXECUTE 
  out: CELL 
  out: FIND 
  out: PARSE 
  out: CREATE 
  out: DOES> 
  out: IMMEDIATE 
  out: : 
  out: EXIT 
  out: ; 
;e

e: check-float-opcodes
  out: FP@ 
  out: FP! 
  out: SF@ 
  out: SF! 
  out: FDUP 
  out: FNIP 
  out: FDROP 
  out: FOVER 
  out: FSWAP 
  out: FNEGATE 
  out: F0< 
  out: F0= 
  out: F= 
  out: F< 
  out: F> 
  out: F<> 
  out: F<= 
  out: F>= 
  out: F+ 
  out: F- 
  out: F* 
  out: F/ 
  out: 1/F 
  out: S>F 
  out: F>S 
  out: SFLOAT 
  out: SFLOATS 
  out: SFLOAT+ 
  out: PI 
  out: FSQRT 
;e

e: check-files
  out: NON-BLOCK 
  out: FILE-SIZE 
  out: RESIZE-FILE 
  out: REPOSITION-FILE 
  out: FILE-POSITION 
  out: READ-FILE 
  out: WRITE-FILE 
  out: RENAME-FILE 
  out: DELETE-FILE 
  out: CREATE-FILE 
  out: OPEN-FILE 
  out: FLUSH-FILE 
  out: CLOSE-FILE 
  out: BIN 
  out: R/W 
  out: W/O 
  out: R/O 
;e

e: check-files-reverse
  out: R/O 
  out: W/O 
  out: R/W 
  out: BIN 
  out: CLOSE-FILE 
  out: FLUSH-FILE 
  out: OPEN-FILE 
  out: CREATE-FILE 
  out: DELETE-FILE 
  out: RENAME-FILE 
  out: WRITE-FILE 
  out: READ-FILE 
  out: FILE-POSITION 
  out: REPOSITION-FILE 
  out: RESIZE-FILE 
  out: FILE-SIZE 
  out: NON-BLOCK 
;e

e: check-blocks
  out: editor 
  out: list 
  out: copy 
  out: thru 
  out: load 
  out: flush 
  out: update 
  out: empty-buffers 
  out: buffer 
  out: block 
  out: save-buffers 
  out: default-use 
  out: use 
  out: open-blocks 
  out: block-id 
  out: scr 
  out: block-fid 
;e

e: check-vocabulary
  out: internals 
  out: sealed 
  out: also 
  out: only 
  out: transfer{ 
  out: }transfer 
  out: transfer 
  out: definitions 
  out: vocabulary 
;e

e: check-[]conds
  out: [IF] 
  out: [ELSE] 
  out: [THEN] 
  out: DEFINED? 
;e

e: check-utils
  out: words 
  out: vlist 
  out: order 
  out: see 
  out: .s 
  out: startswith? 
  out: str= 
  out: :noname 
  out: forget 
  out: dump 
  out: assert 
;e

e: check-snapshots
  out: reset 
  out: revive 
  out: startup: 
  out: remember 
  out: restore 
  out: save 
  out: dump-file 
;e

e: check-ansi
  out: set-title 
  out: page 
  out: at-xy 
  out: normal 
  out: bg 
  out: fg 
  out: ansi 
;e

e: check-tasks
  out: start-task 
  out: task 
  out: pause 
  out: tasks 
;e

e: check-args
  out: argv 
  out: argc 
;e

e: check-highlevel
  out: include 
  out: included 
;e

e: check-allocation
  out: resize 
  out: free 
  out: allocate 
;e

e: check-phase1
  check-highlevel-floats
  check-vocabulary
  check-[]conds
  check-boot
;e

e: check-opcodes
  check-float-opcodes
  check-extra-opcodes
  check-core-opcodes
;e

e: check-desktop
  check-args
  check-ansi
;e

e: check-phase2
  check-blocks
  out: streams 
  check-highlevel
  check-snapshots
  check-locals
  check-utils
  out: ms 
  check-tasks
;e

DEFINED? windows [IF]

e: test-windows-forth-namespace
  internals voclist
  out: ansi 
  out: editor 
  out: streams 
  out: tasks 
  out: windows 
  out: internals 
  out: FORTH 
;e

e: test-windows-forth-namespace
  ' forth list-from
  out: FORTH 
  check-desktop
  check-phase2
  check-allocation
  out: ok 
  out: ms 
  check-files
  out: ms-ticks 
  out: default-key? 
  out: default-key 
  out: default-type 
  out: windows 
  check-phase1
  out: GETPROCADDRESS 
  out: LOADLIBRARYA 
  check-opcodes
  out: forth-builtins 
;e

[ELSE] DEFINED? posix [IF]

e: test-posix-forth-namespace
  internals voclist
  out: sockets 
  out: ansi 
  out: editor 
  out: streams 
  out: tasks 
  out: termios 
  out: posix 
  out: internals 
  out: FORTH 
;e

e: test-posix-forth-namespace
  ' forth list-from
  out: FORTH 
  out:  
  out: web-interface 
  out: httpd 
  out: telnetd 
  out: sockets 
  check-desktop
  check-phase2
  out: form 
  out: termios 
  check-allocation
  out: ok 
  out: #! 
  out: ms-ticks 
  out: ms 
  check-files
  out: default-key 
  out: default-type 
  out: posix 
  check-phase1
  out: DLSYM 
  check-opcodes
  out: forth-builtins 
;e 

[ELSE]

e: test-esp32-forth-namespace
  internals voclist
  out: ansi 
  out: registers 
  out: oled 
  out: bluetooth 
  out: rtos 
  out: rmt 
  out: interrupts 
  out: sockets 
  out: Serial 
  out: ledc 
  out: SPIFFS 
  out: spi_flash 
  out: SD_MMC 
  out: SD 
  out: WiFi 
  out: Wire 
  out: ESP 
  out: editor 
  out: streams 
  out: tasks 
  out: internals 
  out: FORTH 
;e

e: check-esp32-platform
  out: ok 
  out: LED 
  out: OUTPUT 
  out: INPUT 
  out: HIGH 
  out: LOW 
  out: page 
  out: tone 
  out: freq 
  out: duty 
  out: adc 
  out: pin 
  out: default-key? 
  out: default-key 
  out: default-type 
;e

e: check-esp32-builtins
  out: pinMode 
  out: digitalWrite 
  out: digitalRead 
  out: analogRead 
  out: pulseIn 
  out: MS-TICKS 
  out: TERMINATE 
  check-files-reverse
  out: dacWrite 
  out: MDNS.begin 
;e

e: test-esp32-forth-namespace
  ' forth list-from
  out: FORTH 
  out: camera-server 
  out: camera 
  out: telnetd 
  out: bterm 
  out: timers 
  out: registers 
  out: webui 
  out: login 
  out:  
  out:  
  out: web-interface 
  out: httpd 
  check-esp32-platform
  out: oled 
  out: bluetooth 
  out: rtos 
  out: rmt 
  out: interrupts 
  out: sockets 
  out: Serial 
  out: ledc 
  out: SPIFFS 
  out: spi_flash 
  out: SD_MMC 
  out: SD 
  out: WiFi 
  out: Wire 
  out: ESP 
  check-phase2
  check-allocation
  check-phase1
  check-esp32-builtins
  check-opcodes
  out: forth-builtins 
;e 

[THEN] [THEN] 
