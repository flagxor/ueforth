/*
 * Copyright 2021 Bradley D. Nelson
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
 * ESP32forth v{{VERSION}}
 * Revision: {{REVISION}}
 */

#include "esp32/platform.h"
#include "esp32/options.h"
#include "common/tier0_opcodes.h"
#include "common/tier1_opcodes.h"
#include "common/tier2_opcodes.h"
#include "common/floats.h"
#include "common/calls.h"
#include "common/bits.h"
#include "esp32/builtins.h"
#include "esp32/builtins.cpp"
#include "esp32/main.cpp"
