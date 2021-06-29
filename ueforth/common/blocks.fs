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

( Block Files )
internals definitions
: clobber-line ( a -- a' ) dup 63 bl fill 63 + nl over c! 1+ ;
: clobber ( a -- ) 15 for clobber-line next drop ;
0 value block-dirty
create block-data 1024 allot
forth definitions internals

-1 value block-fid   variable scr   -1 value block-id
: open-blocks ( a n -- )
   block-fid 0< 0= if block-fid close-file throw -1 to block-fid then
   2dup r/w open-file if drop r/w create-file throw else nip nip then to block-fid ;
: use ( "name" -- ) bl parse open-blocks ;
defer default-use
internals definitions
: common-default-use s" blocks.fb" open-blocks ;
' common-default-use is default-use
: use?!   block-fid 0< if default-use then ;
: grow-blocks ( n -- ) 1024 * block-fid file-size throw max block-fid resize-file throw ;
forth definitions internals
: save-buffers
   block-dirty if
     block-id grow-blocks block-id 1024 * block-fid reposition-file throw
     block-data 1024 block-fid write-file throw
     block-fid flush-file throw
     0 to block-dirty
   then ;
: block ( n -- a ) use?! dup block-id = if drop block-data exit then
                   save-buffers dup grow-blocks
                   dup 1024 * block-fid reposition-file throw
                   block-data clobber
                   block-data 1024 block-fid read-file throw drop
                   to block-id block-data ;
: buffer ( n -- a ) use?! dup block-id = if drop block-data exit then
                    save-buffers to block-id block-data ;
: empty-buffers   -1 to block-id ;
: update   -1 to block-dirty ;
: flush   save-buffers empty-buffers ;

( Loading )
: load ( n -- ) block 1024 evaluate ;
: thru ( a b -- ) over - 1+ for aft dup >r load r> 1+ then next drop ;

( Utility )
: copy ( from to -- )
   swap block pad 1024 cmove pad swap block 1024 cmove update ;

( Editing )
: list ( n -- ) scr ! ." Block " scr @ . cr scr @ block
   15 for dup 63 type [char] | emit space 15 r@ - . cr 64 + next drop ;
internals definitions
: @line ( n -- ) 64 * scr @ block + ;
: e' ( n -- ) @line clobber-line drop update ;
forth definitions internals
vocabulary editor   also editor definitions
: l    scr @ list ;   : n    1 scr +! l ;  : p   -1 scr +! l ;
: wipe   15 for r@ e' next l ;   : e   e' l ;
: d ( n -- ) dup 1+ @line swap @line 15 @line over - cmove 15 e ;
: r ( n "line" -- ) 0 parse 64 min rot dup e @line swap cmove l ;
: a ( n "line" -- ) dup @line over 1+ @line 16 @line over - cmove> r ;
only forth definitions
