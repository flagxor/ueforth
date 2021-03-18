VERSION=7.0.3
REVISION=$(shell git rev-parse HEAD)

OUT = out
GEN = $(OUT)/gen
RES = $(OUT)/resources
WEB = $(OUT)/web
POSIX = $(OUT)/posix
WINDOWS = $(OUT)/windows
ARDUINO = $(OUT)/arduino
DEPLOY = $(OUT)/deploy

ifneq ($(OS),Windows_NT)
  OS = $(shell uname -s)
endif

CFLAGS_COMMON = -O2 -I ./ -I $(OUT)

CFLAGS_MINIMIZE = \
                -s \
                -DUEFORTH_MINIMAL \
                -fno-exceptions \
                -ffreestanding \
                -fno-stack-check \
                -fno-stack-protector \
                -fno-stack-protector \
                -fomit-frame-pointer \
                -mno-stack-arg-probe \
                -fno-ident -Wl,--build-id=none \
                -ffunction-sections -fdata-sections \
                -fmerge-all-constants
CFLAGS = $(CFLAGS_COMMON) \
         $(CFLAGS_MINIMIZE) \
         -Wall \
         -Werror \
         -no-pie

ifeq ($(OS),Darwin)
  CFLAGS += -Wl,-dead_strip
else
  CFLAGS += -Wl,--gc-sections
endif

STRIP_ARGS = -S

ifeq ($(OS),Darwin)
 STRIP_ARGS += -x
else
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
               /LIBPATH:"c:/Program Files (x86)/Windows Kits/10/Lib/10.0.19041.0/ucrt/x86"

WIN_LFLAGS64 = /LIBPATH:"c:/Program Files (x86)/Microsoft SDKs/Windows/v7.1A/Lib/x64" \
               /LIBPATH:"c:/Program Files (x86)/Microsoft Visual Studio/2019/Community/VC/Tools/MSVC/14.28.29333/lib/x64" \
               /LIBPATH:"c:/Program Files (x86)/Windows Kits/10/Lib/10.0.19041.0/ucrt/x64" \

TARGETS = $(WEB)/terminal.html \
          $(WEB)/ueforth.js \
          $(POSIX)/ueforth \
          $(ARDUINO)/ESP32forth/ESP32forth.ino

FINDQ = find 2>/dev/null

PROGFILES = /mnt/c/Program Files (x86)
CL32 = "$(shell $(FINDQ) "${PROGFILES}/Microsoft Visual Studio" -name cl.exe | grep /Hostx86/x86/ | head -n 1)"
CL64 = "$(shell $(FINDQ) "${PROGFILES}/Microsoft Visual Studio" -name cl.exe | grep /Hostx86/x64/ | head -n 1)"
LINK32 = "$(shell $(FINDQ) "${PROGFILES}/Microsoft Visual Studio" -name link.exe | grep /Hostx86/x86/ | head -n 1)"
LINK64 = "$(shell $(FINDQ) "${PROGFILES}/Microsoft Visual Studio" -name link.exe | grep /Hostx86/x64/ | head -n 1)"
RC32 = "$(shell $(FINDQ) "${PROGFILES}/Windows Kits" -name rc.exe | grep /x86/ | head -n 1)"
RC64 = "$(shell $(FINDQ) "${PROGFILES}/Windows Kits" -name rc.exe | grep /x64/ | head -n 1)"

# Selectively enable windows if tools available
DEPLOYABLE := 1
ifneq ("", $(CL32))
  ifneq ("", $(RC32))
    TARGETS += $(WINDOWS)/uEf32.exe
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
    TARGETS += $(WINDOWS)/uEf64.exe
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

all: $(TARGETS) tests $(DEPLOY_TARGETS)

clean:
	rm -rf $(OUT)

# ---- TESTS ----

tests: unit_tests see_all_test

unit_tests: $(POSIX)/ueforth common/all_tests.fs
	$^

