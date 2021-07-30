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

vocabulary windows   windows definitions

( DLL Handling )
create calls
internals
' call0 , ' call1 , ' call2 , ' call3 , ' call4 , ' call5 ,
' call6 , ' call7 , ' call8 , ' call9 , ' call10 ,
windows
: sofunc ( z n a "name" -- )
   swap >r swap GetProcAddress dup 0= throw create , r> cells calls + @ ,
   does> dup @ swap cell+ @ execute ;
: dll ( z "name" -- )
   LoadLibraryA dup 0= throw create , does> @ sofunc ;

0 constant NULL

0 constant MB_OK
1 constant MB_OKCANCEL
2 constant MB_ABORTRETRYIGNORE
3 constant MB_YESNOCANCEL
4 constant MB_YESNO
5 constant MB_RETRYCANCEL
6 constant MB_CANCELTRYCONTINUE

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

z" User32.dll" dll User32
z" MessageBoxA" 4 User32 MessageBoxA

z" Kernel32.dll" dll Kernel32
z" AllocConsole" 0 Kernel32 AllocConsole
z" ExitProcess" 1 Kernel32 ExitProcess
z" GetStdHandle" 1 Kernel32 GetStdHandle
z" GetConsoleMode" 2 Kernel32 GetConsoleMode
z" SetConsoleMode" 2 Kernel32 SetConsoleMode
z" FlushConsoleInputBuffer" 1 Kernel32 FlushConsoleInputBuffer
z" Sleep" 1 Kernel32 Sleep
z" WaitForSingleObject" 2 Kernel32 WaitForSingleObject

z" GetLastError" 0 Kernel32 GetLastError
z" CreateFileA" 7 Kernel32 CreateFileA
z" ReadFile" 5 Kernel32 ReadFile
z" WriteFile" 5 Kernel32 WriteFile
z" CloseHandle" 1 Kernel32 CloseHandle
z" FlushFileBuffers" 1 Kernel32 FlushFileBuffers
z" DeleteFileA" 1 Kernel32 DeleteFileA
z" MoveFileA" 2 Kernel32 MoveFileA
z" SetFilePointer" 4 Kernel32 SetFilePointer
z" SetEndOfFile" 1 Kernel32 SetEndOfFile
z" GetFileSize" 2 Kernel32 GetFileSize

AllocConsole drop
STD_INPUT_HANDLE GetStdHandle constant stdin
STD_OUTPUT_HANDLE GetStdHandle constant stdout
STD_ERROR_HANDLE GetStdHandle constant stderr
variable console-mode
stdin console-mode GetConsoleMode drop
stdin console-mode @ ENABLE_LINE_INPUT ENABLE_MOUSE_INPUT or
                     ENABLE_WINDOW_INPUT or invert and SetConsoleMode drop
stdout console-mode GetConsoleMode drop
stdout console-mode @ ENABLE_VIRTUAL_TERMINAL_PROCESSING or SetConsoleMode drop

: win-type ( a n -- ) stdout -rot NULL NULL WriteFile drop ;
' win-type is type
: raw-key ( -- n ) 0 >r stdin rp@ 1 NULL NULL ReadFile drop r> ;
: win-key? ( -- f ) stdin 0 WaitForSingleObject 0= ;
' win-key? is key?
: win-key ( -- n ) raw-key dup 13 = if drop nl then ;
' win-key is key
: win-bye ( -- ) 0 ExitProcess drop ;
' win-bye is bye
-1 echo !

ansi
: set-title ( a n -- ) esc ." ]0;" type bel ;
windows
s" uEforth" set-title

( Window File Specific )
1 constant FILE_SHARE_READ
2 constant FILE_SHARE_WRITE
2 constant CREATE_ALWAYS
3 constant OPEN_EXISTING
$80 constant FILE_ATTRIBUTE_NORMAL
0 constant FILE_BEGIN
1 constant FILE_CURRENT
2 constant FILE_END

( I/O Error Helpers )
: ior ( f -- ior ) if GetLastError else 0 then ;
: 0=ior ( n -- n ior ) 0= ior ;
: d0<ior ( n -- n ior ) dup 0< ior ;
: invalid?ior ( n -- ior ) $ffffffff = ior ;

forth definitions windows

( Generic Files )
$80000000 constant r/o  ( GENERIC_READ )
$40000000 constant w/o  ( GENERIC_WRITE )
r/o w/o or constant r/w
: open-file ( a n fam -- fh ior )
   >r s>z r> FILE_SHARE_READ FILE_SHARE_WRITE or NULL
   OPEN_EXISTING FILE_ATTRIBUTE_NORMAL NULL CreateFileA d0<ior ;
: create-file ( a n fam -- fh ior )
   >r s>z r> FILE_SHARE_READ FILE_SHARE_WRITE or NULL
   CREATE_ALWAYS FILE_ATTRIBUTE_NORMAL NULL CreateFileA d0<ior ;
: close-file ( fh -- ior ) CloseHandle 0=ior ;
: flush-file ( fh -- ior ) FlushFileBuffers 0=ior ;
: delete-file ( a n -- ior ) s>z DeleteFileA 0=ior ;
: rename-file ( a n a n -- ior ) s>z -rot s>z swap MoveFileA 0=ior ;
: read-file ( a n fh -- n ior ) -rot 0 >r rp@ NULL ReadFile r> swap 0=ior ;
: write-file ( a n fh -- ior )
   -rot dup >r 0 >r rp@ NULL WriteFile
   if r> r> <> else rdrop rdrop GetLastError then ;
: file-position ( fh -- n ior )
   0 NULL FILE_CURRENT SetFilePointer dup invalid?ior ;
: reposition-file ( n fh -- ior )
   swap NULL FILE_BEGIN SetFilePointer invalid?ior ;
: resize-file ( n fh -- ior )
   dup file-position dup if drop 2drop 1 ior exit else drop then >r
   dup -rot reposition-file if rdrop drop 1 ior exit then
   dup SetEndOfFile 0= if rdrop drop 1 ior exit then
   r> swap reposition-file ;
: file-size ( fh -- n ior ) NULL GetFileSize dup invalid?ior ;

( Other Utils )
: ms ( n -- ) Sleep ;

forth

( Setup entry )
: ok   ." uEforth v{{VERSION}} - rev {{REVISION}}" cr prompt refill drop quit ;
' forth ( leave on stack for fini.fs )
