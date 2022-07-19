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

VERSION=7.0.7.0
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
DEPLOY = $(OUT)/deploy

CFLAGS_COMMON = -O2 -I ./ -I $(OUT)

CFLAGS_MINIMIZE = \
                -s \
                -DUEFORTH_MINIMAL \
                -fno-exceptions \
                -ffreestanding \
                -fno-stack-protector \
                -fomit-frame-pointer \
                -mno-stack-arg-probe \
                -fno-ident -Wl,--build-id=none \
                -ffunction-sections -fdata-sections \
                -fmerge-all-constants
CFLAGS = $(CFLAGS_COMMON) \
         $(CFLAGS_MINIMIZE) \
         -std=c++11 \
         -Wall \
         -Werror \
         -no-pie \
         -Wl,--gc-sections
STRIP_ARGS = -S \
             --strip-unneeded \
             --remove-section=.note.gnu.gold-version \
             --remove-section=.comment \
             --remove-section=.note \
             --remove-section=.note.gnu.build-id \
             --remove-section=.note.ABI-tag
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
          esp32_sim_target
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

# Selectively enable windows if tools available
DEPLOYABLE := 1
ifneq ("", $(CL32))
  ifneq ("", $(RC32))
    TARGETS += win32_target
    TESTS += win32_tests
  else
    $(warning "Missing Visual Studio rc.exe skipping 32-bit Windows.")
    DEPLOYABLE := 0
  endif
else
  $(warning "Missing Visual Studio cl.exe skipping 32-bit Windows.")
  DEPLOYABLE := 0
endif
ifneq ("", $(CL64))
  ifneq ("", $(RC64))
    TARGETS += win64_target
    TESTS += win64_tests
  else
    $(warning "Missing Visual Studio rc.exe skipping 64-bit Windows.")
    DEPLOYABLE := 0
  endif
else
  $(warning "Missing Visual Studio cl.exe skipping 64-bit Windows.")
  DEPLOYABLE := 0
endif

# Decide if we can deploy.
DEPLOY_TARGETS =
ifeq (1, $(DEPLOYABLE))
  DEPLOY_TARGETS := $(DEPLOY)/app.yaml
else
  $(warning "Missing some platforms skipping deployment build.")
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

sanity_test_web: $(WEB)/ueforth.js
	echo '120 3 + . cr bye' | $(D8) $< | tools/check_web_sanity.js

# ---- GENERATED ----

$(GEN):
	mkdir -p $@

COMMON_PHASE1 = common/comments.fs \
                common/boot.fs \
                common/io.fs \
                common/conditionals.fs \
                common/vocabulary.fs \
                common/floats.fs \
                common/structures.fs

COMMON_PHASE1e = common/comments.fs \
                                    common/tier2a_forth.fs \
                 common/boot.fs \
                                    common/tier2b_forth.fs \
                 common/io.fs \
                 common/conditionals.fs \
                 common/vocabulary.fs \
                 common/floats.fs \
                 common/structures.fs

COMMON_PHASE2 = common/utils.fs common/locals.fs

COMMON_FILETOOLS = common/tasks.fs common/streams.fs \
                   common/filetools.fs common/including.fs \
                   common/blocks.fs

COMMON_DESKTOP = common/ansi.fs common/desktop.fs \
                 common/graphics.fs common/graphics_utils.fs common/heart.fs

POSIX_BOOT =  $(COMMON_PHASE1) \
              posix/posix.fs posix/allocation.fs posix/termios.fs \
              $(COMMON_PHASE2) $(COMMON_FILETOOLS) $(COMMON_DESKTOP) \
              posix/x11.fs \
              posix/graphics.fs \
              posix/sockets.fs posix/telnetd.fs posix/httpd.fs posix/web_interface.fs \
              posix/autoboot.fs \
              common/fini.fs
$(GEN)/posix_boot.h: tools/source_to_string.js $(POSIX_BOOT) | $(GEN)
	$< boot $(VERSION) $(REVISION) $(POSIX_BOOT) >$@

WINDOWS_BOOT = $(COMMON_PHASE1) \
               windows/windows_core.fs \
               windows/windows_files.fs \
               windows/windows_console.fs \
               windows/windows_user.fs \
               windows/windows_gdi.fs \
               windows/windows_messages.fs \
               windows/allocation.fs \
               $(COMMON_PHASE2) $(COMMON_FILETOOLS) $(COMMON_DESKTOP) \
               windows/graphics.fs \
               posix/autoboot.fs \
               common/fini.fs
