( Implement Vocabularies )
: forth    [ current @ ] literal context ! ;
: vocabulary ( "name" ) create 0 , current @ 2 cells + , does> cell+ context ! ;
: definitions   context @ current ! ;
