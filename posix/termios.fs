\ Copyright 2021 Bradley D. Nelson
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

( Terminal Handling )

vocabulary termios   termios definitions also posix

z" tcgetattr" 2 sysfunc tcgetattr
z" tcsetattr" 3 sysfunc tcsetattr
z" ioctl" 3 sysfunc ioctl

( Blocking )
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
             _ECHO ICANON or invert new-termios .c_lflag sl@ and new-termios .c_lflag l!
             0 VTIME new-termios .c_cc[] c!
             0 VMIN new-termios .c_cc[] c!
             new-termios termios! ;
: normal-mode   old-termios termios! ;

( Screen Size )
$5413 constant TIOCGWINSZ
4 2 * constant sizeof(winsize)
create winsize sizeof(winsize) allot
: form ( -- h w ) stdin TIOCGWINSZ winsize ioctl throw
                  winsize sl@ dup $ffff and swap $10000 / ;

0 value pending
: termios-key? ( -- f ) pending if -1 else stdin-key to pending pending 0<> then ;
: termios-key ( -- n ) begin termios-key? 0= while repeat pending 0 to pending ;

nodelay-mode
' termios-key is key
' termios-key? is key?

forth definitions

: form form ;

only forth definitions
