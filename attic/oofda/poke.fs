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

needs oofda.fs
needs lib/array.fs

also classing definitions also forth

variable provider
: do@Inject ( xt -- o ) provider @ swap execute ;
create name-buffer 200 allot  0 value name-length
: 0name   0 to name-length ;
: +name ( a n -- ) dup >r name-buffer name-length + swap cmove
                       r> +to name-length ;
: name ( -- a n ) name-buffer name-length ;
: @Inject ( "name" -- o ) 0name s" [ ' .provide" +name bl parse +name s"  ]" +name
                          name evaluate postpone literal postpone do@Inject ; immediate
: do@Singleton ( n -- n ) this + dup @ if @ rdrop exit then
                          r> swap [ here 7 cells + ] literal swap >r >r >r exit
                          r> over >r ! r> ;
: @Singleton   this .size postpone literal
               cell this .grow
               postpone do@Singleton ; immediate

previous previous definitions

class Component
  value providers
  : .construct   50 Array .new to providers ;
  : .include ( m -- ) .new providers .append ;
  : .hasMethod ( m n -- f )
     providers .get ClassClass .getClass .getMethod ['] undefined <> ;
  : .countHasMethod { m -- f }
     0 providers .length 0 do
       m i this .hasMethod if 1+ then
     loop ;
  : .pickHasMethod { m -- provider }
     0 providers .length 0 do
       m i this .hasMethod if i providers .get unloop exit then
     loop -1 throw ;
  : .fallback { xt } xt >body @ { m }
     provider @ { old } this provider !
     m this .countHasMethod { matches }
     matches 1 > if ." Multiple Providers: " xt >name type cr -1 throw then
     matches 1 <> if xt error-fallback then
     m this .pickHasMethod xt execute
     old provider ! ;
end-class
