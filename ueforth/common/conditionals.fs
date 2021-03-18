( Interpret time conditionals )

: DEFINED? ( "name" -- xt|0 )
   bl parse find state @ if aliteral then ; immediate
defer [SKIP]
: [THEN] ;   : [ELSE] [SKIP] ;   : [IF] 0= if [SKIP] then ;
: [SKIP]' 0 begin postpone defined? dup if
    dup ['] [IF] = if swap 1+ swap then
    dup ['] [ELSE] = if swap dup 0 <= if 2drop exit then swap then
    dup ['] [THEN] = if swap 1- dup 0< if 2drop exit then swap then
  then drop again ;
' [SKIP]' is [SKIP]
