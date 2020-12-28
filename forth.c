#include <stdio.h>
#include <stdint.h>
#include <stdlib.h>
#include <string.h>

#define HEAP_SIZE (10 * 1024 * 1024)
#define STACK_SIZE (16 * 1024)

typedef intptr_t cell_t;
#if __SIZEOF_POINTER__ == 8
typedef __int128_t dcell_t;
#elif __SIZEOF_POINTER__ == 4
typedef int64_t dcell_t;
#else
# error "unsupported cell size"
#endif

#define DUP *++sp = tos
#define DROP tos = *sp--
#define NEXT w = *ip++; goto **(void **) w
#define CELL_LEN(n) (((n) + sizeof(cell_t) - 1) & ~(sizeof(cell_t) - 1))

#define OPCODE_LIST \
  X("0=", OP_ZEQUAL, tos = !tos) \
  X("0<", OP_ZLESS, tos = tos < 0) \
  X("+", OP_PLUS, tos += *sp--) \
  X("*/MOD", OP_SSMOD, d = (dcell_t) tos; \
                       m = (dcell_t) *sp; \
                       n = (dcell_t) sp[-1]; \
                       n *= m; \
                       tos = (cell_t) (n / d); \
                       *--sp = (cell_t) (n % d)) \
  X("AND", OP_AND, tos &= *sp--) \
  X("OR", OP_OR, tos |= *sp--) \
  X("XOR", OP_XOR, tos ^= *sp--) \
  X("DUP", OP_DUP, DUP) \
  X("SWAP", OP_SWAP, t = tos; tos = *sp; *sp = t) \
  X("OVER", OP_OVER, DUP; tos = sp[-1]) \
  X("DROP", OP_DROP, DROP) \
  X("SP@", OP_SPAT, DUP; tos = (cell_t) sp) \
  X("SP!", OP_SPSTORE, sp = (cell_t *) tos; DROP) \
  X("RP@", OP_RPAT, DUP; tos = (cell_t) rp) \
  X("RP!", OP_RPSTORE, rp = (cell_t *) tos; DROP) \
  X(">R", OP_TOR, *++rp = tos; DROP) \
  X("R>", OP_FROMR, DUP; tos = *rp--) \
  X("R@", OP_RAT, DUP; tos = *rp) \
  X("CELL", OP_CELL, DUP; tos = sizeof(cell_t)) \
  X("@", OP_AT, tos = *(cell_t *) tos) \
  X("C@", OP_CAT, tos = *(uint8_t *) tos) \
  X("!", OP_STORE, *(cell_t *) tos = *sp; --sp; DROP) \
  X("C!", OP_CSTORE, *(uint8_t *) tos = *sp; --sp; DROP) \
  X("BRANCH", OP_BRANCH, ip = (cell_t *) *ip) \
  X("0BRANCH", OP_ZBRANCH, if (!tos) ip = (cell_t *) *ip; else ++ip; DROP) \
  X("DOLIT", OP_DOLIT, DUP; tos = *(cell_t *) ip++) \
  X("FIND", OP_FIND, tos = find(*(cell_t *) *sp, tos, sp)) \
  X("PARSE", OP_PARSE, DUP; tos = parse(tos, sp)) \
  X("CREATE", OP_CREATE, t = parse(32, &tmp); \
                         create((const char *) tmp, t, 0, && OP_DOCREATE)) \
  X("IMMEDIATE", OP_IMMEDIATE, g_last[-1] |= 1) \
  X("DOES>", OP_DOES, *g_heap++ = (cell_t) && OP_DODOES /* TODO */) \
  X("HERE", OP_HERE, DUP; tos = (cell_t) g_heap) \
  X("ALLOT", OP_ALLOT, g_heap = (cell_t *) (tos + (cell_t) g_heap); tos = *sp--) \
  X("STATE", OP_STATE, DUP; tos = (cell_t) &g_state) \
  X("BASE", OP_BASE, DUP; tos = (cell_t) &g_base) \
  X("LAST", OP_LAST, DUP; tos = (cell_t) &g_last) \
  X("&TIB", OP_TIB, DUP; tos = (cell_t) &g_tib) \
  X("#TIB", OP_NTIB, DUP; tos = (cell_t) &g_ntib) \
  X(">IN", OP_TIN, DUP; tos = (cell_t) &g_tin) \
  X(":", OP_COLON, t = parse(32, &tmp); \
                   create((const char *) tmp, t, 0, && OP_DOCOL); \
                   g_state = -1) \
  X("EXIT", OP_EXIT, ip = (void *) *rp--) \
  X("QUIT", OP_QUIT, DUP; sp = quit(sp, &tmp); \
            DROP; --ip; if (tmp) (w = tmp); \
            if (tmp) goto **(void **) w) \
  X(";", OP_SEMICOLON, *g_heap++ = (cell_t) g_last; g_state = 0) \

