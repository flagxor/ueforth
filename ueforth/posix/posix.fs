( Shared Library Handling )
1 constant RTLD_LAZY
2 constant RTLD_NOW
0 z" dlopen" dlsym constant 'dlopen
: dlopen ( z n -- a ) 'dlopen call2 ;
create calls
' call0 , ' call1 , ' call2 , ' call3 , ' call4 , ' call5 ,
' call6 , ' call7 , ' call8 , ' call9 , ' call10 ,
: sofunc ( z n a "name" -- )
   swap >r swap dlsym dup 0= throw create , r> cells calls + @ ,
   does> dup @ swap cell+ @ execute ;
: sysfunc ( z n "name" -- ) 0 sofunc ;
: shared-library ( z "name" -- )
   RTLD_NOW dlopen dup 0= throw create , does> @ sofunc ;
: sign-extend ( n -- n ) >r rp@ l@ rdrop ;

( Major Syscalls )
z" open" 3 sysfunc open
z" creat" 2 sysfunc creat
z" close" 1 sysfunc close
z" read" 3 sysfunc read
z" write" 3 sysfunc write
z" lseek" 3 sysfunc lseek
z" fsync" 1 sysfunc fsync
z" exit" 1 sysfunc sysexit
z" fork" 0 sysfunc fork
z" wait" 1 sysfunc wait
z" waitpid" 3 sysfunc waitpid
z" mmap" 6 sysfunc mmap
z" munmap" 2 sysfunc munmap
z" unlink" 1 sysfunc unlink
z" rename" 2 sysfunc rename
z" malloc" 1 sysfunc malloc
z" free" 1 sysfunc sysfree
z" realloc" 2 sysfunc realloc
z" usleep" 1 sysfunc usleep

( Errno )
z" __errno_location" 0 sysfunc __errno_location
: errno ( -- n ) __errno_location l@ ;

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
decimal

( Hookup I/O )
: stdout-write ( a n -- ) stdout -rot write drop ;
' stdout-write is type
: stdin-key ( -- n ) 0 >r stdin rp@ 1 read drop r> ;
' stdin-key is key
: posix-bye   0 sysexit ;
' posix-bye is bye

( I/O Error Helpers )
: 0ior ( n -- n ior ) dup 0= if errno else 0 then ;
: 0<ior ( n -- n ior ) dup 0< if errno else 0 then ;

( Generic Files )
O_RDONLY constant r/o
O_WRONLY constant w/o
O_RDWR constant r/w
octal 777 constant 0777 decimal
: open-file ( a n fam -- fh ior ) >r s>z r> 0777 open sign-extend 0<ior ;
: create-file ( a n fam -- fh ior )
   >r s>z r> O_CREAT or 0777 open sign-extend 0<ior ;
: close-file ( fh -- ior ) close sign-extend ;
: flush-file ( fh -- ior ) fsync sign-extend ;
: delete-file ( a n -- ior ) s>z unlink sign-extend ;
: rename-file ( a n a n -- ior ) s>z -rot s>z swap rename sign-extend ;
: read-file ( a n fh -- n ior ) -rot read 0<ior ;
: write-file ( a n fh -- ior ) -rot dup >r write r> = 0= ;
: file-position ( fh -- n ior ) dup 0 SEEK_CUR lseek 0<ior ;
: reposition-file ( n fh -- ior ) swap SEEK_SET lseek 0< ;
: file-size ( fh -- n ior )
   dup 0 SEEK_CUR lseek >r
   dup 0 SEEK_END lseek r> swap >r
         SEEK_SET lseek drop r> 0<ior ;

( Other Utils )
: ms ( n -- ) 1000 * usleep drop ;
