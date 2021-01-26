( Terminal Handling )

z" tcgetattr" 2 sysfunc tcgetattr
z" tcsetattr" 3 sysfunc tcsetattr
z" fcntl" 3 sysfunc fcntl

( Blocking )
4 constant F_SETFL
2048 constant FNDELAY
: nodelay-mode   stdin F_SETFL FNDELAY fcntl throw ;
: delay-mode   stdin F_SETFL 0 fcntl throw ;

( Raw Mode )
4 16 * 20 + constant sizeof(termios)
create old-termios sizeof(termios) allot
create new-termios sizeof(termios) allot
: .c_lflag   3 4 * + ;
: .c_cc[]   4 4 * + + ;
2 constant ICANON
8 constant _ECHO
2 constant TCSAFLUSH
5 constant VTIME
6 constant VMIN
: termios@ ( a -- ) stdin swap tcgetattr drop ;
: termios! ( a -- ) stdin TCSAFLUSH rot tcsetattr throw ;
old-termios termios@
: raw-mode   new-termios termios@
             _ECHO ICANON or invert new-termios .c_lflag l@ and new-termios .c_lflag l!
             0 VTIME new-termios .c_cc[] c!
             0 VMIN new-termios .c_cc[] c!
             new-termios termios! ;
: normal-mode   old-termios termios! ;
