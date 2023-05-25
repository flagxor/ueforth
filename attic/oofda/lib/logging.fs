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

needs ../oofda.fs

class Logger
  m: .logString
  : .cr   nl >r rp@ 1 this .logString rdrop ;
  : .logNumber ( n -- ) <# #s #> this .logString ;
  : .log ( a n -- ) this .logString this .cr ;
end-class

class NullLogger extends Logger
  : .logString ( a n -- ) 2drop ;
end-class

class ConsoleLogger extends Logger
  : .logString ( a n -- ) type ;
end-class

class FileLogger extends Logger
  value handle
  : .construct ( a n -- ) w/o create-file throw to handle ;
  : .logString ( a n -- ) handle write-file drop ;
  : .close   handle close-file throw ;
end-class
