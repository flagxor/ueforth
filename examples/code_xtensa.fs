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
  $02 code1, $48 code1,             ( l32i.n  a4, a2, 0 )
  $11 code1, $44 code1, $f0 code1,  ( slli    a4, a4, 1 )
  $02 code1, $49 code1,             ( s32i.n  a4, a2, 0 )
  $f0 code1, $0d code1,             ( ret.n )
end-code

see my2*
123 my2* . cr
bye
