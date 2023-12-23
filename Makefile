# Copyright 2021 Bradley D. Nelson
#
# Licensed under the Apache License, Version 2.0 (the "License");
# you may not use this file except in compliance with the License.
# You may obtain a copy of the License at
#
#     http://www.apache.org/licenses/LICENSE-2.0
#
# Unless required by applicable law or agreed to in writing, software
# distributed under the License is distributed on an "AS IS" BASIS,
# WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
# See the License for the specific language governing permissions and
# limitations under the License.

VERSION=7.0.7.16
STABLE_VERSION=7.0.6.19
OLD_STABLE_VERSION=7.0.5.4
REVISION=$(shell git rev-parse HEAD | head -c 20)
REVSHORT=$(shell echo $(REVISION) | head -c 7)

OUT = out
GEN = $(OUT)/gen
RES = $(OUT)/resources
WEB = $(OUT)/web
POSIX = $(OUT)/posix
WINDOWS = $(OUT)/windows
ESP32 = $(OUT)/esp32
ESP32_SIM = $(OUT)/esp32-sim
PICO_ICE = $(OUT)/pico-ice
PICO_ICE_SIM = $(OUT)/pico-ice-sim
DEPLOY = $(OUT)/deploy

OS = $(shell uname -s)

CFLAGS_COMMON = -O2 -I ./ -I $(OUT)

CFLAGS_MINIMIZE = \
                -s \
                -DUEFORTH_MINIMAL \
                -fno-exceptions \
                -ffreestanding \
                -fno-stack-protector \
                -fomit-frame-pointer \
                -fno-ident \
                -ffunction-sections -fdata-sections \
                -fmerge-all-constants
CFLAGS = $(CFLAGS_COMMON) \
         $(CFLAGS_MINIMIZE) \
         -std=c++11 \
         -Wall \
         -Werror \
         -no-pie \
         -Wl,--gc-sections
ifeq ($(OS),Darwin)
  CFLAGS += -Wl,-dead_strip -D_GNU_SOURCE
endif
ifeq ($(OS),Linux)
  CFLAGS_MINIMIZE += -Wl,--build-id=none
  CFLAGS += -s -Wl,--gc-sections -no-pie -Wl,--build-id=none
endif

STRIP_ARGS = -S
ifeq ($(OS),Darwin)
  STRIP_ARGS += -x
endif
ifeq ($(OS),Linux)
  STRIP_ARGS += --strip-unneeded \
                --remove-section=.note.gnu.gold-version \
                --remove-section=.comment \
                --remove-section=.note \
                --remove-section=.note.gnu.build-id \
                --remove-section=.note.ABI-tag
endif

LIBS=-ldl

WIN_CFLAGS = $(CFLAGS_COMMON) \
             -I "c:/Program Files (x86)/Microsoft SDKs/Windows/v7.1A/Include" \
             -I "c:/Program Files (x86)/Microsoft Visual Studio/2019/Community/VC/Tools/MSVC/14.28.29333/include" \
             -I "c:/Program Files (x86)/Windows Kits/10/Include/10.0.19041.0/ucrt"

WIN_LFLAGS32 = /LIBPATH:"c:/Program Files (x86)/Microsoft SDKs/Windows/v7.1A/Lib" \
               /LIBPATH:"c:/Program Files (x86)/Microsoft Visual Studio/2019/Community/VC/Tools/MSVC/14.28.29333/lib/x86" \
               /LIBPATH:"c:/Program Files (x86)/Windows Kits/10/Lib/10.0.19041.0/ucrt/x86" \
               $(WIN_LIBS)

WIN_LFLAGS64 = /LIBPATH:"c:/Program Files (x86)/Microsoft SDKs/Windows/v7.1A/Lib/x64" \
               /LIBPATH:"c:/Program Files (x86)/Microsoft Visual Studio/2019/Community/VC/Tools/MSVC/14.28.29333/lib/x64" \
               /LIBPATH:"c:/Program Files (x86)/Windows Kits/10/Lib/10.0.19041.0/ucrt/x64" \
               $(WIN_LIBS)

WIN_LIBS=user32.lib

TARGETS = posix_target \
          web_target \
          esp32_target \
          esp32_sim_target \
          pico_ice_target \
          pico_ice_sim_target
TESTS = posix_tests web_tests esp32_sim_tests

LSQ = ls 2>/dev/null

PROGFILES = /mnt/c/Program Files (x86)
MSVS = "${PROGFILES}/Microsoft Visual Studio"
MSKITS = "${PROGFILES}/Windows Kits"
CL32 = "$(shell $(LSQ) ${MSVS}/*/*/VC/Tools/MSVC/*/bin/Hostx86/x86/cl.exe | head -n 1)"
CL64 = "$(shell $(LSQ) ${MSVS}/*/*/VC/Tools/MSVC/*/bin/Hostx86/x64/cl.exe | head -n 1)"
LINK32 = "$(shell $(LSQ) ${MSVS}/*/*/VC/Tools/MSVC/*/bin/Hostx86/x86/link.exe | head -n 1)"
LINK64 = "$(shell $(LSQ) ${MSVS}/*/*/VC/Tools/MSVC/*/bin/Hostx86/x64/link.exe | head -n 1)"
RC32 = "$(shell $(LSQ) ${MSKITS}/*/bin/*/x86/rc.exe | head -n 1)"
RC64 = "$(shell $(LSQ) ${MSKITS}/*/bin/*/x64/rc.exe | head -n 1)"

