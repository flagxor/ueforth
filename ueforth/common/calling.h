#ifndef CALLTYPE
# define CALLTYPE
#endif
typedef cell_t (CALLTYPE *call_t)();

#define CALLING_OPCODE_LIST \
  Y(CALL0, tos = ((call_t) tos)()) \
  Y(CALL1, tos = ((call_t) tos)(*sp); --sp) \
  Y(CALL2, tos = ((call_t) tos)(sp[-1], *sp); sp -= 2) \
  Y(CALL3, tos = ((call_t) tos)(sp[-2], sp[-1], *sp); sp -= 3) \
  Y(CALL4, tos = ((call_t) tos)(sp[-3], sp[-2], sp[-1], *sp); sp -= 4) \
  Y(CALL5, tos = ((call_t) tos)(sp[-4], sp[-3], sp[-2], sp[-1], *sp); sp -= 5) \
  Y(CALL6, tos = ((call_t) tos)(sp[-5], sp[-4], sp[-3], sp[-2], sp[-1], *sp); sp -= 6) \
  Y(CALL7, tos = ((call_t) tos)(sp[-6], sp[-5], sp[-4], sp[-3], sp[-2], sp[-1], *sp); sp -= 7) \
  Y(CALL8, tos = ((call_t) tos)(sp[-7], sp[-6], sp[-5], sp[-4], sp[-3], sp[-2], sp[-1], *sp); sp -= 8) \
  Y(CALL9, tos = ((call_t) tos)(sp[-8], sp[-7], sp[-6], sp[-5], sp[-4], sp[-3], sp[-2], sp[-1], *sp); sp -= 9) \
  Y(CALL10, tos = ((call_t) tos)(sp[-9], sp[-8], sp[-7], sp[-6], sp[-5], sp[-4], sp[-3], sp[-2], sp[-1], *sp); sp -= 10) \

