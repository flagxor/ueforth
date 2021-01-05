( DLL Handling )
create calls
' call0 , ' call1 , ' call2 , ' call3 , ' call4 , ' call5 ,
' call6 , ' call7 , ' call8 , ' call9 , ' call10 ,
: sofunc ( z n a "name" -- )
   swap >r swap GetProcAddress dup 0= throw create , r> cells calls + @ ,
   does> dup @ swap cell+ @ execute ;
: dll ( z "name" -- )
   LoadLibraryA dup 0= throw create , does> @ sofunc ;

0 constant NULL

0 constant MB_OK
1 constant MB_OKCANCEL
2 constant MB_ABORTRETRYIGNORE
3 constant MB_YESNOCANCEL
4 constant MB_YESNO
5 constant MB_RETRYCANCEL
6 constant MB_CANCELTRYCONTINUE

-10 constant STD_INPUT_HANDLE
-11 constant STD_OUTPUT_HANDLE
-12 constant STD_ERROR_HANDLE

$0001 constant ENABLE_PROCESSED_INPUT
$0002 constant ENABLE_LINE_INPUT
$0004 constant ENABLE_ECHO_INPUT
$0008 constant ENABLE_WINDOW_INPUT
$0010 constant ENABLE_MOUSE_INPUT
$0020 constant ENABLE_INSERT_MODE
$0040 constant ENABLE_QUICK_EDIT_MODE
$0200 constant ENABLE_VIRTUAL_TERMINAL_INPUT

$0001 constant ENABLE_PROCESSED_OUTPUT
$0002 constant ENABLE_WRAP_AT_EOL_OUTPUT
$0004 constant ENABLE_VIRTUAL_TERMINAL_PROCESSING
$0008 constant DISABLE_NEWLINE_AUTO_RETURN
$0010 constant ENABLE_LVB_GRID_WORLDWIDE

z" User32.dll" dll User32
z" MessageBoxA" 4 User32 MessageBoxA

z" Kernel32.dll" dll Kernel32
z" AllocConsole" 0 Kernel32 AllocConsole
z" ExitProcess" 1 Kernel32 ExitProcess
z" GetStdHandle" 1 Kernel32 GetStdHandle
z" ReadFile" 5 Kernel32 ReadFile
z" WriteFile" 5 Kernel32 WriteFile
z" GetConsoleMode" 2 Kernel32 GetConsoleMode
z" SetConsoleMode" 2 Kernel32 SetConsoleMode

AllocConsole drop
STD_INPUT_HANDLE GetStdHandle constant stdin
STD_OUTPUT_HANDLE GetStdHandle constant stdout
STD_ERROR_HANDLE GetStdHandle constant stderr
variable console-mode
stdout console-mode GetConsoleMode drop
stdout console-mode @ ENABLE_VIRTUAL_TERMINAL_PROCESSING or SetConsoleMode drop

: win-type ( a n -- ) stdout -rot NULL NULL WriteFile drop ;
' win-type is type
: raw-key ( -- n ) 0 >r stdin rp@ 1 NULL NULL ReadFile drop r> ;
: win-key ( -- n ) begin raw-key dup 13 = while drop repeat ;
' win-key is key
: win-bye ( -- ) 0 ExitProcess drop ;
' win-bye is bye

