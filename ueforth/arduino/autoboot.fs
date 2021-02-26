internals definitions

( Change default block source on arduino )
: arduino-default-use s" /spiffs/blocks.fb" open-blocks ;
' arduino-default-use is default-use

( Setup remember file )
: arduino-remember-filename   s" /spiffs/myforth" ;
' arduino-remember-filename is remember-filename

( Check for autoexec.fs and run if present.
  Failing that, try to revive save image. )
: autoexec
   s" /spiffs/autoexec.fs" ['] included catch 2drop drop
   ['] revive catch drop ;
' autoexec ( leave on the stack for fini.fs )

forth definitions
