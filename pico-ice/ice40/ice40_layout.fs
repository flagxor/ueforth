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

needs ice40_config.fs
needs flyclasses.fs

ice40 definitions
vocabulary synthesis   synthesis definitions
also flyclasses

54 constant logic-width
42 constant ram-width
logic-width ram-width - constant logicram-diff

flyclass CramBit
flyclass CramCell
  flyclass Output
    flyclass Input
      flyclass Input0
      flyclass Input1
      flyclass Input2
      flyclass Input3
  flyclass LocalG
    flyclass LocalG0
    flyclass LocalG1
    flyclass LocalG2
    flyclass LocalG3
  flyclass IOLocalG
    flyclass IOLocalG0
    flyclass IOLocalG1
  flyclass SpanWire
    flyclass Sp4HR
    flyclass Sp4VB
    flyclass Sp12HR
    flyclass Sp12VB
  flyclass IOPin
  flyclass IOInput
  flyclass IOOutput
    flyclass IOOutput0
    flyclass IOOutput1
    flyclass IOEnable
    flyclass IOFabOutput
  flyclass LUTFFGlobal
    flyclass FFSetReset
    flyclass FFClock
    flyclass FFEnable
flyclass GlobalNetwork
flyclass NotConnected

method .create ( <various> o -- o )
method .optionCount ( o -- n )
method .optionWire ( i o -- wire )
method .getOption ( o -- n )
method .setOption ( n o -- )
method .print ( o -- )

