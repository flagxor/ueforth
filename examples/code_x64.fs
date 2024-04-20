#! /usr/bin/env ueforth

\ Copyright 2022 Bradley D. Nelson
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

asm forth 

code my2*
  $48 code1, $89 code1, $f8 code1, ( mov %rdi, %rax )
  $48 code1, $d1 code1, $27 code1, ( shlq [%rdi] )
  $c3 code1,                       ( ret )
end-code

see my2*
123 my2* . cr
bye