D8 = "$(shell $(LSQ) ${HOME}/src/v8/v8/out/x64.release/d8)"

NODEJS = "$(shell $(LSQ) /usr/bin/nodejs)"

ifeq ("", $(NODEJS))
  $(error "ERROR: Missing nodejs. Run: sudo apt-get install nodejs")
endif

# Selectively enable windows if tools available
DEPLOYABLE := 1
ifneq ("", $(CL32))
  ifneq ("", $(RC32))
    TARGETS += win32_target
    TESTS += win32_tests
  else
    $(warning "WARNING: Missing Visual Studio rc.exe skipping 32-bit Windows.")
    DEPLOYABLE := 0
  endif
else
  $(warning "WARNING: Missing Visual Studio cl.exe skipping 32-bit Windows.")
  DEPLOYABLE := 0
endif
ifneq ("", $(CL64))
  ifneq ("", $(RC64))
    TARGETS += win64_target
    TESTS += win64_tests
  else
    $(warning "WARNING: Missing Visual Studio rc.exe skipping 64-bit Windows.")
    DEPLOYABLE := 0
  endif
else
  $(warning "WARNING: Missing Visual Studio cl.exe skipping 64-bit Windows.")
  DEPLOYABLE := 0
endif

# Decide if we can deploy.
DEPLOY_TARGETS =
ifeq (1, $(DEPLOYABLE))
  DEPLOY_TARGETS := $(DEPLOY)/app.yaml
else
  $(warning "WARNING: Missing some platforms skipping deployment build.")
endif

WEB_D8_TESTS =
# Decide if we have d8.
ifneq ("", $(D8))
  WEB_D8_TESTS += sanity_test_web
endif

all: targets tests $(DEPLOY_TARGETS)
fast: posix esp32_sim esp32

targets: $(TARGETS)
tests: $(TESTS)

n:
	mkdir -p out
	tools/configure.py >out/build.ninja
	ninja -C out/

clean-esp32:
	rm -rf $(ESP32)/esp32*_build $(ESP32)/esp32*_cache

vet:
	$(MAKE) clean
	$(MAKE)
	$(MAKE) esp32-build
	$(MAKE) esp32s2-build
	$(MAKE) esp32c3-build
	$(MAKE) clean-esp32
	$(MAKE) add-optional
	$(MAKE) esp32-build
	$(MAKE) clean
	$(MAKE)

.PHONY: clean
clean:
	rm -rf $(OUT)

# ---- TESTS ----

posix_tests: unit_tests_posix see_all_test_posix save_restore_test
win32_tests: unit_tests_win32
win64_tests: unit_tests_win64
web_tests: $(WEB_D8_TESTS)
esp32_tests:
esp32_sim_tests: unit_tests_esp32_sim see_all_test_esp32_sim sizes

# ---- UNIT TESTS ----

unit_tests_posix: $(POSIX)/ueforth common/all_tests.fs
	$^

unit_tests_esp32_sim: \
    $(ESP32_SIM)/Esp32forth-sim \
    common/ansi.fs \
    common/all_tests.fs
	echo "include $(word 2,$^) include $(word 3,$^) \n1 terminate" | $<

unit_tests_win32: $(WINDOWS)/uEf32.exe common/all_tests.fs
	wine $^

unit_tests_win64: $(WINDOWS)/uEf64.exe common/all_tests.fs
	wine $^

# ---- OTHER TESTS ----

see_all_test_posix: $(POSIX)/ueforth
	echo internals see-all bye | $< >/dev/null

see_all_test_esp32_sim: $(ESP32_SIM)/Esp32forth-sim
	echo internals see-all bye | $< >/dev/null

save_restore_test: $(POSIX)/ueforth
	echo ': square dup * ; save /tmp/save_restore_test.bin bye' | $< >/dev/null
	echo 'restore /tmp/save_restore_test.bin 4 square 16 - posix sysexit' | $< >/dev/null

sizes: $(ESP32_SIM)/Esp32forth-sim
	echo internals size-all bye | $< | tools/memuse.py >$(ESP32_SIM)/sizes.txt

sanity_test_web: $(WEB)/ueforth.js tools/check_web_sanity.py
	echo '120 3 + . cr bye' | $(D8) $< | ./tools/check_web_sanity.py

# ---- GENERATED ----

$(GEN):
	mkdir -p $@

