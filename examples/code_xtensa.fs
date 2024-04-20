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
  $004136 code3, ( entry   a1, 32 )
  $0288   code2, ( l32i.n  a8, a2, 0 )
  $1188f0 code3, ( slli    a8, a8, 1 )
  $0289   code2, ( s32i.n  a8, a2, 0 )
  $f01d   code2, ( retw.n )
end-code

see my2*
123 my2* . cr
bye
