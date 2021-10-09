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

#define FLOATING_POINT_LIST \
  Y(DOFLIT, *++fp = *(float *) ip++) \
  X("FP@", FPAT, DUP; tos = (cell_t) fp) \
  X("FP!", FPSTORE, fp = (float *) tos; DROP) \
  X("SF@", FAT, *++fp = *(float *) tos; DROP) \
  X("SF!", FSTORE, *(float *) tos = *fp--; DROP) \
  X("FDUP", FDUP, fp[1] = *fp; ++fp) \
  X("FNIP", FNIP, fp[-1] = *fp; --fp) \
  X("FDROP", FDROP, --fp) \
  X("FOVER", FOVER, fp[1] = fp[-1]; ++fp) \
  X("FSWAP", FSWAP, float ft = fp[-1]; fp[-1] = *fp; *fp = ft) \
  X("FNEGATE", FNEGATE, *fp = -*fp) \
  X("F0<", FZLESS, DUP; tos = *fp-- < 0.0f ? -1 : 0) \
  X("F0=", FZEQUAL, DUP; tos = *fp-- == 0.0f ? -1 : 0) \
  X("F+", FPLUS, fp[-1] += *fp; --fp) \
  X("F-", FMINUS, fp[-1] -= *fp; --fp) \
  X("F*", FSTAR, fp[-1] *= *fp; --fp) \
  X("F/", FSLASH, fp[-1] /= *fp; --fp) \
  X("1/F", FINVERSE, *fp = 1.0 / *fp) \
  X("S>F", STOF, *++fp = (float) tos; DROP) \
  X("F>S", FTOS, DUP; tos = (cell_t) *fp--) \

