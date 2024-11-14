#! /usr/bin/env ueforth

needs arrays.fs

vocabulary json also internals also arrays also json definitions

defer getchar
-1 value token
: skip   getchar to token ;

variable insource
variable inlength
: in ( a n -- ) inlength ! insource ! skip ;
: ingetchar ( -- n )
   inlength @ 0= if -1 exit then
   insource @ c@ 1 insource +! -1 inlength +! ;
' ingetchar is getchar

s" DICTIONARY" _s aconstant DICT
: {{   [[ DICT ;
: }}   ]] ;

: space? ( ch -- f ) dup 8 = over 10 = or over 13 = or swap 32 = or ;
: whitespace    begin token space? while skip repeat ;
: expect ( a n -- ) for aft dup c@ token = assert 1+ skip then next drop ;
: sliteral ( a n -- ) postpone $@ dup , zplace ;
: e:   bl parse sliteral postpone expect ; immediate

: escaped
   e: \
   token skip case
     [char] " of [char] " _c catenate endof
     [char] \ of [char] \ _c catenate endof
     [char] / of [char] / _c catenate endof
     [char] b of 8 _c catenate endof
     [char] n of nl _c catenate endof
     [char] r of 13 _c catenate endof
     [char] t of 8 _c catenate endof
     [char] u of 255 _c catenate skip skip skip skip endof
     -1 throw
   endcase
;

: jstring
   e: " s" " _s
   begin token [char] " <> while
     token [char] \ = if
       escaped
     else
       token _c catenate
       skip
     then
   repeat
   e: "
;

defer jvalue

: jobject
   e: { whitespace
   DICT box
   begin
     token [char] } = if skip exit then
     jstring box whitespace e: : whitespace jvalue box
     catenate box catenate
     token [char] } = if skip exit then
     e: , whitespace
   again
   e: }
;

: digit? ( -- f ) token [char] 0 >= token [char] 9 <= and ;
: jdigit   token [char] 0 - skip ;
: jinteger   digit? assert jdigit
             begin digit? while 10 * jdigit + repeat ;
: jfraction   digit? assert 10 * jdigit + >r 1- r>
              begin digit? while 10 * jdigit + >r 1- r> repeat ;
: jnumber
  token [char] - = if skip -1 else 1 then
  token [char] 0 = if skip 0 else jinteger then 0 swap
  token [char] . = if skip jfraction then
  swap >r * r>
  token [char] e = token [char] E = or if
    skip
    token [char] - = if
      skip -1
    else
      token [char] + = if skip then 1
    then
    jinteger * +
  then
  dup if 10e s>f f** s>f f* _f else drop _i then
;

: jarray
   e: [ whitespace
   0 MIXED array
   begin
     token [char] ] = if skip exit then
     jvalue box catenate whitespace
     token [char] ] = if skip exit then
     e: , whitespace
   again
;

:noname
   whitespace
   token case
     [char] " of jstring endof
     [char] { of jobject endof
     [char] [ of jarray endof
     [char] t of e: true s" true" _s endof
     [char] f of e: false s" false" _s endof
     [char] n of e: null s" null" _s endof
     jnumber
   endcase
   whitespace
; is jvalue

: json> ( a -- a ) top top >count @ in jvalue anip ;

: butlast? ( n -- f ) top >count @ 1- <> ;

: >json ( a: a -- a )
  top >type @ case
    MIXED of
      top >count @ 1 > if top @ DICT top @ adrop = else 0 then if
        _s" {" >a top >count @ 1 ?do
          adup i a@ 0 a@ recurse _s" :" ,c a> aswap ,c >a
          adup i a@ 1 a@ recurse a> aswap ,c >a
          i butlast? if a> _s" ," ,c >a then
        loop a> _s" }" ,c
      else
        _s" [" >a top >count @ 0 ?do
          adup i a@ recurse a> aswap ,c >a
          i butlast? if a> _s" ," ,c >a then
        loop a> _s" ]" ,c
      then
    endof
    STRING of [char] " _c >a top top >count @ a> _s ,c [char] " _c ,c endof
    INTEGER of
      _s" " >a
      top >count @ 0 ?do
        top i cells + @ <# #s #> a> _s"  " ,c _s ,c >a
      loop a> endof
    REAL of
      _s" " >a top >count @ 0 ?do
        top i sfloats + sf@ <# #fs #> a> _s" " ,c _s ,c >a
      loop a> endof
  endcase
  anip
;

previous previous previous forth definitions
