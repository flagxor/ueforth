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
typedef cell_t (CALLTYPE *call_t)();
#define ct0 ((call_t) n0)

#define CALLING_OPCODE_LIST \
  Y(CALL0, n0 = ct0()) \
  Y(CALL1, n0 = ct0(n1); --sp) \
  Y(CALL2, n0 = ct0(n2, n1); sp -= 2) \
  Y(CALL3, n0 = ct0(n3, n2, n1); sp -= 3) \
  Y(CALL4, n0 = ct0(n4, n3, n2, n1); sp -= 4) \
  Y(CALL5, n0 = ct0(n5, n4, n3, n2, n1); sp -= 5) \
  Y(CALL6, n0 = ct0(n6, n5, n4, n3, n2, n1); sp -= 6) \
  Y(CALL7, n0 = ct0(n7, n6, n5, n4, n3, n2, n1); sp -= 7) \
  Y(CALL8, n0 = ct0(n8, n7, n6, n5, n4, n3, n2, n1); sp -= 8) \
  Y(CALL9, n0 = ct0(n9, n8, n7, n6, n5, n4, n3, n2, n1); sp -= 9) \
  Y(CALL10, n0 = ct0(n10, n9, n8, n7, n6, n5, n4, n3, n2, n1); sp -= 10) \

