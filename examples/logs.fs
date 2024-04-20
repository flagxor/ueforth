#! /usr/bin/env ueforth

also recognizers
also internals

vocabulary gauss-logs also gauss-logs also gauss-logs definitions

256 constant ~precision

forth definitions

: ~* ( ~ ~ -- ~ ) + ;
: ~/ ( ~ ~ -- ~ ) - ;
: ~/1 ( ~ -- ~ ) negate ;

: ~>f ( ~ -- f ) 2e s>f ~precision s>f f/ f** ;
: f>~ ( f -- ~ ) fln 2e fln f/ ~precision s>f f* 0.5e f+ floor f>s ;
: ~. ( ~ -- ) ~>f f. ;

gauss-logs definitions

: ~summer ( ~ -- ~ ) ~>f 1e f+ f>~ ;
: ~differ ( ~ -- ~ ) 1e ~>f f- fabs f>~ ;
: ~order ( ~ ~ -- ~ ~ ) 2dup max >r min r> ;

0 value entries
: tabulate
  begin
    entries ~summer ,
    entries ~differ ,
    1 +to entries
    entries ~summer entries =
    entries ~differ entries = and if exit then
  again
;
create table tabulate 
: ~summer1 ( ~ -- ~ ) dup entries < if 2* cells table + @ then ;
: ~differ1 ( ~ -- ~ ) dup entries < if 2* 1+ cells table + @ then ;

forth definitions

: ~+ ( ~ ~ -- ~ ) ~order over - ~summer1 + ;
: ~- ( ~ ~ -- ~ ) ~order over - ~differ1 + ;

gauss-logs definitions

: print-digits   11 0 do i . i s>f f>~ dup . ~. cr loop ;
: ~dig, ( n -- ) , ;
create digits ( 1-10 )
-99999999 ,
        0 ~dig,
    65536 ~dig,
   103872 ~dig,
   131072 ~dig,
   152170 ~dig,
   169408 ~dig,
   183983 ~dig,
   196608 ~dig,
   207744 ~dig,
   217706 ~dig,
: dig ( n n -- ~ ) >r cells digits + @ r> * ~precision 65536 */ ;
: ~>s ( ~ -- n ) ~>f f>s ;
: s>~ ( n -- ~ ) s>f f>~ ;

0 value result
0 value places
0 value fract
: !digit ( ch -- ) dup [char] 0 < over [char] 9 > or if -1 throw then ;
: =digit ( ch -- )
  dup [char] . = if drop -1 to fract exit then
  !digit
  [char] 0 - to result ;
: +digit ( ch -- )
  dup [char] . = if drop -1 to fract exit then
  !digit
  fract if 1 +to places then
  [char] 0 - result 10 * + to result ;
: ~conv ( a n -- )
  >r 1+ dup c@ =digit r>
  2 - for aft 1+ dup c@ +digit then next
  drop
  result s>~ 10 places dig ~/
  ['] aliteral rectype-num
;
: rec-~ ( a n -- )
  dup 2 < if 2drop rectype-none exit then
  over c@ [char] ~ <> if 2drop rectype-none exit then
  0 to fract
  0 to result
  0 to places
  ['] ~conv catch if 2drop rectype-none exit then
;
' rec-~ +recognizer

forth definitions
previous previous previous previous
