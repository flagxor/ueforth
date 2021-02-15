( Local Variables )

( NOTE: These are not yet gforth compatible )

internals definitions

variable scope-depth
: scope-doer   create does> @ rp@ + @ ;
scope-doer scope-template
: scope-clear
   begin scope-depth @ while postpone rdrop cell scope-depth +! repeat
   0 scope ! ;
: scope-create ( a n -- )
   dup >r $place r> , ( name )
   scope @ , 0 , here scope ! ( link, flags )
   ['] scope-template dup @ , cell+ @ ,
   cell negate scope-depth +!   scope-depth @ , ;

( NOTE: This is not ANSForth compatible )
: (local) ( a n -- )
   >r >r postpone >r postpone ahead r> r> scope-create postpone then ;
: }? ( a n -- ) 1 <> if drop 0 exit then c@ [char] } = ;

also forth definitions

: {   begin bl parse 2dup }? if 2drop exit then (local) again ; immediate
: ;   scope-clear postpone ; ; immediate

only forth definitions
