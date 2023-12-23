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

needs interrupts.fs

internals definitions
transfer timers-builtins
forth definitions

( Lazy loaded timers )
: timers r|

vocabulary timers   timers definitions
  also registers also interrupts also internals
transfer timers-builtins

( Initialize all timers, to mimic pre-2.0 behavior. )
0 0 timer_init_null
0 1 timer_init_null
1 0 timer_init_null
1 1 timer_init_null

( 0-4 -> two cells )
: t>nx ( t -- n x ) dup 2/ 1 and swap 1 and ;

create tmp 2 cells allot
: timer@ ( t -- lo hi ) t>nx tmp timer_get_counter_value throw tmp 2@ ;
: timer! ( lo hi t -- ) >r tmp 2! r> t>nx tmp timer_set_counter_value throw ;
: alarm@ ( t -- lo hi ) t>nx tmp timer_get_alarm_value throw tmp 2@ ;
: alarm! ( lo hi t -- ) >r tmp 2! r> t>nx tmp timer_set_alarm_value throw ;

: enable! ( v t )
   swap >r t>nx r> if timer_start else timer_pause then throw ;
: increase! ( v t ) swap >r t>nx r> timer_set_counter_mode throw ;
: autoreload! ( v t ) swap >r t>nx r> timer_set_auto_reload throw ;
: divider! ( v t ) swap >r t>nx r> timer_set_divider throw ;
: alarm-enable! ( v t ) swap >r t>nx r> timer_set_alarm throw ;

: int-enable! ( f t -- )
   swap >r t>nx r> if timer_enable_intr else timer_disable_intr then throw ;

: onalarm ( xt t ) swap >r t>nx r>
                   0 ESP_INTR_FLAG_EDGE timer_isr_callback_add throw ;
: interval ( xt usec t ) 0 over enable!
                         0 over int-enable!
                         80 over divider!
                         swap over 0 swap alarm!
                         dup >r onalarm r>
                         dup >r 0 0 r> timer!
                         1 over increase!
                         1 over autoreload!
                         1 over alarm-enable!
                         1 over int-enable!
                         1 swap enable! ;

only forth definitions
timers
| evaluate ;
