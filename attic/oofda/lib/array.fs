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

needs ../oofda.fs

class Array
  value data
  value length
  value capacity
  : .construct ( n -- ) to capacity
                        here to data capacity cells allot
                        0 to length ;
  : .get ( n -- n ) cells data + @ ;
  : .set ( n n -- ) cells data + ! ;
  : .length ( -- n ) length ;
  : .capacity ( -- n ) capacity ;
  : .append ( n -- ) this .length this .capacity >= throw
                     this .length this .set 1 +to length ;
  : .length ( -- n ) length ;
end-class
