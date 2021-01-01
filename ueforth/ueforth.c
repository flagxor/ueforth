#include <dlfcn.h>
#include <inttypes.h>
#include <stdint.h>
#include <stdio.h>
#include <stdlib.h>
#include <string.h>

#define HEAP_SIZE (10 * 1024 * 1024)
#define STACK_SIZE (16 * 1024)

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
#define NEXT w = *ip++; goto **(void **) w
#define CELL_LEN(n) (((n) + sizeof(cell_t) - 1) / sizeof(cell_t))
#define FIND(name) find(name, sizeof(name) - 1)
#define LOWER(ch) ((ch) & 95)

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
  X("ALITERAL", OP_ALITERAL, *g_heap++ = g_DOLIT_XT; *g_heap++ = tos; DROP) \
  X("CELL", OP_CELL, DUP; tos = sizeof(cell_t)) \
  X("TYPE", OP_TYPE, fwrite((void *) *sp, 1, tos, stdout); --sp; DROP) \
  X("KEY", OP_KEY, DUP; tos = fgetc(stdin)) \
  X("SYSEXIT", OP_SYSEXIT, DUP; exit(tos)) \
  X("FIND", OP_FIND, tos = find((const char *) *sp, tos); --sp) \
  X("PARSE", OP_PARSE, DUP; tos = parse(tos, sp)) \
  X("S>NUMBER?", OP_CONVERT, tos = convert((const char *) *sp, tos, sp); \
                             if (!tos) --sp) \
  X("CREATE", OP_CREATE, t = parse(32, &tmp); \
                         create((const char *) tmp, t, 0, && OP_DOCREATE); \
                         *g_heap++ = 0) \
  X("DOES>", OP_DOES, *g_last = (cell_t) && OP_DODOES; \
                      g_last[1] = (cell_t) ip; goto OP_EXIT) \
  X("IMMEDIATE", OP_IMMEDIATE, g_last[-1] |= 1) \
  X("'HEAP", OP_HEAP, DUP; tos = (cell_t) &g_heap) \
  X("STATE", OP_STATE, DUP; tos = (cell_t) &g_state) \
  X("BASE", OP_BASE, DUP; tos = (cell_t) &g_base) \
  X("LAST", OP_LAST, DUP; tos = (cell_t) &g_last) \
  X("'TIB", OP_TIB, DUP; tos = (cell_t) &g_tib) \
  X("#TIB", OP_NTIB, DUP; tos = (cell_t) &g_ntib) \
  X(">IN", OP_TIN, DUP; tos = (cell_t) &g_tin) \
  X("'THROW", OP_TTHROW, DUP; tos = (cell_t) &g_throw) \
  X("DLSYM", OP_DLSYM, tos = (cell_t) dlsym((void *) *sp, (void *) tos); --sp) \
  X("CALL0", OP_CALL0, tos = ((cell_t (*)()) tos)()) \
  X("CALL1", OP_CALL1, tos = ((cell_t (*)()) tos)(*sp); --sp) \
  X("CALL2", OP_CALL2, tos = ((cell_t (*)()) tos)(sp[-1], *sp); sp -= 2) \
  X("CALL3", OP_CALL3, tos = ((cell_t (*)()) tos)(sp[-2], sp[-1], *sp); sp -= 3) \
  X("CALL4", OP_CALL4, tos = ((cell_t (*)()) tos)(sp[-3], sp[-2], sp[-1], *sp); sp -= 4) \
  X("CALL5", OP_CALL5, tos = ((cell_t (*)()) tos)(sp[-4], sp[-3], sp[-2], sp[-1], *sp); sp -= 5) \
  X("CALL6", OP_CALL6, tos = ((cell_t (*)()) tos)(sp[-5], sp[-4], sp[-3], sp[-2], sp[-1], *sp); sp -= 6) \
  X(":", OP_COLON, t = parse(32, &tmp); \
                   create((const char *) tmp, t, 0, && OP_DOCOL); \
                   g_state = -1) \
  X("EVAL1", OP_EVAL1, DUP; sp = eval1(sp, &tmp); \
            DROP; if (tmp) (w = tmp); \
            if (tmp) goto **(void **) w) \
  X("EXIT", OP_EXIT, ip = (void *) *rp--) \
  X(";", OP_SEMICOLON, *g_heap++ = g_DOEXIT_XT; g_state = 0) \

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
" : < ( a b -- a<b ) - 0< ; "
" : > ( a b -- a>b ) swap - 0< ; "
" : = ( a b -- a!=b ) - 0= ; "
" : <> ( a b -- a!=b ) = 0= ; "
" : emit ( n -- ) >r rp@ 1 type rdrop ; "
" : bl 32 ;   : space bl emit ; "
" : nl 10 ;   : cr nl emit ; "
" : 1+ 1 + ;   : 1- 1 - ; "
" : 2* 2 * ;   : 2/ 2 / ; "
" : +! ( n a -- ) swap over @ + swap ! ; "
" : bye   0 sysexit ; "

