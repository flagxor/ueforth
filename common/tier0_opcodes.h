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

#include <inttypes.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>

typedef intptr_t cell_t;
typedef uintptr_t ucell_t;

#define XV(flags, name, op, code) Z(flags, name, op, code)
#define YV(flags, op, code) Z(flags, #op, op, code)
#define X(name, op, code) Z(forth, name, op, code)
#define Y(op, code) Z(forth, #op, op, code)

#define NIP (--sp)
#define NIPn(n) (sp -= (n))
#define DROP (tos = *sp--)
#define DROPn(n) (NIPn(n-1), DROP)
#define DUP (*++sp = tos)
#define PUSH DUP; tos = (cell_t)

#define PARK   DUP; *++rp = (cell_t) fp; *++rp = (cell_t) sp; *++rp = (cell_t) ip
#define UNPARK ip = (cell_t *) *rp--; sp = (cell_t *) *rp--; fp = (float *) *rp--; DROP

#define TOFLAGS(xt) ((uint8_t *) (((cell_t *) (xt)) - 1))
#define TONAMELEN(xt) (TOFLAGS(xt) + 1)
#define TOPARAMS(xt) ((uint16_t *) (TOFLAGS(xt) + 2))
#define TOSIZE(xt) (CELL_ALIGNED(*TONAMELEN(xt)) + sizeof(cell_t) * (3 + *TOPARAMS(xt)))
#define TOLINK(xt) (((cell_t *) (xt)) - 2)
#define TONAME(xt) ((*TOFLAGS(xt) & BUILTIN_MARK) ? (*(char **) TOLINK(xt)) \
    : (((char *) TOLINK(xt)) - CELL_ALIGNED(*TONAMELEN(xt))))
#define TOBODY(xt) (((cell_t *) xt) + ((void *) *((cell_t *) xt) == ADDROF(DOCREATE) || \
                                       (void *) *((cell_t *) xt) == ADDROF(DODOES) ? 2 : 1))

#ifndef COMMA
# define COMMA(n) *g_sys->heap++ = (cell_t) (n)
# define CCOMMA(n) *(uint8_t *) g_sys->heap = (n); \
                   g_sys->heap = (cell_t *) (1 + ((cell_t) g_sys->heap));
# define DOES(ip) **g_sys->current = (cell_t) ADDROF(DODOES); (*g_sys->current)[1] = (cell_t) ip
# define DOIMMEDIATE() *TOFLAGS(*g_sys->current) |= IMMEDIATE
# define UNSMUDGE() *TOFLAGS(*g_sys->current) &= ~SMUDGE; finish()
#endif

#ifndef SSMOD_FUNC
# if __SIZEOF_POINTER__ == 8
typedef __int128_t dcell_t;
# elif __SIZEOF_POINTER__ == 4 || defined(_M_IX86)
typedef int64_t dcell_t;
# else
#  error "unsupported cell size"
# endif
# define SSMOD_FUNC dcell_t d = (dcell_t) *sp * (dcell_t) sp[-1]; \
                    --sp; cell_t a = (cell_t) (d < 0 ? ~(~d / tos) : d / tos); \
                    *sp = (cell_t) (d - ((dcell_t) a) * tos); tos = a
#endif

typedef struct {
  const char *name;
  union {
    struct {
      uint8_t flags, name_length;
      uint16_t vocabulary;
    };
    cell_t multi;  // Forces cell alignment throughout.
  };
  const void *code;
} BUILTIN_WORD;