$(GEN)/posix_boot.h: posix/posix_boot.fs | $(GEN)
	./tools/importation.py -i $< -o $@ \
    -I . -I $(GEN) --name boot --header cpp --depsout $@.dd \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)
-include $(GEN)/posix_boot.h.dd

$(GEN)/windows_boot_extra.h: windows/windows_boot_extra.fs | $(GEN)
	./tools/importation.py -i $< -o $@ \
    -I . -I $(GEN) --name boot_extra --header win --depsout $@.dd \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)
-include $(GEN)/windows_boot_extra.h.dd

$(GEN)/windows_boot.h: windows/windows_boot.fs | $(GEN)
	./tools/importation.py -i $< -o $@ \
    -I . -I $(GEN) --name boot --header win --depsout $@.dd \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)
-include $(GEN)/windows_boot.h.dd

$(GEN)/pico_ice_boot.h: pico-ice/pico_ice_boot.fs | $(GEN)
	./tools/importation.py -i $< -o $@ \
    -I . -I $(GEN) --name boot --header cpp --depsout $@.dd \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)
-include $(GEN)/pico_ice_boot.h.dd

$(GEN)/esp32_boot.h: esp32/esp32_boot.fs | $(GEN)
	./tools/importation.py -i $< -o $@ \
    -I . -I $(GEN) --name boot --header cpp --depsout $@.dd \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)
-include $(GEN)/esp32_boot.h.dd

$(GEN)/esp32_assembler.h: common/assembler.fs | $(GEN)
	./tools/importation.py -i $< -o $@ \
    -I . -I $(GEN) --name assembler_source --header cpp --depsout $@.dd \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)
-include $(GEN)/esp32_assembler.h.dd

$(GEN)/esp32_xtensa-assembler.h: esp32/optional/assemblers/xtensa-assembler.fs | $(GEN)
	./tools/importation.py -i $< -o $@ \
    -I . -I $(GEN) --name xtensa_assembler_source --header cpp --depsout $@.dd \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)
-include $(GEN)/esp32_xtensa-assembler.h.dd

$(GEN)/esp32_riscv-assembler.h: esp32/optional/assemblers/riscv-assembler.fs | $(GEN)
	./tools/importation.py -i $< -o $@ \
    -I . -I $(GEN) --name riscv_assembler_source --header cpp --depsout $@.dd \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)
-include $(GEN)/esp32_riscv-assembler.h.dd

$(GEN)/esp32_camera.h: esp32/optional/camera/camera_server.fs | $(GEN)
	./tools/importation.py -i $< -o $@ \
    -I . -I $(GEN) --name camera_source --header cpp --depsout $@.dd \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)
-include $(GEN)/esp32_camera.h.dd

$(GEN)/esp32_interrupts.h: esp32/optional/interrupts/timers.fs | $(GEN)
	./tools/importation.py -i $< -o $@ \
    -I . -I $(GEN) --name interrupts_source --header cpp --depsout $@.dd \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)
-include $(GEN)/esp32_interrupts.h.dd

$(GEN)/esp32_oled.h: esp32/optional/oled/oled.fs | $(GEN)
	./tools/importation.py -i $< -o $@ \
    -I . -I $(GEN) --name oled_source --header cpp --depsout $@.dd \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)
-include $(GEN)/esp32_oled.h.dd

$(GEN)/esp32_spi-flash.h: esp32/optional/spi-flash/spi-flash.fs | $(GEN)
	./tools/importation.py -i $< -o $@ \
    -I . -I $(GEN) --name spi_flash_source --header cpp --depsout $@.dd \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)
-include $(GEN)/esp32_spi-flash.h.dd

$(GEN)/esp32_serial-bluetooth.h: esp32/optional/serial-bluetooth/serial-bluetooth.fs | $(GEN)
	./tools/importation.py -i $< -o $@ \
    -I . -I $(GEN) --name serial_blueooth_source --header cpp --depsout $@.dd \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)
-include $(GEN)/esp32_serial-bluetooth.h.dd

OPTIONAL_MODULES = \
  $(ESP32)/ESP32forth/assemblers.h \
  $(ESP32)/ESP32forth/camera.h \
  $(ESP32)/ESP32forth/oled.h \
  $(ESP32)/ESP32forth/interrupts.h \
  $(ESP32)/ESP32forth/rmt.h \
  $(ESP32)/ESP32forth/serial-bluetooth.h \
  $(ESP32)/ESP32forth/spi-flash.h

add-optional: $(OPTIONAL_MODULES)

drop-optional:
	rm -f $(OPTIONAL_MODULES)

$(ESP32)/ESP32forth/%.h: $(ESP32)/ESP32forth/optional/%.h
	cp $< $@

$(GEN)/dump_web_opcodes: web/dump_web_opcodes.c | $(GEN)
	$(CXX) $(CFLAGS) $< -o $@ -MD -MF $@.dd
-include $(GEN)/dump_web_opcodes.dd

$(GEN)/web_cases.js: $(GEN)/dump_web_opcodes | $(GEN)
	$< cases >$@

