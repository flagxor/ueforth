( ------------------------------------------------------------ )
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

( Expand Graphics for Windows )

grf internals definitions
also windows

z" GrfClass" constant GrfClassName
z" uEforth" constant GrfWindowTitle

0 value hinstance
0 value GrfClass
0 value hwnd
create ps PAINTSTRUCT allot
create msgbuf MSG allot

255 192 0 RGB CreateSolidBrush constant orange
0 255 0 RGB CreateSolidBrush constant green
create side RECT allot
0 side ->left !
0 side ->top !
200 side ->right !
100 side ->bottom !


: GrfWindowProc { hwnd msg w l }
  WM_DESTROY msg = if
    0 PostQuitMessage
    0 exit
  then
  WM_PAINT msg = if
    hwnd ps BeginPaint drop
    ps ->hdc @ ps ->rcPaint orange FillRect drop
    ps ->hdc @ side green FillRect drop
    hwnd ps EndPaint drop
    0 exit
  then
  hwnd msg w l DefWindowProcA
;

grf definitions
also internals
also windows

: window { width height }
  NULL GetModuleHandleA to hinstance

  pad WINDCLASSA erase
    WindowProcShim pad ->lpfnWndProc !
    hinstance pad ->hInstance !
    GrfClassName pad ->lpszClassName !
    NULL IDC_ARROW LoadCursorA pad ->hCursor !
    hinstance IDI_MAIN_ICON LoadIconA pad ->hIcon !
  pad RegisterClassA to GrfClass

  0 GrfClass GrfWindowTitle WS_OVERLAPPEDWINDOW
    CW_USEDEFAULT CW_USEDEFAULT width height
    NULL NULL hinstance ['] GrfWindowProc callback
    CreateWindowExA to hwnd

  hwnd SW_SHOWMAXIMIZED ShowWindow drop
  hwnd SetForegroundWindow drop
;

: wait
  begin msgbuf NULL 0 0 GetMessageA while
    msgbuf TranslateMessage drop
    msgbuf DispatchMessageA drop
  repeat
;

only forth definitions
