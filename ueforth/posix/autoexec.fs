( Include first argument if any )
internals definitions
: autoexec
   ( Open passed file if any. )
   argc 2 >= if 1 argv included exit then
   ( Open remembered file if any. )
   ['] revive catch drop
;
' autoexec ( leave on dstack for fini.fs )
forth definitions
