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

( Add more words that are usually in extra_opcodes.h )

: fsqrt ( r -- r ) 1e 20 0 do fover fover f/ f+ 0.5e f* loop fnip ;

3.14159265359e fconstant pi

( Transfer internals that are extra opcodes )
internals definitions
transfer{
  'heap 'context 'latestxt 'notfound
  'heap-start 'heap-size 'stack-cells
  'boot 'boot-size 'tib
  'argc 'argv 'runner fill32
}transfer
forth definitions