$(GEN)/windows_boot.h: tools/source_to_string.js $(WINDOWS_BOOT) | $(GEN)
	$< -win boot $(VERSION) $(REVISION) $(WINDOWS_BOOT) >$@

ESP32_BOOT = $(COMMON_PHASE1) \
             esp32/allocation.fs \
             $(COMMON_PHASE2) $(COMMON_FILETOOLS) \
             esp32/bindings.fs esp32/platform.fs \
             posix/httpd.fs posix/web_interface.fs esp32/web_interface.fs \
             esp32/registers.fs esp32/timers.fs \
             esp32/bterm.fs posix/telnetd.fs \
             esp32/camera.fs esp32/camera_server.fs \
             esp32/autoboot.fs common/fini.fs
$(GEN)/esp32_boot.h: tools/source_to_string.js $(ESP32_BOOT) | $(GEN)
	$< boot $(VERSION) $(REVISION) $(ESP32_BOOT) >$@

$(GEN)/dump_web_opcodes: \
    web/dump_web_opcodes.c \
    common/tier0_opcodes.h \
    common/tier1_opcodes.h \
    common/bits.h \
    common/floats.h | $(GEN)
	$(CXX) $(CFLAGS) $< -o $@

$(GEN)/web_cases.js: $(GEN)/dump_web_opcodes | $(GEN)
	$< cases >$@

$(GEN)/web_dict.js: $(GEN)/dump_web_opcodes | $(GEN)
	$< dict >$@

$(GEN)/web_sys.js: $(GEN)/dump_web_opcodes | $(GEN)
	$< sys >$@

WEB_BOOT =  $(COMMON_PHASE1e) \
            web/platform.fs \
            $(COMMON_PHASE2) \
            web/fini.fs
$(GEN)/web_boot.js: tools/source_to_string.js $(WEB_BOOT) | $(GEN)
	$< -web boot $(VERSION) $(REVISION) $(WEB_BOOT) >$@

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
web_target: $(WEB)/terminal.html $(WEB)/ueforth.js

$(WEB):
	mkdir -p $(WEB)

$(WEB)/terminal.html: web/terminal.html | $(WEB)
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
    posix/main.c \
    common/tier0_opcodes.h \
    common/tier1_opcodes.h \
    common/tier2_opcodes.h \
    common/calls.h \
    common/calling.h \
    common/floats.h \
    common/interp.h \
    common/bits.h \
    common/core.h \
    $(GEN)/posix_boot.h | $(POSIX)
	$(CXX) $(CFLAGS) $< -o $@ $(LIBS)
	strip $(STRIP_ARGS) $@

# ---- WINDOWS ----

win32: win32_target win32_tests
win64: win64_target win64_tests
win32_target: $(WINDOWS)/uEf32.exe
win64_target: $(WINDOWS)/uEf64.exe

$(WINDOWS):
	mkdir -p $@

$(WINDOWS)/uEf32.obj: \
    windows/main.c \
    common/tier0_opcodes.h \
    common/tier1_opcodes.h \
    common/tier2_opcodes.h \
    common/calls.h \
    common/calling.h \
    common/floats.h \
    common/bits.h \
    common/core.h \
    windows/interp.h \
    $(GEN)/windows_boot.h | $(WINDOWS)
	$(CL32) /c /Fo$@ $(WIN_CFLAGS) $<

$(WINDOWS)/uEf32.exe: \
    $(WINDOWS)/uEf32.obj \
    $(RES)/ueforth_res32.res | $(WINDOWS)
	$(LINK32) /OUT:$@ $(WIN_LFLAGS32) $^

$(WINDOWS)/uEf64.obj: \
    windows/main.c \
    common/tier0_opcodes.h \
    common/tier1_opcodes.h \
    common/tier2_opcodes.h \
    common/calls.h \
    common/calling.h \
    common/floats.h \
    common/bits.h \
    common/core.h \
    windows/interp.h \
    $(GEN)/windows_boot.h | $(WINDOWS)
	$(CL64) /c /Fo$@ $(WIN_CFLAGS) $<

$(WINDOWS)/uEf64.exe: \
    $(WINDOWS)/uEf64.obj \
    $(RES)/ueforth_res64.res | $(WINDOWS)
	$(LINK64) /OUT:$@ $(WIN_LFLAGS64) $^

