#! /usr/bin/env ueforth
\ Copyright 2023 Bradley D. Nelson
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

." test3.fs" cr

needs ../oofda.fs

class Pair
  variable first
  variable second
  : .construct ( x y -- ) second ! first ! ;
  : .unpair   first @ second @ ;
end-class

class Coolness extends Pair
  variable third
  : .construct ( x y z -- ) super .construct third ! ;
  : .unpair   super .unpair third @ ;
end-class

123 456 Pair .new constant h
h .unpair . . cr

123 456 789 Coolness .new constant h2
h2 .unpair . . . cr

class Stack
  variable sp
  create start
  100 cells this .grow
  : .construct   start sp ! ;
  : .empty? ( -- f ) sp @ start @ = ;
  : .push ( n -- ) sp @ ! cell sp +! ;
  : .pop ( -- n ) this .empty? throw
                  cell negate sp +! sp @ @ ;
end-class

100 Stack .new constant s
123 s .push
234 s .push
345 s .push
s .pop . cr
s .pop . cr
s .pop . cr

bye
