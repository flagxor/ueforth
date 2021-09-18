// Copyright 2021 Bradley D. Nelson
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#define FDUP (*++fp = ftos)
#define FDROP (ftos = *fp--)

#define FLOATING_POINT_LIST \
  Y(DOFLIT, FDUP; ftos = *(float *) ip++) \
  X("FP@", FPAT, DUP; tos = (cell_t) fp) \
  X("FP!", FPSTORE, fp = (float *) tos; DROP) \
  X("SF@", FAT, FDUP; ftos = *(float *) tos; DROP) \
  X("SF!", FSTORE, *(float *) tos = ftos; FDROP; DROP) \
  X("FDUP", FDUP, FDUP) \
  X("FDROP", FDROP, FDROP) \
  X("FOVER", FOVER, FDUP; ftos = fp[-1]) \
  X("FSWAP", FSWAP, float ft = ftos; ftos = *fp; *fp = ft) \
  X("FNEGATE", FNEGATE, ftos = -ftos) \
  X("F0<", FZLESS, DUP; tos = ftos < 0 ? -1 : 0; FDROP) \
  X("F+", FPLUS, ftos += *fp--) \
  X("F-", FMINUS, ftos = (*fp--) - ftos) \
  X("F*", FSTAR, ftos *= *fp--) \
  X("S>F", STOF, FDUP; ftos = (float) tos; DROP) \
  X("F>S", FTOS, DUP; tos = (cell_t) ftos; FDROP) \

