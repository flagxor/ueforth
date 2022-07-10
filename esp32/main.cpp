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

void setup() {
  cell_t fh = heap_caps_get_free_size(MALLOC_CAP_INTERNAL);
  cell_t hc = heap_caps_get_largest_free_block(MALLOC_CAP_INTERNAL);
  if (fh - hc < MINIMUM_FREE_SYSTEM_HEAP) {
    hc = fh - MINIMUM_FREE_SYSTEM_HEAP;
  }
  cell_t *heap = (cell_t *) malloc(hc);
  forth_init(0, 0, heap, hc, boot, sizeof(boot));
}

void loop() {
  g_sys->rp = forth_run(g_sys->rp);
}
