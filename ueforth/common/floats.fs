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

: fsqrt ( r -- r ) 1e 20 0 do fover fover f/ f+ 0.5e f* loop fnip ;

6 value precision
: set-precision ( n -- ) to precision ;

internals definitions
: #f+s ( r -- ) fdup precision 0 ?do 10e f* loop
                precision 0 ?do fdup f>s 10 mod [char] 0 + hold 0.1e f* loop
                [char] . hold fdrop f>s #s ;
forth definitions internals

: #fs ( r -- ) fdup f0< if fnegate #f+s [char] - hold else #f+s then ;
: f. ( r -- ) <# #fs #> type space ;

(
internals definitions
$80000000 constant sign-mask
$7f800000 constant exp-mask
$3f000000 constant half-mask
$007fffff constant mantissa-mask
)

forth definitions
