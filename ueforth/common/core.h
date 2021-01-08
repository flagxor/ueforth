#define PRINT_ERRORS 0

#define NEXT w = *ip++; goto **(void **) w
#define CELL_LEN(n) (((n) + sizeof(cell_t) - 1) / sizeof(cell_t))
#define FIND(name) find(name, sizeof(name) - 1)
#define LOWER(ch) ((ch) & 0x5F)

#if PRINT_ERRORS
#include <unistd.h>
#endif

static struct {
  const char *tib;
  cell_t ntib, tin, state, base;
  cell_t *heap, *last, notfound;
  int argc;
  char **argv;
  cell_t DOLIT_XT, DOEXIT_XT;
  cell_t *ip, *sp, *rp;  // Parked alternates
} g_sys;

static cell_t convert(const char *pos, cell_t n, cell_t *ret) {
  *ret = 0;
  cell_t negate = 0;
  cell_t base = g_sys.base;
  if (!n) { return 0; }
  if (pos[0] == '-') { negate = -1; ++pos; --n; }
  if (pos[0] == '$') { base = 16; ++pos; --n; }
  for (; n; --n) {
    uintptr_t d = pos[0] - '0';
    if (d > 9) {
      d = LOWER(d) - 7;
      if (d < 10) { return 0; }
    }
    if (d >= base) { return 0; }
    *ret = *ret * base + d;
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
  cell_t *pos = g_sys.last;
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
  memcpy(g_sys.heap, name, length);  // name
  g_sys.heap += CELL_LEN(length);
  *g_sys.heap++ = length;  // length
  *g_sys.heap++ = (cell_t) g_sys.last;  // link
  *g_sys.heap++ = flags;  // flags
  g_sys.last = g_sys.heap;
  *g_sys.heap++ = (cell_t) op;  // code
}

static char spacefilter(char ch) {
  return ch == '\t' || ch == '\n' || ch == '\r' ? ' ' : ch;
}

static cell_t parse(cell_t sep, cell_t *ret) {
  while (g_sys.tin < g_sys.ntib &&
         spacefilter(g_sys.tib[g_sys.tin]) == sep) { ++g_sys.tin; }
  *ret = (cell_t) (g_sys.tib + g_sys.tin);
  while (g_sys.tin < g_sys.ntib &&
         spacefilter(g_sys.tib[g_sys.tin]) != sep) { ++g_sys.tin; }
  cell_t len = g_sys.tin - (*ret - (cell_t) g_sys.tib);
  if (g_sys.tin < g_sys.ntib) { ++g_sys.tin; }
  return len;
}

static cell_t *evaluate1(cell_t *sp) {
  cell_t call = 0;
  cell_t name;
  cell_t len = parse(' ', &name);
  cell_t xt = find((const char *) name, len);
  if (xt) {
    if (g_sys.state && !(((cell_t *) xt)[-1] & 1)) {  // bit 0 of flags is immediate
      *g_sys.heap++ = xt;
    } else {
      call = xt;
    }
  } else {
    cell_t n;
    cell_t ok = convert((const char *) name, len, &n);
    if (ok) {
      if (g_sys.state) {
        *g_sys.heap++ = g_sys.DOLIT_XT;
        *g_sys.heap++ = n;
      } else {
        *++sp = n;
      }
    } else {
#if PRINT_ERRORS
      write(2, (void *) name, len);
#endif
      *++sp = name;
      *++sp = len;
      *++sp = -1;
      call = g_sys.notfound;
    }
  }
  *++sp = call;
  return sp;
}

static void ueforth_run() {
  if (!g_sys.ip) {
#define X(name, op, code) create(name, sizeof(name) - 1, name[0] == ';', && OP_ ## op);
    PLATFORM_OPCODE_LIST
    OPCODE_LIST
#undef X
    return;
  }
  register cell_t *ip = g_sys.ip, *rp = g_sys.rp, *sp = g_sys.sp, tos, w;
  DROP; NEXT;
#define X(name, op, code) OP_ ## op: { code; } NEXT;
  PLATFORM_OPCODE_LIST
  OPCODE_LIST
#undef X
  OP_DOCOLON: ++rp; *rp = (cell_t) ip; ip = (cell_t *) (w + sizeof(cell_t)); NEXT;
  OP_DOCREATE: DUP; tos = w + sizeof(cell_t) * 2; NEXT;
  OP_DODOES: DUP; tos = w + sizeof(cell_t) * 2;
             ++rp; *rp = (cell_t) ip; ip = (cell_t *) *(cell_t *) (w + sizeof(cell_t)); NEXT;
}

static void ueforth(int argc, char *argv[], void *heap,
                    const char *src, cell_t src_len) {
  memset(&g_sys, 0, sizeof(g_sys));
  g_sys.heap = (cell_t *) heap;
  ueforth_run();
  g_sys.sp = g_sys.heap + sizeof(cell_t); g_sys.heap += STACK_SIZE;
  g_sys.rp = g_sys.heap; g_sys.heap += STACK_SIZE;
  g_sys.last[-1] = 1;  // Make ; IMMEDIATE
  g_sys.DOLIT_XT = FIND("DOLIT");
  g_sys.DOEXIT_XT = FIND("EXIT");
  g_sys.notfound = FIND("DROP");
  g_sys.ip = g_sys.heap;
  *g_sys.heap++ = FIND("EVALUATE1");
  *g_sys.heap++ = FIND("BRANCH");
  *g_sys.heap++ = (cell_t) g_sys.ip;
  g_sys.argc = argc;
  g_sys.argv = argv;
  g_sys.base = 10;
  g_sys.tib = src;
  g_sys.ntib = src_len;
  for (;;) {
    ueforth_run();
  }
}
