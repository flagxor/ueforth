\ Copyright 2021 Bradley D. Nelson
\
\ Licensed under the Apache License, Version 2.0 (the "License");
\ you may not use this file except in compliance with the License.
\ You may obtain a copy of the License at
\
\     http://www.apache.org/licenses/LICENSE-2.0
\
\ Unless required by applicable law or agreed to in writing, software
\ distributed under the License is distributed on an "AS IS" BASIS,
\ WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
\ See the License for the specific language governing permissions and
\ limitations under the License.

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
