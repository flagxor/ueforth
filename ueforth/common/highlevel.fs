( Including Files )
: included ( a n -- )
   r/o open-file dup if nip throw else drop then
   dup file-size throw
   dup allocate throw
   swap 2dup >r >r
   rot dup >r read-file throw drop
   r> close-file throw
   r> r> over >r evaluate
   r> free throw ;
: include ( "name" -- ) bl parse included ; 
