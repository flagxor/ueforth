#include <inttypes.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>

typedef intptr_t cell_t;
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

#define OPCODE_LIST \
  X("0=", OP_ZEQUAL, tos = !tos ? -1 : 0) \
  X("0<", OP_ZLESS, tos = tos < 0 ? -1 : 0) \
  X("+", OP_PLUS, tos += *sp--) \
  X("UM/MOD", OP_UMSMOD, ud = *(udcell_t *) &sp[-1]; \
                         *--sp = (cell_t) (ud % tos); \
                         tos = (cell_t) (ud / tos)) \
  X("*/MOD", OP_SSMOD, d = (dcell_t) *sp * (dcell_t) sp[-1]; \
                       *--sp = (cell_t) (d % tos); \
                       tos = (cell_t) (d / tos)) \
  X("AND", OP_AND, tos &= *sp--) \
  X("OR", OP_OR, tos |= *sp--) \
  X("XOR", OP_XOR, tos ^= *sp--) \
  X("DUP", OP_DUP, DUP) \
  X("SWAP", OP_SWAP, t = tos; tos = *sp; *sp = t) \
  X("OVER", OP_OVER, DUP; tos = sp[-1]) \
  X("DROP", OP_DROP, DROP) \
  X("@", OP_AT, tos = *(cell_t *) tos) \
  X("C@", OP_CAT, tos = *(uint8_t *) tos) \
  X("!", OP_STORE, *(cell_t *) tos = *sp; --sp; DROP) \
  X("C!", OP_CSTORE, *(uint8_t *) tos = *sp; --sp; DROP) \
  X("FILL", OP_FILL, memset((void *) sp[-1], tos, *sp); sp -= 2; DROP) \
  X("MOVE", OP_MOVE, memmove((void *) sp[-1], (void *) *sp, tos); sp -= 2; DROP) \
  X("SP@", OP_SPAT, DUP; tos = (cell_t) sp) \
  X("SP!", OP_SPSTORE, sp = (cell_t *) tos; DROP) \
  X("RP@", OP_RPAT, DUP; tos = (cell_t) rp) \
  X("RP!", OP_RPSTORE, rp = (cell_t *) tos; DROP) \
  X(">R", OP_TOR, *++rp = tos; DROP) \
  X("R>", OP_FROMR, DUP; tos = *rp--) \
  X("R@", OP_RAT, DUP; tos = *rp) \
  X("EXECUTE", OP_EXECUTE, w = tos; DROP; goto **(void **) w) \
  X("BRANCH", OP_BRANCH, ip = (cell_t *) *ip) \
  X("0BRANCH", OP_ZBRANCH, if (!tos) ip = (cell_t *) *ip; else ++ip; DROP) \
  X("DONEXT", OP_DONEXT, if ((*rp)--) ip = (cell_t *) *ip; else (--rp, ++ip)) \
  X("DOLIT", OP_DOLIT, DUP; tos = *(cell_t *) ip++) \
  X("ALITERAL", OP_ALITERAL, *g_sys.heap++ = g_sys.DOLIT_XT; \
                             *g_sys.heap++ = tos; DROP) \
  X("CELL", OP_CELL, DUP; tos = sizeof(cell_t)) \
  X("FIND", OP_FIND, tos = find((const char *) *sp, tos); --sp) \
  X("PARSE", OP_PARSE, DUP; tos = parse(tos, sp)) \
  X("S>NUMBER?", OP_CONVERT, tos = convert((const char *) *sp, tos, sp); \
                             if (!tos) --sp) \
  X("CREATE", OP_CREATE, t = parse(32, &tmp); \
                         create((const char *) tmp, t, 0, && OP_DOCREATE); \
                         *g_sys.heap++ = 0) \
  X("DOES>", OP_DOES, *g_sys.last = (cell_t) && OP_DODOES; \
                      g_sys.last[1] = (cell_t) ip; goto OP_EXIT) \
  X("IMMEDIATE", OP_IMMEDIATE, g_sys.last[-1] |= 1) \
  X("'SYS", OP_SYS, DUP; tos = (cell_t) &g_sys) \
  X(":", OP_COLON, t = parse(32, &tmp); \
                   create((const char *) tmp, t, 0, && OP_DOCOL); \
                   g_sys.state = -1) \
  X("EVAL1", OP_EVAL1, DUP; sp = eval1(sp, &tmp); \
            DROP; if (tmp) (w = tmp); \
            if (tmp) goto **(void **) w) \
  X("EXIT", OP_EXIT, ip = (void *) *rp--) \
  X(";", OP_SEMICOLON, *g_sys.heap++ = g_sys.DOEXIT_XT; g_sys.state = 0) \