$(GEN)/web_dict.js: $(GEN)/dump_web_opcodes | $(GEN)
	$< dict >$@

$(GEN)/web_sys.js: $(GEN)/dump_web_opcodes | $(GEN)
	$< sys >$@

$(GEN)/web_boot.js: web/web_boot.fs | $(GEN)
	./tools/importation.py -i $< -o $@ \
    -I . -I $(GEN) --name boot --header web --depsout $@.dd \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)
-include $(GEN)/web_boot.js.dd

# ---- RESOURCES ----

$(RES):
	mkdir -p $@

$(RES)/eforth16x16.png: images/eforth.png | $(RES)
	convert -resize 16x16 $< $@

$(RES)/eforth32x32.png: images/eforth.png | $(RES)
	convert -resize 32x32 $< $@

$(RES)/eforth48x48.png: images/eforth.png | $(RES)
	convert -resize 48x48 $< $@

$(RES)/eforth256x256.png: images/eforth.png | $(RES)
	convert -resize 256x256 $< $@

ICON_SIZES = $(RES)/eforth256x256.png \
             $(RES)/eforth48x48.png \
             $(RES)/eforth32x32.png \
             $(RES)/eforth16x16.png

$(RES)/eforth.ico: $(ICON_SIZES)
	convert $^ $< $@

$(RES)/ueforth_res32.res: windows/ueforth.rc $(RES)/eforth.ico
	$(RC32) /fo $@ $<

$(RES)/ueforth_res64.res: windows/ueforth.rc $(RES)/eforth.ico
	$(RC64) /fo $@ $<

# ---- WEB ----

web: web_target web_tests
web_target: \
    $(WEB)/terminal.html \
    $(WEB)/lazy_terminal.html \
    $(WEB)/script_lite_test.html \
    $(WEB)/script_test.html \
    $(WEB)/script_test.fs \
    $(WEB)/ueforth.js

$(WEB):
	mkdir -p $(WEB)

$(WEB)/terminal.html: web/terminal.html | $(WEB)
	cp $< $@

$(WEB)/lazy_terminal.html: web/lazy_terminal.html | $(WEB)
	cp $< $@

$(WEB)/script_lite_test.html: web/script_lite_test.html | $(WEB)
	cp $< $@

$(WEB)/script_test.html: web/script_test.html | $(WEB)
	cp $< $@

$(WEB)/script_test.fs: web/script_test.fs | $(WEB)
	cp $< $@

$(WEB)/ueforth.js: \
        web/fuse_web.js \
        web/web.template.js \
        $(GEN)/web_boot.js \
        $(GEN)/web_dict.js \
        $(GEN)/web_cases.js \
        $(GEN)/web_sys.js | $(WEB)
	$^ >$@

# ---- POSIX ----

posix: posix_target posix_tests
posix_target: $(POSIX)/ueforth

$(POSIX):
	mkdir -p $@

$(POSIX)/ueforth: \
    posix/main.c $(GEN)/posix_boot.h | $(POSIX)
	$(CXX) $(CFLAGS) $< -o $@ $(LIBS) -MD -MF $(GEN)/ueforth_posix.dd
	strip $(STRIP_ARGS) $@
-include $(GEN)/ueforth_posix.dd

# ---- WINDOWS ----

win32: win32_target win32_tests
win64: win64_target win64_tests
win32_target: $(WINDOWS)/uEf32.exe
win64_target: $(WINDOWS)/uEf64.exe

$(WINDOWS):
	mkdir -p $@

$(WINDOWS)/uEf32.obj: \
    windows/main.c \
    $(GEN)/windows_boot_extra.h \
    $(GEN)/windows_boot.h | $(WINDOWS)
	./tools/importation.py -i $< -o $@ --no-out -I . -I $(GEN) --depsout $@.dd
	$(CL32) /c /Fo$@ $(WIN_CFLAGS) $<
-include $(WINDOWS)/uEf32.obj.dd

$(WINDOWS)/uEf32.exe: \
    $(WINDOWS)/uEf32.obj \
    $(RES)/ueforth_res32.res | $(WINDOWS)
	$(LINK32) /OUT:$@ $(WIN_LFLAGS32) $^

$(WINDOWS)/uEf64.obj: \
    windows/main.c \
    $(GEN)/windows_boot_extra.h \
    $(GEN)/windows_boot.h | $(WINDOWS)
	./tools/importation.py -i $< -o $@ --no-out -I . -I $(GEN) --depsout $@.dd
	$(CL64) /c /Fo$@ $(WIN_CFLAGS) $<
-include $(WINDOWS)/uEf64.obj.dd

$(WINDOWS)/uEf64.exe: \
    $(WINDOWS)/uEf64.obj \
    $(RES)/ueforth_res64.res | $(WINDOWS)
	$(LINK64) /OUT:$@ $(WIN_LFLAGS64) $^

# ---- ESP32-SIM ----