static const char boot[] =
// Comments 
" : (   41 parse drop drop ; immediate "

// Useful Basic Compound Words
" : 2drop ( n n -- ) drop drop ; "
" : 2dup ( a b -- a b a b ) over over ; "
" : nip ( a b -- b ) swap drop ; "
" : rdrop ( r: n n -- ) r> r> drop >r ; "
" : */ ( n n n -- n ) */mod nip ; "
" : * ( n n -- n ) 1 */ ; "
" : /mod ( n n -- n n ) 1 swap */mod ; "
" : / ( n n -- n ) /mod nip ; "
" : mod ( n n -- n ) /mod drop ; "
" : invert ( n -- ~n ) -1 xor ; "
" : negate ( n -- -n ) invert 1 + ; "
" : - ( n n -- n ) negate + ; "
" : rot ( a b c -- c a b ) >r swap r> swap ; "
" : -rot ( a b c -- b c a ) swap >r swap r> ; "
" : cell+ ( n -- n ) cell + ; "
" : cells ( n -- n ) cell * ; "
" : < ( a b -- a<b ) - 0< ; "
" : > ( a b -- a>b ) swap - 0< ; "
" : = ( a b -- a!=b ) - 0= ; "
" : <> ( a b -- a!=b ) = 0= ; "
" : emit ( n -- ) >r rp@ 1 type rdrop ; "
" : bl 32 ;   : space bl emit ; "
" : nl 10 ;   : cr nl emit ; "

// Compilation State
" : [ 0 state ! ; immediate "
" : ] -1 state ! ; immediate "

// Quoting Words
" : ' parse-name find ; "
" : ['] ' aliteral ; immediate "
" : char parse-name drop c@ ; "
" : [char] char aliteral ; immediate "
" : literal aliteral ; immediate "

// Core Control Flow
" : begin here ; immediate "
" : again ['] branch , , ; immediate "
" : until ['] 0branch , , ; immediate "
" : ahead ['] branch , here 0 , ; immediate "
" : then here swap ! ; immediate "
" : if ['] 0branch , here 0 , ; immediate "
" : else ['] branch , here 0 , swap here swap ! ; immediate "

// Compound words requiring conditionals
" : min 2dup < if drop else nip then ; "
" : max 2dup < if nip else drop then ; "

// Postpone - done here so we have ['] and IF
" : >flags 2 cells - @ ; "
" : immediate? >flags 1 and 1 - 0= ; "
" : postpone ' dup immediate? if , else aliteral ['] , , then ; immediate "

// Counted Loops
" : do postpone swap postpone >r postpone >r here ; immediate "
" : i postpone r@ ; immediate "
" : unloop postpone rdrop postpone rdrop ; immediate "
" : +loop postpone r> postpone + postpone r> "
"         postpone 2dup postpone >r postpone >r "
"         postpone < postpone 0= postpone until "
"         postpone unloop ; immediate "
" : loop 1 aliteral postpone +loop ; immediate "

// Constants and Variables
" : constant create , does> @ ; "
" : variable create 0 , ; "

// Exceptions
" variable handler "
" : catch   sp@ >r handler @ >r rp@ handler ! execute r> handler ! r> drop 0 ; "
" : throw   handler @ rp! r> handler ! r> swap >r sp! drop r> ; "

// Examine Dictionary
" : >link ( xt -- a ) 1 cells - @ ;   : >flags 2 cells - ; "
" : >name ( xt -- a n ) dup 3 cells - @ swap over - 3 cells - swap ; "
" : >body ( xt -- a ) cell+ ; "
" : see. ( xt -- ) >name type space ; "
" : see-one ( xt -- xt+1 ) "
"    dup @ dup ['] dolit: = if drop cell+ dup @ . else see. then cell+ ; "
" : exit= ( xt -- ) ['] exit = ; "
" : see-loop   >body begin see-one dup @ exit= until ; "
" : see   cr ['] : see.  ' dup see.  see-loop drop  ['] ; see.  cr ; "
" : words last @ begin dup >name type space >link dup 0= until drop cr ; "
;

