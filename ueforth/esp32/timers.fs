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

vocabulary timers   timers definitions   also registers also interrupts

$3ff5f000 constant TIMG_BASE
( group n = 0/1, timer x = 0/1, watchdog m = 0-5 )
: TIMGn ( n -- a ) $10000 * TIMG_BASE + ;
: TIMGn_Tx ( n x -- a ) $24 * swap TIMGn + ;
: TIMGn_TxCONFIG_REG ( n x -- a ) TIMGn_Tx 0 cells + ;
: TIMGn_TxLOHI_REG ( n x -- a ) TIMGn_Tx 1 cells + ;
: TIMGn_TxUPDATE_REG ( n x -- a ) TIMGn_Tx 3 cells + ;
: TIMGn_TxALARMLOHI_REG ( n x -- a ) TIMGn_Tx 4 cells + ;
: TIMGn_TxLOADLOHI_REG ( n x -- a ) TIMGn_Tx 6 cells + ;
: TIMGn_TxLOAD_REG ( n x -- a ) TIMGn_Tx 8 cells + ;

: TIMGn_Tx_WDTCONFIGm_REG ( n m -- a ) swap TIMGn cells + $48 + ;
: TIMGn_Tx_WDTFEED_REG ( n -- a ) TIMGn $60 + ;
: TIMGn_Tx_WDTWPROTECT_REG ( n -- a ) TIMGn $6c + ;

: TIMGn_RTCCALICFG_REG ( n -- a ) TIMGn $68 + ;
: TIMGn_RTCCALICFG1_REG ( n -- a ) TIMGn $6c + ;

: TIMGn_Tx_INT_ENA_REG ( n -- a ) TIMGn $98 + ;
: TIMGn_Tx_INT_RAW_REG ( n -- a ) TIMGn $9c + ;
: TIMGn_Tx_INT_ST_REG ( n -- a ) TIMGn $a0 + ;
: TIMGn_Tx_INT_CLR_REG ( n -- a ) TIMGn $a4 + ;

: t>nx ( t -- n x ) dup 2/ 1 and swap 1 and ;

: timer@ ( t -- lo hi )
   dup t>nx TIMGn_TxUPDATE_REG 0 swap !
       t>nx TIMGn_TxLOHI_REG 2@ ;
: timer! ( lo hi t -- )
   dup >r t>nx TIMGn_TxLOADLOHI_REG 2!
       r> t>nx TIMGn_TxLOAD_REG 0 swap ! ;
: alarm ( t -- a ) t>nx TIMGn_TxALARMLOHI_REG ;

: enable! ( v t ) >r 31 $80000000 r> t>nx TIMGn_TxCONFIG_REG m! ;
: increase! ( v t ) >r 30 $40000000 r> t>nx TIMGn_TxCONFIG_REG m! ;
: autoreload! ( v t ) >r 29 $20000000 r> t>nx TIMGn_TxCONFIG_REG m! ;
: divider! ( v t ) >r 13 $1fffc000 r> t>nx TIMGn_TxCONFIG_REG m! ;
: edgeint! ( v t ) >r 12 $1000 r> t>nx TIMGn_TxCONFIG_REG m! ;
: levelint! ( v t ) >r 11 $800 r> t>nx TIMGn_TxCONFIG_REG m! ;
: alarm-enable! ( v t ) >r 10 $400 r> t>nx TIMGn_TxCONFIG_REG m! ;
: alarm-enable@ ( v t ) >r 10 $400 r> t>nx TIMGn_TxCONFIG_REG m@ ;

: int-enable! ( f t -- )
   t>nx swap >r dup 1 swap lshift r> TIMGn_Tx_INT_ENA_REG m! ;

: onalarm ( xt t ) swap >r t>nx r> 0 ESP_INTR_FLAG_EDGE 0
                   timer_isr_register throw ;
: interval ( xt usec t ) 80 over divider!
                         swap over 0 swap alarm 2!
                         1 over increase!
                         1 over autoreload!
                         1 over alarm-enable!
                         1 over edgeint!
                         0 over 0 swap timer!
                         dup >r onalarm r>
                         1 swap enable! ;
: rerun ( t -- ) 1 swap alarm-enable! ;

only forth definitions