#define TIER0_OPCODE_LIST \
  YV(internals, NOP, ) \
  X("0=", ZEQUAL, tos = !tos ? -1 : 0) \
  X("0<", ZLESS, tos = (tos|0) < 0 ? -1 : 0) \
  X("+", PLUS, tos += *sp--) \
  X("U/MOD", USMOD, w = *sp; *sp = (ucell_t) w % (ucell_t) tos; \
                    tos = (ucell_t) w / (ucell_t) tos) \
  X("*/MOD", SSMOD, SSMOD_FUNC) \
  Y(LSHIFT, tos = (*sp << tos); --sp) \
  Y(RSHIFT, tos = (((ucell_t) *sp) >> tos); --sp) \
  Y(ARSHIFT, tos = (*sp >> tos); --sp) \
  Y(AND, tos &= *sp--) \
  Y(OR, tos |= *sp--) \
  Y(XOR, tos ^= *sp--) \
  X("DUP", ALTDUP, DUP) \
  Y(SWAP, w = tos; tos = *sp; *sp = w) \
  Y(OVER, DUP; tos = sp[-1]) \
  X("DROP", ALTDROP, DROP) \
  X("@", AT, tos = *(cell_t *) tos) \
  X("SL@", SLAT, tos = *(int32_t *) tos) \
  X("UL@", ULAT, tos = *(uint32_t *) tos) \
  X("SW@", SWAT, tos = *(int16_t *) tos) \
  X("UW@", UWAT, tos = *(uint16_t *) tos) \
  X("C@", CAT, tos = *(uint8_t *) tos) \
  X("!", STORE, *(cell_t *) tos = *sp--; DROP) \
  X("L!", LSTORE, *(int32_t *) tos = *sp--; DROP) \
  X("W!", WSTORE, *(int16_t *) tos = *sp--; DROP) \
  X("C!", CSTORE, *(uint8_t *) tos = *sp--; DROP) \
  X("SP@", SPAT, DUP; tos = (cell_t) sp) \
  X("SP!", SPSTORE, sp = (cell_t *) tos; DROP) \
  X("RP@", RPAT, DUP; tos = (cell_t) rp) \
  X("RP!", RPSTORE, rp = (cell_t *) tos; DROP) \
  X(">R", TOR, *++rp = tos; DROP) \
  X("R>", FROMR, DUP; tos = *rp; --rp) \
  X("R@", RAT, DUP; tos = *rp) \
  Y(EXECUTE, w = tos; DROP; JMPW) \
  YV(internals, BRANCH, ip = (cell_t *) *ip) \
  YV(internals, 0BRANCH, if (!tos) ip = (cell_t *) *ip; else ++ip; DROP) \
  YV(internals, DONEXT, *rp = *rp - 1; if (~*rp) ip = (cell_t *) *ip; else (--rp, ++ip)) \
  YV(internals, DOLIT, DUP; tos = *ip++) \
  YV(internals, DOSET, *((cell_t *) *ip) = tos; ++ip; DROP) \
  YV(internals, DOCOL, ++rp; *rp = (cell_t) ip; ip = (cell_t *) (w + sizeof(cell_t))) \
  YV(internals, DOCON, DUP; tos = *(cell_t *) (w + sizeof(cell_t))) \
  YV(internals, DOVAR, DUP; tos = w + sizeof(cell_t)) \
  YV(internals, DOCREATE, DUP; tos = w + sizeof(cell_t) * 2) \
  YV(internals, DODOES, DUP; tos = w + sizeof(cell_t) * 2; \
                        ++rp; *rp = (cell_t) ip; \
                        ip = (cell_t *) *(cell_t *) (w + sizeof(cell_t))) \
  YV(internals, ALITERAL, COMMA(g_sys->DOLIT_XT); COMMA(tos); DROP) \
  Y(CELL, DUP; tos = sizeof(cell_t)) \
  XV(internals, "LONG-SIZE", LONG_SIZE, DUP; tos = sizeof(long)) \
  Y(FIND, tos = find((const char *) *sp, tos); --sp) \
  Y(PARSE, DUP; tos = parse(tos, sp)) \
  XV(internals, "S>NUMBER?", \
      CONVERT, tos = convert((const char *) *sp, tos, g_sys->base, sp); \
      if (!tos) --sp) \
  Y(CREATE, DUP; DUP; tos = parse(32, sp); \
            create((const char *) *sp, tos, 0, ADDROF(DOCREATE)); \
            COMMA(0); DROPn(2)) \
  Y(VARIABLE, DUP; DUP; tos = parse(32, sp); \
              create((const char *) *sp, tos, 0, ADDROF(DOVAR)); \
              COMMA(0); DROPn(2)) \
  Y(CONSTANT, DUP; DUP; tos = parse(32, sp); \
              create((const char *) *sp, tos, 0, ADDROF(DOCON)); \
              DROPn(2); COMMA(tos); DROP) \
  X("DOES>", DOES, DOES(ip); ip = (cell_t *) *rp; --rp) \
  Y(IMMEDIATE, DOIMMEDIATE()) \
  X(">BODY", TOBODY, tos = (cell_t) TOBODY(tos)) \
  XV(internals, "'SYS", SYS, DUP; tos = (cell_t) g_sys) \
  YV(internals, YIELD, PARK; return rp) \
  X(":", COLON, DUP; DUP; tos = parse(32, sp); \
                create((const char *) *sp, tos, SMUDGE, ADDROF(DOCOL)); \
                g_sys->state = -1; --sp; DROP) \
  YV(internals, EVALUATE1, PARK; rp = evaluate1(rp); UNPARK; w = tos; DROP; if (w) JMPW) \
  Y(EXIT, ip = (cell_t *) *rp--) \
  XV(internals, "'builtins", TBUILTINS, DUP; tos = (cell_t) &g_sys->builtins->code) \
  XV(forth_immediate, ";", SEMICOLON, COMMA(g_sys->DOEXIT_XT); UNSMUDGE(); g_sys->state = 0)
