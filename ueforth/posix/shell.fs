( Shell like words )
vocabulary shell   shell definitions also posix
512 constant max-path
create cwd max-path allot   0 value cwd#
: +cwd ( a n -- ) dup >r cwd cwd# + swap cmove r> cwd# + to cwd# ;
: /+   s" /" +cwd ;  : /? cwd cwd# 1- + c@ [char] / = ;
/+

: pwd   cwd cwd# type cr ;
: cd..   begin /? cwd# 1- to cwd# until ;
: cd ( "name" ) bl parse /? 0= if /+ then +cwd ;
