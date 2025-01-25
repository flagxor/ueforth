: kind ( "name" -- )
   create 0 ,
   does> @ begin dup while
             state @ >r   here >r   >in @ >r
             dup >r @ catch 0= if rdrop rdrop rdrop rdrop exit then
             r> cell+ @
             r> >in !   r> here - allot    r> state !
         repeat
         -1 throw ;
: :{{   >body :noname ;
: {{   latestxt :{{ ;
: }}   postpone ; here >r , dup @ , r> swap ! ; immediate
: @token ( -- ch ) >in @ tib + c@ ;
: +token   1 >in +! ;
: =token ( ch -- ) @token <> throw +token ;
: within ( ch a b ) >r over <= swap r> <= and ;
: []token ( a b -- ch ) @token -rot within 0= throw @token +token ;
: t'   postpone [char] postpone =token ; immediate
: stoken ( a n -- ) 0 ?do dup c@ =token 1+ loop drop ;
: t"   postpone s" postpone stoken ; immediate
 
