( Camera Server )
DEFINED? camera [IF]

vocabulary camera-server   camera-server definitions
also camera also httpd

: handle-image
  s" image/jpeg" ok-response
  esp_camera_fb_get dup dup @ swap cell+ @ send
  esp_camera_fb_return
;

: handle1
  handleClient
  s" /image" path str= if handle-image exit then
  notfound-response
;

: do-serve    begin ['] handle1 catch drop pause again ;

: server ( port -- )
   server
   camera-config esp_camera_init throw
   do-serve
;

only forth definitions

[THEN]
