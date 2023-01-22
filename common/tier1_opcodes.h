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

#define TIER1_OPCODE_LIST \
  Y(nip, NIP) \
  Y(rdrop, --rp) \
  XV(forth, "*/", STARSLASH, SSMOD_FUNC; NIP) \
  X("*", STAR, tos *= *sp--) \
  X("/mod", SLASHMOD, SLASHMOD_FUNC) \
  X("/", SLASH, SLASH_FUNC) \
  X("mod", MOD, MOD_FUNC) \
  Y(invert, tos = ~tos) \
  Y(negate, tos = -tos) \
  X("-", MINUS, tos = (*sp--) - tos) \
  Y(rot, w = sp[-1]; sp[-1] = *sp; *sp = tos; tos = w) \
  X("-rot", MROT, w = tos; tos = *sp; *sp = sp[-1]; sp[-1] = w) \
  X("?dup", QDUP, if (tos) DUP) \
  X("<", LESS, tos = *sp < tos ? -1 : 0; --sp) \
  X(">", GREATER, tos = *sp > tos ? -1 : 0; --sp) \
  X("<=", LESSEQ, tos = *sp <= tos ? -1 : 0; --sp) \
  X(">=", GREATEREQ, tos = *sp >= tos ? -1 : 0; --sp) \
  X("=", EQUAL, tos = *sp == tos ? -1 : 0; --sp) \
  X("<>", NOTEQUAL, tos = *sp != tos ? -1 : 0; --sp) \
  X("0<>", ZNOTEQUAL, tos = tos ? -1 : 0) \
  Y(bl, DUP; tos = ' ') \
  Y(nl, DUP; tos = '\n') \
  X("1+", ONEPLUS, ++tos) \
  X("1-", ONEMINUS, --tos) \
  X("2*", TWOSTAR, tos = tos << 1) \
  X("2/", TWOSLASH, tos = tos >> 1) \
  X("4*", FOURSTAR, tos = tos << 2) \
  X("4/", FOURSLASH, tos = tos >> 2) \
  X("+!", PLUSSTORE, *((cell_t *) tos) += *sp--; DROP) \
  X("cell+", CELLPLUS, tos += sizeof(cell_t)) \
  Y(cells, tos *= sizeof(cell_t)) \
  X("cell/", CELLSLASH, CELLSLASH_FUNC) \
  X("2drop", TWODROP, NIP; DROP) \
  X("2dup", TWODUP, DUP; tos = sp[-1]; DUP; tos = sp[-1]) \
  X("2@", TWOAT, DUP; *sp = *(cell_t *) tos; tos = ((cell_t *) tos)[1]) \
  X("2!", TWOSTORE, *(cell_t *) tos = sp[-1]; \
      ((cell_t *) tos)[1] = *sp; sp -= 2; DROP) \
  Y(cmove, memmove((void *) *sp, (void *) sp[-1], tos); sp -= 2; DROP) \
  X("cmove>", cmove2, memmove((void *) *sp, (void *) sp[-1], tos); sp -= 2; DROP) \
  Y(fill, memset((void *) sp[-1], tos, *sp); sp -= 2; DROP) \
  Y(erase, memset((void *) *sp, 0, tos); NIP; DROP) \
  Y(blank, memset((void *) *sp, ' ', tos); NIP; DROP) \
  Y(min, tos = tos < *sp ? tos : *sp; NIP) \
  Y(max, tos = tos > *sp ? tos : *sp; NIP) \
  Y(abs, tos = tos < 0 ? -tos : tos) \
  Y(here, DUP; tos = (cell_t) g_sys->heap) \
  Y(allot, g_sys->heap = (cell_t *) (tos + (cell_t) g_sys->heap); DROP) \
  X(",", COMMA, COMMA(tos); DROP) \
  X("c,", CCOMMA, CCOMMA(tos); DROP) \
  XV(internals, "'heap", THEAP, DUP; tos = (cell_t) &g_sys->heap) \
  Y(current, DUP; tos = (cell_t) &g_sys->current) \
  XV(internals, "'context", TCONTEXT, DUP; tos = (cell_t) &g_sys->context) \
  XV(internals, "'latestxt", TLATESTXT, DUP; tos = (cell_t) &g_sys->latestxt) \
  XV(internals, "'notfound", TNOTFOUND, DUP; tos = (cell_t) &g_sys->notfound) \
  XV(internals, "'heap-start", THEAP_START, DUP; tos = (cell_t) &g_sys->heap_start) \
  XV(internals, "'heap-size", THEAP_SIZE, DUP; tos = (cell_t) &g_sys->heap_size) \
  XV(internals, "'stack-cells", TSTACK_CELLS, DUP; tos = (cell_t) &g_sys->stack_cells) \
  XV(internals, "'boot", TBOOT, DUP; tos = (cell_t) &g_sys->boot) \
  XV(internals, "'boot-size", TBOOT_SIZE, DUP; tos = (cell_t) &g_sys->boot_size) \
  XV(internals, "'tib", TTIB, DUP; tos = (cell_t) &g_sys->tib) \
  X("#tib", NTIB, DUP; tos = (cell_t) &g_sys->ntib) \
  X(">in", TIN, DUP; tos = (cell_t) &g_sys->tin) \
  Y(state, DUP; tos = (cell_t) &g_sys->state) \
  Y(base, DUP; tos = (cell_t) &g_sys->base) \
  XV(internals, "'argc", ARGC, DUP; tos = (cell_t) &g_sys->argc) \
  XV(internals, "'argv", ARGV, DUP; tos = (cell_t) &g_sys->argv) \
  XV(internals, "'runner", RUNNER, DUP; tos = (cell_t) &g_sys->runner) \
  XV(internals, "'throw-handler", TTHROW_HANDLER, DUP; tos = (cell_t) &g_sys->throw_handler) \
  Y(context, DUP; tos = (cell_t) (g_sys->context + 1)) \
  Y(latestxt, DUP; tos = (cell_t) g_sys->latestxt) \
  XV(forth_immediate, "[", LBRACKET, g_sys->state = 0) \
  XV(forth_immediate, "]", RBRACKET, g_sys->state = -1) \
  YV(forth_immediate, literal, COMMA(g_sys->DOLIT_XT); COMMA(tos); DROP)
