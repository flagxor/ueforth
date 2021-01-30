( ANSI Codes )
: n. ( n -- ) base @ swap decimal <# #s #> type base ! ;
: esc   27 emit ;   : bel   7 emit ;
: at-xy ( x y -- ) esc ." [" 1+ n. ." ;" 1+ n. ." H" ;
: page   esc ." [2J" esc ." [H" ;
: normal   esc ." [0m" ;
: fg ( n -- ) esc ." [38;5;" n. ." m" ;
: bg ( n -- ) esc ." [48;5;" n. ." m" ;
: clear-to-eol   esc ." [0K" ;
: scroll-down   esc ." D" ;
: scroll-up   esc ." M" ;
: hide   esc ." [?25l" ;
: show   esc ." [?25h" ;
: terminal-save   esc ." [?1049h" ;
: terminal-restore   esc ." [?1049l" ;