esp32_sim: esp32_sim_target esp32_sim_tests
esp32_sim_target: $(ESP32_SIM)/Esp32forth-sim

$(ESP32_SIM):
	mkdir -p $@

$(GEN)/print-esp32-builtins: esp32/print-builtins.cpp | $(GEN)
	$(CXX) $(CFLAGS) $< -o $@ -MD -MF $@.dd
-include $(GEN)/print-esp32-builtins.dd

$(GEN)/esp32_sim_opcodes.h: $(GEN)/print-esp32-builtins | $(GEN)
	$< >$@

$(ESP32_SIM)/Esp32forth-sim: \
    esp32/sim_main.cpp \
    $(GEN)/esp32_boot.h \
    $(GEN)/esp32_sim_opcodes.h | $(ESP32_SIM)
	$(CXX) $(CFLAGS) $< -o $@ -MD -MF $(GEN)/esp32_sim.dd
	strip $(STRIP_ARGS) $@
-include $(GEN)/esp32_sim.dd

# ---- ESP32 ----

esp32: esp32_target esp32_sim esp32_tests esp32_sim_tests
esp32_target: $(ESP32)/ESP32forth.zip

$(ESP32)/ESP32forth:
	mkdir -p $@

$(ESP32)/ESP32forth/optional:
	mkdir -p $@

$(ESP32)/ESP32forth/ESP32forth.ino: \
    esp32/ESP32forth.ino \
    $(GEN)/esp32_boot.h | $(ESP32)/ESP32forth
	./tools/importation.py -i $< -o $@ \
    --keep-first-comment \
    -I . -I $(GEN) --depsout $(GEN)/esp32.dd \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)
-include $(GEN)/esp32.dd

$(ESP32)/ESP32forth/README.txt: esp32/README.txt | $(ESP32)/ESP32forth
	./tools/importation.py -i $< -o $@ \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)

$(ESP32)/ESP32forth/optional/README-optional.txt: \
    esp32/optional/README-optional.txt | $(ESP32)/ESP32forth/optional
	./tools/importation.py -i $< -o $@ \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)

$(ESP32)/ESP32forth/optional/assemblers.h: \
    esp32/optional/assemblers/assemblers.h \
    $(GEN)/esp32_assembler.h \
    $(GEN)/esp32_xtensa-assembler.h \
    $(GEN)/esp32_riscv-assembler.h | $(ESP32)/ESP32forth/optional
	./tools/importation.py -i $< -o $@ \
    --keep-first-comment \
    -I . -I $(GEN) --depsout $(GEN)/esp32_optional_assemblers.h.dd \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)
-include $(GEN)/esp32_optional_assemblers.h.dd

$(ESP32)/ESP32forth/optional/camera.h: \
    esp32/optional/camera/camera.h \
    $(GEN)/esp32_camera.h | $(ESP32)/ESP32forth/optional
	./tools/importation.py -i $< -o $@ \
    --keep-first-comment \
    -I . -I $(GEN) --depsout $(GEN)/esp32_optional_camera.h.dd \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)
-include $(GEN)/esp32_optional_camera.h.dd

$(ESP32)/ESP32forth/optional/interrupts.h: \
    esp32/optional/interrupts/interrupts.h \
    $(GEN)/esp32_interrupts.h | $(ESP32)/ESP32forth/optional
	./tools/importation.py -i $< -o $@ \
    --keep-first-comment \
    -I . -I $(GEN) --depsout $(GEN)/esp32_optional_interrupts.h.dd \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)
-include $(GEN)/esp32_optional_interrupts.h.dd

$(ESP32)/ESP32forth/optional/oled.h: \
    esp32/optional/oled/oled.h \
    $(GEN)/esp32_oled.h | $(ESP32)/ESP32forth/optional
	./tools/importation.py -i $< -o $@ \
    --keep-first-comment \
    -I . -I $(GEN) --depsout $(GEN)/esp32_optional_oled.h.dd \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)
-include $(GEN)/esp32_optional_oled.h.dd

$(ESP32)/ESP32forth/optional/rmt.h: \
    esp32/optional/rmt.h | $(ESP32)/ESP32forth/optional
	./tools/importation.py -i $< -o $@ \
    --keep-first-comment \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)

$(ESP32)/ESP32forth/optional/serial-bluetooth.h: \
    esp32/optional/serial-bluetooth/serial-bluetooth.h \
    $(GEN)/esp32_serial-bluetooth.h | $(ESP32)/ESP32forth/optional
	./tools/importation.py -i $< -o $@ \
    --keep-first-comment \
    -I . -I $(GEN) --depsout $(GEN)/esp32_optional_serial-bluetooth.h.dd \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)
-include $(GEN)/esp32_optional_serial-bluetooth.h.dd

