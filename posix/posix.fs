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

vocabulary posix   posix definitions

( Shared Library Handling )
1 constant RTLD_LAZY
2 constant RTLD_NOW
0 z" dlopen" dlsym constant 'dlopen
: dlopen ( z n -- a ) 'dlopen [ internals ] call2 [ posix ] ;
create calls
internals
 ' call0 ,  ' call1 ,  ' call2 ,  ' call3 ,  ' call4 ,
 ' call5 ,  ' call6 ,  ' call7 ,  ' call8 ,  ' call9 ,
' call10 , ' call11 , ' call12 , ' call13 , ' call14 , ' call15 ,
posix
: sofunc ( z n a "name" -- )
   swap >r swap dlsym dup 0= throw create , r> cells calls + @ ,
   does> dup @ swap cell+ @ execute ;
: sysfunc ( z n "name" -- ) 0 sofunc ;
: shared-library ( z "name" -- )
   RTLD_NOW dlopen dup 0= throw create , does> @ sofunc ;
: sign-extend ( n -- n ) >r rp@ sl@ rdrop ;

( Major Syscalls )
z" open" 3 sysfunc open
z" creat" 2 sysfunc creat
z" close" 1 sysfunc close
z" read" 3 sysfunc read
z" write" 3 sysfunc write
z" lseek" 3 sysfunc lseek
z" ftruncate" 2 sysfunc ftruncate
z" fsync" 1 sysfunc fsync
z" exit" 1 sysfunc sysexit
z" fork" 0 sysfunc fork
z" wait" 1 sysfunc wait
z" waitpid" 3 sysfunc waitpid
z" mmap" 6 sysfunc mmap
z" munmap" 2 sysfunc munmap
z" mprotect" 3 sysfunc mprotect
z" unlink" 1 sysfunc unlink
z" rename" 2 sysfunc rename
z" malloc" 1 sysfunc malloc
z" free" 1 sysfunc sysfree
z" realloc" 2 sysfunc realloc
z" usleep" 1 sysfunc usleep
z" signal" 2 sysfunc signal

( Directories )
z" chdir" 1 sysfunc chdir
z" mkdir" 2 sysfunc mkdir
z" rmdir" 1 sysfunc rmdir
z" getwd" 1 sysfunc getwd
z" opendir" 1 sysfunc opendir
z" closedir" 1 sysfunc closedir
z" readdir" 1 sysfunc readdir
: .d_type ( a -- n ) 18 + c@ ;
: .d_name ( a -- z ) 19 + ;

( Errno )
z" __errno_location" 0 sysfunc __errno_location
: errno ( -- n ) __errno_location sl@ ;

( Default Pipes )
0 constant stdin
1 constant stdout
2 constant stderr

( Seek )
0 constant SEEK_SET
1 constant SEEK_CUR
2 constant SEEK_END

( mmap )
0 constant PROT_NONE
1 constant PROT_READ
2 constant PROT_WRITE
4 constant PROT_EXEC
$2 constant MAP_PRIVATE
$10 constant MAP_FIXED
$20 constant MAP_ANONYMOUS

( open )
octal
0 constant O_RDONLY
1 constant O_WRONLY
2 constant O_RDWR
100 constant O_CREAT
200 constant O_TRUNC
2000 constant O_APPEND
4000 constant O_NONBLOCK
decimal

( Hookup I/O )
: stdout-write ( a n -- ) stdout -rot write drop ;
: stdin-key ( -- n ) 0 >r stdin rp@ 1 read drop r> ;

also forth definitions
: default-type stdout-write ;
: default-key stdin-key ;
only posix definitions
' default-type is type
' default-key is key
' sysexit is terminate

( I/O Error Helpers )
: 0<ior ( n -- ior ) 0< if errno else 0 then ;
: 0=ior ( n -- ior ) 0= if errno else 0 then ;
: d0<ior ( n -- n ior ) dup 0<ior ;
: d0=ior ( n -- n ior ) dup 0=ior ;

