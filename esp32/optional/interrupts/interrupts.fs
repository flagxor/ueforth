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

vocabulary interrupts   interrupts definitions
transfer interrupts-builtins
DEFINED? gpio_config [IF]
0 constant ESP_INTR_FLAG_DEFAULT
: ESP_INTR_FLAG_LEVELn ( n=1-6 -- n ) 1 swap lshift ;
1 7 lshift constant ESP_INTR_FLAG_NMI
1 8 lshift constant ESP_INTR_FLAG_SHARED
1 9 lshift constant ESP_INTR_FLAG_EDGE
1 10 lshift constant ESP_INTR_FLAG_IRAM
1 11 lshift constant ESP_INTR_FLAG_INTRDISABLED
( Prefix these with # because GPIO_INTR_DISABLE conflicts with a function. )
0 constant #GPIO_INTR_DISABLE
1 constant #GPIO_INTR_POSEDGE
2 constant #GPIO_INTR_NEGEDGE
3 constant #GPIO_INTR_ANYEDGE
4 constant #GPIO_INTR_LOW_LEVEL
5 constant #GPIO_INTR_HIGH_LEVEL
( Easy word to trigger on any change to a pin )
ESP_INTR_FLAG_DEFAULT gpio_install_isr_service drop
: pinchange ( xt pin ) dup #GPIO_INTR_ANYEDGE gpio_set_intr_type throw
                       swap 0 gpio_isr_handler_add throw ;
[THEN]
forth definitions
