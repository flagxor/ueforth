( Words with OS assist )
z" GetProcessHeap" 0 Kernel32 GetProcessHeap
z" HeapAlloc" 3 Kernel32 HeapAlloc
z" HeapFree" 3 Kernel32 HeapFree
z" HeapReAlloc" 4 Kernel32 HeapReAlloc
GetProcessHeap constant process-heap
: allocate ( n -- a ior ) process-heap 0 rot HeapAlloc dup 0= ;
: free ( a -- ior ) process-heap 0 rot HeapFree drop 0 ;
: resize ( a n -- a ior ) process-heap -rot 0 -rot HeapReAlloc dup 0= ;