static cell_t *g_heap;
static const char *g_tib;
static cell_t g_ntib = sizeof(boot), g_tin = 0;
static cell_t *g_last = 0;
static cell_t g_base = 10;
static cell_t g_state = 0;
static cell_t g_DOLIT_XT;

static cell_t convert(const char *pos, cell_t n, cell_t *ret) {
  *ret = 0;
  cell_t negate = 0;
  if (!n) { return 0; }
  if (pos[0] == '-') { negate = -1; ++pos; --n; }
  for (; n; --n) {
    uintptr_t d = pos[0] - '0';
    if (d > 9) {
      d &= 95;
      d -= 7;
      if (d < 10) { return 0; }
    }
    if (d >= (uintptr_t) g_base) { return 0; }
    *ret = *ret * g_base + d;
    ++pos;
  }
  if (negate) { *ret = -*ret; }
  return -1;
}

static cell_t same(const char *a, const char *b, cell_t len) {
  for (;len && (*a & 95) == (*b & 95); --len, ++a, ++b);
  return len;
}

static cell_t find(cell_t name, cell_t len, cell_t *ret) {
  cell_t *pos = g_last;
  cell_t clen = CELL_LEN(len);
  while (pos) {
    if (len == pos[-3] &&
        same((const char *) name, (const char *) &pos[-3 - clen], len) == 0) {
      *ret = (cell_t) pos;
      return -1;
    }
    pos = (cell_t *) pos[-2];  // Follow link
  }
  return 0;
}

static void create(const char *name, cell_t length, cell_t flags, void *op) {
  memcpy(g_heap, name, length);  // name
  g_heap += CELL_LEN(length);
  *g_heap++ = length;  // length
  *g_heap++ = (cell_t) g_last;  // link
  *g_heap++ = flags;  // flags
  g_last = g_heap;
  *g_heap++ = (cell_t) op;  // code
}

static cell_t parse(cell_t sep, cell_t *ret) {
  while (g_tin < g_ntib && g_tib[g_tin] == sep) { ++g_tin; }
  *ret = (cell_t) (g_tib + g_tin);
  while (g_tin < g_ntib && g_tib[g_tin] != sep) { ++g_tin; }
  return g_tin - (*ret - (cell_t) g_tib);
}

static cell_t *quit(cell_t *sp, cell_t *call) {
  *call = 0;
  cell_t name;
  cell_t len = parse(' ', &name);
  cell_t xt;
  cell_t found = find(name, len, &xt);
  if (found) {
    if (g_state && !(((cell_t *) xt)[-1] & 1)) {  // bit 0 of flags is immediate
      *g_heap++ = xt;
    } else {
      *call = xt;
    }
  } else {
    cell_t n;
    cell_t ok = convert((const char *) name, len, &n);
    if (ok) {
      if (g_state) {
        *g_heap++ = g_DOLIT_XT;
        *g_heap++ = n;
      } else {
        *sp += n;
      }
    } else {
      fprintf(stderr, "Bad Word: ");
      fwrite((const char *) name, 1, len, stderr);
      fprintf(stderr, "\n");
      exit(1);
    } 
  }
  return sp;
}

int main(int argc, char *argv[]) {
  g_heap = malloc(HEAP_SIZE);
  register cell_t *sp = g_heap; g_heap += STACK_SIZE;
  register cell_t *rp = g_heap; g_heap += STACK_SIZE;
  register cell_t tos = 0, *ip, t, w;
  dcell_t m, n, d;
  cell_t tmp;
#define X(name, op, code) create(name, sizeof(name) - 1, name[0] == ';', && op);
  OPCODE_LIST
#undef X
  g_last[-1] = 1;  // Make ; IMMEDIATE
  find((cell_t) "DOLIT", 5, &g_DOLIT_XT);
  ip = g_heap;
  find((cell_t) "QUIT", 4, g_heap++);
  g_tib = boot;
fprintf(stderr, "right: %p\n", && OP_COLON);
  NEXT;
#define X(name, op, code) op: fprintf(stderr, name "\n"); code; NEXT;
  OPCODE_LIST
#undef X
  OP_DOCREATE: DUP; tos = w + sizeof(cell_t) * 2; NEXT;
  OP_DODOES: DUP; tos = w + sizeof(cell_t) * 2;
             *++rp = (cell_t) ip; ip = (cell_t *) (w + sizeof(cell_t)); NEXT;
  OP_DOCOL: *++rp = (cell_t) ip; ip = (cell_t *) (w + sizeof(cell_t)); NEXT;
}