( errno.h )
11 constant EAGAIN
32 constant EPIPE

( Signal Handling )
0 constant SIG_DFL
1 constant SIG_IGN

( Signals )
1 constant SIGHUP
2 constant SIGINT
9 constant SIGKILL
7 constant SIGBUS
13 constant SIGPIPE

( Ignore SIGPIPE )
SIGPIPE SIG_IGN signal drop

( Modes )
octal 777 constant 0777 decimal

( Clock )
z" clock_gettime" 2 sysfunc clock_gettime
: timespec ( "name" ) create 0 , 0 , ;
0 constant CLOCK_REALTIME
1 constant CLOCK_MONOTONIC
2 constant CLOCK_PROCESS_CPUTIME_ID
3 constant CLOCK_THREAD_CPUTIME_ID
4 constant CLOCK_MONOTONIC_RAW
5 constant CLOCK_REALTIME_COARSE
6 constant CLOCK_MONOTONIC_COARSE
7 constant CLOCK_BOOTTIME
8 constant CLOCK_REALTIME_ALARM
9 constant CLOCK_BOOTTIME_ALARM

( File control )
z" fcntl" 3 sysfunc fcntl
4 constant F_SETFL
2048 constant FNDELAY ( 04000 )

forth definitions posix

( Generic Files )
O_RDONLY constant R/O
O_WRONLY constant W/O
O_RDWR constant R/W
: BIN ( fh -- fh ) ;

: CLOSE-FILE ( fh -- ior ) close 0<ior ;
: FLUSH-FILE ( fh -- ior ) fsync 0<ior ;
: OPEN-FILE ( a n fam -- fh ior ) >r s>z r> 0777 open sign-extend d0<ior ;
: CREATE-FILE ( a n fam -- fh ior )
   >r s>z r> O_CREAT or 0777 open sign-extend d0<ior ;
: DELETE-FILE ( a n -- ior ) s>z unlink 0<ior ;
: RENAME-FILE ( a n a n -- ior ) s>z -rot s>z swap rename 0<ior ;
: WRITE-FILE ( a n fh -- ior ) -rot dup >r write r> = 0=ior ;
: READ-FILE ( a n fh -- n ior ) -rot read d0<ior ;
: FILE-POSITION ( fh -- n ior ) 0 SEEK_CUR lseek d0<ior ;
: REPOSITION-FILE ( n fh -- ior ) swap SEEK_SET lseek 0<ior ;
: RESIZE-FILE ( n fh -- ior ) swap ftruncate 0<ior ;
: FILE-SIZE ( fh -- n ior )
   dup 0 SEEK_CUR lseek >r
   dup 0 SEEK_END lseek r> swap >r
         SEEK_SET lseek drop r> d0<ior ;
( Non-standard )
: NON-BLOCK ( fh -- ior ) F_SETFL FNDELAY fcntl ;

( Directories )
: OPEN-DIR ( a n -- dh ior ) s>z opendir d0=ior ;
: CLOSE-DIR ( dh -- ior ) closedir 0<ior ;
: READ-DIR ( dh -- a n ) readdir dup if .d_name z>s else 0 then ;

( Other Utils )
: ms ( n -- ) 1000 * usleep drop ;
: ms-ticks ( -- n )
   0 >r 0 >r CLOCK_MONOTONIC_RAW rp@ cell - clock_gettime throw
   r> 1000000 / r> 1000 * + ;

( Shell ops )
: cd ( "path" -- ) bl parse s>z chdir throw ;
: mkdir ( "path" -- ) bl parse s>z 0777 mkdir throw ;
: rmdir ( "path" -- ) bl parse s>z rmdir throw ;
: pwd   here getwd z>s type cr ;

forth

( Setup entry )
internals : ok   ." uEforth" raw-ok ; forth
