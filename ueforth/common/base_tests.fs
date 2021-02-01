( Tests Base Operations )
: test-empty-stack   depth 0= assert ;
: test-add   123 111 + 234 = assert ;
: test-dup-depth   123 depth 1 = assert dup depth 2 = assert 2drop ;
: test-dup-values   456 dup 456 = assert 456 = assert ;
: test-2drop   123 456 2drop depth 0= assert ;
: test-nip   123 456 nip depth 1 = assert 456 = assert ;
: test-rdrop   111 >r 222 >r rdrop r> 111 = assert ;
: test-*/    1000000 22 7 */ 3142857 = assert ;
: test-bl   bl 32 = assert ;
: test-0=   123 0= 0= assert 0 0= assert ;
