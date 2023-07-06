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

internals definitions
transfer camera-builtins
DEFINED? esp_camera_init [IF]
forth definitions

( Lazy loaded camera handling for ESP32-CAM )
: camera r|

vocabulary camera   camera definitions
  also internals
transfer camera-builtins

0 constant PIXFORMAT_RGB565
1 constant PIXFORMAT_YUV422
2 constant PIXFORMAT_GRAYSCALE
3 constant PIXFORMAT_JPEG
4 constant PIXFORMAT_RGB888
5 constant PIXFORMAT_RAW
6 constant PIXFORMAT_RGB444
7 constant PIXFORMAT_RGB555

0 constant FRAMESIZE_96x96    ( 96x96)
1 constant FRAMESIZE_QQVGA    ( 160x120 )
2 constant FRAMESIZE_QCIF     ( 176x144 )
3 constant FRAMESIZE_HQVGA    ( 240x176 )
4 constant FRAMESIZE_240x240  ( 176x144 )
5 constant FRAMESIZE_QVGA     ( 320x240 )
6 constant FRAMESIZE_CIF      ( 400x296 )
7 constant FRAMESIZE_HVGA     ( 480x320 )
8 constant FRAMESIZE_VGA      ( 640x480 )
9 constant FRAMESIZE_SVGA     ( 800x600 )
10 constant FRAMESIZE_XGA     ( 1024x768 )
11 constant FRAMESIZE_HD      ( 1280x720 )
12 constant FRAMESIZE_SXGA    ( 1280x1024 )
13 constant FRAMESIZE_UXGA    ( 1600x1200 )

( See https://github.com/espressif/esp32-camera/blob/master/driver/include/esp_camera.h )
( Settings for AI_THINKER )
create camera-config
  32 , ( pin_pwdn ) -1 , ( pin_reset ) 0 , ( pin_xclk )
  26 , ( pin_sscb_sda ) 27 , ( pin_sscb_scl )
  35 , 34 , 39 , 36 , 21 , 19 , 18 , 5 , ( pin_d7 - pin_d0 )
  25 , ( pin_vsync ) 23 , ( pin_href ) 22 , ( pin_pclk )
  20000000 , ( xclk_freq_hz )
  0 , ( ledc_timer ) 0 , ( ledc_channel )
  here
  PIXFORMAT_JPEG , ( pixel_format )
  here
  FRAMESIZE_VGA , ( frame_size )
  here
  12 , ( jpeg_quality 0-63 low good )
  here
  1 , ( fb_count )
constant camera-fb-count
constant camera-jpeg-quality
constant camera-frame-size
constant camera-format

: field@ ( n -- n ) dup create , cell+ does> @ + @ ;

0
field@ fb->buf
field@ fb->len
field@ fb->width
field@ fb->height
field@ fb->format
field@ fb->sec
field@ fb->usec
drop

5 cells
field@ s->xclk_freq_hz ( a )
field@ s->init_status ( s )
field@ s->reset ( s )
field@ s->set_pixformat ( s pixformat )
field@ s->set_framesize ( s framesize )
field@ s->set_contrast ( s level )
field@ s->set_brightness ( s level )
field@ s->set_saturation ( s level )
field@ s->set_sharpness ( s level )
field@ s->set_denoise ( s level )
field@ s->set_gainceiling ( s gainceil )
field@ s->set_quality ( s quality )
field@ s->set_colorbar ( s enable )
field@ s->set_whitebal ( s enable )
field@ s->set_gain_ctrl ( s enable )
field@ s->set_exposure_ctrl ( s enable )
field@ s->set_hmirror ( s enable )
field@ s->set_vflip ( s enable )

field@ s->set_aec2 ( s enable )
field@ s->set_awb_gain ( s enable )
field@ s->set_agc_gain ( s gain )
field@ s->set_aec_value ( s gain )

field@ s->set_special_effect ( s effect )
field@ s->set_wb_mode ( s mode )
field@ s->set_ae_level ( s level )

field@ s->set_raw_gma ( s enable )
field@ s->set_lenc ( s enable )

field@ s->get_reg ( s reg mask )
field@ s->set_reg ( s reg mask value )
field@ s->set_res_raw ( s startX startY endX endY offsetX offsetY totalX totalY outputX outputY scale binning )
field@ s->set_pll ( s bypass mul sys root pre seld5 pclken pclk )
field@ s->set_xclk ( s time xclk )
drop

forth definitions
camera
| evaluate ;

[ELSE]
forth definitions
[THEN]
