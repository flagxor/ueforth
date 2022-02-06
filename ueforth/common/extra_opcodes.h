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

#define EXTRA_OPCODE_LIST \
  Y(nip, NIP) \
  Y(rdrop, --rp) \
  X("*/", STARSLASH, SSMOD_FUNC; NIP) \
  X("*", STAR, tos *= *sp--) \
  X("/mod", SLASHMOD, DUP; *sp = 1; SSMOD_FUNC) \
  X("/", SLASH, DUP; *sp = 1; SSMOD_FUNC; NIP) \
  Y(mod, DUP; *sp = 1; SSMOD_FUNC; DROP) \
  Y(invert, tos = ~tos) \
  Y(negate, tos = -tos) \
  X("-", MINUS, tos = (*sp--) - tos) \
  Y(rot, w = sp[-1]; sp[-1] = *sp; *sp = tos; tos = w) \
  X("-rot", MROT, w = tos; tos = *sp; *sp = sp[-1]; sp[-1] = w) \
  X("<", LESS, tos = (*sp--) < tos ? -1 : 0) \
  X(">", GREATER, tos = (*sp--) > tos ? -1 : 0) \
  X("<=", LESSEQ, tos = (*sp--) <= tos ? -1 : 0) \
  X(">=", GREATEREQ, tos = (*sp--) >= tos ? -1 : 0) \
  X("=", EQUAL, tos = (*sp--) == tos ? -1 : 0) \
  X("<>", NOTEQUAL, tos = (*sp--) != tos ? -1 : 0) \
  X("0<>", ZNOTEQUAL, tos = tos ? -1 : 0) \
  Y(bl, DUP; tos = ' ') \
  Y(nl, DUP; tos = '\n') \
  X("1+", ONEPLUS, ++tos) \
  X("1-", ONEMINUS, --tos) \
  X("2*", TWOSTAR, tos <<= 1) \
  X("2/", TWOSLASH, tos >>= 1) \
  X("4*", FOURSTAR, tos <<= 2) \
  X("4/", FOURSLASH, tos >>= 2) \
  X("+!", PLUSSTORE, *(cell_t *) tos += *sp--; DROP) \
  X("cell+", CELLPLUS, tos += sizeof(cell_t)) \
  X("cells", CELLSTAR, tos *= sizeof(cell_t)) \
  X("cell/", CELLSLASH, DUP; tos = sizeof(cell_t); DUP; *sp = 1; SSMOD_FUNC; NIP) \
  X("2drop", TWODROP, NIP; DROP) \
  X("2dup", TWODUP, DUP; tos = sp[-1]; DUP; tos = sp[-1]) \
  X("2@", TWOAT, DUP; *sp = ((cell_t *) tos)[1]; tos = *(cell_t *) tos) \
  X("2!", TWOSTORE, DUP; ((cell_t *) tos)[0] = sp[-1]; \
      ((cell_t *) tos)[1] = *sp; sp -= 2; DROP) \
  Y(cmove, memmove((void *) *sp, (void *) sp[-1], tos); sp -= 2; DROP) \
  X("cmove>", cmove2, memmove((void *) *sp, (void *) sp[-1], tos); sp -= 2; DROP) \
  Y(fill, memset((void *) sp[-1], tos, *sp); sp -= 2; DROP) \
  Y(erase, memset((void *) *sp, 0, tos); NIP; DROP) \
  Y(blank, memset((void *) *sp, ' ', tos); NIP; DROP) \
  Y(min, tos = tos < *sp ? tos : *sp; NIP) \
  Y(max, tos = tos > *sp ? tos : *sp; NIP) \
  Y(abs, tos = tos < 0 ? -tos : tos) \
  Y(aligned, tos = CELL_ALIGNED(tos)) \
  X(">flags", TOFLAGS, tos = *TOFLAGS(tos)) \
  X(">params", TOPARAMS, tos = *TOPARAMS(tos)) \
  X(">size", TOSIZE, tos = TOSIZE(tos)) \
  X(">link&", TOLINKAT, tos = (cell_t) TOLINK(tos)) \
  X(">link", TOLINK, tos = *TOLINK(tos)) \
  X(">name", TONAME, DUP; *sp = (cell_t) TONAME(tos); tos = *TONAMELEN(tos)) \
  X(">body", TOBODY, tos = (cell_t) TOBODY(tos))
