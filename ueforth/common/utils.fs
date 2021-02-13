( Words built after boot )

( For tests and asserts )
: assert ( f -- ) 0= throw ;

( Examine Memory )
: dump ( a n -- )
   cr 0 do i 16 mod 0= if cr then dup i + c@ . loop drop cr ;

( Remove from Dictionary )
: forget ( "name" ) ' dup >link current @ !  >name drop here - allot ;

internals definitions
: mem= ( a a n -- f)
   for aft 2dup c@ swap c@ <> if 2drop rdrop 0 exit then 1+ swap 1+ then next 2drop -1 ;
forth definitions also internals
: str= ( a n a n -- f) >r swap r@ <> if rdrop 2drop 0 exit then r> mem= ;
: startswith? ( a n a n -- f ) >r swap r@ < if rdrop 2drop 0 exit then r> mem= ;
: .s   ." <" depth n. ." > " raw.s cr ;
only forth definitions

( List words in Dictionary / Vocabulary )
internals definitions
75 value line-width
: onlines ( n xt -- n xt )
   swap dup line-width > if drop 0 cr then over >name nip + 1+ swap ;
: >name-length ( xt -- n ) dup 0= if exit then >name nip ;
forth definitions also internals
: vlist   0 context @ @ begin dup >name-length while onlines dup see. >link repeat 2drop cr ;
: words   0 context @ @ begin dup while onlines dup see. >link repeat 2drop cr ;
: see-all   0 context @ @ begin dup while onlines dup see-xt >link repeat 2drop cr ;
only forth definitions
