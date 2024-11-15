\ Copyright 2024 Bradley D. Nelson
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

: slurp-file ( a n -- a n )
  r/o open-file throw >r
  r@ file-size throw ( sz )
  dup 1+ allocate throw swap ( data sz )
  2dup r@ read-file throw drop
  r> close-file throw
  2dup + 0 swap c!
;
