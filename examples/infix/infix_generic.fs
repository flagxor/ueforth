#! /usr/bin/env ueforth

vocabulary infix   infix definitions

variable pending
: token ( -- a ) >in @ tib + ;
: full? ( -- f ) >in @ #tib @ < ;
: (   token 0
      begin full? over 0< 0= and while
        token c@ [char] ( = if 1+ then
        token c@ [char] ) = if 1- then
        1 >in +!
      repeat
      drop token over - 1- 0 max
      dup 0= pending ! evaluate ; immediate
: scarf   bl parse evaluate
          pending @ if postpone ( then ;
: enact   state @ if , else execute then ;
: +   scarf ['] + enact ; immediate
: -   scarf ['] - enact ; immediate
: *   scarf ['] * enact ; immediate
: /   scarf ['] / enact ; immediate

forth definitions
