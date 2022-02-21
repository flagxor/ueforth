// Copyright 2021 Bradley D. Nelson
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#ifndef CALLTYPE
# define CALLTYPE
#endif

#ifdef __cplusplus
typedef cell_t (CALLTYPE *call_t)(...);
#else
typedef cell_t (CALLTYPE *call_t)();
#endif

#define ct0 ((call_t) n0)

#define CALLING_OPCODE_LIST \
  YV(internals, CALL0, n0 = ct0()) \
  YV(internals, CALL1, n0 = ct0(n1); --sp) \
  YV(internals, CALL2, n0 = ct0(n2, n1); sp -= 2) \
  YV(internals, CALL3, n0 = ct0(n3, n2, n1); sp -= 3) \
  YV(internals, CALL4, n0 = ct0(n4, n3, n2, n1); sp -= 4) \
  YV(internals, CALL5, n0 = ct0(n5, n4, n3, n2, n1); sp -= 5) \
  YV(internals, CALL6, n0 = ct0(n6, n5, n4, n3, n2, n1); sp -= 6) \
  YV(internals, CALL7, n0 = ct0(n7, n6, n5, n4, n3, n2, n1); sp -= 7) \
  YV(internals, CALL8, n0 = ct0(n8, n7, n6, n5, n4, n3, n2, n1); sp -= 8) \
  YV(internals, CALL9, n0 = ct0(n9, n8, n7, n6, n5, n4, n3, n2, n1); sp -= 9) \
  YV(internals, CALL10, n0 = ct0(n10, n9, n8, n7, n6, n5, n4, n3, n2, n1); sp -= 10) \
  YV(internals, CALL11, n0 = ct0(n11, n10, n9, n8, n7, n6, n5, n4, n3, n2, n1); sp -= 11) \
  YV(internals, CALL12, n0 = ct0(n12, n11, n10, n9, n8, n7, n6, n5, n4, n3, n2, n1); sp -= 12) \
  YV(internals, CALL13, n0 = ct0(n13, n12, n11, n10, n9, n8, n7, n6, n5, n4, n3, n2, n1); sp -= 13) \
  YV(internals, CALL14, n0 = ct0(n14, n13, n12, n11, n10, n9, n8, n7, n6, n5, n4, n3, n2, n1); sp -= 14) \
  YV(internals, CALL15, n0 = ct0(n15, n14, n13, n12, n11, n10, n9, n8, n7, n6, n5, n4, n3, n2, n1); sp -= 15)