# ---- ESP32-SIM ----

esp32_sim: esp32_sim_target esp32_sim_tests
esp32_sim_target: $(ESP32_SIM)/Esp32forth-sim

$(ESP32_SIM):
	mkdir -p $@

$(GEN)/print-esp32-builtins: \
    esp32/print-builtins.cpp esp32/builtins.h | $(GEN)
	$(CXX) $(CFLAGS) $< -o $@

$(GEN)/esp32_sim_opcodes.h: $(GEN)/print-esp32-builtins | $(GEN)
	$< >$@

$(ESP32_SIM)/Esp32forth-sim: \
    esp32/sim_main.cpp \
    esp32/main.cpp \
    common/tier0_opcodes.h \
    common/tier1_opcodes.h \
    common/tier2_opcodes.h \
    common/floats.h \
    common/calling.h \
    common/floats.h \
    common/bits.h \
    common/core.h \
    common/interp.h \
    $(GEN)/esp32_boot.h \
    $(GEN)/esp32_sim_opcodes.h | $(ESP32_SIM)
	$(CXX) $(CFLAGS) $< -o $@
	strip $(STRIP_ARGS) $@

# ---- ESP32 ----

esp32: esp32_target esp32_sim esp32_tests esp32_sim_tests
esp32_target: $(ESP32)/ESP32forth/ESP32forth.ino

$(ESP32)/ESP32forth:
	mkdir -p $@

ESP32_PARTS = tools/replace.js \
              esp32/template.ino \
              common/tier0_opcodes.h \
              common/tier1_opcodes.h \
              common/tier2_opcodes.h \
              common/floats.h \
              common/calling.h \
              common/bits.h \
              common/core.h \
              common/interp.h \
              esp32/options.h \
              esp32/builtins.h \
              esp32/builtins.cpp \
              esp32/main.cpp \
              $(GEN)/esp32_boot.h

$(ESP32)/ESP32forth/ESP32forth.ino: $(ESP32_PARTS) | $(ESP32)/ESP32forth
	cat esp32/template.ino | tools/replace.js \
     VERSION=$(VERSION) \
     REVISION=$(REVISION) \
     tier0_opcodes=@common/tier0_opcodes.h \
     tier1_opcodes=@common/tier1_opcodes.h \
     tier2_opcodes=@common/tier2_opcodes.h \
     calling=@common/calling.h \
     floats=@common/floats.h \
     bits=@common/bits.h \
     core=@common/core.h \
     interp=@common/interp.h \
     options=@esp32/options.h \
     builtins.h=@esp32/builtins.h \
     builtins.cpp=@esp32/builtins.cpp \
     main.cpp=@esp32/main.cpp \
     boot=@$(GEN)/esp32_boot.h \
     >$@

# ---- PACKAGE ----

$(ESP32)/ESP32forth.zip: $(ESP32)/ESP32forth/ESP32forth.ino
	cd $(ESP32) && rm -f ESP32forth.zip && zip -r ESP32forth.zip ESP32forth

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

publish: publish-esp32 publish-linux publish-web publish-windows publish-index

# ---- DEPLOY ----

$(DEPLOY):
	mkdir -p $@

REPLACE = tools/replace.js \
          COMMON=@site/common.html \
          POSIX_COMMON=@site/posix_common.html \
          DESKTOP_COMMON=@site/desktop_common.html \
          MENU=@site/menu.html \
          VERSION=${VERSION} \
          STABLE_VERSION=${STABLE_VERSION} \
          OLD_STABLE_VERSION=${OLD_STABLE_VERSION}
UE_REPLACE = $(REPLACE) FORTH=uEForth
ESP_REPLACE = $(REPLACE) FORTH=ESP32forth

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
	cat site/web.html | $(ESP_REPLACE) >$(DEPLOY)/web.html
	cat site/ESP32forth.html | $(ESP_REPLACE) >$(DEPLOY)/ESP32forth.html
	cat site/index.html | $(UE_REPLACE) >$(DEPLOY)/index.html
	cat site/linux.html | $(UE_REPLACE) >$(DEPLOY)/linux.html
	cat site/windows.html | $(UE_REPLACE) >$(DEPLOY)/windows.html
	cat site/internals.html | $(UE_REPLACE) >$(DEPLOY)/internals.html
	cat site/classic.html | $(UE_REPLACE) >$(DEPLOY)/classic.html

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
