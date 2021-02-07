( Variable useful but non-critical words )

: .s   ." <" depth <# #s #> type ." > "
       depth 0 max for aft sp@ r@ cells - @ . then next cr ;
: assert ( f -- ) 0= throw ;

internals definitions
: mem= ( a a n -- f)
   for aft 2dup c@ swap c@ <> if 2drop rdrop 0 exit then 1+ swap 1+ then next 2drop -1 ;
forth definitions also internals
: str= ( a n a n -- f) >r swap r@ <> if rdrop 2drop 0 exit then r> mem= ;
: startswith? ( a n a n -- f ) >r swap r@ < if rdrop 2drop 0 exit then r> mem= ;
only forth definitions