$(ESP32)/ESP32forth/optional/spi-flash.h: \
    esp32/optional/spi-flash/spi-flash.h \
    $(GEN)/esp32_spi-flash.h | $(ESP32)/ESP32forth/optional
	./tools/importation.py -i $< -o $@ \
    --keep-first-comment \
    -I . -I $(GEN) --depsout $(GEN)/esp32_optional_spi-flash.h.dd \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)
-include $(GEN)/esp32_optional_spi-flash.h.dd

# ---- ESP32 ARDUINO BUILD AND FLASH ----

LOCALAPPDATAR=$(subst \,/,$(shell cmd.exe 2>/dev/null /c echo %LOCALAPPDATA%))
LOCALAPPDATA=$(subst C:/,/mnt/c/,${LOCALAPPDATAR})
ARDUINO_CLI="${LOCALAPPDATA}/Programs/arduino-ide/resources/app/lib/backend/resources/arduino-cli.exe"
WINTMP="${LOCALAPPDATA}/Temp"
WINTMP2="${LOCALAPPDATAR}/Temp"

ESP32_BOARD_esp32=--fqbn=esp32:esp32:esp32:PSRAM=disabled,PartitionScheme=no_ota,CPUFreq=240,FlashMode=qio,FlashFreq=80,FlashSize=4M,UploadSpeed=921600,LoopCore=1,EventsCore=1,DebugLevel=none,EraseFlash=none

ESP32_BOARD_esp32s2=--fqbn=esp32:esp32:esp32s2:CDCOnBoot=default,MSCOnBoot=default,DFUOnBoot=default,UploadMode=default,PSRAM=disabled,PartitionScheme=default,CPUFreq=240,FlashMode=qio,FlashFreq=80,FlashSize=4M,UploadSpeed=921600,DebugLevel=none,EraseFlash=none

ESP32_BOARD_esp32s3=--fqbn=esp32:esp32:esp32s3:PSRAM=disabled,FlashMode=qio,FlashSize=4M,LoopCore=1,EventsCore=1,USBMode=hwcdc,CDCOnBoot=default,MSCOnBoot=default,DFUOnBoot=default,UploadMode=default,PartitionScheme=default,CPUFreq=240,UploadSpeed=921600,DebugLevel=none,EraseFlash=none

ESP32_BOARD_esp32c3=--fqbn=esp32:esp32:esp32c3:CDCOnBoot=default,PartitionScheme=default,CPUFreq=160,FlashMode=qio,FlashFreq=80,FlashSize=4M,UploadSpeed=921600,DebugLevel=none,EraseFlash=none

ESP32_BOARD_esp32cam=--fqbn=esp32:esp32:esp32cam:CPUFreq=240,FlashMode=qio,PartitionScheme=huge_app,FlashFreq=80,DebugLevel=none,EraseFlash=none

$(ESP32)/%_build:
	mkdir -p $@

$(ESP32)/%_cache:
	mkdir -p $@

$(ESP32)/%_build/ESP32forth.ino.bin: $(ESP32)/ESP32forth/ESP32forth.ino | \
    $(ESP32)/%_build $(ESP32)/%_cache
	mkdir -p ${WINTMP}/ueforth_esp32/
	rm -rf ${WINTMP}/ueforth_esp32/ESP32forth/
	cp -r $(ESP32)/ESP32forth ${WINTMP}/ueforth_esp32/
	cd ${WINTMP} && ${ARDUINO_CLI} compile \
    ${ESP32_BOARD_$(subst _build,,$(notdir $(word 1,$|)))} \
    --build-path $(word 1,$|) \
    --build-cache-path $(word 2,$|) \
    ${WINTMP2}/ueforth_esp32/ESP32forth/ESP32forth.ino

.PRECIOUS: $(ESP32)/%_build
.PRECIOUS: $(ESP32)/%_cache

.PHONY: esp32all
esp32all: \
  $(ESP32)/esp32_build/ESP32forth.ino.bin \
  $(ESP32)/esp32s2_build/ESP32forth.ino.bin \
  $(ESP32)/esp32s3_build/ESP32forth.ino.bin \
  $(ESP32)/esp32c3_build/ESP32forth.ino.bin \
  $(ESP32)/esp32cam_build/ESP32forth.ino.bin

PORT?=COM3

.PHONY: putty
putty:
	${HOME}/Desktop/putty.exe -serial ${PORT} -sercfg 115200 &

.PHONY: %-flash
%-flash: $(ESP32)/%_build/ESP32forth.ino.bin | \
    $(ESP32)/%_build $(ESP32)/%_cache
	${ARDUINO_CLI} compile \
	  --port $(PORT) \
	  --upload \
    ${ESP32_BOARD_$(subst -flash,,$(notdir $(word 1,$@)))} \
    --build-path $(word 1,$|) \
    --build-cache-path $(word 2,$|) \
    $(ESP32)/ESP32forth/ESP32forth.ino

.PHONY: %-build
%-build: $(ESP32)/%_build/ESP32forth.ino.bin
	echo "done"

# ---- PICO-ICE ----

pico-ice: pico_ice_target
pico_ice_target: $(PICO_ICE)/ueforth-pico-ice.zip

