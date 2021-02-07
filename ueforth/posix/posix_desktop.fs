( Arguments )
internals
: 'argc ( -- a ) 'sys 9 cells + ;
: 'argv ( -- a ) 'sys 10 cells + ;
forth
: argc ( -- n ) 'argc @ ;
: argv ( n -- a n ) cells 'argv @ + @ z>s ;

( Load Libraries )
: xlib   s" posix/xlib_test.fs" included ;
