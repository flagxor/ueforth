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

( Trying some things with tasks )

: printer1   42 emit 1000 ms ;
: printer2   43 emit 500 ms ;
: runner   begin pause again ;

' printer1 1000 1000 task print1
' printer2 1000 1000 task print2
print1 start-task
print2 start-task
runner
