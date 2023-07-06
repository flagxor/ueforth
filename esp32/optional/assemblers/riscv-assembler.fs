\ Copyright 2022 Bradley D. Nelson
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

RISC-V? [IF]

( Lazy loaded RISC-V assembler )
: riscv-assembler r|

current @
also assembler definitions
vocabulary riscv riscv definitions

32 names zero x1 x2 x3 x4 x5 x6 x7 x8 x9 x10 x11 x12 x13 x14 x15 x16 x17 x18 x19 x20 x21 x22 x23 x24 x25 x26 x27 x28 x29 x30 x31
: nop ;
: reg. ( n -- ) dup if base @ >r decimal ." x" . r> base ! else drop ." zero " then ;
: register ( -- in print ) ['] nop ['] reg. ;

: reg>reg' ( n -- n ) 8 - 7 and ;
: reg'. ( n -- ) 8 + reg. ;
: register' ( -- in print ) ['] reg>reg' ['] reg'. ;

: numeric ( -- in print ) ['] nop ['] . ;

numeric operand i   : iiii   i i i i ;

( Offsets for JAL )
: >ofs ( n -- n ) chere - dup 20 rshift 1 and 31 12 - lshift
                          over 1 rshift $3ff and 21 12 - lshift or
                          over 11 rshift 1 and 20 12 - lshift or
                          swap 12 rshift $ff and or ;
: ofs. ( n -- ) dup 31 12 - rshift 1 and 20 lshift
                over 21 12 - rshift $3ff and 1 lshift or
                over 20 12 - rshift 1 and 11 lshift or
                swap $ff and 12 lshift or 21 sextend address @ + . ;
' >ofs ' ofs. operand ofs
: offset   20 for aft ofs then next ;

register  operand rd#    : rd    rd# rd# rd# rd# rd# ;
register' operand rd#'   : rd'   rd#' rd#' rd#' ;
register  operand rs1#   : rs1   rs1# rs1# rs1# rs1# rs1# ;
register' operand rs1#'  : rs1'  rs1#' rs1#' rs1#' ;
register  operand rs2#   : rs2   rs2# rs2# rs2# rs2# rs2# ;
register' operand rs2#'  : rs2'  rs2#' rs2#' rs2#' ;

: R-TYPE { fn7 fn3 opc }
  fn7 7 bits  rs2  rs1  fn3 3 bits  rd      opc 7 bits OP ;
: I-TYPE { fn3 opc }
  iiii iiii iiii   rs1  fn3 3 bits  rd      opc 7 bits OP ;
: S-TYPE { fn3 opc }
  iiii i i i  rs2  rs1  fn3 3 bits  i iiii  opc 7 bits OP ;
: B-TYPE { fn3 opc }
  iiii i i i  rs2  rs1  fn3 3 bits  i iiii  opc 7 bits OP ;
: U-TYPE { opc }
  iiii iiii iiii iiii iiii          rd      opc 7 bits OP ;
: J-TYPE { opc }
  offset                            rd      opc 7 bits OP ;

$37 U-TYPE LUI,
$17 U-TYPE AUIPC,
$6F J-TYPE JAL,
$0 $67 I-TYPE JALR,

$0 $63 B-TYPE BEQ,   $1 $63 B-TYPE BNE,
$4 $63 B-TYPE BLT,   $5 $63 B-TYPE BGE,
$6 $63 B-TYPE BLTU,  $7 $63 B-TYPE BGEU,

$0 $03 I-TYPE LB,   $1 $03 I-TYPE LH,   $2 $03 I-TYPE LW,
$4 $03 I-TYPE LBU,  $5 $03 I-TYPE LHU,

$0 $23 S-TYPE SB,  $1 $23 S-TYPE SH,  $2 $23 S-TYPE SW,

$0 $13 I-TYPE ADDI,  $2 $13 I-TYPE SLTI,  $3 $13 I-TYPE SLTIU,
$4 $13 I-TYPE XORI,  $6 $13 I-TYPE ORI,   $7 $13 I-TYPE ANDI,

