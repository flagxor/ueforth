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

." test7.fs" cr

needs ../poke.fs
needs ../lib/logging.fs

class Counter
  value log
  value range
  m: .provideRange
  m: .provideLogger
  m: .logString
  m: .logNumber
  m: .cr
  : .construct   @Inject Range to range
                 @Inject Logger to log ;
  : .doit ( n -- ) s" Counter at: " log .logString
                   log .logNumber log .cr ;
  : .run   range 0 do i 1+ this .doit loop ;
end-class

class CountingModule
  : .provideCounter @Singleton Counter .new ;
  : .provideRange 10 ;
end-class

class LoggingModule
\  : .provideLogFilename s" log.txt" ;
\  : .provideLogger @Singleton @Inject LogFilename FileLogger .new ;
  : .provideLogger @Singleton ConsoleLogger .new ;
end-class

class ProgramComponent extends Component
  : .construct   super .construct
                 LoggingModule this .include
                 CountingModule this .include ;
end-class

ProgramComponent .new .provideCounter .run

bye