see_all_test: $(POSIX)/ueforth
	echo "internals see-all bye" | $< >/dev/null

# ---- GENERATED ----

$(GEN):
	mkdir -p $@

POSIX_BOOT =  common/boot.fs common/conditionals.fs common/vocabulary.fs \
              common/hide_calls.fs common/ansi.fs \
              posix/posix.fs posix/posix_highlevel.fs posix/termios.fs common/locals.fs \
              common/utils.fs common/highlevel.fs common/filetools.fs posix/posix_desktop.fs \
              common/tasks.fs common/streams.fs common/blocks.fs posix/autoboot.fs \
              common/fini.fs
$(GEN)/posix_boot.h: common/source_to_string.js $(POSIX_BOOT) | $(GEN)
	echo "ok" | cat $(POSIX_BOOT) - | $< boot $(VERSION) $(REVISION) >$@

WINDOWS_BOOT = common/boot.fs common/conditionals.fs common/vocabulary.fs \
               common/hide_calls.fs common/ansi.fs \
               windows/windows.fs windows/windows_highlevel.fs common/highlevel.fs \
               common/utils.fs common/filetools.fs common/tasks.fs common/streams.fs \
               common/blocks.fs common/locals.fs \
               common/fini.fs
$(GEN)/windows_boot.h: common/source_to_string.js $(WINDOWS_BOOT) | $(GEN)
	echo "ok" | cat $(WINDOWS_BOOT) - | $< boot $(VERSION) $(REVISION) >$@

ARDUINO_BOOT = common/boot.fs common/conditionals.fs common/vocabulary.fs \
               arduino/arduino.fs arduino/arduino_highlevel.fs \
               arduino/bindings.fs common/highlevel.fs \
               common/filetools.fs common/utils.fs common/locals.fs \
               common/tasks.fs common/streams.fs arduino/arduino_server.fs \
               arduino/arduino_bterm.fs arduino/telnetd.fs \
               arduino/esp_camera.fs common/blocks.fs \
               arduino/autoboot.fs \
               common/fini.fs
$(GEN)/arduino_boot.h: common/source_to_string.js $(ARDUINO_BOOT) | $(GEN)
	echo "ok" | cat $(ARDUINO_BOOT) - | $< boot $(VERSION) $(REVISION) >$@

$(GEN)/dump_web_opcodes: web/dump_web_opcodes.c common/opcodes.h | $(GEN)
	$(CC) $(CFLAGS) $< -o $@

$(GEN)/web_cases.js: $(GEN)/dump_web_opcodes | $(GEN)
	$< cases >$@

$(GEN)/web_dict.js: $(GEN)/dump_web_opcodes | $(GEN)
	$< dict >$@

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

$(WEB):
	mkdir -p $(WEB)

$(WEB)/terminal.html: web/terminal.html | $(WEB)
	cp $< $@

$(WEB)/ueforth.js: \
        web/fuse_web.js \
        web/web.template.js \
        common/boot.fs \
        $(GEN)/web_dict.js \
        $(GEN)/web_cases.js | $(WEB)
	$^ >$@

# ---- POSIX ----

$(POSIX):
	mkdir -p $@

$(POSIX)/ueforth: \
    posix/posix_main.c \
    common/opcodes.h \
    common/interp.h \
    common/core.h \
    $(GEN)/posix_boot.h | $(POSIX)
	$(CC) $(CFLAGS) $< -o $@ $(LIBS)
	strip $(STRIP_ARGS) $@

# ---- WINDOWS ----

$(WINDOWS):
	mkdir -p $@

$(WINDOWS)/uEf32.obj: \
    windows/windows_main.c \
    common/opcodes.h \
    common/core.h \
    windows/windows_interp.h \
    $(GEN)/windows_boot.h | $(WINDOWS)
	$(CL32) /c /Fo$@ $(WIN_CFLAGS) $<

