#! /usr/bin/env ueforth
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

." test4.fs" cr

needs ../lib/stack.fs
needs ../lib/queue.fs

100 Stack .new constant s
123 s .push
234 s .push
345 s .push
s .pop . cr
s .pop . cr
s .pop . cr

100 Queue .new constant q
123 q .push
234 q .push
q .pop . cr
345 q .push
q .pop . cr
456 q .push
q .pop . cr
q .pop . cr

bye
