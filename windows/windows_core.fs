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

vocabulary windows   windows definitions

( DLL Handling )
create calls
internals
 ' call0 ,  ' call1 ,  ' call2 ,  ' call3 ,  ' call4 ,
 ' call5 ,  ' call6 ,  ' call7 ,  ' call8 ,  ' call9 ,
' call10 , ' call11 , ' call12 , ' call13 , ' call14 , ' call15 ,
windows
transfer windows-builtins
: sofunc ( z n a "name" -- )
   >r dup 15 > throw r> ( Check there aren't too many args )
   swap >r swap GetProcAddress dup 0= throw create , r> cells calls + @ ,
   does> dup @ swap cell+ @ execute ;
: dll ( z "name" -- )
   LoadLibraryA dup 0= throw create , does> @ sofunc ;
: contains? ( z 'lib -- f ) >body @ swap GetProcAddress ;

z" Kernel32.dll" dll Kernel32

z" ExitProcess" 1 Kernel32 ExitProcess
z" Sleep" 1 Kernel32 Sleep
z" GetTickCount" 0 Kernel32 GetTickCount
z" WaitForSingleObject" 2 Kernel32 WaitForSingleObject
z" GetLastError" 0 Kernel32 GetLastError
z" GetCommandLineW" 0 Kernel32 GetCommandLineW
z" GetModuleHandleA" 1 Kernel32 GetModuleHandleA

z" Shell32.dll" dll Shell32
z" CommandLineToArgvW" 2 Shell32 CommandLineToArgvW

variable wargc  variable wargv
GetCommandLineW wargc CommandLineToArgvW wargv !
: wz>sz ( a -- a n )
   here swap begin dup sw@ 0<> while dup sw@ c, 2 + repeat drop 0 c, align ;
: wargs-convert ( dst )
   wargv @ wargc @ for aft
      dup @ wz>sz >r swap r> over ! cell+ swap cell+
   then next 2drop ;
also internals
wargc @ 'argc !
here 'argv ! wargc @ cells allot
'argv @ wargs-convert

0 constant NULL

forth definitions

( Other Utils )
: ms ( n -- ) Sleep ;
: ms-ticks ( -- n ) GetTickCount ;

only forth

( Setup entry )
internals : ok   ." uEforth" raw-ok ; forth
