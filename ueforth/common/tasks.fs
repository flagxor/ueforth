( Cooperative Tasks )

vocabulary tasks   tasks definitions

variable task-list

forth definitions tasks

: task ( xt rsz dsz "name" )
   create here >r 0 , 0 , 0 ,
   here cell+ r@ cell+ ! cells allot
   here r@ 2 cells + ! cells allot
   dup 0= if drop else >body r@ 2 cells + @ ! then rdrop ;

: start-task ( t -- )
   task-list @ if
     task-list @ @ over !
     task-list @ !
   else
     dup task-list !
     dup !
   then
;

: pause
  rp@ task-list @ 2 cells + !
  sp@ task-list @ cell+ !
  task-list @ @ task-list !
  task-list @ cell+ @ sp!
  task-list @ 2 cells + @ rp!
;

DEFINED? ms-ticks [IF]
  : ms ( n -- ) ms-ticks >r begin pause ms-ticks r@ - over >= until rdrop drop ;
[THEN]

tasks definitions
0 0 0 task main-task   main-task start-task
forth definitions
