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

/*
 * ESP32forth Interrupts v{{VERSION}}
 * Revision: {{REVISION}}
 */

#define INTERRUPT_STACK_CELLS 64

#include "esp_intr_alloc.h"
#include "driver/timer.h"
#include "driver/gpio.h"

static cell_t EspIntrAlloc(cell_t source, cell_t flags, cell_t xt, cell_t arg, void *ret);
static cell_t GpioIsrHandlerAdd(cell_t pin, cell_t xt, cell_t arg);
static cell_t TimerIsrCallbackAdd(cell_t group, cell_t timer, cell_t xt, cell_t arg, cell_t flags);
static void TimerInitNull(cell_t group, cell_t timer);

#define OPTIONAL_INTERRUPTS_VOCABULARIES V(interrupts) V(timers)
#define OPTIONAL_INTERRUPTS_SUPPORT \
  XV(internals, "interrupts-source", INTERRUPTS_SOURCE, \
      PUSH interrupts_source; PUSH sizeof(interrupts_source) - 1) \
  YV(interrupts, gpio_config, n0 = gpio_config((const gpio_config_t *) a0)) \
  YV(interrupts, gpio_reset_pin, n0 = gpio_reset_pin((gpio_num_t) n0)) \
  YV(interrupts, gpio_set_intr_type, n0 = gpio_set_intr_type((gpio_num_t) n1, (gpio_int_type_t) n0); NIP) \
  YV(interrupts, gpio_intr_enable, n0 = gpio_intr_enable((gpio_num_t) n0)) \
  YV(interrupts, gpio_intr_disable, n0 = gpio_intr_disable((gpio_num_t) n0)) \
  YV(interrupts, gpio_set_level, n0 = gpio_set_level((gpio_num_t) n1, n0); NIP) \
  YV(interrupts, gpio_get_level, n0 = gpio_get_level((gpio_num_t) n0)) \
  YV(interrupts, gpio_set_direction, n0 = gpio_set_direction((gpio_num_t) n1, (gpio_mode_t) n0); NIP) \
  YV(interrupts, gpio_set_pull_mode, n0 = gpio_set_pull_mode((gpio_num_t) n1, (gpio_pull_mode_t) n0); NIP) \
  YV(interrupts, gpio_wakeup_enable, n0 = gpio_wakeup_enable((gpio_num_t) n1, (gpio_int_type_t) n0); NIP) \
  YV(interrupts, gpio_wakeup_disable, n0 = gpio_wakeup_disable((gpio_num_t) n0)) \
  YV(interrupts, gpio_pullup_en, n0 = gpio_pullup_en((gpio_num_t) n0)) \
  YV(interrupts, gpio_pullup_dis, n0 = gpio_pullup_dis((gpio_num_t) n0)) \
  YV(interrupts, gpio_pulldown_en, n0 = gpio_pulldown_en((gpio_num_t) n0)) \
  YV(interrupts, gpio_pulldown_dis, n0 = gpio_pulldown_dis((gpio_num_t) n0)) \
  YV(interrupts, gpio_hold_en, n0 = gpio_hold_en((gpio_num_t) n0)) \
  YV(interrupts, gpio_hold_dis, n0 = gpio_hold_dis((gpio_num_t) n0)) \
  YV(interrupts, gpio_deep_sleep_hold_en, gpio_deep_sleep_hold_en()) \
  YV(interrupts, gpio_deep_sleep_hold_dis, gpio_deep_sleep_hold_dis()) \
  YV(interrupts, gpio_install_isr_service, n0 = gpio_install_isr_service(n0)) \
  YV(interrupts, gpio_uninstall_isr_service, gpio_uninstall_isr_service()) \
  YV(interrupts, gpio_isr_handler_add, n0 = GpioIsrHandlerAdd(n2, n1, n0); NIPn(2)) \
  YV(interrupts, gpio_isr_handler_remove, n0 = gpio_isr_handler_remove((gpio_num_t) n0)) \
  YV(interrupts, gpio_set_drive_capability, n0 = gpio_set_drive_capability((gpio_num_t) n1, (gpio_drive_cap_t) n0); NIP) \
  YV(interrupts, gpio_get_drive_capability, n0 = gpio_get_drive_capability((gpio_num_t) n1, (gpio_drive_cap_t *) a0); NIP) \
  YV(interrupts, esp_intr_alloc, n0 = EspIntrAlloc(n4, n3, n2, n1, a0); NIPn(4)) \
  YV(interrupts, esp_intr_free, n0 = esp_intr_free((intr_handle_t) n0)) \
  YV(timers, timer_isr_callback_add, n0 = TimerIsrCallbackAdd(n4, n3, n2, n1, n0); NIPn(4)) \
  YV(timers, timer_init_null, TimerInitNull(n1, n0); DROPn(2)) \
  YV(timers, timer_get_counter_value, \
      n0 = timer_get_counter_value((timer_group_t) n2, (timer_idx_t) n1, \
                                   (uint64_t *) a0); NIPn(2)) \
  YV(timers, timer_set_counter_value, \
      uint64_t val = *(uint64_t *) a0; \
      n0 = timer_set_counter_value((timer_group_t) n2, (timer_idx_t) n1, val); NIPn(2)) \
  YV(timers, timer_start, \
      n0 = timer_start((timer_group_t) n1, (timer_idx_t) n0); NIP) \
  YV(timers, timer_pause, \
      n0 = timer_pause((timer_group_t) n1, (timer_idx_t) n0); NIP) \
  YV(timers, timer_set_counter_mode, \
      n0 = timer_set_counter_mode((timer_group_t) n2, (timer_idx_t) n1, \
                                  (timer_count_dir_t) n0); NIPn(2)) \
  YV(timers, timer_set_auto_reload, \
      n0 = timer_set_auto_reload((timer_group_t) n2, (timer_idx_t) n1, \
                                 (timer_autoreload_t) n0); NIPn(2)) \
  YV(timers, timer_set_divider, \
      n0 = timer_set_divider((timer_group_t) n2, (timer_idx_t) n1, n0); NIPn(2)) \
  YV(timers, timer_set_alarm_value, uint64_t val = *(uint64_t *) a0; \
      n0 = timer_set_alarm_value((timer_group_t) n2, (timer_idx_t) n1, val); NIPn(2)) \
  YV(timers, timer_get_alarm_value, \
      n0 = timer_get_alarm_value((timer_group_t) n2, (timer_idx_t) n1, \
                                 (uint64_t *) a0); NIPn(2)) \
  YV(timers, timer_set_alarm, \
      n0 = timer_set_alarm((timer_group_t) n2, (timer_idx_t) n1, \
                           (timer_alarm_t) n0); NIPn(2)) \
  YV(timers, timer_group_intr_enable, \
      n0 = timer_group_intr_enable((timer_group_t) n1, (timer_intr_t) n0); NIP) \
  YV(timers, timer_group_intr_disable, \
      n0 = timer_group_intr_disable((timer_group_t) n1, (timer_intr_t) n0); NIP) \
  YV(timers, timer_enable_intr, \
      n0 = timer_enable_intr((timer_group_t) n1, (timer_idx_t) n0); NIP) \
  YV(timers, timer_disable_intr, \
      n0 = timer_disable_intr((timer_group_t) n1, (timer_idx_t) n0); NIP)

