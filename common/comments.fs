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

: (   bl nl + 1- parse drop drop ; immediate ( Now can do comments! )
( bl=32 nl=10 so nl+32-1=41, right paren )
: \   nl parse drop drop ; immediate
: #!   nl parse drop drop ; immediate  ( shebang for scripts )
