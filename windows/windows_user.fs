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
also internals

z" User32.dll" dll User32

z" MessageBoxA" 4 User32 MessageBoxA
0 constant MB_OK
1 constant MB_OKCANCEL
2 constant MB_ABORTRETRYIGNORE
3 constant MB_YESNOCANCEL
4 constant MB_YESNO
5 constant MB_RETRYCANCEL
6 constant MB_CANCELTRYCONTINUE

z" RegisterClassA" 1 User32 RegisterClassA
struct WINDCLASSA
  i16 field ->style
  ptr field ->lpfnWndProc
  i32 field ->cbClsExtra
  i32 field ->cbWndExtra
  ptr field ->hInstance
  ptr field ->hIcon
  ptr field ->hCursor
  ptr field ->hbrBackground
  ptr field ->lpszMenuName
  ptr field ->lpszClassName

z" ShowWindow" 2 User32 ShowWindow
0 constant SW_HIDE
1 constant SW_NORMAL
2 constant SW_SHOWMINIMIZED
3 constant SW_MAXIMIZED
4 constant SW_SHOWNOACTIVATE
5 constant SW_SHOW
6 constant SW_MINIMIZE
7 constant SW_SHWOMINNOACTIVE
8 constant SW_SHOWNA
9 constant SW_RESTORE
10 constant SW_SHOWDEFAULT
11 constant SW_FORCEMINIMIZE
SW_NORMAL constant SW_SHOWNORMAL
SW_MAXIMIZED constant SW_SHOWMAXIMIZED

z" SetForegroundWindow" 1 User32 SetForegroundWindow
z" DefWindowProcA" 4 User32 DefWindowProcA

: callback ( xt -- ) here >r , ['] yield , r> ;

z" CreateWindowExA" 12 User32 CreateWindowExA
$00000000 constant WS_OVERLAPPED
$00010000 constant WS_MAXIMIZEBOX
$00020000 constant WS_MINIMIZEBOX
$00040000 constant WS_THICKFRAME
$00080000 constant WS_SYSMENU
$00100000 constant WS_HSCROLL
$00200000 constant WS_VSCROLL
$00400000 constant WS_DLGFRAME
$00800000 constant WS_BORDER
$01000000 constant WS_MAXIMIZE
$02000000 constant WS_CLIPCHILDREN
$04000000 constant WS_CLIPSIBLINGS
$08000000 constant WS_DISABLED
$10000000 constant WS_VISIBLE
$20000000 constant WS_MINIMIZE
$40000000 constant WS_CHILD
$80000000 constant WS_POPUP
WS_MAXIMIZEBOX constant WS_TABSTOP  ( With dialog boxes )
WS_MINIMIZEBOX constant WS_GROUP  ( With dialog boxes )
WS_CHILD constant WS_CHILDWINDOW
WS_MINIMIZE constant WS_ICONIC
WS_OVERLAPPED constant WS_TILED
WS_DLGFRAME WS_BORDER or constant WS_CAPTION
WS_OVERLAPPED WS_CAPTION or WS_SYSMENU or
WS_THICKFRAME or WS_MINIMIZEBOX or WS_MAXIMIZEBOX or constant WS_OVERLAPPEDWINDOW
WS_POPUP WS_BORDER or WS_SYSMENU or constant WS_POPUPWINDOW
WS_OVERLAPPEDWINDOW constant WS_TILEDWINDOW

( General use )
$400000 constant DefaultInstance
1001 constant IDI_MAIN_ICON
$80000000 constant CW_USEDEFAULT

struct POINT
  i32 field ->x
  i32 field ->y

struct RECT
  i32 field ->left
  i32 field ->top
  i32 field ->right
  i32 field ->bottom

z" GetMessageA" 4 User32 GetMessageA
z" PeekMessageA" 5 User32 PeekMessageA
z" TranslateMessage" 1 User32 TranslateMessage
z" DispatchMessageA" 1 User32 DispatchMessageA
struct MSG
    ptr field ->hwnd
    i32 field ->message
    i16 field ->wParam
    i32 field ->lParam
    i32 field ->time
   POINT field ->pt
    i32 field ->lPrivate
0 constant PM_NOREMOVE
1 constant PM_REMOVE
2 constant PM_NOYIELD

z" GetDC" 1 User32 GetDC
z" BeginPaint" 2 User32 BeginPaint
z" EndPaint" 2 User32 EndPaint
struct PAINTSTRUCT
   ptr field ->hdc
   i32 field ->fErase
  RECT field ->rcPaint
   i32 field ->fRestore
   i32 field ->fIncUpdate
    32 field ->rgbReserved

z" FillRect" 3 User32 FillRect
z" PostQuitMessage" 1 User32 PostQuitMessage

z" LoadCursorA" 2 User32 LoadCursorA
32512 constant IDC_ARROW
32513 constant IDC_IBEAM
32514 constant IDC_WAIT
32515 constant IDC_CROSS
32516 constant IDC_UPARROW
32640 constant IDC_SIZE
32641 constant IDC_ICON
32642 constant IDC_SIZENWSE
32643 constant IDC_SIZENESW
32644 constant IDC_SIZEWE
32645 constant IDC_SIZENS
32646 constant IDC_SIZEALL
32648 constant IDC_NO
32649 constant IDC_HAND
32650 constant IDC_APPSTARTING
32651 constant IDC_HELP

z" LoadIconA" 2 User32 LoadIconA
32512 constant IDI_APPLICATION
32513 constant IDI_HAND
32514 constant IDI_QUESTION
32515 constant IDI_EXCLAMATION
32516 constant IDI_ASTERISK
32517 constant IDI_WINLOGO
32518 constant IDI_SHIELD
IDI_EXCLAMATION constant IDI_WARNING
IDI_HAND constant IDI_ERROR
IDI_ASTERISK constant IDI_INFORMATION

: GET_Y_LPARAM ( n -- n ) >r rp@ 2 + sw@ rdrop ;
: GET_X_LPARAM ( n -- n ) >r rp@ sw@ rdrop ;

18 constant VK_ALT

( Check for Windows 10 DPI awareness )
z" SetThreadDpiAwarenessContext" ' User32 contains? [IF]
  z" SetThreadDpiAwarenessContext" 1 User32 SetThreadDpiAwarenessContext
  : dpi-aware   -2 SetThreadDpiAwarenessContext drop ;
[ELSE]
  : dpi-aware ;
[THEN]

only forth definitions
