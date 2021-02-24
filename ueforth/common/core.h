#define PRINT_ERRORS 0

#define CELL_LEN(n) (((n) + sizeof(cell_t) - 1) / sizeof(cell_t))
#define FIND(name) find(name, sizeof(name) - 1)
#define LOWER(ch) ((ch) & 0x5F)
#define IMMEDIATE 1
#define SMUDGE 2
#define VOCABULARY_DEPTH 16

#if PRINT_ERRORS
#include <unistd.h>
#endif

static struct {
  const char *tib;
  cell_t ntib, tin, state, base;
  cell_t *heap, **current, ***context, notfound;
  int argc;
  char **argv;
  cell_t *(*runner)(cell_t *rp);  // pointer to forth_run
  cell_t *rp;  // spot to park main thread
  cell_t DOLIT_XT, DOEXIT_XT, YIELD_XT;
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
  for (cell_t ***voc = g_sys.context; *voc; ++voc) {
    cell_t *pos = **voc;
    cell_t clen = CELL_LEN(len);
    while (pos) {
      if (!(pos[-1] & SMUDGE) && len == pos[-3] &&
          same(name, (const char *) &pos[-3 - clen], len) == 0) {
        return (cell_t) pos;
      }
      pos = (cell_t *) pos[-2];  // Follow link
    }
  }
  return 0;
}

static void create(const char *name, cell_t length, cell_t flags, void *op) {
  char *pos = (char *) g_sys.heap;
  for (cell_t n = length; n; --n) { *pos++ = *name++; }  // name
  g_sys.heap += CELL_LEN(length);
  *g_sys.heap++ = length;  // length
  *g_sys.heap++ = (cell_t) *g_sys.current;  // link
  *g_sys.heap++ = flags;  // flags
  *g_sys.current = g_sys.heap;
  *g_sys.heap++ = (cell_t) op;  // code
}

static int match(char sep, char ch) {
  return sep == ch || (sep == ' ' && (ch == '\t' || ch == '\n' || ch == '\r'));
}

static cell_t parse(cell_t sep, cell_t *ret) {
  while (g_sys.tin < g_sys.ntib &&
         match(sep, g_sys.tib[g_sys.tin])) { ++g_sys.tin; }
  *ret = (cell_t) (g_sys.tib + g_sys.tin);
  while (g_sys.tin < g_sys.ntib &&
         !match(sep, g_sys.tib[g_sys.tin])) { ++g_sys.tin; }
  cell_t len = g_sys.tin - (*ret - (cell_t) g_sys.tib);
  if (g_sys.tin < g_sys.ntib) { ++g_sys.tin; }
  return len;
}

static cell_t *evaluate1(cell_t *sp) {
  cell_t call = 0;
  cell_t name;
  cell_t len = parse(' ', &name);
  if (len == 0) { *++sp = 0; return sp; }  // ignore empty
  cell_t xt = find((const char *) name, len);
  if (xt) {
    if (g_sys.state && !(((cell_t *) xt)[-1] & IMMEDIATE)) {
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
      write(2, "\n", 1);
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

static cell_t *forth_run(cell_t *initrp);

static void forth_init(int argc, char *argv[], void *heap,
                         const char *src, cell_t src_len) {
  g_sys.heap = (cell_t *) heap + 4;  // Leave a little room.
  cell_t *sp = g_sys.heap + 1; g_sys.heap += STACK_SIZE;
  cell_t *rp = g_sys.heap + 1; g_sys.heap += STACK_SIZE;

  // FORTH vocabulary
  *g_sys.heap++ = 0; cell_t *forth = g_sys.heap;
  *g_sys.heap++ = 0;  *g_sys.heap++ = 0;  *g_sys.heap++ = 0;
  // Vocabulary stack
  g_sys.current = (cell_t **) forth;
  g_sys.context = (cell_t ***) g_sys.heap;
  *g_sys.heap++ = (cell_t) forth;
  for (int i = 0; i < VOCABULARY_DEPTH; ++i) { *g_sys.heap++ = 0; }

  forth_run(0);
  (*g_sys.current)[-1] = IMMEDIATE;  // Make last word ; IMMEDIATE
  g_sys.DOLIT_XT = FIND("DOLIT");
  g_sys.DOEXIT_XT = FIND("EXIT");
  g_sys.YIELD_XT = FIND("YIELD");
  g_sys.notfound = FIND("DROP");
  cell_t *start = g_sys.heap;
  *g_sys.heap++ = FIND("EVALUATE1");
  *g_sys.heap++ = FIND("BRANCH");
  *g_sys.heap++ = (cell_t) start;
  g_sys.argc = argc;
  g_sys.argv = argv;
  g_sys.base = 10;
  g_sys.tib = src;
  g_sys.ntib = src_len;
  *++rp = (cell_t) sp;
  *++rp = (cell_t) start;
  g_sys.rp = rp;
  g_sys.runner = forth_run;
}
