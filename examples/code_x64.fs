#! /usr/bin/env ueforth

asm forth 

code my2*
  $48 code1, $89 code1, $f8 code1, ( mov %rdi, %rax )
  $48 code1, $d1 code1, $27 code1, ( shlq [%rdi] )
  $c3 code1,                       ( ret )
end-code

123 my2* . cr
bye
