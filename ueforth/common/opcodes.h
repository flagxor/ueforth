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

#define PUSH DUP; tos =
#define DUP *++sp = tos
#define DROP tos = *sp--
#define COMMA(n) *g_sys.heap++ = (n)
#define IMMEDIATE() g_sys.last[-1] |= 1
#define DOES(ip) *g_sys.last = (cell_t) && OP_DODOES; g_sys.last[1] = (cell_t) ip
#define UMSMOD ud = *(udcell_t *) &sp[-1]; \
               --sp; *sp = (cell_t) (ud % tos); \
               tos = (cell_t) (ud / tos)
#define SSMOD d = (dcell_t) *sp * (dcell_t) sp[-1]; \
              --sp; *sp = (cell_t) (((udcell_t) d) % tos); \
              tos = (cell_t) (d < 0 ? ~(~d / tos) : d / tos)

#define OPCODE_LIST \
  X("0=", ZEQUAL, tos = !tos ? -1 : 0) \
  X("0<", ZLESS, tos = (tos|0) < 0 ? -1 : 0) \
  X("+", PLUS, tos = (tos + *sp) | 0; --sp) \
  X("UM/MOD", UMSMOD, UMSMOD) \
  X("*/MOD", SSMOD, SSMOD) \
  X("AND", AND, tos = tos & *sp; --sp) \
  X("OR", OR, tos = tos | *sp; --sp) \
  X("XOR", XOR, tos = tos ^ *sp; --sp) \
  X("DUP", DUP, DUP) \
  X("SWAP", SWAP, w = tos; tos = (*sp)|0; *sp = w) \
  X("OVER", OVER, DUP; tos = sp[-1] | 0) \
  X("DROP", DROP, DROP) \
  X("@", AT, tos = (*(cell_t *) tos)|0) \
  X("L@", LAT, tos = (*(int32_t *) tos)|0) \
  X("C@", CAT, tos = (*(uint8_t *) tos)|0) \
  X("!", STORE, *(cell_t *) tos = (*sp)|0; --sp; DROP) \
  X("L!", LSTORE, *(int32_t *) tos = (*sp)|0; --sp; DROP) \
  X("C!", CSTORE, *(uint8_t *) tos = (*sp)|0; --sp; DROP) \
  X("FILL", FILL, memset((void *) (sp[-1] | 0), tos | 0, (*sp | 0)); sp -= 2; DROP) \
  X("MOVE", MOVE, memmove((void *) (sp[-1] | 0), (void *) (*sp | 0), tos | 0); sp -= 2; DROP) \
  X("SP@", SPAT, DUP; tos = (cell_t) sp) \
  X("SP!", SPSTORE, sp = (cell_t *) tos; DROP) \
  X("RP@", RPAT, DUP; tos = (cell_t) rp) \
  X("RP!", RPSTORE, rp = (cell_t *) tos; DROP) \
  X(">R", TOR, ++rp; *rp = tos; DROP) \
  X("R>", FROMR, DUP; tos = (*rp)|0; --rp) \
  X("R@", RAT, DUP; tos = (*rp)|0) \
  X("EXECUTE", EXECUTE, w = tos; DROP; goto **(void **) w) \
  X("BRANCH", BRANCH, ip = (cell_t *) (*ip | 0)) \
  X("0BRANCH", ZBRANCH, if (!tos) ip = (cell_t *) (*ip | 0); else ++ip; DROP) \
  X("DONEXT", DONEXT, *rp = ((*rp|0) - 1) | 0; \
                      if ((*rp|0)) ip = (cell_t *) (*ip | 0); else (--rp, ++ip)) \
  X("DOLIT", DOLIT, DUP; tos = (*ip | 0); ++ip) \
  X("ALITERAL", ALITERAL, COMMA(g_sys.DOLIT_XT | 0); COMMA(tos | 0); DROP) \
  X("CELL", CELL, DUP; tos = sizeof(cell_t)) \
  X("FIND", FIND, tos = find((const char *) (*sp | 0), tos|0)|0; --sp) \
  X("PARSE", PARSE, DUP; tos = parse(tos|0, (cell_t *) ((cell_t) sp | 0))|0) \
  X("S>NUMBER?", CONVERT, \
      tos = convert((const char *) (*sp | 0), tos|0, (cell_t *) ((cell_t) sp | 0))|0; \
      if (!tos) --sp) \
  X("CREATE", CREATE, DUP; DUP; tos = parse(32, (cell_t *) ((cell_t) sp | 0))|0; \
                      create((const char *) (*sp | 0), tos|0, 0, && OP_DOCREATE); \
                         COMMA(0); --sp; DROP) \
  X("DOES>", DOES, DOES((cell_t *) ((cell_t) ip|0)); ip = (cell_t *) (*rp | 0); --rp) \
  X("IMMEDIATE", IMMEDIATE, IMMEDIATE()) \
  X("'SYS", SYS, DUP; tos = (cell_t) &g_sys) \
  X(":", COLON, DUP; DUP; tos = parse(32, (cell_t *) ((cell_t) sp | 0))|0; \
                create((const char *) (*sp | 0), tos|0, 0, && OP_DOCOLON); \
                   g_sys.state = -1; --sp; DROP) \
  X("EVALUATE1", EVALUATE1, \
      DUP; sp = (cell_t *) ((cell_t) evaluate1((cell_t *) ((cell_t) sp | 0))|0); \
      w = (*sp | 0); --sp; DROP; \
      if (w) goto **(void **) w) \
  X("EXIT", EXIT, ip = (cell_t *) (*rp | 0); --rp) \
  X(";", SEMICOLON, COMMA(g_sys.DOEXIT_XT | 0); g_sys.state = 0) \