$(PICO_ICE)/ueforth-pico-ice:
	mkdir -p $@

$(PICO_ICE)/ueforth-pico-ice/ueforth-pico-ice.uf2: \
    $(PICO_ICE)/ueforth_pico_ice.uf2 | $(PICO_ICE)/ueforth-pico-ice
	cp $< $@

.FORCE:
$(PICO_ICE)/ueforth_pico_ice.uf2: \
    .FORCE \
    $(GEN)/pico_ice_boot.h \
    $(PICO_ICE)/build.ninja
	ninja -C $(PICO_ICE) ueforth_pico_ice

$(PICO_ICE)/build.ninja: \
    pico-ice/pico-sdk/README.md \
    pico-ice/pico-ice-sdk/README.md \
    pico-ice/pico-sdk/lib/tinyusb/README.rst
	cmake $(PICO_ICE) -G Ninja -S pico-ice -B $(PICO_ICE)

pico-ice/pico-sdk/README.md:
	git submodule update --init pico-ice/pico-sdk

pico-ice/pico-sdk/lib/tinyusb/README.rst: pico-ice/pico-sdk/README.md
	cd pico-ice/pico-sdk && git submodule update --init lib/tinyusb

pico-ice/pico-ice-sdk/README.md:
	git submodule update --init pico-ice/pico-ice-sdk

$(PICO_ICE)/ueforth-pico-ice/README.txt: pico-ice/README.txt | $(PICO_ICE)/ueforth-pico-ice
	./tools/importation.py -i $< -o $@ \
    -DVERSION=$(VERSION) -DREVISION=$(REVISION)

$(PICO_ICE)/ueforth-pico-ice/LICENSE: LICENSE
	cp $< $@

$(PICO_ICE)/ueforth-pico-ice/pico-ice-sdk-LICENSE.md: pico-ice/pico-ice-sdk/LICENSE.md
	cp $< $@

$(PICO_ICE)/ueforth-pico-ice/pico-sdk-LICENSE.TXT: pico-ice/pico-sdk/LICENSE.TXT
	cp $< $@

# ---- PICO-ICE-SIM ----

pico-ice-sim: pico_ice_sim_target
pico_ice_sim_target: $(PICO_ICE_SIM)/ueforth_pico_ice_sim

$(PICO_ICE_SIM):
	mkdir -p $@

$(PICO_ICE_SIM)/ueforth_pico_ice_sim: \
    pico-ice/main.c $(GEN)/pico_ice_boot.h | $(PICO_ICE_SIM) $(GEN)
	$(CXX) $(CFLAGS) -DUEFORTH_SIM=1 $< -o $@ -MD -MF $(GEN)/pico_ice_sim.dd
-include $(GEN)/pico_ice_sim.dd

# ---- PACKAGE ESP32 ----

$(ESP32)/ESP32forth.zip: \
    $(ESP32)/ESP32forth/ESP32forth.ino \
    $(ESP32)/ESP32forth/README.txt \
    $(ESP32)/ESP32forth/optional/README-optional.txt \
    $(ESP32)/ESP32forth/optional/assemblers.h \
    $(ESP32)/ESP32forth/optional/camera.h \
    $(ESP32)/ESP32forth/optional/oled.h \
    $(ESP32)/ESP32forth/optional/interrupts.h \
    $(ESP32)/ESP32forth/optional/rmt.h \
    $(ESP32)/ESP32forth/optional/serial-bluetooth.h \
    $(ESP32)/ESP32forth/optional/spi-flash.h
	cd $(ESP32) && rm -f ESP32forth.zip && zip -r ESP32forth.zip ESP32forth

# ---- PACKAGE pico-ice ----

$(PICO_ICE)/ueforth-pico-ice.zip: \
    $(PICO_ICE)/ueforth-pico-ice/ueforth-pico-ice.uf2 \
    $(PICO_ICE)/ueforth-pico-ice/README.txt \
    $(PICO_ICE)/ueforth-pico-ice/LICENSE \
    $(PICO_ICE)/ueforth-pico-ice/pico-ice-sdk-LICENSE.md \
    $(PICO_ICE)/ueforth-pico-ice/pico-sdk-LICENSE.TXT
	cd $(PICO_ICE) && rm -f ueforth-pico-ice.zip && zip -r ueforth-pico-ice.zip ueforth-pico-ice

# ---- Publish to Archive ----

ARCHIVE=gs://eforth/releases
GSUTIL=CLOUDSDK_CORE_PROJECT=eforth gsutil
GSUTIL_CP=$(GSUTIL) \
          -h "Cache-Control:public, max-age=60" \
          cp -a public-read

publish-esp32: $(ESP32)/ESP32forth.zip
	$(GSUTIL_CP) \
    $(ESP32)/ESP32forth.zip \
    $(ARCHIVE)/ESP32forth-$(VERSION)-$(REVSHORT).zip
	$(GSUTIL_CP) \
    $(ESP32)/ESP32forth.zip \
    $(ARCHIVE)/ESP32forth-$(VERSION).zip

