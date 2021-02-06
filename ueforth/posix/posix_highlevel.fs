( Words with OS assist )
posix
: allocate ( n -- a ior ) malloc dup 0= ;
: free ( a -- ior ) sysfree drop 0 ;
: resize ( a n -- a ior ) realloc dup 0= ;
forth

