\ Copyright 2022 Bradley D. Nelson
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

windows definitions
also structures

z" Gdi32.dll" dll Gdi32

z" DeleteObject" 1 Gdi32 DeleteObject

z" CreateSolidBrush" 1 Gdi32 CreateSolidBrush
: RGB ( r g b -- n ) 16 lshift swap 8 lshift + + ;

5 constant COLOR_WINDOW

z" GetStockObject" 1 Gdi32 GetStockObject
$80000000 constant WHITE_BRUSH
$80000001 constant LTGRAY_BRUSH
$80000002 constant GRAY_BRUSH
$80000003 constant DKGRAY_BRUSH
$80000004 constant BLACK_BRUSH
$80000005 constant NULL_BRUSH
$80000006 constant WHITE_PEN
$80000007 constant BLACK_PEN
$8000000a constant OEM_FIXED_FONT
$8000000b constant ANSI_FIXED_FONT
$8000000c constant ANSI_VAR_FONT
$8000000d constant SYSTEM_FONT
$8000000e constant DEVICE_DEFAULT_PALETTE
$8000000f constant DEFAULT_PALETTE
$80000010 constant SYSTEM_FIXED_FONT
$80000011 constant DEFAULT_GUI_FONT
$80000012 constant DC_BRUSH
$80000013 constant DC_PEN

z" StretchDIBits" 13 Gdi32 StretchDIBits
struct RGBQUAD
  i8 field ->rgbBlue
  i8 field ->rgbGreen
  i8 field ->rgbRed
  i8 field ->rgbReserved
struct BITMAPINFOHEADER
  i16 field ->biSize
  i32 field ->biWidth
  i32 field ->biHeight
  i16 field ->biPlanes
  i16 field ->biBitCount
  i32 field ->biCompression
  i32 field ->biSizeImage
  i32 field ->biXPelsPerMeter
  i32 field ->biYPelsPerMeter
  i32 field ->biClrUsed
  i32 field ->biClrImportant
struct BITMAPINFO
  BITMAPINFOHEADER field ->bmiHeader
           RGBQUAD field ->bmiColors

0 constant BI_RGB
0 constant DIB_RGB_COLORS

$00cc0020 constant SRCCOPY

only forth definitions
