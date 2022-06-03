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

needs sha256.fs

e: test-sha256
  hashing
  0 0 sha256
    s" E3B0C44298FC1C149AFBF4C8996FB92427AE41E4649B934CA495991B7852B855" str= assert

  s" A" sha256
    s" 559AEAD08264D5795D3909718CDD05ABD49572E84FE55590EEF31A88A08FDFFD" str= assert

  0 here ! here 1 sha256
    s" 6E340B9CFFB37A989CA544E6BB780A2C78901D3FB33738768511A30617AFA01D" str= assert

  s" The quick brown fox jumped over the lazy dog." sha256
    s" 68B1282B91DE2C054C36629CB8DD447F12F096D3E3C587978DC2248444633483" str= assert

  here 1024 32 fill here 1024 sha256
    s" 67FA8B7E479417053708E46F2B3669C2F1C2857DF57ACBFF83AC6A06EA0232E9" str= assert
;e
