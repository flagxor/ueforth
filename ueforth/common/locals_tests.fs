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

( Testing Locals )

e: test-locals-one
  : test { a } a a * ;
  4 test 16 = assert
;e

e: test-locals-two
  : test { a b } a a a b b ;
  7 8 test .s
  out: <5> 7 7 7 8 8 
  sp0 sp!
;e

e: test-alignment
  30 allot
  : color24 { r g b } r 16 lshift g 8 lshift b or or ;
  1 2 3 color24 66051 = assert
;e

e: test-longname
  : setPixelColor { pixelNum } pixelNum ;
  1 setPixelColor 1 = assert
;e