$(WINDOWS)/uEf32.exe: \
    $(WINDOWS)/uEf32.obj \
    $(RES)/ueforth_res32.res | $(WINDOWS)
	$(LINK32) /OUT:$@ $(WIN_LFLAGS32) $^

$(WINDOWS)/uEf64.obj: \
    windows/windows_main.c \
    common/opcodes.h \
    common/core.h \
    windows/windows_interp.h \
    $(GEN)/windows_boot.h | $(WINDOWS)
	$(CL64) /c /Fo$@ $(WIN_CFLAGS) $<

$(WINDOWS)/uEf64.exe: \
    $(WINDOWS)/uEf64.obj \
    $(RES)/ueforth_res64.res | $(WINDOWS)
	$(LINK64) /OUT:$@ $(WIN_LFLAGS64) $^

# ---- ARDUINO ----

$(ARDUINO)/ESP32forth:
	mkdir -p $@

ARDUINO_PARTS = arduino/arduino.template.ino \
                common/opcodes.h \
                common/calling.h \
                common/core.h \
                common/interp.h \
                $(GEN)/arduino_boot.h

$(ARDUINO)/ESP32forth/ESP32forth.ino: \
    arduino/fuse_ino.js $(ARDUINO_PARTS) | $(ARDUINO)/ESP32forth
	$< $(VERSION) $(REVISION) $(ARDUINO_PARTS) >$@

# ---- PACKAGE ----

$(ARDUINO)/ESP32forth.zip: $(ARDUINO)/ESP32forth/ESP32forth.ino
	cd $(ARDUINO) && zip -r ESP32forth.zip ESP32forth

# ---- DEPLOY ----

$(DEPLOY):
	mkdir -p $@

REPLACE = common/replace.js \
          COMMON=@site/common.html \
          DESKTOP_COMMON=@site/desktop_common.html \
          MENU=@site/menu.html \
          VERSION=${VERSION} \
          REVISION=${REVISION}
UE_REPLACE = $(REPLACE) FORTH=uEForth
ESP_REPLACE = $(REPLACE) FORTH=ESP32forth

$(DEPLOY)/app.yaml: $(ARDUINO)/ESP32forth.zip \
                    $(wildcard site/*.html) \
                    site/app.yaml \
                    site/eforth.go \
                    $(TARGETS) | $(DEPLOY)
	mkdir -p $(DEPLOY)
	cp -r site/static $(DEPLOY)/static
	cp -r site/downloads $(DEPLOY)/downloads
	cp site/*.go $(DEPLOY)/
	cp site/*.yaml $(DEPLOY)/
	cp -r $(ARDUINO)/ESP32forth.zip $(DEPLOY)/downloads
	cp -r $(POSIX)/ueforth $(DEPLOY)/downloads/ueforth.linux
	cp -r $(WINDOWS)/uEf32.exe $(DEPLOY)/downloads/uEf32.exe
	cp -r $(WINDOWS)/uEf64.exe $(DEPLOY)/downloads/uEf64.exe
	cp -r $(RES)/eforth.ico $(DEPLOY)/downloads/favicon.ico
	cp site/.gcloudignore $(DEPLOY)
	cat site/ESP32forth.html | $(ESP_REPLACE) >$(DEPLOY)/ESP32forth.html
	cat site/index.html | $(UE_REPLACE) >$(DEPLOY)/index.html
	cat site/linux.html | $(UE_REPLACE) >$(DEPLOY)/linux.html
	cat site/windows.html | $(UE_REPLACE) >$(DEPLOY)/windows.html
	cat site/internals.html | $(UE_REPLACE) >$(DEPLOY)/internals.html
	cat site/classic.html | $(UE_REPLACE) >$(DEPLOY)/classic.html

deploy: clean all
	cd out/deploy && gcloud app deploy -q --project esp32forth *.yaml
	cd out/deploy && gcloud app deploy -q --project eforth *.yaml
