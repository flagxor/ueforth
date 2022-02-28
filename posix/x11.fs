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

( Lazy load bindings for Xlib )

: x11 r|

forth also posix also structures
vocabulary x11   also x11 definitions

z" libX11.so" shared-library xlib

z" XOpenDisplay" 1 xlib XOpenDisplay ( a -- a )
z" XBlackPixel" 2 xlib XBlackPixel ( a n -- n )
z" XWhitePixel" 2 xlib XWhitePixel ( a n -- n )
z" XDisplayOfScreen" 1 xlib XDisplayOfScreen ( a -- a )
z" XScreenOfDisplay" 2 xlib XScreenOfDisplay ( a n -- a )
z" XDefaultColormap" 2 xlib XDefaultColormap ( a n -- n )
z" XDefaultScreen" 1 xlib XDefaultScreen ( a -- n )
z" XRootWindow" 2 xlib XRootWindow ( a n -- n )
z" XCheckMaskEvent" 3 xlib XCheckMaskEvent ( a n a -- n )
z" XCreateGC" 4 xlib XCreateGC ( a n n a -- a )
z" XCreateImage" 10 xlib XCreateImage ( a a n n n a n n n n -- a )
z" XCreateSimpleWindow" 9 xlib XCreateSimpleWindow ( a n n n n n n n n -- n )
z" XDefaultDepth" 2 xlib XDefaultDepth ( a n -- n )
z" XDefaultVisual" 2 xlib XDefaultVisual ( a n -- a )
z" XDestroyImage" 1 xlib XDestroyImage ( a -- void )
z" XFlush" 1 xlib XFlush ( a -- void )
z" XLookupString" 5 xlib XLookupString ( a a n a a -- n )
z" XMapWindow" 2 xlib XMapWindow ( a n -- void )
z" XNextEvent" 2 xlib XNextEvent ( a a -- void )
z" XPutImage" 10 xlib XPutImage ( a n a a n n n n n n -- void )
z" XSelectInput" 3 xlib XSelectInput ( a n n -- void )
z" XDrawString" 7 xlib XDrawString ( a n n n n a n -- void )
z" XSetForeground" 3 xlib XSetForeground ( a a n -- void )
z" XSetBackground" 3 xlib XSetBackground ( a a n -- void )
z" XFillRectangle" 7 xlib XFillRectangle ( a n n n n n n -- void )

0 constant XYBitmap
1 constant XYPixmap
2 constant ZPixmap

0 constant NULL

32 cells constant xevent-size

: win ( n "name" ) long ;
: time ( n "name" ) long ;
: bool ( n "name" ) i32 ;

vocabulary xany  also xany definitions
struct XAnyEvent
   i32 field ->type
  long field ->serial
  bool field ->send_event  ( Bool )
   ptr field ->display
   win field ->window
previous definitions

vocabulary xbutton  also xbutton definitions
struct XButtonEvent
   i32 field ->type
  long field ->serial
  bool field ->send_event
   ptr field ->display
   win field ->window
   win field ->root
   win field ->subwindow
  time field ->time
   i32 field ->x
   i32 field ->y
   i32 field ->x_root
   i32 field ->y_root
   i32 field ->state
   i32 field ->button
  bool field ->same_screen
previous definitions

vocabulary xkey  also xkey definitions
struct XKeyEvent
   i32 field ->type
  long field ->serial
  bool field ->send_event
   ptr field ->display
   win field ->window
   win field ->root
   win field ->subwindow
  time field ->time
   i32 field ->x
   i32 field ->y
   i32 field ->x_root
   i32 field ->y_root
   i32 field ->state
   i32 field ->keycode
  bool field ->same_screen
previous definitions

vocabulary xmotion  also xmotion definitions
struct XMotionEvent
   i32 field ->type
  long field ->serial
  bool field ->send_event
   ptr field ->display
   win field ->window
   win field ->root
   win field ->subwindow
  time field ->time
   i32 field ->x
   i32 field ->y
   i32 field ->x_root
   i32 field ->y_root
   i32 field ->state
    i8 field ->is_hint
  bool field ->same_screen
previous definitions

vocabulary xconfigure  also xconfigure definitions
struct XConfigureEvent
   i32 field ->type
  long field ->serial
  bool field ->send_event
   ptr field ->display
   win field ->event
   win field ->window
   i32 field ->x
   i32 field ->y
   i32 field ->width
   i32 field ->height
   i32 field ->border_width
   win field ->above
  bool field ->override_redirect
previous definitions

vocabulary xexposure  also xexposure definitions
struct XExposeEvent
   i32 field ->type
  long field ->serial
  bool field ->send_event
   ptr field ->display
   win field ->window
   i32 field ->x
   i32 field ->y
   i32 field ->width
   i32 field ->height
   i32 field ->count
previous definitions

0 constant NoEventMask
1 : xmask   dup constant 2* ;
xmask KeyPressMask
xmask KeyReleaseMask
xmask ButtonPressMask
xmask ButtonReleaseMask
xmask EnterWindowMask
xmask LeaveWindowMask
xmask PointerMotionMask
xmask PointerMotionHintMask
xmask Button1MotionMask
xmask Button2MotionMask
xmask Button3MotionMask
xmask Button4MotionMask
xmask Button5MotionMask
xmask ButtonMotionMask
xmask KeymapStateMask
xmask ExposureMask
xmask VisibilityChangeMask
xmask StructureNotifyMask
xmask ResizeRedirectMask
xmask SubstructureNotifyMask
xmask SubstructureRedirectMask
xmask FocusChangeMask
xmask PropertyChangeMask
xmask ColormapChangeMask
xmask OwnerGrabButtonMask
drop

2 : xevent#   dup constant 1+ ;
xevent# KeyPress
xevent# KeyRelease
xevent# ButtonPress
xevent# ButtonRelease
xevent# MotionNotify
xevent# EnterNotify
xevent# LeaveNotify
xevent# FocusIn
xevent# FocusOut
xevent# KeymapNotify
xevent# Expose
xevent# GraphicsExpose
xevent# NoExpose
xevent# VisibilityNotify
xevent# CreateNotify
xevent# DestroyNotify
xevent# UnmapNotify
xevent# MapNotify
xevent# MapRequest
xevent# ReparentNotify
xevent# ConfigureNotify
xevent# ConfigureRequest
xevent# GravityNotify
xevent# ResizeRequest
xevent# CirculateNotify
xevent# CirculateRequest
xevent# PropertyNotify
xevent# SelectionClear
xevent# SelectionRequest
xevent# SelectionNotify
xevent# ColormapNotify
xevent# ClientMessage
xevent# MappingNotify
xevent# GenericEvent
drop

previous previous forth definitions
x11
| evaluate ;
