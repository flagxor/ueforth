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

windows definitions
also structures

z" Gdi32.dll" dll Gdi32

z" DeleteObject" 1 Gdi32 DeleteObject
z" CreateSolidBrush" 1 Gdi32 CreateSolidBrush

5 constant COLOR_WINDOW

: RGB ( r g b -- n ) 16 lshift swap 8 lshift + + ;

only forth definitions