// Dictionary and Cells
" : here ( -- a ) 'heap @ ; "
" : allot ( n -- ) 'heap +! ; "
" : cell+ ( n -- n ) cell + ; "
" : cells ( n -- n ) cell * ; "
" : cell/ ( n -- n ) cell / ; "
" : aligned ( a -- a ) cell 1 - dup >r + r> invert and ; "
" : align   here aligned here - allot ; "
" : , ( n --  ) here ! cell allot ; "
" : c, ( ch -- ) here c! 1 allot ; "

// Compilation State
" : [ 0 state ! ; immediate "
" : ] -1 state ! ; immediate "

// Quoting Words
" : ' bl parse find ; "
" : ['] ' aliteral ; immediate "
" : char bl parse drop c@ ; "
" : [char] char aliteral ; immediate "
" : literal aliteral ; immediate "

// Core Control Flow
" : begin   here ; immediate "
" : again   ['] branch , , ; immediate "
" : until   ['] 0branch , , ; immediate "
" : ahead   ['] branch , here 0 , ; immediate "
" : then   here swap ! ; immediate "
" : if   ['] 0branch , here 0 , ; immediate "
" : else   ['] branch , here 0 , swap here swap ! ; immediate "
" : while   ['] 0branch , here 0 , swap ; immediate "
" : repeat   ['] branch , , here swap ! ; immediate "
" : aft   drop ['] branch , here 0 , here swap ; immediate "

// Compound words requiring conditionals
" : min 2dup < if drop else nip then ; "
" : max 2dup < if nip else drop then ; "
" : abs ( n -- +n ) dup 0< if negate then ; "

// Postpone - done here so we have ['] and IF
" : >flags ( xt -- flags ) cell - ; "
" : immediate? ( xt -- f ) >flags @ 1 and 0= 0= ; "
" : postpone ' dup immediate? if , else aliteral ['] , , then ; immediate "

// Counted Loops
" : for   postpone >r postpone begin ; immediate "
" : next   postpone donext , ; immediate "
" : do   postpone swap postpone >r postpone >r here ; immediate "
" : i   postpone r@ ; immediate "
" : j   rp@ 3 cells - @ ; "
" : unloop   postpone rdrop postpone rdrop ; immediate "
" : +loop   postpone r> postpone + postpone r> "
"           postpone 2dup postpone >r postpone >r "
"           postpone < postpone 0= postpone until "
"           postpone unloop ; immediate "
" : loop   1 aliteral postpone +loop ; immediate "

// Constants and Variables
" : constant create , does> @ ; "
" : variable create 0 , ; "

// Stack Convience
" sp@ constant sp0 "
" rp@ constant rp0 "
" : depth ( -- n ) sp@ sp0 - cell/ ; "

// Exceptions
" variable handler "
" : catch   sp@ >r handler @ >r rp@ handler ! execute r> handler ! r> drop 0 ; "
" : throw   handler @ rp! r> handler ! r> swap >r sp! drop r> ; "
" ' throw 'throw ! "

// Numeric Output
" variable hld "
" : pad ( -- a ) here 80 + ; "
" : digit ( u -- c ) 9 over < 7 and + 48 + ; "
" : extract ( n base -- n c ) 0 swap um/mod swap digit ; "
" : <# ( -- ) pad hld ! ; "
" : hold ( c -- ) hld @ 1 - dup hld ! c! ; "
" : # ( u -- u ) base @ extract hold ; "
" : #s ( u -- 0 ) begin # dup while repeat ; "
" : sign ( n -- ) 0< if 45 hold then ; "
" : #> ( w -- b u ) drop hld @ pad over - ; "
" : str ( n -- b u ) dup >r abs <# #s r> sign #> ; "
" : hex ( -- ) 16 base ! ; "
" : decimal ( -- ) 10 base ! ; "
" : u. ( u -- ) <# #s #> space type ; "
" : . ( w -- ) base @ 10 xor if u. exit then str space type ; "
" : ? ( a -- ) @ . ; "

// ( Strings )
" : $.   r@ dup cell+ swap @ type r> dup @ aligned + cell+ >r ; "
" : .\"   [char] \" parse postpone $. dup , 0 do dup c@ c, 1+ loop drop align ; immediate "
" : $@   r@ dup cell+ swap @ r> dup @ aligned + cell+ >r ; "
" : s\"   [char] \" parse postpone $@ dup , 0 do dup c@ c, 1+ loop drop align ; immediate "
" : z$@   r@ cell+ r> dup @ aligned + cell+ >r ; "
" : z\"   [char] \" parse postpone z$@ dup 1+ , 0 do dup c@ c, 1+ loop drop 0 c, align ; immediate "

