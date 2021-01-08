( Arguments )
: 'argc ( -- a ) 'sys 8 cells + ;
: 'argv ( -- a ) 'sys 9 cells + ;
: argc ( -- n ) 'argc @ ;
: argv ( n -- a n ) cells 'argv @ + @ z>s ;

( Load Libraries )
: xlib   s" posix/xlib_test.fs" included ;
