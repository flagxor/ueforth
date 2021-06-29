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

#define SET tos = (cell_t)

#define n0 tos
#define n1 (*sp)
#define n2 sp[-1]
#define n3 sp[-2]
#define n4 sp[-3]
#define n5 sp[-4]
#define n6 sp[-5]
#define n7 sp[-6]
#define n8 sp[-7]
#define n9 sp[-8]
#define n10 sp[-9]

#define a0 ((void *) tos)
#define a1 (*(void **) &n1)
#define a2 (*(void **) &n2)
#define a3 (*(void **) &n3)
#define a4 (*(void **) &n4)
#define a5 (*(void **) &n5)
#define a6 (*(void **) &n6)

#define b0 ((uint8_t *) tos)
#define b1 (*(uint8_t **) &n1)
#define b2 (*(uint8_t **) &n2)
#define b3 (*(uint8_t **) &n3)
#define b4 (*(uint8_t **) &n4)
#define b5 (*(uint8_t **) &n5)
#define b6 (*(uint8_t **) &n6)

#define c0 ((char *) tos)
#define c1 (*(char **) &n1)
#define c2 (*(char **) &n2)
#define c3 (*(char **) &n3)
#define c4 (*(char **) &n4)
#define c5 (*(char **) &n5)
#define c6 (*(char **) &n6)