struct handle_interrupt_args {
  cell_t xt;
  cell_t arg;
};

static void IRAM_ATTR HandleInterrupt(void *arg) {
  struct handle_interrupt_args *args = (struct handle_interrupt_args *) arg;
  cell_t code[2];
  code[0] = args->xt;
  code[1] = g_sys->YIELD_XT;
  cell_t fstack[INTERRUPT_STACK_CELLS];
  cell_t rstack[INTERRUPT_STACK_CELLS];
  cell_t stack[INTERRUPT_STACK_CELLS];
  stack[0] = args->arg;
  cell_t *rp = rstack;
  *++rp = (cell_t) code;
  *++rp = (cell_t) (fstack + 1);
  *++rp = (cell_t) (stack + 1);
  forth_run(rp);
}

static bool IRAM_ATTR HandleInterruptAndRet(void *arg) {
  HandleInterrupt(arg);
  return true;
}

static cell_t EspIntrAlloc(cell_t source, cell_t flags, cell_t xt, cell_t arg, void *ret) {
  // NOTE: Leaks memory.
  struct handle_interrupt_args *args = (struct handle_interrupt_args *) malloc(sizeof(struct handle_interrupt_args));
  args->xt = xt;
  args->arg = arg;
  return esp_intr_alloc(source, flags, HandleInterrupt, args, (intr_handle_t *) ret);
}

static cell_t GpioIsrHandlerAdd(cell_t pin, cell_t xt, cell_t arg) {
  // NOTE: Leaks memory.
  struct handle_interrupt_args *args = (struct handle_interrupt_args *) malloc(sizeof(struct handle_interrupt_args));
  args->xt = xt;
  args->arg = arg;
  return gpio_isr_handler_add((gpio_num_t) pin, HandleInterrupt, args);
}

static void TimerInitNull(cell_t group, cell_t timer) {
  // Seems to be required starting in the 2.0 IDE.
  timer_config_t config;
  memset(&config, 0, sizeof(config));
  config.divider = 2;
  timer_init((timer_group_t) group, (timer_idx_t) timer, &config);
}

static cell_t TimerIsrCallbackAdd(cell_t group, cell_t timer, cell_t xt, cell_t arg, cell_t flags) {
  // NOTE: Leaks memory.
  struct handle_interrupt_args *args = (struct handle_interrupt_args *) malloc(sizeof(struct handle_interrupt_args));
  args->xt = xt;
  args->arg = arg;
  return timer_isr_callback_add((timer_group_t) group, (timer_idx_t) timer, HandleInterruptAndRet, args, flags);
}

#include "gen/esp32_interrupts.h"
