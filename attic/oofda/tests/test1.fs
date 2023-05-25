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

." test1.fs" cr

needs ../oofda.fs

class Foo
  variable x
  variable y
  : .setX   x ! ;
  : .setY   y ! ;
  : .getX   x @ ;
  : .getY   y @ ;
  : .length2   this .getX dup * this .getY dup * + ;
  : .construct   0 x ! 0 y ! ;
  : .print   ." x: " this .getX . ."  y: " this .getY . cr ;
end-class

Foo .new .print

class Bar extends Foo
  : .dude   this .print this .print ;
end-class

Foo .new .print

Bar .new constant h
h .dude
123 h .setX 456 h .setY
h .dude
bye
