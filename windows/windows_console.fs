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

windows definitions

-10 constant STD_INPUT_HANDLE
-11 constant STD_OUTPUT_HANDLE
-12 constant STD_ERROR_HANDLE

$0001 constant ENABLE_PROCESSED_INPUT
$0002 constant ENABLE_LINE_INPUT
$0004 constant ENABLE_ECHO_INPUT
$0008 constant ENABLE_WINDOW_INPUT
$0010 constant ENABLE_MOUSE_INPUT
$0020 constant ENABLE_INSERT_MODE
$0040 constant ENABLE_QUICK_EDIT_MODE
$0200 constant ENABLE_VIRTUAL_TERMINAL_INPUT

$0001 constant ENABLE_PROCESSED_OUTPUT
$0002 constant ENABLE_WRAP_AT_EOL_OUTPUT
$0004 constant ENABLE_VIRTUAL_TERMINAL_PROCESSING
$0008 constant DISABLE_NEWLINE_AUTO_RETURN
$0010 constant ENABLE_LVB_GRID_WORLDWIDE

z" AllocConsole" 0 Kernel32 AllocConsole
z" ExitProcess" 1 Kernel32 ExitProcess
z" GetStdHandle" 1 Kernel32 GetStdHandle
z" GetConsoleMode" 2 Kernel32 GetConsoleMode
z" SetConsoleMode" 2 Kernel32 SetConsoleMode
z" FlushConsoleInputBuffer" 1 Kernel32 FlushConsoleInputBuffer

0 value console-started
0 value stdin
0 value stdout
0 value stderr
variable console-mode

: init-console
  console-started if exit then
  -1 to console-started
  AllocConsole drop
  STD_INPUT_HANDLE GetStdHandle to stdin
  STD_OUTPUT_HANDLE GetStdHandle to stdout
  STD_ERROR_HANDLE GetStdHandle to stderr
  stdin console-mode GetConsoleMode drop
  stdin console-mode @ ENABLE_LINE_INPUT ENABLE_MOUSE_INPUT or
                       ENABLE_WINDOW_INPUT or invert and SetConsoleMode drop
  stdout console-mode GetConsoleMode drop
  stdout console-mode @ ENABLE_VIRTUAL_TERMINAL_PROCESSING or SetConsoleMode drop
  SetupCtrlBreakHandler
;

: win-type ( a n -- ) init-console stdout -rot NULL NULL WriteFile drop ;
: raw-key ( -- n )
   0 >r stdin rp@ 1 NULL NULL ReadFile 0= if rdrop -1 exit then r> ;
: win-key? ( -- f ) stdin 0 WaitForSingleObject 0= ;
: win-key ( -- n ) raw-key dup 13 = if drop nl then ;

also forth definitions
: default-type win-type ;
: default-key win-key ;
: default-key? win-key? ;
only windows definitions
' default-type is type
' default-key is key
' default-key? is key?
' ExitProcess is terminate

only forth definitions
