( Implement Vocabularies )
: forth    [ current @ ] literal context ! ;
: vocabulary ( "name" ) create 0 , current @ 2 cells + , does> cell+ context ! ;
: definitions   context @ current ! ;
: vlist   0 context @ @ begin onlines dup see. >link dup 0= if 2drop cr exit then
                        dup >name nip 0= until 2drop cr ;
