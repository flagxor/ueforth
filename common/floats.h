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

#include <math.h>

#define FLOATING_POINT_LIST \
  YV(internals, DOFLIT, *++fp = *(float *) ip; ++ip) \
  X("FP@", FPAT, DUP; tos = (cell_t) fp) \
  X("FP!", FPSTORE, fp = (float *) tos; DROP) \
  X("SF@", FAT, *++fp = *(float *) tos; DROP) \
  X("SF!", FSTORE, *(float *) tos = *fp--; DROP) \
  Y(FDUP, fp[1] = *fp; ++fp) \
  Y(FNIP, fp[-1] = *fp; --fp) \
  Y(FDROP, --fp) \
  Y(FOVER, fp[1] = fp[-1]; ++fp) \
  Y(FSWAP, ft = fp[-1]; fp[-1] = *fp; *fp = ft) \
  Y(FROT, ft = fp[-2]; fp[-2] = fp[-1]; fp[-1] = *fp; *fp = ft) \
  Y(FNEGATE, *fp = -*fp) \
  X("F0<", FZLESS, DUP; tos = *fp < 0.0f ? -1 : 0; --fp) \
  X("F0=", FZEQUAL, DUP; tos = *fp == 0.0f ? -1 : 0; --fp) \
  X("F=", FEQUAL, DUP; tos = fp[-1] == fp[0] ? -1 : 0; fp -= 2) \
  X("F<", FLESS, DUP; tos = fp[-1] < fp[0] ? -1 : 0; fp -= 2) \
  X("F>", FGREATER, DUP; tos = fp[-1] > fp[0] ? -1 : 0; fp -= 2) \
  X("F<>", FNEQUAL, DUP; tos = fp[-1] != fp[0] ? -1 : 0; fp -= 2) \
  X("F<=", FLESSEQ, DUP; tos = fp[-1] <= fp[0] ? -1 : 0; fp -= 2) \
  X("F>=", FGREATEREQ, DUP; tos = fp[-1] >= fp[0] ? -1 : 0; fp -= 2) \
  X("F+", FPLUS, fp[-1] = fp[-1] + *fp; --fp) \
  X("F-", FMINUS, fp[-1] = fp[-1] - *fp; --fp) \
  X("F*", FSTAR, fp[-1] = fp[-1] * *fp; --fp) \
  X("F/", FSLASH, fp[-1] = fp[-1] / *fp; --fp) \
  X("1/F", FINVERSE, *fp = 1.0f / *fp) \
  X("S>F", STOF, *++fp = (float) tos; DROP) \
  X("F>S", FTOS, DUP; tos = (cell_t) *fp--) \
  XV(internals, "S>FLOAT?", FCONVERT, tos = fconvert((const char *) *sp, tos, fp)|0; --sp) \
  Y(SFLOAT, DUP; tos = sizeof(float)) \
  Y(SFLOATS, tos *= sizeof(float)) \
  X("SFLOAT+", SFLOATPLUS, tos += sizeof(float)) \
  X("PI", PI_CONST, *++fp = (float) 3.14159265359) \
  Y(FSIN, *fp = sin(+*fp)) \
  Y(FCOS, *fp = cos(+*fp)) \
  Y(FSINCOS, fp[1] = cos(+*fp); *fp = sin(+*fp); ++fp) \
  Y(FATAN2, fp[-1] = atan2(+fp[-1], +*fp); --fp) \
  X("F**", FSTARSTAR, fp[-1] = pow(+fp[-1], +*fp); --fp) \
  Y(FLOOR, *fp = floor(+*fp)) \
  Y(FEXP, *fp = exp(+*fp)) \
  Y(FLN, *fp = log(+*fp)) \
  Y(FABS, *fp = fabs(+*fp)) \
  Y(FMIN, fp[-1] = fmin(+fp[-1], +*fp); --fp) \
  Y(FMAX, fp[-1] = fmax(+fp[-1], +*fp); --fp) \
  Y(FSQRT, *fp = sqrt(+*fp))

