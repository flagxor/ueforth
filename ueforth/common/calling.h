#ifndef CALLTYPE
# define CALLTYPE
#endif
typedef cell_t (CALLTYPE *call_t)();

#define CALLING_OPCODE_LIST \
  X("CALL0", OP_CALL0, tos = ((call_t) tos)()) \
  X("CALL1", OP_CALL1, tos = ((call_t) tos)(*sp); --sp) \
  X("CALL2", OP_CALL2, tos = ((call_t) tos)(sp[-1], *sp); sp -= 2) \
  X("CALL3", OP_CALL3, tos = ((call_t) tos)(sp[-2], sp[-1], *sp); sp -= 3) \
  X("CALL4", OP_CALL4, tos = ((call_t) tos)(sp[-3], sp[-2], sp[-1], *sp); sp -= 4) \
  X("CALL5", OP_CALL5, tos = ((call_t) tos)(sp[-4], sp[-3], sp[-2], sp[-1], *sp); sp -= 5) \
  X("CALL6", OP_CALL6, tos = ((call_t) tos)(sp[-5], sp[-4], sp[-3], sp[-2], sp[-1], *sp); sp -= 6) \
  X("CALL7", OP_CALL7, tos = ((call_t) tos)(sp[-6], sp[-5], sp[-4], sp[-3], sp[-2], sp[-1], *sp); sp -= 7) \
  X("CALL8", OP_CALL8, tos = ((call_t) tos)(sp[-7], sp[-6], sp[-5], sp[-4], sp[-3], sp[-2], sp[-1], *sp); sp -= 8) \
  X("CALL9", OP_CALL9, tos = ((call_t) tos)(sp[-8], sp[-7], sp[-6], sp[-5], sp[-4], sp[-3], sp[-2], sp[-1], *sp); sp -= 9) \
  X("CALL10", OP_CALL10, tos = ((call_t) tos)(sp[-9], sp[-8], sp[-7], sp[-6], sp[-5], sp[-4], sp[-3], sp[-2], sp[-1], *sp); sp -= 10) \

