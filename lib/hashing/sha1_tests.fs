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

needs sha1.fs

e: test-sha1
  hashing
  s" The quick brown fox jumps over the lazy dog" sha1
    s" 2FD4E1C67A2D28FCED849EE1BB76E7391B93EB12" str= assert

  s" The quick brown fox jumps over the lazy cog" sha1
    s" DE9F2C7FD25E1B3AFAD3E85A0BD17D9B100DB4B3" str= assert

  0 0 sha1
    s" DA39A3EE5E6B4B0D3255BFEF95601890AFD80709" str= assert

  here 1024 32 fill here 1024 sha1
    s" 84C169D0021D73D6A508C9A2859571EAF5D90687" str= assert
;e
