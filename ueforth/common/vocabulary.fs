( Implement Vocabularies )
: forth   [ current @ ] literal context ! ;
: vocabulary ( "name" ) create 0 , current @ 2 cells + , does> cell+ context ! ;
: definitions   context @ current ! ;
: >name-length ( xt -- n ) dup 0= if exit then >name nip ;
: vlist   0 context @ @ begin dup >name-length while onlines dup see. >link repeat 2drop cr ;
: transfer ( "name" ) ' context @ begin 2dup @ <> while @ >link& repeat nip
                      dup @ swap dup @ >link swap ! current @ @ over >link& !
                      current @ ! ;
