OUT = out
GEN = $(OUT)/gen
RES = $(OUT)/resources
WEB = $(OUT)/web
POSIX = $(OUT)/posix
WINDOWS = $(OUT)/windows
ARDUINO = $(OUT)/arduino
DEPLOY = $(OUT)/deploy

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
         -Wall \
         -Werror \
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
               /LIBPATH:"c:/Program Files (x86)/Windows Kits/10/Lib/10.0.19041.0/ucrt/x86"

WIN_LFLAGS64 = /LIBPATH:"c:/Program Files (x86)/Microsoft SDKs/Windows/v7.1A/Lib/x64" \
               /LIBPATH:"c:/Program Files (x86)/Microsoft Visual Studio/2019/Community/VC/Tools/MSVC/14.28.29333/lib/x64" \
               /LIBPATH:"c:/Program Files (x86)/Windows Kits/10/Lib/10.0.19041.0/ucrt/x64" \

TARGETS = $(WEB)/terminal.html \
          $(WEB)/ueforth.js \
          $(POSIX)/ueforth \
          $(ARDUINO)/ueforth/ueforth.ino

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

tests: core_test

core_test: $(POSIX)/ueforth common/core_test.fs \
        common/core_test.fs.golden
	echo "include common/core_test.fs" | $< | \
        diff - common/core_test.fs.golden

# ---- GENERATED ----

$(GEN):
	mkdir -p $@

POSIX_BOOT =  common/boot.fs common/ansi.fs \
              posix/posix.fs posix/posix_highlevel.fs \
              common/highlevel.fs common/filetools.fs posix/posix_desktop.fs \
              common/tasks.fs common/streams.fs common/blocks.fs
$(GEN)/posix_boot.h: common/source_to_string.js $(POSIX_BOOT) | $(GEN)
	echo "ok" | cat $(POSIX_BOOT) - | $< boot >$@

WINDOWS_BOOT = common/boot.fs common/ansi.fs \
               windows/windows.fs windows/windows_highlevel.fs common/highlevel.fs \
               common/tasks.fs common/streams.fs common/blocks.fs
$(GEN)/windows_boot.h: common/source_to_string.js $(WINDOWS_BOOT) | $(GEN)
	echo "ok" | cat $(WINDOWS_BOOT) - | $< boot >$@

ARDUINO_BOOT = common/boot.fs arduino/arduino.fs \
               posix/posix_highlevel.fs common/highlevel.fs common/filetools.fs \
               common/tasks.fs common/streams.fs arduino/arduino_server.fs \
               arduino/esp_camera.fs common/blocks.fs \
               arduino/autoboot.fs
$(GEN)/arduino_boot.h: common/source_to_string.js $(ARDUINO_BOOT) | $(GEN)
	echo "ok" | cat $(ARDUINO_BOOT) - | $< boot >$@

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

$(ARDUINO)/ueforth:
	mkdir -p $@

$(ARDUINO)/ueforth/ueforth.ino: \
        arduino/fuse_ino.js \
        arduino/arduino.template.ino \
        common/opcodes.h \
        common/core.h \
        common/interp.h \
        $(GEN)/arduino_boot.h | $(ARDUINO)/ueforth
	$^ >$@

# ---- PACKAGE ----

$(ARDUINO)/ueforth-arduino-esp32.zip: $(ARDUINO)/ueforth/ueforth.ino
	cd $(ARDUINO) && zip -r ueforth-arduino-esp32.zip ueforth

# ---- DEPLOY ----

$(DEPLOY):
	mkdir -p $@

$(DEPLOY)/app.yaml: $(ARDUINO)/ueforth-arduino-esp32.zip \
                    site/index.html \
                    site/app.yaml \
                    site/eforth.go \
                    $(TARGETS) | $(DEPLOY)
	mkdir -p $(DEPLOY)/static
	cp -r $(ARDUINO)/ueforth-arduino-esp32.zip $(DEPLOY)/static
	cp -r $(POSIX)/ueforth $(DEPLOY)/static/ueforth.linux
	cp -r $(WINDOWS)/uEf32.exe $(DEPLOY)/static/uEf32.exe
	cp -r $(WINDOWS)/uEf64.exe $(DEPLOY)/static/uEf64.exe
	cp -r $(RES)/eforth.ico $(DEPLOY)/static/favicon.ico
	cp -r site/* $(DEPLOY)
	cp -r site/.gcloudignore $(DEPLOY)
