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

( Words with OS assist )
windows
z" GetProcessHeap" 0 Kernel32 GetProcessHeap
z" HeapAlloc" 3 Kernel32 HeapAlloc
z" HeapFree" 3 Kernel32 HeapFree
z" HeapReAlloc" 4 Kernel32 HeapReAlloc
GetProcessHeap constant process-heap
: allocate ( n -- a ior ) process-heap 0 rot HeapAlloc dup 0= ;
: free ( a -- ior ) process-heap 0 rot HeapFree drop 0 ;
: resize ( a n -- a ior ) process-heap -rot 0 -rot HeapReAlloc dup 0= ;
forth
