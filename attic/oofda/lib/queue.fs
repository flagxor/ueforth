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

needs stack.fs

class Queue
  value stack1
  value stack2
  : .construct ( n -- ) dup Stack .new to stack1
                            Stack .new to stack2 ;
  : .push ( n -- ) stack1 .push ;
  : .transfer   begin stack1 .empty? 0=
                while stack1 .pop stack2 .push repeat ;
  : .pop ( -- n ) stack2 .empty? if this .transfer then
                  stack2 .pop ;
  : .empty? ( -- f ) stack1 .empty? stack2 .empty? and ;
end-class
