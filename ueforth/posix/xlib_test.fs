include posix/xlib.fs

z" :0" XOpenDisplay constant display
display XDefaultScreen constant screen
display screen XBlackPixel constant black
display screen XWhitePixel constant white
display screen XRootWindow constant root-window
display root-window 0 0 640 480 0 black white XCreateSimpleWindow constant window
display window XMapWindow drop
display window 0 NULL XCreateGC constant gc

ExposureMask
ButtonPressMask or
ButtonReleaseMask or
KeyPressMask or
KeyReleaseMask or
PointerMotionMask or
StructureNotifyMask or constant event-mask
display window event-mask XSelectInput drop

create event xevent-size allot
: de event xevent-size dump cr cr ;
: 1e display event XNextEvent drop de ;
: gg begin 1e again ;