publish-pico-ice: $(PICO_ICE)/ueforth-pico-ice.zip
	$(GSUTIL_CP) \
    $(PICO_ICE)/ueforth-pico-ice.zip \
    $(ARCHIVE)/ueforth-pico-ice-$(VERSION)-$(REVSHORT).zip
	$(GSUTIL_CP) \
    $(PICO_ICE)/ueforth-pico-ice.zip \
    $(ARCHIVE)/ueforth-pico-ice-$(VERSION).zip

publish-linux: $(POSIX)/ueforth
	$(GSUTIL_CP) \
    $(POSIX)/ueforth \
    $(ARCHIVE)/ueforth-$(VERSION)-$(REVSHORT).linux
	$(GSUTIL_CP) \
    $(POSIX)/ueforth \
    $(ARCHIVE)/ueforth-$(VERSION).linux

publish-web: $(WEB)/ueforth.js
	$(GSUTIL_CP) \
    $(WEB)/ueforth.js \
    $(ARCHIVE)/ueforth-$(VERSION)-$(REVSHORT).js
	$(GSUTIL_CP) \
    $(WEB)/ueforth.js \
    $(ARCHIVE)/ueforth-$(VERSION).js

publish-windows: $(WINDOWS)/uEf32.exe $(WINDOWS)/uEf64.exe
	$(GSUTIL_CP) \
    $(WINDOWS)/uEf32.exe \
    $(ARCHIVE)/uEf32-$(VERSION)-$(REVSHORT).exe
	$(GSUTIL_CP) \
    $(WINDOWS)/uEf32.exe \
    $(ARCHIVE)/uEf32-$(VERSION).exe
	$(GSUTIL_CP) \
    $(WINDOWS)/uEf64.exe \
    $(ARCHIVE)/uEf64-$(VERSION)-$(REVSHORT).exe
	$(GSUTIL_CP) \
    $(WINDOWS)/uEf64.exe \
    $(ARCHIVE)/uEf64-$(VERSION).exe

publish-index: | $(GEN)
	$(GSUTIL) ls -l gs://eforth/releases | tools/webindex.py >$(GEN)/archive.html
	$(GSUTIL_CP) \
    $(GEN)/archive.html \
    gs://eforth/releases/archive.html

publish: publish-esp32 publish-pico-ice publish-linux publish-web publish-windows publish-index

# ---- DEPLOY ----

$(DEPLOY):
	mkdir -p $@

REPLACE = ./tools/importation.py -I site \
          -DVERSION=${VERSION} \
          -DSTABLE_VERSION=${STABLE_VERSION} \
          -DOLD_STABLE_VERSION=${OLD_STABLE_VERSION}
UE_REPLACE = $(REPLACE) -DFORTH=uEForth
ESP_REPLACE = $(REPLACE) -DFORTH=ESP32forth

$(DEPLOY)/app.yaml: $(RES)/eforth.ico \
                    $(wildcard site/*.html) \
                    site/static/eforth.css \
                    site/app.yaml \
                    site/eforth.go \
                    $(TARGETS) | $(DEPLOY)
	rm -rf $(DEPLOY)/
	mkdir -p $(DEPLOY)
	cp -r site/static $(DEPLOY)/static
	cp $(RES)/eforth.ico $(DEPLOY)/static/favicon.ico
	cp site/*.go $(DEPLOY)/
	cp site/*.yaml $(DEPLOY)/
	cp site/.gcloudignore $(DEPLOY)
	cp out/web/ueforth.js $(DEPLOY)/
	$(ESP_REPLACE) -i site/web.html -o $(DEPLOY)/web.html
	$(ESP_REPLACE) -i site/ESP32forth.html -o $(DEPLOY)/ESP32forth.html
	$(UE_REPLACE) -i site/pico-ice.html -o $(DEPLOY)/pico-ice.html
	$(UE_REPLACE) -i site/index.html -o $(DEPLOY)/index.html
	$(UE_REPLACE) -i site/linux.html -o $(DEPLOY)/linux.html
	$(UE_REPLACE) -i site/windows.html -o $(DEPLOY)/windows.html
	$(UE_REPLACE) -i site/internals.html -o $(DEPLOY)/internals.html
	$(UE_REPLACE) -i site/classic.html -o $(DEPLOY)/classic.html

deploy: all
	cd out/deploy && gcloud app deploy -q --project esp32forth *.yaml
	cd out/deploy && gcloud app deploy -q --project eforth *.yaml

d8: web
	${HOME}/src/v8/v8/out/x64.release/d8 out/web/ueforth.js

# ---- INSTALL ----

install: $(POSIX)/ueforth
	sudo cp $< /usr/bin/ueforth

win-install: $(WINDOWS)/uEf32.exe $(WINDOWS)/uEf64.exe
	cp $^ ~/Desktop/
