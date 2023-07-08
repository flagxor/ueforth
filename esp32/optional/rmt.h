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

#include "driver/rmt.h"

#define OPTIONAL_RMT_VOCABULARY V(rmt)
#define OPTIONAL_RMT_SUPPORT \
  YV(rmt, rmt_set_clk_div, n0 = rmt_set_clk_div((rmt_channel_t) n1, n0); NIP) \
  YV(rmt, rmt_get_clk_div, n0 = rmt_get_clk_div((rmt_channel_t) n1, b0); NIP) \
  YV(rmt, rmt_set_rx_idle_thresh, n0 = rmt_set_rx_idle_thresh((rmt_channel_t) n1, n0); NIP) \
  YV(rmt, rmt_get_rx_idle_thresh, \
    n0 = rmt_get_rx_idle_thresh((rmt_channel_t) n1, (uint16_t *) a0); NIP) \
  YV(rmt, rmt_set_mem_block_num, n0 = rmt_set_mem_block_num((rmt_channel_t) n1, n0); NIP) \
  YV(rmt, rmt_get_mem_block_num, n0 = rmt_get_mem_block_num((rmt_channel_t) n1, b0); NIP) \
  YV(rmt, rmt_set_tx_carrier, n0 = rmt_set_tx_carrier((rmt_channel_t) n4, n3, n2, n1, \
                                                (rmt_carrier_level_t) n0); NIPn(4)) \
  YV(rmt, rmt_set_mem_pd, n0 = rmt_set_mem_pd((rmt_channel_t) n1, n0); NIP) \
  YV(rmt, rmt_get_mem_pd, n0 = rmt_get_mem_pd((rmt_channel_t) n1, (bool *) a0); NIP) \
  YV(rmt, rmt_tx_start, n0 = rmt_tx_start((rmt_channel_t) n1, n0); NIP) \
  YV(rmt, rmt_tx_stop, n0 = rmt_tx_stop((rmt_channel_t) n0)) \
  YV(rmt, rmt_rx_start, n0 = rmt_rx_start((rmt_channel_t) n1, n0); NIP) \
  YV(rmt, rmt_rx_stop, n0 = rmt_rx_stop((rmt_channel_t) n0)) \
  YV(rmt, rmt_tx_memory_reset, n0 = rmt_tx_memory_reset((rmt_channel_t) n0)) \
  YV(rmt, rmt_rx_memory_reset, n0 = rmt_rx_memory_reset((rmt_channel_t) n0)) \
  YV(rmt, rmt_set_memory_owner, n0 = rmt_set_memory_owner((rmt_channel_t) n1, (rmt_mem_owner_t) n0); NIP) \
  YV(rmt, rmt_get_memory_owner, n0 = rmt_get_memory_owner((rmt_channel_t) n1, (rmt_mem_owner_t *) a0); NIP) \
  YV(rmt, rmt_set_tx_loop_mode, n0 = rmt_set_tx_loop_mode((rmt_channel_t) n1, n0); NIP) \
  YV(rmt, rmt_get_tx_loop_mode, n0 = rmt_get_tx_loop_mode((rmt_channel_t) n1, (bool *) a0); NIP) \
  YV(rmt, rmt_set_rx_filter, n0 = rmt_set_rx_filter((rmt_channel_t) n2, n1, n0); NIPn(2)) \
  YV(rmt, rmt_set_source_clk, n0 = rmt_set_source_clk((rmt_channel_t) n1, (rmt_source_clk_t) n0); NIP) \
  YV(rmt, rmt_get_source_clk, n0 = rmt_get_source_clk((rmt_channel_t) n1, (rmt_source_clk_t * ) a0); NIP) \
  YV(rmt, rmt_set_idle_level, n0 = rmt_set_idle_level((rmt_channel_t) n2, n1, \
        (rmt_idle_level_t) n0); NIPn(2)) \
  YV(rmt, rmt_get_idle_level, n0 = rmt_get_idle_level((rmt_channel_t) n2, \
        (bool *) a1, (rmt_idle_level_t *) a0); NIPn(2)) \
  YV(rmt, rmt_get_status, n0 = rmt_get_status((rmt_channel_t) n1, (uint32_t *) a0); NIP) \
  YV(rmt, rmt_set_rx_intr_en, n0 = rmt_set_rx_intr_en((rmt_channel_t) n1, n0); NIP) \
  YV(rmt, rmt_set_err_intr_en, n0 = rmt_set_err_intr_en((rmt_channel_t) n1, (rmt_mode_t) n0); NIP) \
  YV(rmt, rmt_set_tx_intr_en, n0 = rmt_set_tx_intr_en((rmt_channel_t) n1, n0); NIP) \
  YV(rmt, rmt_set_tx_thr_intr_en, n0 = rmt_set_tx_thr_intr_en((rmt_channel_t) n2, n1, n0); NIPn(2)) \
  YV(rmt, rmt_set_gpio, n0 = rmt_set_gpio((rmt_channel_t) n3, (rmt_mode_t) n2, (gpio_num_t) n1, n0); NIPn(3)) \
  YV(rmt, rmt_config, n0 = rmt_config((const rmt_config_t *) a0)) \
  YV(rmt, rmt_isr_register, n0 = rmt_isr_register((void (*)(void*)) a3, a2, n1, \
        (rmt_isr_handle_t *) a0); NIPn(3)) \
  YV(rmt, rmt_isr_deregister, n0 = rmt_isr_deregister((rmt_isr_handle_t) n0)) \
  YV(rmt, rmt_fill_tx_items, n0 = rmt_fill_tx_items((rmt_channel_t) n3, \
        (rmt_item32_t *) a2, n1, n0); NIPn(3)) \
  YV(rmt, rmt_driver_install, n0 = rmt_driver_install((rmt_channel_t) n2, n1, n0); NIPn(2)) \
  YV(rmt, rmt_driver_uinstall, n0 = rmt_driver_uninstall((rmt_channel_t) n0)) \
  YV(rmt, rmt_get_channel_status, n0 = rmt_get_channel_status((rmt_channel_status_result_t *) a0)) \
  YV(rmt, rmt_get_counter_clock, n0 = rmt_get_counter_clock((rmt_channel_t) n1, (uint32_t *) a0); NIP) \
  YV(rmt, rmt_write_items, n0 = rmt_write_items((rmt_channel_t) n3, (rmt_item32_t *) a2, n1, n0); NIPn(3)) \
  YV(rmt, rmt_wait_tx_done, n0 = rmt_wait_tx_done((rmt_channel_t) n1, n0); NIP) \
  YV(rmt, rmt_get_ringbuf_handle, n0 = rmt_get_ringbuf_handle((rmt_channel_t) n1, (RingbufHandle_t *) a0); NIP) \
  YV(rmt, rmt_translator_init, n0 = rmt_translator_init((rmt_channel_t) n1, (sample_to_rmt_t) n0); NIP) \
  YV(rmt, rmt_translator_set_context, n0 = rmt_translator_set_context((rmt_channel_t) n1, a0); NIP) \
  YV(rmt, rmt_translator_get_context, n0 = rmt_translator_get_context((const size_t *) a1, (void **) a0); NIP) \
  YV(rmt, rmt_write_sample, n0 = rmt_write_sample((rmt_channel_t) n3, b2, n1, n0); NIPn(3))