method .getXY ( o -- x y )
method .getBit ( o -- b ) ( overloaded for wires and CramBits )
method .setBit ( b o -- )
method .inside ( x y o -- o' )
method .isLogic? ( o -- f )
method .isRam? ( o -- f )
method .isIO? ( o -- f )
method .isBottom? ( o -- f )
method .isInside? ( o -- f )
method .listBits ( x o -- )
method .enableBit ( o -- bit )
method .inputEnableBit ( o -- bit )
method .setNoResetBit ( o -- bit )
method .asyncResetBit ( o -- bit )
method .carryEnableBit ( o -- bit )
method .dffEnableBit ( o -- bit )
method .setPath ( n o -- )
method .getPath ( o -- n )
method .getInput ( n o -- wire )
method .setLogic ( n o -- )
method .getLogic ( o -- n )
method .routes ( xt target o -- ) ( xt gets: bit wire )
method .walk ( xt o -- ) ( xt gets: bit wire )
method .makeOutput ( o -- )
method .makeInput ( o -- )
method .setPinType ( n o -- )
method .getPinType ( o -- n )
method .getParity ( o -- n )
method .getRow ( o -- n )

initiate

CramBit implementation
  0 cram-bank-width 2* 1- field x
  0 cram-height 1- field y
  m: .create ( x y o -- o ) put y put x ;m
  m: .print { o -- } ." CramBit(" o x . ."  , " o y . ." ) " ;m
  m: .setBit { b o -- } b o x o y cram! ;m
  m: .getBit { o -- b } o x o y cram@ ;m

: Sp4RVB { x y i -- o } x 1+ y i Sp4VB .create ;

: span4_horz { i o -- wire } o .getXY i Sp4HR .create ;
: span4_vert { i o -- wire }
   o .isBottom? if
     o .getXY 1+ i Sp4VB .create
   else
     i 35 > if NotConnected .create exit then
     o .getXY i Sp4VB .create
   then ;
: span12_vert { i o -- wire }
   o .isBottom? if
     o .getXY 1+ i Sp12VB .create
   else
     i 21 > if NotConnected .create exit then
     o .getXY i Sp12VB .create
   then ;

CramCell implementation
  -13 cells-width 13 + 1- field cx
  -13 cells-height 13 + 1- field cy
  m: .create ( x y o -- o ) put cy put cx ;m
  m: .print { o -- } ." CramCell(" o cx . ."  , " o cy . ." ) " ;m
  m: .getXY { o -- cx cy } o cx o cy ;m
  m: .inside { x y o -- o' } o cx 54 * x +
                               o cx 6 > if logicram-diff - then
                               o cx 19 > if logicram-diff - then
                             o cy 16 * y + CramBit .create ;m
  m: .isInside? { o -- f } o cx 0 >=  o cx cells-width <  and
                           o cy 0 >=  o cy cells-height <  and and ;m
  m: .isIO? { o -- f } o cx 0 = o cx cells-width 1- = or
                      o cy 0 = or o cy cells-height 1- = or ;m
  m: .isBottom? { o -- g } o cy 0= ;m
  m: .isRam? { o -- f } o cx bram-column1 = o cx bram-column2 = or o .isIO? 0= and ;m
  m: .isLogic? { o -- f } o .isRam? 0= o .isIO? 0= and ;m
  : route12 { x y i m n target xt o }
     target i o span12_vert = if
       x y o .inside o .getXY m n IOInput .create xt execute
     then ;
  : route4v { x y i m n target xt o }
     target i o span4_vert = if
       x y o .inside o .getXY m n IOInput .create xt execute
     then ;
  : route4h { x y i m n target xt o }
     target i o span4_horz = if
       x y o .inside o .getXY m n IOInput .create xt execute
     then ;
  m: .routes { xt target o -- }
    o .isIO? if
      5  1  0   0 0 target xt o route12
      5  3  8   0 0 target xt o route12
      5  5  16  0 0 target xt o route12
      23 0  16  0 0 target xt o route4v
      23 1  0   0 0 target xt o route4v
      23 2  40  0 0 target xt o route4v
      23 3  4   0 0 target xt o route4h
      25 0  24  0 0 target xt o route4v
      25 1  8   0 0 target xt o route4v
      25 2  0   0 0 target xt o route4h
      25 3  8   0 0 target xt o route4h
      26 1  32  0 0 target xt o route4v
      26 2  12  0 0 target xt o route4h

      4  6  2   0 1 target xt o route12
      5  6  10  0 1 target xt o route12
      4  7  18  0 1 target xt o route12
      23 4  18  0 1 target xt o route4v
      23 5  2   0 1 target xt o route4v
      23 6  42  0 1 target xt o route4v
      23 7  5   0 1 target xt o route4h 
      25 4  26  0 1 target xt o route4v
      25 5  10  0 1 target xt o route4v
      25 6  1   0 1 target xt o route4h
      25 7  9   0 1 target xt o route4h
      26 5  34  0 1 target xt o route4v
      26 6  13  0 1 target xt o route4h

      4  9   4   1 0 target xt o route12
      5  9   12  1 0 target xt o route12
      4  8   20  1 0 target xt o route12
      23 8   20  1 0 target xt o route4v
      23 9   4   1 0 target xt o route4v
      23 10  44  1 0 target xt o route4v
      23 11  6   1 0 target xt o route4h 
      25 8   28  1 0 target xt o route4v
      25 9   12  1 0 target xt o route4v
      25 10  2   1 0 target xt o route4h
      25 11  10  1 0 target xt o route4h
      26 9   36  1 0 target xt o route4v
      26 10  14  1 0 target xt o route4h

      5  10  6   1 1 target xt o route12
      5  12  14  1 1 target xt o route12
      5  14  22  1 1 target xt o route12
      23 12  22  1 1 target xt o route4v
      23 13  6   1 1 target xt o route4v
      23 14  46  1 1 target xt o route4v
      23 15  7   1 1 target xt o route4h
      25 12  30  1 1 target xt o route4v
      25 13  14  1 1 target xt o route4v
      25 14  3   1 1 target xt o route4h
      25 15  11  1 1 target xt o route4h
      26 13  38  1 1 target xt o route4v
      26 13  15  1 1 target xt o route4h
      exit
    then
    o .isInside? 0= if exit then
    8 0 do
      target o .getXY i 2* Sp4HR .create = if
        46 i 2* 1+ o .inside   o .getXY i Output .create   xt execute
      then
      target o .getXY i 2* 16 + Sp4HR .create = if
        46 i 2* o .inside   o .getXY i Output .create   xt execute
      then
      target o .getXY i 2* 32 + Sp4HR .create = if
        47 i 2* 1+ o .inside   o .getXY i Output .create   xt execute
      then
      target o .getXY i 2* 1+ Sp4RVB = if
        52 i 2* 1+ o .inside   o .getXY i Output .create   xt execute
      then
      target o .getXY i 2* 17 + Sp4RVB = if
        53 i 2* o .inside   o .getXY i Output .create   xt execute
      then
      target o .getXY i 2* 33 + Sp4RVB = if
        53 i 2* 1+ o .inside   o .getXY i Output .create   xt execute
      then
    loop
    4 0 do
      target o .getXY i 2* 8 + Sp12HR .create = if
        47 i 2* o .inside   o .getXY i Output .create   xt execute
      then
      target o .getXY i 2* Sp12HR .create = if
        47 i 2* 8 + o .inside   o .getXY i 4 + Output .create   xt execute
      then

      target o .getXY i 2* Sp4VB .create = if
        48 i 2* o .inside   o .getXY i Output .create   xt execute
      then
      target o .getXY i 2* 16 + Sp4VB .create = if
        48 i 2* 1+ o .inside   o .getXY i Output .create   xt execute
      then
      target o .getXY i 2* 16 + Sp12HR .create = if
        48 i 2* 8 + o .inside   o .getXY i 4 + Output .create   xt execute
      then
      target o .getXY i 2* 8 + Sp4VB .create = if
        48 i 2* 1+ 8 + o .inside   o .getXY i 4 + Output .create   xt execute
      then

      target o .getXY i 2* Sp12VB .create = if
        51 i 2* o .inside   o .getXY i Output .create   xt execute
      then
      target o .getXY i 2* 32 + Sp4VB .create = if
        51 i 2* 1+ o .inside   o .getXY i Output .create   xt execute
      then
      target o .getXY i 2* 40 + Sp4VB .create = if
        51 i 2* 8 + o .inside   o .getXY i 4 + Output .create   xt execute
      then
      target o .getXY i 2* 24 + Sp4VB .create = if
        51 i 2* 1+ 8 + o .inside   o .getXY i 4 + Output .create   xt execute
      then

      target o .getXY i 2* 16 + Sp12VB .create = if
        52 i 2* o .inside   o .getXY i Output .create   xt execute
      then
      target o .getXY i 2* 8 + Sp4VB .create = if
        52 i 2* 8 + o .inside   o .getXY i 4 + Output .create   xt execute
      then
    loop
  ;m

: getBits { n i w -- n i } n w .getBit 1 and i lshift or i 1+ ;
: setBits { n w -- n } n 1 and w .setBit n 2/ ;

Output implementation CramCell extension
  0 7 field bit
  m: .create { cx cy b o -- o } cx cy o CramCell :: .create b swap put bit ;m
  m: .print { o -- } ." Output(" o .getXY swap . . ."  , " o bit . ." ) " ;m
  m: .getBit ( o -- b ) bit ;m
  m: .optionCount { o -- n } 0 ;m
  m: .optionWire ( i o -- wire ) abort ;m
  m: .setOption ( n o -- ) abort ;m
  m: .getOption { o -- n } 0 ;m
  create input_table   Input0 , Input1 , Input2 , Input3 ,
  m: .getInput { n o -- wire } o .getXY o .getBit n cells input_table + @ .create ;m
  create logic_table   $04 c, $14 c, $15 c, $05 c, $06 c, $16 c, $17 c, $07 c,
                       $03 c, $13 c, $12 c, $02 c, $01 c, $11 c, $10 c, $00 c,
  m: .setLogic { n o -- } n ['] setBits o .listBits drop ;m
  m: .getLogic { o -- n } 0 0 ['] getBits o .listBits drop ;m
  m: .listBits { x o -- } 16 0 do
                            i logic_table + c@ 16 /mod >r 36 + r> o .getBit 2* + o .inside x execute
                          loop ;m
  m: .setNoResetBit { o -- bit } 44 o .getBit 2* 1+ o .inside ;m
  m: .asyncResetBit { o -- bit } 45 o .getBit 2* 1+ o .inside ;m
  m: .carryEnableBit { o -- bit } 44 o .getBit 2* o .inside ;m
  m: .dffEnableBit { o -- bit } 45 o .getBit 2* o .inside ;m

Input implementation Output extension
  m: .getPath { o -- n } 0 0 ['] getBits o .listBits drop ;m
  m: .setPath { n o -- } n ['] setBits o .listBits drop ;m
  m: .optionCount { o -- n } 17 ;m
  m: .getOption { o -- n } o .enableBit .getBit if
                             o .getPath 1+
                           else
                             0
                           then ;m
  m: .setOption { n o -- } n if
                             -1 o .enableBit .setBit
                             n 1- o .setPath
                           else
                             0 o .enableBit .setBit
                             0 o .setPath ( for good measure )
                           then ;m

: inOptWire { i o lo hi -- wire }
  i 0= if NotConnected .create exit then
  i 1- 2 rshift 3 and { rt }
  o .getXY
    o .getBit 1 and if hi else lo then rt rshift 1 and i 1- 2* or
    LocalG .create
;

Input0 implementation Input extension
  m: .print { o -- } ." Input0(" o .getXY swap . . ."  , " o .getBit . ." ) " ;m
  m: .enableBit { o -- wire } 29 o .getBit 2* 1+ o .inside ;m
  m: .listBits { x o -- } 26 o .getBit 2* 1+ o .inside x execute
                          26 o .getBit 2*    o .inside x execute
                          27 o .getBit 2* 1+ o .inside x execute
                          28 o .getBit 2* 1+ o .inside x execute ;m
  m: .optionWire ( i o -- wire ) $a $5 inOptWire ;m

Input1 implementation Input extension
  m: .print { o -- } ." Input1(" o .getXY swap . . ."  , " o .getBit . ." ) " ;m
  m: .enableBit { o -- wire } 29 o .getBit 2* o .inside ;m
  m: .listBits { x o -- } 30 o .getBit 2* 1+ o .inside x execute
                          30 o .getBit 2*    o .inside x execute
                          27 o .getBit 2*    o .inside x execute
                          28 o .getBit 2*    o .inside x execute ;m
  m: .optionWire ( i o -- wire ) $5 $a inOptWire ;m

Input2 implementation Input extension
  m: .print { o -- } ." Input2(" o .getXY swap . . ."  , " o .getBit . ." ) " ;m
  m: .enableBit { o -- wire } 32 o .getBit 2* 1+ o .inside ;m
  m: .listBits { x o -- } 35 o .getBit 2* 1+ o .inside x execute
                          35 o .getBit 2*    o .inside x execute
                          34 o .getBit 2* 1+ o .inside x execute
                          33 o .getBit 2* 1+ o .inside x execute ;m
  m: .optionWire ( i o -- wire ) $a $5 inOptWire ;m

Input3 implementation Input extension
  m: .print { o -- } ." Input3(" o .getXY swap . . ."  , " o .getBit . ." ) " ;m
  m: .enableBit { o -- wire } 32 o .getBit 2* o .inside ;m
  m: .listBits { x o -- } 31 o .getBit 2* 1+ o .inside x execute
                          31 o .getBit 2*    o .inside x execute
                          34 o .getBit 2*    o .inside x execute
                          33 o .getBit 2*    o .inside x execute ;m
  m: .optionWire ( i o -- wire ) $5 $a inOptWire ;m

LocalG implementation CramCell extension
  0 31 field identifier
  create localg_table   LocalG0 , LocalG1 , LocalG2 , LocalG3 ,
  m: .create { cx cy id o -- o }
    id 4 mod cells localg_table + @ to o
    cx cy o CramCell :: .create id swap put identifier ;m
  m: .print { o -- } ." LocalG(" o .getXY swap . . ."  , g" o identifier 8 /mod . . ." ) " ;m
  m: .getPath { o -- n } 0 0 ['] getBits o .listBits drop ;m
  m: .setPath { n o -- } n ['] setBits o .listBits drop ;m
  m: .optionCount { o -- n } 15 ;m
  m: .getBit { o -- n } o identifier 4 / 8 mod ;m
  m: .getOption { o -- n } o .enableBit .getBit if
                             o .getPath 1-
                           else
                             0
                           then ;m
  m: .setOption { n o -- } n if
                             -1 o .enableBit .setBit
                             n 1+ o .setPath
                           else
                             0 o .enableBit .setBit
                             0 o .setPath ( for good measure )
                           then ;m
  : sp4skew { id off -- n } id 8 mod id 16 >= if 24 + then off + ;
  : sp12skew { id off -- n cls } id 8 mod off + id 16 >= if Sp12VB else Sp12HR then ;
  m: .optionWire { i o -- wire }
    i 0 >= assert    i o .optionCount < assert
    i 0= if NotConnected .create exit then
    i case
      ( TODO: shift by 2 and add r_v_b )
      6 of o .getXY o identifier 0 sp12skew .create exit endof
      7 of o .getXY o identifier 8 sp12skew .create exit endof
      8 of o .getXY o identifier 16 sp12skew .create exit endof
      9 of o .getXY o identifier 16 sp4skew Sp4VB .create exit endof
      10 of o .getXY o identifier 0 sp4skew Sp4HR .create exit endof
      11 of o .getXY o identifier 0 sp4skew Sp4VB .create exit endof
      12 of o .getXY o identifier 8 sp4skew Sp4VB .create exit endof
      13 of o .getXY o identifier 8 sp4skew Sp4HR .create exit endof
      14 of o .getXY o identifier 16 sp4skew Sp4HR .create exit endof
    endcase
    o identifier 16 < if
      i case
        1 of 0 -1 endof
        2 of 0 1 endof
        3 of 0 0 endof
        4 of 1 -1 endof
        5 of -1 0 endof
      endcase
    else
      i case
        1 of 1 1 endof
        2 of -1 1 endof
        3 of 0 0 endof
        4 of -1 -1 endof
        5 of 1 0 endof
      endcase
    then
    { x y }
    o .getXY y + to y x + to x
    x 1 < if NotConnected .create exit then
    x cells-width 1- >= if NotConnected .create exit then
    y cells-height >= if NotConnected .create exit then
    y 0< if NotConnected .create exit then
    x y o identifier 8 mod Output .create
  ;m

LocalG0 implementation LocalG extension
  m: .enableBit { o -- wire } 17 o .getBit 2* 1+ o .inside ;m
  m: .listBits { x o -- } 14 o .getBit 2* 1+ o .inside x execute
                          15 o .getBit 2* 1+ o .inside x execute
                          14 o .getBit 2*    o .inside x execute
                          16 o .getBit 2* 1+ o .inside x execute ;m

LocalG1 implementation LocalG extension
  m: .enableBit { o -- wire } 17 o .getBit 2* o .inside ;m
  m: .listBits { x o -- } 18 o .getBit 2* 1+ o .inside x execute
                          15 o .getBit 2*    o .inside x execute
                          18 o .getBit 2*    o .inside x execute
                          16 o .getBit 2*    o .inside x execute ;m

LocalG2 implementation LocalG extension
  m: .enableBit { o -- wire } 22 o .getBit 2* 1+ o .inside ;m
  m: .listBits { x o -- } 25 o .getBit 2* 1+ o .inside x execute
                          24 o .getBit 2* 1+ o .inside x execute
                          25 o .getBit 2*    o .inside x execute
                          23 o .getBit 2* 1+ o .inside x execute ;m

LocalG3 implementation LocalG extension
  m: .enableBit { o -- wire } 22 o .getBit 2* o .inside ;m
  m: .listBits { x o -- } 21 o .getBit 2* 1+ o .inside x execute
                          24 o .getBit 2*    o .inside x execute
                          21 o .getBit 2*    o .inside x execute
                          23 o .getBit 2*    o .inside x execute ;m

create iopermy 0 , 1 , 3 , 2 , 4 , 5 , 7 , 6 , 8 , 9 , 11 , 10 , 12 , 13 , 15 , 14 ,
: >iopermy ( n -- n ) cells iopermy + @ ;

IOLocalG implementation CramCell extension
  0 15 field identifier
  create localg_table   IOLocalG0 , IOLocalG1 ,
  m: .create { cx cy id o -- o }
    id 2 mod cells localg_table + @ to o
    cx cy o CramCell :: .create id swap put identifier ;m
  m: .print { o -- } ." IOLocalG(" o .getXY swap . . ."  , g" o identifier 8 /mod . . ." ) " ;m
  m: .inside { x y o -- wire } x y >iopermy o .isBottom? if 15 swap - then o CramCell :: .inside ;m
  m: .getPath { o -- n } 0 0 ['] getBits o .listBits drop ;m
  m: .setPath { n o -- } n ['] setBits o .listBits drop ;m
  m: .optionCount { o -- n } 15 ;m
  m: .getBit { o -- n } o identifier 8 mod ;m
  m: .getRow { o -- n } o identifier 2/ ;m
  m: .getOption { o -- n } o .enableBit .getBit if
                             o .getPath 1-
                           else
                             0
                           then ;m
  m: .setOption { n o -- } n if
                             -1 o .enableBit .setBit
                             n 1+ o .setPath
                           else
                             0 o .enableBit .setBit
                             0 o .setPath ( for good measure )
                           then ;m
  : iosp4v { offset o -- wire } o .getBit offset + o span4_vert ;
  : iosp12v { offset o -- wire } o .getBit offset + o span12_vert ;
  : iosp4h { offset o -- wire } o .getBit offset + o span4_horz ;
  m: .optionWire { i o -- wire }
    i 0 >= assert    i o .optionCount < assert
    i 0= if NotConnected .create exit then
    1 +to i ( There's no 1 )
    i case
      5 of 0 o iosp4h exit endof
      6 of 8 o iosp4h exit endof
      7 of 0 o iosp12v exit endof
      8 of 8 o iosp12v exit endof
      9 of 16 o iosp12v exit endof
      10 of 0 o iosp4v exit endof
      11 of 8 o iosp4v exit endof
      12 of 16 o iosp4v exit endof
      13 of 24 o iosp4v exit endof
      14 of 32 o iosp4v exit endof
      15 of 40 o iosp4v exit endof
    endcase
    i 3 - 1 o .getXY nip if negate then { x y }
    o .getXY y + to y x + to x
    ( Could use .isLogic ? ) 
    x 1 < if NotConnected .create exit then
    x cells-width 1- >= if NotConnected .create exit then
    y cells-height 1- >= if NotConnected .create exit then
    y 1 < if NotConnected .create exit then
    x y o .getBit Output .create
  ;m

IOLocalG0 implementation IOLocalG extension
  m: .enableBit { o -- wire } 19 o .getRow 2* 1+ o .inside ;m
  m: .listBits { x o -- } 16 o .getRow 2* 1+ o .inside x execute
                          16 o .getRow 2*    o .inside x execute
                          17 o .getRow 2* 1+ o .inside x execute
                          18 o .getRow 2* 1+ o .inside x execute ;m

IOLocalG1 implementation IOLocalG extension
  m: .enableBit { o -- wire } 19 o .getRow 2* o .inside ;m
  m: .listBits { x o -- } 20 o .getRow 2* 1+ o .inside x execute
                          20 o .getRow 2*    o .inside x execute
                          17 o .getRow 2*    o .inside x execute
                          18 o .getRow 2*    o .inside x execute ;m

: optcount { n bit wire -- n } n 1+ ;
: optget { goal n bit wire -- goal n } bit .getBit if n else goal then n 1+ ;
: optset { goal n bit wire -- goal n } goal n = bit .setBit goal n 1+ ;
: optwire { answer goal n bit wire -- answer goal n } n goal = if wire else answer then goal n 1+ ;
: cross { idx n -- off ii } idx n /mod swap over 1 and xor ;

SpanWire implementation CramCell extension
  m: .optionCount { o -- n } 1 ['] optcount o .walk ;m
  m: .getOption { o -- n } 0 1 ['] optget o .walk drop ;m
  m: .setOption { n o -- } n 1 ['] optset o .walk 2drop ;m
  m: .optionWire { i o -- wire } NotConnected .create i 1 ['] optwire o .walk 2drop ;m

Sp4HR implementation SpanWire extension
  0 11 field index
  m: .create { cx cy idx o -- o } cx idx 12 cross { ii } - cy o SpanWire :: .create
                                  ii swap put index ;m
  m: .print { o -- } ." Sp4HR(" o .getXY swap . . ."  , " o index . ." ) " ;m
  m: .walk { xt o -- } 6 -5 do xt o o .getXY swap i + swap CramCell .create .routes loop ;m

Sp4VB implementation SpanWire extension
  0 11 field index
  m: .create { cx cy idx o -- o } cx cy idx 12 cross { ii } + o SpanWire :: .create
                                  ii swap put index ;m
  m: .print { o -- } ." Sp4VB(" o .getXY swap . . ."  , " o index . ." ) " ;m
  m: .walk { xt o -- } 6 -5 do xt o o .getXY i + CramCell .create .routes loop ;m

Sp12HR implementation SpanWire extension
  0 1 field index
  m: .create { cx cy idx o -- o } cx idx 2 cross { ii } - cy o SpanWire :: .create
                                  ii swap put index ;m
  m: .print { o -- } ." Sp12HR(" o .getXY swap . . ."  , " o index . ." ) " ;m
  m: .walk { xt o -- } 13 -13 do xt o o .getXY swap i + swap CramCell .create .routes loop ;m

Sp12VB implementation SpanWire extension
  0 1 field index
  m: .create { cx cy idx o -- o } cx cy idx 2 cross { ii } + o SpanWire :: .create
                                  ii swap put index ;m
  m: .print { o -- } ." Sp12VB(" o .getXY swap . . ."  , " o index . ." ) " ;m
  m: .walk { xt o -- } 13 -13 do xt o o .getXY i + CramCell .create .routes loop ;m

IOPin implementation CramCell extension
  0 1 field pinIndex
  m: .create { x y pin o -- o } x y o CramCell :: .create pin swap put pinIndex ;m
  m: .print { o -- } ." IOPin(" o .getXY swap . . o pinIndex . ." ) " ;m
  m: .inside { x y o -- wire } o .getXY nip 0= if 15 y - to y then x y o CramCell :: .inside ;m
  : nopullup { o -- }
    o pinIndex 0= if
      -1 37 7 o .inside .setBit
    else
      -1 37 12 o .inside .setBit
    then
  ;
  m: .makeInput { o -- }
    1 o .setPinType
    -1 o .enableBit .setBit
    -1 o .inputEnableBit .setBit
    o nopullup
(
    o .getXY o pinIndex 0 IOInput .create
)
    o .getXY o pinIndex 2* Output .create
  ;m
  m: .makeOutput { o -- output }
    25 o .setPinType
    -1 o .enableBit .setBit
    0 o .inputEnableBit .setBit
    o nopullup
    o .getXY o pinIndex IOOutput0 .create
  ;m
  m: .getPinType { o -- n } 0 0 ['] getBits o .listBits drop ;m
  m: .setPinType { n o -- } n ['] setBits o .listBits drop ;m
  m: .listBits { x o -- ... }
     5 2 o pinIndex 11 * + o .inside x execute
     4 2 o pinIndex 11 * + o .inside x execute
     5 0 o pinIndex 11 * + o .inside x execute
     4 0 o pinIndex 11 * + o .inside x execute
     4 4 o pinIndex 11 * + o .inside x execute
     5 4 o pinIndex 11 * + o .inside x execute
  ;m
  m: .enableBit { o -- bit } o pinIndex if 27 1 else 26 7 then o .inside ;m
  m: .inputEnableBit { o -- bit } 27 9 o pinIndex 1 xor 2* - o .inside ;m

: ioOptWire { i o parity -- wire }
  o .getXY i 2* parity xor i 4 >= if 1 xor then IOLocalG .create
;

IOInput implementation SpanWire extension
  0 1 field pinIndex
  0 1 field dinNum
  m: .create { x y pin din o -- o } x y o CramCell :: .create pin swap put pinIndex din swap put dinNum ;m
  m: .print { o -- } ." IOInput(" o .getXY swap . . o pinIndex . o dinNum . ." ) " ;m
  m: .optionCount { o -- n } 0 ;m
  m: .getOption { o -- n } 0 ;m

IOOutput implementation CramCell extension
  0 1 field pinIndex
  m: .create { x y pin o -- o } x y o CramCell :: .create pin swap put pinIndex ;m
  m: .print { o -- } ." IOOutput(" o .getXY swap . . o pinIndex . ." ) " ;m
  m: .inside { x y o -- wire } x y o .isBottom? if 15 swap - then o CramCell :: .inside ;m
  m: .getPath { o -- n } 0 0 ['] getBits o .listBits drop ;m
  m: .setPath { n o -- } n ['] setBits o .listBits drop ;m
  m: .optionCount { o -- n } 9 ;m
  m: .getOption { o -- n } o .enableBit .getBit if
                             o .getPath 1+
                           else
                             0
                           then ;m
  m: .setOption { n o -- } n if
                             -1 o .enableBit .setBit
                             n 1- o .setPath
                           else
                             0 o .enableBit .setBit
                             0 o .setPath ( for good measure )
                           then ;m
  m: .optionWire { i o -- wire }
     i 0= if NotConnected .create exit then
     i 1- o o pinIndex o .getParity xor ioOptWire ;m

IOOutput0 implementation IOOutput extension
  m: .print { o -- } ." IOOutput0(" o .getXY swap . . o pinIndex . ." ) " ;m
  m: .enableBit { o -- wire } 35 5 o pinIndex 5 * + o .inside ;m
  m: .listBits { x o -- ... }
     34 5 o pinIndex 5 * + o .inside x execute
     35 4 o pinIndex 7 * + o .inside x execute
     34 4 o pinIndex 7 * + o .inside x execute
  ;m
  m: .getParity { o -- n } 0 ;m

IOOutput1 implementation IOOutput extension
  m: .print { o -- } ." IOOutput1(" o .getXY swap . . o pinIndex . ." ) " ;m
  m: .enableBit { o -- wire } 33 9 o .inside ;m
  m: .listBits { x o -- ... }
     32 9 o pinIndex 5 * + o .inside x execute
     32 8 o pinIndex 7 * + o .inside x execute
     33 8 o pinIndex 7 * + o .inside x execute
  ;m
  m: .getParity { o -- n } 1 ;m

IOEnable implementation IOOutput extension
  m: .print { o -- } ." IOEnable(" o .getXY swap . . o pinIndex . ." ) " ;m
  m: .enableBit { o -- wire } 33 5 o .inside ;m
  m: .listBits { x o -- ... }
     32 5 o pinIndex 5 * + o .inside x execute
     32 4 o pinIndex 7 * + o .inside x execute
     33 4 o pinIndex 7 * + o .inside x execute
  ;m
  m: .getParity { o -- n } 1 ;m

IOFabOutput implementation IOOutput extension
  m: .create { x y o -- o } x y 0 o IOOutput :: .create ;m
  m: .print { o -- } ." IOFabOutput(" o .getXY swap . . ." ) " ;m
  m: .enableBit { o -- wire } 37 4 o .inside ;m
  m: .listBits { x o -- ... }
     37 5 o .inside x execute
     36 4 o .inside x execute
     36 5 o .inside x execute
  ;m
  m: .getParity { o -- n } 0 ;m

LUTFFGlobal implementation CramCell extension
  m: .print { o -- } ." LUTFFGlobal(" o .getXY swap . . ." ) " ;m
  m: .getPath { o -- n } 0 0 ['] getBits o .listBits drop ;m
  m: .setPath { n o -- } n ['] setBits o .listBits drop ;m
  m: .optionCount { o -- n } 9 ;m
  m: .getOption { o -- n } o .enableBit .getBit if
                             o .getPath 1+
                           else
                             0
                           then ;m
  m: .setOption { n o -- } n if
                             -1 o .enableBit .setBit
                             n 1- o .setPath
                           else
                             0 o .enableBit .setBit
                             0 o .setPath ( for good measure )
                           then ;m
  m: .optionWire { i o -- wire }
     o .getXY i case
       ( TODO: glb_netwk )
       5 of 0 o .getRow + LocalG .create exit endof
       6 of 9 o .getRow + LocalG .create exit endof
       7 of 16 o .getRow + LocalG .create exit endof
       8 of 25 o .getRow + LocalG .create exit endof
     endcase 2drop NotConnected .create ;m

FFSetReset implementation LUTFFGlobal extension
  m: .print { o -- } ." FFSetReset(" o .getXY swap . . ." ) " ;m
  m: .enableBit { o -- wire } 1 14 o .inside ;m
  m: .listBits { x o -- ... }
     0 15 o .inside x execute
     0 14 o .inside x execute
     1 15 o .inside x execute
  ;m
  m: .getRow ( -- n ) 4 ;m
  m: .optionWire { i o -- wire }
    i 0 > i 5 < and if i 1- 2* GlobalNetwork .create exit then
    i o LUTFFGlobal :: .optionWire ;m

FFEnable implementation LUTFFGlobal extension
  m: .print { o -- } ." FFEnable(" o .getXY swap . . ." ) " ;m
  m: .enableBit { o -- wire } 1 4 o .inside ;m
  m: .listBits { x o -- ... }
     0 5 o .inside x execute
     0 4 o .inside x execute
     1 5 o .inside x execute
  ;m
  m: .getRow ( -- n ) 2 ;m
  m: .optionWire { i o -- wire }
    i 0 > i 5 < and if i 1- 2* 1+ GlobalNetwork .create exit then
    i o LUTFFGlobal :: .optionWire ;m

FFClock implementation LUTFFGlobal extension
  m: .print { o -- } ." FFClock(" o .getXY swap . . ." ) " ;m
  m: .enableBit { o -- wire } 2 2 o .inside ;m
  m: .optionCount { o -- n } 13 ;m
  m: .listBits { x o -- ... }
     0 3 o .inside x execute
     1 2 o .inside x execute
     0 2 o .inside x execute
     2 1 o .inside x execute
  ;m
  m: .optionWire { i o -- wire }
    i 0 > i 9 < and if i 1- GlobalNetwork .create exit then
    i 4 - o LUTFFGlobal :: .optionWire ;m

GlobalNetwork implementation
  0 7 field index
  m: .create { i o -- o } i o put index ;m
  m: .print { o -- } ." GlobalNetwork(" o index . ." ) " ;m
  m: .getOption { o -- n } 0 ;m
  m: .optionCount { o -- n } 0 ;m

NotConnected implementation
  m: .create ( o -- o ) ;m
  m: .print { o -- } ." NotConnected" ;m
  m: .getOption { o -- n } 0 ;m
  m: .optionCount { o -- n } 0 ;m

: pin ( n -- o ) pin#s IOPin .create ;

: route { src dst -- f }
  \ ." SRC: " src .print ."  DST: " dst .print cr
  src dst = if -1 exit then
  dst .getOption { p }
  p if src p dst .optionWire recurse exit then
  dst .optionCount { n }
  n 0 ?do
    i dst .setOption
    src i dst .optionWire recurse if -1 unloop exit then
    0 dst .setOption
  loop 0
;

: route. { src dst }
  ." ROUTE: "
  begin src dst <> while
    dst .print ." <- "
    dst .getOption dst .optionWire to dst
  repeat
  src .print cr
;

previous
forth definitions

