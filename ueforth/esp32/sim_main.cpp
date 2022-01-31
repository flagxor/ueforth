// Copyright 2022 Bradley D. Nelson
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

#include "esp32/config.h"
#include "esp32/options.h"
#include "common/opcodes.h"
#include "common/floats.h"
#include "common/calling.h"
#include "gen/esp32_sim_opcodes.h"
#include "common/core.h"
#include "common/interp.h"
#include "gen/esp32_boot.h"
#include "esp32/main.cpp"

int main(int argc, char *argv[]) {
  setup();
  for (;;) {
    loop();
  }
}
