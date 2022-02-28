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

also windows also internals

z" MyClass" constant MyClassName
z" Test Window" constant MyWindowTitle

NULL GetModuleHandleA constant hinst

pad WINDCLASSA erase
  WindowProcShim pad ->lpfnWndProc !
  hinst pad ->hInstance !
  MyClassName pad ->lpszClassName !
  NULL IDC_ARROW LoadCursorA pad ->hCursor !
  hinst IDI_MAIN_ICON LoadIconA pad ->hIcon !
pad RegisterClassA constant myclass

create ps PAINTSTRUCT allot

255 192 0 RGB CreateSolidBrush constant orange
0 255 0 RGB CreateSolidBrush constant green

create side RECT allot
0 side ->left !
0 side ->top !
200 side ->right !
100 side ->bottom !

: MyWindowProc { hwnd msg w l }
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

0 myclass MyWindowTitle WS_OVERLAPPEDWINDOW
  CW_USEDEFAULT CW_USEDEFAULT 640 480
  NULL NULL hinst ' MyWindowProc callback
  CreateWindowExA constant hwnd

hwnd SW_SHOWMAXIMIZED ShowWindow drop
hwnd SetForegroundWindow drop

create mymsg msg allot
: pump
  begin mymsg NULL 0 0 GetMessageA while
    \ mymsg ->message @ WM_>name type cr
    mymsg TranslateMessage drop
    mymsg DispatchMessageA drop
  repeat
;
pump
bye
