/*
 * Copyright 2023 Bradley D. Nelson
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
*/

/*
 * ESP32forth Assemblers v{{VERSION}}
 * Revision: {{REVISION}}
 */

#define OPTIONAL_ASSEMBLERS_SUPPORT \
  XV(internals, "assembler-source", ASSEMBLER_SOURCE, \
      PUSH assembler_source; PUSH sizeof(assembler_source) - 1) \
  PLATFORM_ASSEMBLER_SUPPORT

#include "gen/esp32_assembler.h"

#if defined(__riscv)

# define PLATFORM_ASSEMBLER_SUPPORT \
   XV(internals, "riscv-assembler-source", RISCV_ASSEMBLER_SOURCE, \
       PUSH riscv_assembler_source; PUSH sizeof(riscv_assembler_source) - 1)

#include "gen/esp32_riscv-assembler.h"

#else

# define PLATFORM_ASSEMBLER_SUPPORT \
   XV(internals, "xtensa-assembler-source", XTENSA_ASSEMBLER_SOURCE, \
       PUSH xtensa_assembler_source; PUSH sizeof(xtensa_assembler_source) - 1)

#include "gen/esp32_xtensa-assembler.h"

#endif