$00 $1 $13 R-TYPE SLLI,
$00 $5 $13 R-TYPE SRLI,  $20 $5 $13 R-TYPE SRAI,

$00 $0 $33 R-TYPE ADD,   $20 $0 $33 R-TYPE SUB,

$00 $1 $33 R-TYPE SLL,  $00 $2 $33 R-TYPE SLT,
$00 $3 $33 R-TYPE SLTU, $00 $4 $33 R-TYPE XOR,

$00 $5 $33 R-TYPE SRL,  $20 $5 $33 R-TYPE SRA,

$00 $6 $33 R-TYPE OR,   $20 $7 $33 R-TYPE AND,

( TODO FENCE, FENCE.I )

o o o o o o o o o o o o   o o o o o  o o o  o o o o o  o o o l l l l OP ECALL,
o o o o o o o o o o o l   o o o o o  o o o  o o o o o  o o o l l l l OP EBREAK,

( TODO CSR* )

o o o  o o o o o o o o  o o o  o o  OP C.ILL,
o o o  i i i i i i i i  rd'    o o  OP C.ADDI4SP,
o o l  i i i  rs1' i i  rd'    o o  OP C.FLD,
o l o  i i i  rs1' i i  rd'    o o  OP C.LW,
o l l  i i i  rs1' i i  rd'    o o  OP C.FLW,
( 4 reserved )
l o l  i i i  rs1' i i  rd'    o o  OP C.FSD,
l l o  i i i  rs1' i i  rd'    o o  OP C.SW,
l l l  i i i  rs1' i i  rd'    o o  OP C.FSW,

o o o  o  o o o o o  o o o o o   o l  OP C.NOP,
o o o  i  rs1        i i i i i   o l  OP C.ADDI,
o o l  i  i i i i i  i i i i i   o l  OP C.JAL,
o l o  i  rd         i i i i i   o l  OP C.LI,
o l l  i  rd         i i i i i   o l  OP C.LUI,
l o o  i  o o  rs1'  i i i i i   o l  OP C.SRLI,
l o o  i  o l  rs1'  i i i i i   o l  OP C.SRAI,
l o o  i  l o  rs1'  i i i i i   o l  OP C.ANDI,
l o o  o  l l  rs1'  o o  rs2'   o l  OP C.SUB,
l o o  o  l l  rs1'  o l  rs2'   o l  OP C.XOR,
l o o  o  l l  rs1'  l o  rs2'   o l  OP C.OR,
l o o  o  l l  rs1'  l l  rs2'   o l  OP C.AND,
l o o  l  l l  rs1'  o o  rs2'   o l  OP C.SUBW,
l o o  l  l l  rs1'  o l  rs2'   o l  OP C.ADDW,
l o l  i  i i i i i  i i i i i   o l  OP C.J,
l l o  i  i i  rs1'  i i i i i   o l  OP BEQZ,
l l l  i  i i  rs1'  i i i i i   o l  OP BNEZ,

o o o  i  rs1        i i i i i   l o  OP C.SLLI,
o o l  i  rd         i i i i i   l o  OP C.FLDSP,
o l o  i  rd         i i i i i   l o  OP C.LWSP,
o l l  i  rd         i i i i i   l o  OP C.FLWSP,
l o o  o  rs1        o o o o o   l o  OP C.JR,
l o o  o  rd         rs2         l o  OP C.MV,
l o o  l  o o o o o  o o o o o   l o  OP C.EBREAK,
l o o  l  rs1        o o o o o   l o  OP C.JALR,
l o o  l  rd         rs2         l o  OP C.ADD,
l o l  i  i i i i i  rs2         l o  OP C.FSDSP,
l l o  i  i i i i i  rs2         l o  OP C.SWSP,
l l l  i  i i i i i  rs2         l o  OP C.FSWSP,

also forth definitions
: riscv-assembler riscv ;
previous previous
riscv-assembler
current !

| evaluate ;

[THEN]
