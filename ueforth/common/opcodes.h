#include <inttypes.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>

typedef intptr_t cell_t;
typedef uintptr_t ucell_t;
#if __SIZEOF_POINTER__ == 8
typedef __int128_t dcell_t;
typedef __uint128_t udcell_t;
#elif __SIZEOF_POINTER__ == 4
typedef int64_t dcell_t;
typedef uint64_t udcell_t;
#else
# error "unsupported cell size"
#endif

#define DUP *++sp = tos
#define DROP tos = *sp--
#define COMMA(n) *g_sys.heap++ = (n)
#define IMMEDIATE() g_sys.last[-1] |= 1
#define DOES(ip) *g_sys.last = (cell_t) && OP_DODOES; g_sys.last[1] = (cell_t) ip
#ifndef SSMOD_FUNC
#define SSMOD_FUNC dcell_t d = (dcell_t) *sp * (dcell_t) sp[-1]; \
                   --sp; *sp = (cell_t) (((udcell_t) d) % tos); \
                   tos = (cell_t) (d < 0 ? ~(~d / tos) : d / tos)
#endif
#define PARK DUP; g_sys.ip = ip; g_sys.rp = rp; g_sys.sp = sp

#define OPCODE_LIST \
  X("0=", ZEQUAL, tos = !tos ? -1 : 0) \
  X("0<", ZLESS, tos = (tos|0) < 0 ? -1 : 0) \
  X("+", PLUS, tos += *sp--) \
  X("U/MOD", USMOD, w = *sp; *sp = (ucell_t) w % (ucell_t) tos; \
                    tos = (ucell_t) w / (ucell_t) tos) \
  X("*/MOD", SSMOD, SSMOD_FUNC) \
  X("AND", AND, tos &= *sp--) \
  X("OR", OR, tos |= *sp--) \
  X("XOR", XOR, tos ^= *sp--) \
  X("DUP", DUP, DUP) \
  X("SWAP", SWAP, w = tos; tos = *sp; *sp = w) \
  X("OVER", OVER, DUP; tos = sp[-1]) \
  X("DROP", DROP, DROP) \
  X("@", AT, tos = *(cell_t *) tos) \
  X("L@", LAT, tos = *(int32_t *) tos) \
  X("C@", CAT, tos = *(uint8_t *) tos) \
  X("!", STORE, *(cell_t *) tos = *sp--; DROP) \
  X("L!", LSTORE, *(int32_t *) tos = *sp--; DROP) \
  X("C!", CSTORE, *(uint8_t *) tos = *sp--; DROP) \
  X("SP@", SPAT, DUP; tos = (cell_t) sp) \
  X("SP!", SPSTORE, sp = (cell_t *) tos; DROP) \
  X("RP@", RPAT, DUP; tos = (cell_t) rp) \
  X("RP!", RPSTORE, rp = (cell_t *) tos; DROP) \
  X(">R", TOR, *++rp = tos; DROP) \
  X("R>", FROMR, DUP; tos = *rp; --rp) \
  X("R@", RAT, DUP; tos = *rp) \
  X("EXECUTE", EXECUTE, w = tos; DROP; goto **(void **) w) \
  X("BRANCH", BRANCH, ip = (cell_t *) *ip) \
  X("0BRANCH", ZBRANCH, if (!tos) ip = (cell_t *) *ip; else ++ip; DROP) \
  X("DONEXT", DONEXT, *rp = *rp - 1; \
                      if (~*rp) ip = (cell_t *) *ip; else (--rp, ++ip)) \
  X("DOLIT", DOLIT, DUP; tos = *ip++) \
  X("ALITERAL", ALITERAL, COMMA(g_sys.DOLIT_XT); COMMA(tos); DROP) \
  X("CELL", CELL, DUP; tos = sizeof(cell_t)) \
  X("FIND", FIND, tos = find((const char *) *sp, tos); --sp) \
  X("PARSE", PARSE, DUP; tos = parse(tos, sp)) \
  X("S>NUMBER?", CONVERT, tos = convert((const char *) *sp, tos, sp); \
                          if (!tos) --sp) \
  X("CREATE", CREATE, DUP; DUP; tos = parse(32, sp); \
                      create((const char *) *sp, tos, 0, && OP_DOCREATE); \
                      COMMA(0); --sp; DROP) \
  X("DOES>", DOES, DOES(ip); ip = (cell_t *) *rp; --rp) \
  X("IMMEDIATE", IMMEDIATE, IMMEDIATE()) \
  X("'SYS", SYS, DUP; tos = (cell_t) &g_sys) \
  X("YIELD", YIELD, PARK; return) \
  X(":", COLON, DUP; DUP; tos = parse(32, sp); \
                create((const char *) *sp, tos, 0, && OP_DOCOLON); \
                g_sys.state = -1; --sp; DROP) \
  X("EVALUATE1", EVALUATE1, DUP; sp = evaluate1(sp); w = *sp--; DROP; \
                            if (w) goto **(void **) w) \
  X("EXIT", EXIT, ip = (cell_t *) *rp--) \
  X(";", SEMICOLON, COMMA(g_sys.DOEXIT_XT); g_sys.state = 0) \

