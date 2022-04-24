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

( Runs in the parent source dir context )
e: test-included?
  also internals included-files
  s" including_tests/x.fs" included? 0= assert
  s" including_tests/foo/a.fs" included? 0= assert
  s" including_tests/foo/b.fs" included? 0= assert
  s" including_tests/bar/a.fs" included? 0= assert
  s" including_tests/bar/b.fs" included? 0= assert
  include including_tests/x.fs
  s" including_tests/x.fs" included? assert
  s" including_tests/foo/a.fs" included? assert
  s" including_tests/foo/b.fs" included? assert
  s" including_tests/bar/a.fs" included? assert
  s" including_tests/bar/b.fs" included? assert
  to included-files
  out: x.fs 1
  out: foo/a.fs
  out: bar/b.fs
  out: x.fs 2
  out: bar/a.fs
  out: foo/b.fs
  out: x.fs 3
;e

( Runs in the parent source dir context )
e: test-needs
  also internals included-files
  include including_tests/x.fs
  include including_tests/x.fs
  to included-files
  out: x.fs 1
  out: foo/a.fs
  out: bar/b.fs
  out: x.fs 2
  out: bar/a.fs
  out: foo/b.fs
  out: x.fs 3
  out: x.fs 1
  out: x.fs 2
  out: x.fs 3
;e
