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

needs ../poke.fs
needs ../lib/array.fs
needs ../lib/string.fs

( A logger to log steps while brewing coffee. )
class CoffeeLogger
  value logs
  : .construct   30 Array .new to logs ;
  : .log ( a n -- ) String .new logs .append ;
  : .dump   logs .length 0 ?do
               i logs .get .get type cr
            loop cr ;
end-class

class LoggerModule
  : .provideCoffeeLogger @Singleton CoffeeLogger .new ;
end-class

( A coffee maker to brew the coffee. )
class CoffeeMaker
  value logger
  value heater
  value pump
  m: .provideCoffeeLogger
  m: .provideHeater m: .providePump
  m: .on m: .off m: .pump m: .isHot?
  : .construct   @Inject CoffeeLogger to logger
                 @Inject Heater to heater
                 @Inject Pump to pump ;
  : .brew   heater .on
            pump .pump
            s" [_]P coffee! [_]P " logger .log
            heater .off ;
end-class

class CoffeeMakerModule
  : .provideCoffeeMaker CoffeeMaker .new ;
end-class

( An electric heater to heat the coffee. )
class ElectricHeater
  value logger
  value heating
  : .construct   @Inject CoffeeLogger to logger
                 0 to heating ;
  : .on   -1 to heating
          s" ~ ~ ~ heating ~ ~ ~" logger .log ;
  : .off   0 to heating ;
  : .isHot? ( -- f ) heating ;
end-class

( A thermosiphon to pump the coffee. )
class Thermosiphon
  value logger
  value heater
  : .construct   @Inject CoffeeLogger to logger
                 @Inject Heater to heater ;
  : .pump   heater .isHot? if
              s" => => pumping => =>" logger .log
            then ;
end-class

class HeaterModule
  : .provideHeater @Singleton ElectricHeater .new ;
end-class

class PumpModule
  : .providePump Thermosiphon .new ;
end-class

( The main app responsible for brewing the coffee and printing the logs. )
class CoffeeApp extends Component
  : .construct   super .construct
                 HeaterModule this .include
                 PumpModule this .include
                 LoggerModule this .include
                 CoffeeMakerModule this .include ;
end-class

CoffeeApp .new constant coffeeShop
coffeeShop .provideCoffeeMaker .brew
coffeeShop .provideCoffeeLogger .dump

bye