// Examine Dictionary
" : >name ( xt -- a n ) 3 cells - dup @ swap over aligned - swap ; "
" : >link ( xt -- a ) 2 cells - @ ; "
" : >body ( xt -- a ) cell+ ; "
" : see. ( xt -- ) >name type space ; "
" : see-one ( xt -- xt+1 ) "
"    dup @ dup ['] DOLIT = if drop cell+ dup @ . else see. then cell+ ; "
" : exit= ( xt -- ) ['] exit = ; "
" : see-loop   >body begin see-one dup @ exit= until ; "
" : see   cr ['] : see.  ' dup see.  see-loop drop  ['] ; see.  cr ; "
" : words   last @ begin dup see. >link dup 0= until drop cr ; "

// ( Input )
" : accept ( a n -- n ) 0 swap begin 2dup < while "
"    key dup nl = if 2drop nip exit then "
"    >r rot r> over c! 1+ -rot swap 1+ swap repeat drop nip ; "
" 200 constant input-limit "
" : tib ( -- a ) 'tib @ ; "
" create input-buffer   input-limit allot "
" : tib-setup   input-buffer 'tib ! ; "
" : refill   tib-setup tib input-limit accept #tib ! 0 >in ! -1 ; "

// ( REPL )
" : prompt   .\"  ok\" cr ; "
" : eval-line   begin >in @ #tib @ < while eval1 repeat ; "
" : query   begin ['] eval-line catch if .\" ERROR\" cr then prompt refill drop again ; "
" : ok   .\" uEForth\" cr prompt refill drop query ; "
" ok "
;

static cell_t *g_heap;
static const char *g_tib;
static cell_t g_ntib = sizeof(boot), g_tin = 0;
static cell_t *g_last = 0, g_base = 10, g_state = 0, g_throw = 0;
static cell_t g_DOLIT_XT, g_DOEXIT_XT;

static cell_t convert(const char *pos, cell_t n, cell_t *ret) {
  *ret = 0;
  cell_t negate = 0;
  if (!n) { return 0; }
  if (pos[0] == '-') { negate = -1; ++pos; --n; }
  for (; n; --n) {
    uintptr_t d = pos[0] - '0';
    if (d > 9) {
      d = LOWER(d) - 7;
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
  for (;len && LOWER(*a) == LOWER(*b); --len, ++a, ++b);
  return len;
}

static cell_t find(const char *name, cell_t len) {
  cell_t *pos = g_last;
  cell_t clen = CELL_LEN(len);
  while (pos) {
    if (len == pos[-3] &&
        same(name, (const char *) &pos[-3 - clen], len) == 0) {
      return (cell_t) pos;
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
  cell_t len = g_tin - (*ret - (cell_t) g_tib);
  if (g_tin < g_ntib) { ++g_tin; }
  return len;
}

static cell_t *eval1(cell_t *sp, cell_t *call) {
  *call = 0;
  cell_t name;
  cell_t len = parse(' ', &name);
  cell_t xt = find((const char *) name, len);
  if (xt) {
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
        *++sp = n;
      }
    } else {
      *++sp = -1;
      *call = g_throw;
    }
  }
  return sp;
}

int main(int argc, char *argv[]) {
  g_heap = malloc(HEAP_SIZE);
  register cell_t *sp = g_heap; g_heap += STACK_SIZE;
  register cell_t *rp = g_heap; g_heap += STACK_SIZE;
  register cell_t tos = 0, *ip, t, w;
  dcell_t d;
  udcell_t ud;
  cell_t tmp;
#define X(name, op, code) create(name, sizeof(name) - 1, name[0] == ';', && op);
  OPCODE_LIST
#undef X
  g_last[-1] = 1;  // Make ; IMMEDIATE
  g_DOLIT_XT = FIND("DOLIT");
  g_DOEXIT_XT = FIND("EXIT");
  ip = g_heap;
  *g_heap++ = FIND("EVAL1");
  *g_heap++ = FIND("BRANCH");
  *g_heap++ = (cell_t) ip;
  g_tib = boot;
  NEXT;
#define X(name, op, code) op: code; NEXT;
  OPCODE_LIST
#undef X
  OP_DOCREATE: DUP; tos = w + sizeof(cell_t) * 2; NEXT;
  OP_DODOES: DUP; tos = w + sizeof(cell_t) * 2;
             *++rp = (cell_t) ip; ip = (cell_t *) *(cell_t *) (w + sizeof(cell_t)); NEXT;
  OP_DOCOL: *++rp = (cell_t) ip; ip = (cell_t *) (w + sizeof(cell_t)); NEXT;
}
