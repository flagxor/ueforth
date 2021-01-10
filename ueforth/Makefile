OUT = out
GEN = $(OUT)/gen
RES = $(OUT)/resources
WEB = $(OUT)/web
POSIX = $(OUT)/posix
WINDOWS = $(OUT)/windows
ARDUINO = $(OUT)/arduino
DEPLOY = $(OUT)/deploy

CFLAGS_COMMON = -Wall -Werror \
                -O2 \
                -s \
                -ffreestanding \
                -fno-exceptions \
                -fno-stack-check \
                -fno-stack-protector \
                -fno-stack-protector \
                -fomit-frame-pointer \
                -mno-stack-arg-probe \
                -fno-ident -Wl,--build-id=none \
                -ffunction-sections -fdata-sections \
                -fmerge-all-constants \
                -I ./ -I $(OUT)
CFLAGS = $(CFLAGS_COMMON) \
         -Wl,--gc-sections
STRIP_ARGS = -S \
             --strip-unneeded \
             --remove-section=.note.gnu.gold-version \
             --remove-section=.comment \
             --remove-section=.note \
             --remove-section=.note.gnu.build-id \
             --remove-section=.note.ABI-tag
LIBS=-ldl

WIN_CFLAGS = $(CFLAGS_COMMON) -mwindows -nostdlib \

WIN_LIBS = -lkernel32

TARGETS = $(WEB)/terminal.html \
          $(WEB)/ueforth.js \
          $(POSIX)/ueforth \
          $(ARDUINO)/ueforth/ueforth.ino

# Selectively enable windows if tools available
DEPLOYABLE = 1
ifneq (, $(shell which i686-w64-mingw32-windres))
  ifneq (, $(shell which i686-w64-mingw32-gcc))
    TARGETS += $(WINDOWS)/uEforth32.exe
  else
    $(warning "Missing i686-w64-mingw32-gcc skipping 32-bit Windows.")
    DEPLOYABLE := 0
  endif
else
  $(warning "Missing i686-w64-mingw32-windres skipping 32-bit Windows.")
  DEPLOYABLE := 0
endif
ifneq (, $(shell which x86_64-w64-mingw32-windres))
  ifneq (, $(shell which x86_64-w64-mingw32-gcc))
    TARGETS += $(WINDOWS)/uEforth64.exe
  else
    $(warning "Missing x86_64-w64-mingw32-gcc skipping 64-bit Windows.")
    DEPLOYABLE := 0
  endif
else
  $(warning "Missing x86_64-w64-mingw32-windres skipping 64-bit Windows.")
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

POSIX_BOOT =  common/boot.fs common/terminal.fs \
              posix/posix.fs posix/posix_highlevel.fs \
              common/filetools.fs posix/posix_desktop.fs \
              common/tasks.fs common/streams.fs
$(GEN)/posix_boot.h: common/source_to_string.js $(POSIX_BOOT) | $(GEN)
	echo "ok" | cat $(POSIX_BOOT) - | $< boot >$@

WINDOWS_BOOT = common/boot.fs common/terminal.fs windows/windows.fs
$(GEN)/windows_boot.h: common/source_to_string.js $(WINDOWS_BOOT) | $(GEN)
	echo "ok" | cat $(WINDOWS_BOOT) - | $< boot >$@

ARDUINO_BOOT = common/boot.fs arduino/arduino.fs \
               posix/posix_highlevel.fs common/filetools.fs \
               common/tasks.fs common/streams.fs arduino/arduino_server.fs \
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

$(RES)/ueforth_res32.o: windows/ueforth.rc $(RES)/eforth.ico
	i686-w64-mingw32-windres $< $@

$(RES)/ueforth_res64.o: windows/ueforth.rc $(RES)/eforth.ico
	x86_64-w64-mingw32-windres $< $@

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
    common/core.h \
    $(GEN)/posix_boot.h | $(POSIX)
	$(CC) $(CFLAGS) $< -o $@ $(LIBS)
	strip $(STRIP_ARGS) $@

# ---- WINDOWS ----

$(WINDOWS):
	mkdir -p $@

$(WINDOWS)/uEforth32.exe: \
    windows/windows_main.c \
    common/opcodes.h \
    common/core.h \
    $(GEN)/windows_boot.h \
    $(RES)/ueforth_res32.o | $(WINDOWS)
	i686-w64-mingw32-gcc \
    $(WIN_CFLAGS) $< $(RES)/ueforth_res32.o -o $@ $(WIN_LIBS)

$(WINDOWS)/uEforth64.exe: \
    windows/windows_main.c \
    common/opcodes.h \
    common/core.h \
    $(GEN)/windows_boot.h \
    $(RES)/ueforth_res64.o | $(WINDOWS)
	x86_64-w64-mingw32-gcc \
    $(WIN_CFLAGS) $< $(RES)/ueforth_res64.o -o $@ $(WIN_LIBS)

# ---- ARDUINO ----

$(ARDUINO)/ueforth:
	mkdir -p $@

$(ARDUINO)/ueforth/ueforth.ino: \
        arduino/fuse_ino.js \
        arduino/arduino.template.ino \
        common/opcodes.h \
        common/core.h \
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
	cp -r $(WINDOWS)/uEforth32.exe $(DEPLOY)/static/uEforth32.exe
	cp -r $(WINDOWS)/uEforth64.exe $(DEPLOY)/static/uEforth64.exe
	cp -r $(RES)/eforth.ico $(DEPLOY)/static/favicon.ico
	cp -r site/* $(DEPLOY)
	cp -r site/.gcloudignore $(DEPLOY)
