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

also forth definitions

( Generic Files )
$80000000 constant R/O ( GENERIC_READ )
$40000000 constant W/O  ( GENERIC_WRITE )
R/O W/O or constant R/W
: BIN ( fh -- fh ) ;
: CLOSE-FILE ( fh -- ior ) CloseHandle 0=ior ;
: FLUSH-FILE ( fh -- ior ) FlushFileBuffers 0=ior ;
: OPEN-FILE ( a n fam -- fh ior )
   >r s>z r> FILE_SHARE_READ FILE_SHARE_WRITE or NULL
   OPEN_EXISTING FILE_ATTRIBUTE_NORMAL NULL CreateFileA d0<ior ;
: CREATE-FILE ( a n fam -- fh ior )
   >r s>z r> FILE_SHARE_READ FILE_SHARE_WRITE or NULL
   CREATE_ALWAYS FILE_ATTRIBUTE_NORMAL NULL CreateFileA d0<ior ;
: DELETE-FILE ( a n -- ior ) s>z DeleteFileA 0=ior ;
: RENAME-FILE ( a n a n -- ior ) s>z -rot s>z swap MoveFileA 0=ior ;
: WRITE-FILE ( a n fh -- ior )
   -rot dup >r 0 >r rp@ NULL WriteFile
   if r> r> <> else rdrop rdrop GetLastError then ;
: READ-FILE ( a n fh -- n ior ) -rot 0 >r rp@ NULL ReadFile r> swap 0=ior ;
: FILE-POSITION ( fh -- n ior )
   0 NULL FILE_CURRENT SetFilePointer dup invalid?ior ;
: REPOSITION-FILE ( n fh -- ior )
   swap NULL FILE_BEGIN SetFilePointer invalid?ior ;
: RESIZE-FILE ( n fh -- ior )
   dup file-position dup if drop 2drop 1 ior exit else drop then >r
   dup -rot reposition-file if rdrop drop 1 ior exit then
   dup SetEndOfFile 0= if rdrop drop 1 ior exit then
   r> swap reposition-file ;
: FILE-SIZE ( fh -- n ior ) NULL GetFileSize dup invalid?ior ;
: NON-BLOCK ( fh -- ior ) 1 throw ;  ( IMPLEMENT! )

only forth definitions
