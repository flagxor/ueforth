( Change default block source on arduino )
: arduino-default-use s" /spiffs/blocks.fb" open-blocks ;
' arduino-default-use is default-use

( Check for autoexec.fs and run if present )
: autoexec ( a n -- ) s" /spiffs/autoexec.fs" ['] included catch 2drop drop ;
autoexec
