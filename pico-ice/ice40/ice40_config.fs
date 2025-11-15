\ Copyright 2025 Bradley D. Nelson
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

vocabulary ice40   ice40 definitions

( For the 5k model in the pico-ice )

26 constant cells-width
32 constant cells-height
6 constant bram-column1
19 constant bram-column2

692 constant cram-bank-width
336 constant cram-height-lower
176 constant cram-height-upper
cram-height-lower cram-height-upper + constant cram-height
cram-bank-width 2* cram-height * 8 / constant cram-size

160 constant bram-width-lower
80 constant bram-width-upper
256 constant bram-bank-height
128 constant bram-chunk-size

( For the SG48 package in the pico-ice )
48 constant pinmax   pinmax 1+ 2* cells constant pinsize
create pinmap   pinsize allot   pinmap pinsize 0 fill
0 value pinpos   : x 1 +to pinpos ;   : p ( p# -- ) pinpos swap cells pinmap + ! x ;
: pin#s ( p# -- x y b ) cells pinmap + @ 24 /mod >r 1+ r> 2 /mod 1 xor 31 * swap ;
\ 1 2 3    4    5    6    7    8    9 10 11   12   13 14  15   16   17   18   19 20   21   22   23   24  COLUMN
\ | | |    |    |    |    |    |    |  |  |    |    |  |   |    |    |    |    |  |    |    |    |    |
  x x x 39 p 40 p 41 p    x 42 p 43 p  x  x    x 37 p  x   x 32 p 28 p 26 p 23 p  x    x    x    x    x \ 0 TOP
  x x x    x    x    x    x 38 p 36 p  x  x 35 p 34 p  x   x 31 p    x 27 p 25 p  x    x    x    x    x \ 1
\ | | |    |    |    |    |    |    |  |  |    |    |  |   |    |    |    |    |  |    |    |    |    |  PIN#
  x x x    x 46 p 47 p 48 p  2 p  4 p  x  x    x    x  x 9 p 10 p 11 p 12 p 13 p  x    x    x 14 p 15 p \ 0
  x x x    x    x 44 p 45 p    x  3 p  x  x    x  6 p  x   x    x    x 21 p 20 p  x 19 p 18 p 17 p 16 p \ 1 BOT

( Whole bitmap for config ram )
cram-size allocate throw constant cram
cram cram-size 0 fill

( Clear, read, and write as a bitmap )
: clear   cram cram-size 0 fill ;
: cr& ( x y -- a*8 ) cram-height-lower /mod if
                       cram-height-upper 1- swap - cram-height-lower +
                     then cram-bank-width * >r
                     cram-bank-width 2 - /mod if
                       cram-bank-width 1- swap - 2 - cram-bank-width cram-height * +
                     then r> + ;
: bit! ( b pos v -- v ) >r 1 swap 7 swap - lshift dup invert r> and >r swap 0<> and r> or ;
: cram! ( b x y -- ) cr& 8 /mod cram + dup >r c@ bit! r> c! ;
: cram@ ( x y -- b ) cr& 8 /mod cram + c@ swap 7 swap - rshift 1 and 0<> ;  ( UNTESTED )

forth definitions

