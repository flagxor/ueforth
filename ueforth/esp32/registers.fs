vocabulary registers   registers definitions

( Tools for working with bit masks )
: m! ( val shift mask a -- )
   dup >r @ over invert and >r >r lshift r> and r> or r> ! ;
: m@ ( shift mask a -- val ) @ and swap rshift ;

only forth definitions
