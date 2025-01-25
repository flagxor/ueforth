#! /usr/bin/env ueforth

needs parsing.fs
also internals

: space? ( ch -- f )
   dup bl =  over nl = or  over 10 = or  swap 9 = or ;

kind <SPACE>
  {{ }}
  {{ @token space? 0= throw +token <SPACE> }}

: st'  postpone <SPACE> postpone t' postpone <SPACE> ; immediate
: st"  postpone <SPACE> postpone t" postpone <SPACE> ; immediate

kind <DIGIT>
  {{ [char] 0 [char] 9 []token [char] 0 - }}

: letnum? ( ch -- f )
   dup [char] 0 [char] 9 within
   over [char] A [char] Z within or
   over [char] a [char] z within or
   swap [char] _ = or ;

kind <IDENTIFIER>
  {{ >in @ tib + 0 begin @token letnum? while 1+ +token repeat dup 0= throw }}

kind <NUMBER'>
  {{ <DIGIT> 1 }}
  {{ <DIGIT> <NUMBER'> rot over 0 do 10 * loop -rot 1+ >r + r> }}
kind <NUMBER>
  {{ <NUMBER'> drop aliteral }}

kind <FORTH>
  {{ [char] ] parse evaluate }}
 
kind <EXPRESSION'>

kind <IDENTIFIERS>
  {{ }}
  {{ <SPACE> <IDENTIFIER> <SPACE> evaluate <IDENTIFIERS> }}

kind <FACTOR>
  {{ <SPACE> <IDENTIFIER> <SPACE> evaluate <IDENTIFIERS> }}
  {{ <SPACE> <NUMBER> <SPACE> }}
  {{ st' ( <EXPRESSION'> st' ) }}
  {{ st' [ <FORTH> }}

kind <TERM>
  {{ <FACTOR> }}
  {{ <FACTOR> st' * <TERM> postpone * }}
  {{ <FACTOR> st' / <TERM> postpone / }}
  {{ <FACTOR> st" mod" <TERM> postpone mod }}
  {{ <FACTOR> st" and" <TERM> postpone and }}

kind <SIMPLE-EXPRESSION>
  {{ <TERM> }}
  {{ <TERM> st' + <SIMPLE-EXPRESSION> postpone + }}
  {{ <TERM> st' - <SIMPLE-EXPRESSION> postpone - }}
  {{ <TERM> st" or" <SIMPLE-EXPRESSION> postpone or }}
  {{ st' + <SIMPLE-EXPRESSION> }}
  {{ st' - <SIMPLE-EXPRESSION> postpone negate }}

kind <EXPRESSION>
  {{ <SIMPLE-EXPRESSION> }}
  {{ <SIMPLE-EXPRESSION> st' = <EXPRESSION> postpone = }}
  {{ <SIMPLE-EXPRESSION> st" <>" <EXPRESSION> postpone <> }}
  {{ <SIMPLE-EXPRESSION> st' < <EXPRESSION> postpone < }}
  {{ <SIMPLE-EXPRESSION> st" <=" <EXPRESSION> postpone <= }}
  {{ <SIMPLE-EXPRESSION> st" >=" <EXPRESSION> postpone >= }}
  {{ <SIMPLE-EXPRESSION> st' > <EXPRESSION> postpone > }}

' <EXPRESSION'> :{{ <EXPRESSION> }}

kind <STATEMENTS>
  {{ <EXPRESSION> <STATEMENTS> }}
  {{ st' } }}

kind def
  {{ : st' { postpone { st' { <STATEMENTS> postpone ; }}

kind on
  {{ ' :{{ st' { <STATEMENTS> postpone }} }}

kind expr
  {{ :noname <EXPRESSION> postpone ; execute }}

