// Copyright 2023 Bradley D. Nelson
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

#if defined(CONFIG_IDF_TARGET_ESP32)
# define UEFORTH_PLATFORM_IS_ESP32 (-1)
#else
# define UEFORTH_PLATFORM_IS_ESP32 0
#endif

#if defined(CONFIG_IDF_TARGET_ESP32S2)
# define UEFORTH_PLATFORM_IS_ESP32S2 (-1)
#else
# define UEFORTH_PLATFORM_IS_ESP32S2 0
#endif

#if defined(CONFIG_IDF_TARGET_ESP32S3)
# define UEFORTH_PLATFORM_IS_ESP32S3 (-1)
#else
# define UEFORTH_PLATFORM_IS_ESP32S3 0
#endif

#if defined(CONFIG_IDF_TARGET_ESP32C3)
# define UEFORTH_PLATFORM_IS_ESP32C3 (-1)
#else
# define UEFORTH_PLATFORM_IS_ESP32C3 0
#endif

#if defined(BOARD_HAS_PSRAM)
# define UEFORTH_PLATFORM_HAS_PSRAM (-1)
#else
# define UEFORTH_PLATFORM_HAS_PSRAM 0
#endif

#if defined(__XTENSA__)
# define UEFORTH_PLATFORM_IS_XTENSA (-1)
#else
# define UEFORTH_PLATFORM_IS_XTENSA 0
#endif

#if defined(__riscv)
# define UEFORTH_PLATFORM_IS_RISCV (-1)
#else
# define UEFORTH_PLATFORM_IS_RISCV 0
#endif
