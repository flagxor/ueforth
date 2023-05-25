#! /usr/bin/env ueforth
\ Copyright 2023 Bradley D. Nelson
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

DEFER log
DEFER heater-on
DEFER heater-off
DEFER pump
: brew   heater-on
         pump
         s" [_]P coffee! [_]P " log
         heater-off ;

: console-log ( a n -- ) type cr ;
' console-log IS log

DEFER hot?
: thermosiphon
    hot? if s" => => pumping => =>" log then ;
' thermosiphon IS pump

0 value switch
: electric-on   -1 TO switch
                s" ~ ~ ~ heating ~ ~ ~" log ;
: electric-off   0 TO switch ;
' electric-on IS heater-on
' electric-off IS heater-off
' switch IS hot?

brew

bye
