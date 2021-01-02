#define NEXT w = *ip++; goto **(void **) w
#define CELL_LEN(n) (((n) + sizeof(cell_t) - 1) / sizeof(cell_t))
#define FIND(name) find(name, sizeof(name) - 1)
#define LOWER(ch) ((ch) & 95)

static cell_t *g_heap;
static const char *g_tib;
static cell_t g_ntib, g_tin = 0;
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

static void ueforth(const char *src, cell_t src_len) {
  g_heap = malloc(HEAP_SIZE);
  register cell_t *sp = g_heap; g_heap += STACK_SIZE;
  register cell_t *rp = g_heap; g_heap += STACK_SIZE;
  register cell_t tos = 0, *ip, t, w;
  dcell_t d;
  udcell_t ud;
  cell_t tmp;
#define X(name, op, code) create(name, sizeof(name) - 1, name[0] == ';', && op);
  PLATFORM_OPCODE_LIST
  OPCODE_LIST
#undef X
  g_last[-1] = 1;  // Make ; IMMEDIATE
  g_DOLIT_XT = FIND("DOLIT");
  g_DOEXIT_XT = FIND("EXIT");
  ip = g_heap;
  *g_heap++ = FIND("EVAL1");
  *g_heap++ = FIND("BRANCH");
  *g_heap++ = (cell_t) ip;
  g_tib = src;
  g_ntib = src_len;
  NEXT;
#define X(name, op, code) op: code; NEXT;
  PLATFORM_OPCODE_LIST
  OPCODE_LIST
#undef X
  OP_DOCREATE: DUP; tos = w + sizeof(cell_t) * 2; NEXT;
  OP_DODOES: DUP; tos = w + sizeof(cell_t) * 2;
             *++rp = (cell_t) ip; ip = (cell_t *) *(cell_t *) (w + sizeof(cell_t)); NEXT;
  OP_DOCOL: *++rp = (cell_t) ip; ip = (cell_t *) (w + sizeof(cell_t)); NEXT;
}
