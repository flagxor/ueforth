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

#include <setjmp.h>
#include <signal.h>

static __thread jmp_buf g_forth_fault;
static __thread int g_forth_signal;

#define FAULT_ENTRY \
  if (setjmp(g_forth_fault)) { THROWIT(g_forth_signal); }

static void forth_signal_handler(int sig) {
  switch (sig) {
    case SIGSEGV: g_forth_signal = -9; break;
    case SIGBUS: g_forth_signal = -23; break;
    case SIGINT: g_forth_signal = -28; break;
    case SIGFPE: g_forth_signal = -10; break;
    default: g_forth_signal = -256 - sig; break;
  }
  sigset_t ss;
  sigemptyset(&ss);
  sigprocmask(SIG_SETMASK, &ss, 0);
  longjmp(g_forth_fault, 1);
}

static void forth_faults_setup(void) {
  struct sigaction sa;
  memset(&sa, 0, sizeof(sa));
  sa.sa_handler = forth_signal_handler;
  sigaction(SIGSEGV, &sa, 0);
  sigaction(SIGBUS, &sa, 0);
  sigaction(SIGINT, &sa, 0);
  sigaction(SIGFPE, &sa, 0);
}
