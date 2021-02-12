#define JMPW goto **(void **) w
#define NEXT w = *ip++; JMPW
#define ADDR_DOCOLON && OP_DOCOLON
#define ADDR_DOCREATE && OP_DOCREATE
#define ADDR_DODOES && OP_DODOES

static cell_t *forth_run(cell_t *init_rp) {
  if (!init_rp) {
#define X(name, op, code) create(name, sizeof(name) - 1, name[0] == ';', && OP_ ## op);
    PLATFORM_OPCODE_LIST
    OPCODE_LIST
#undef X
    return 0;
  }
  register cell_t *ip, *rp, *sp, tos, w;
  rp = init_rp;  ip = (cell_t *) *rp--;  sp = (cell_t *) *rp--;
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
