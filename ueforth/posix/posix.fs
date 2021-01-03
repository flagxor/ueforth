( Shared Library Handling )
1 constant RTLD_LAZY
2 constant RTLD_NOW
: dlopen ( n z -- a ) [ 0 z" dlopen" dlsym aliteral ] call2 ;
create calls
' call0 , ' call1 , ' call2 , ' call3 , ' call4 ,
' call5 , ' call6 , ' call7 , ' call8 , ' call9 ,
: sofunc ( z n a "name" -- )
   swap >r swap dlsym dup 0= throw create , r> cells calls + @ ,
   does> dup @ swap cell+ @ execute ;
: sysfunc ( z n "name" -- ) 0 sofunc ;
: shared-library ( z "name" -- )
   RTLD_NOW dlopen 0= throw create , does> @ sofunc ;

( Major Syscalls )
z" open" 3 sysfunc open
z" creat" 2 sysfunc creat
z" close" 1 sysfunc close
z" read" 3 sysfunc read
z" write" 3 sysfunc write
z" lseek" 3 sysfunc lseek
z" exit" 1 sysfunc sysexit
z" fork" 0 sysfunc fork
z" wait" 1 sysfunc wait
z" waitpid" 3 sysfunc waitpid
z" mmap" 6 sysfunc mmap
z" munmap" 2 sysfunc munmap

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

( Terminal handling )
: n. ( n -- ) base @ swap decimal <# #s #> type base ! ;
: esc   27 emit ;
: at-xy ( x y -- ) esc ." [" 1+ n. ." ;" 1+ n. ." H" ;
: page   esc ." [2J" esc ." [H" ;
: normal   esc ." [0m" ;
: fg ( n -- ) esc ." [38;5;" n. ." m" ;
: bg ( n -- ) esc ." [48;5;" n. ." m" ;
: clear-to-eol   esc ." [0K" ;
: scroll-down   esc ." D" ;
: scroll-up   esc ." M" ;
: hide   esc ." [?25l" ;
: show   esc ." [?25h" ;
: terminal-save   esc ." [?1049h" ;
: terminal-restore   esc ." [?1049l" ;

( Hookup I/O )
: stdout-write ( a n -- ) stdout -rot write drop ;
' stdout-write is type
: stdin-key ( -- n ) 0 >r stdin rp@ 1 read drop r> ;
' stdin-key is key
: posix-bye   0 sysexit ;
' posix-bye is bye

( I/O Error Helpers )
: 0ior ( n -- n ior ) dup 0= if errno else 0 then ;

( Words with OS assist )
z" malloc" 1 sysfunc malloc
z" free" 1 sysfunc sysfree
z" realloc" 2 sysfunc realloc
: allocate ( n -- a ior ) malloc 0ior ;
: free ( a -- ior ) sysfree 0 ;
: resize ( a n -- a ior ) realloc 0ior ;
