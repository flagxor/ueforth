OUT = out
GEN = $(OUT)/gen
RES = $(OUT)/resources
WEB = $(OUT)/web
POSIX = $(OUT)/posix
WINDOWS = $(OUT)/windows
ARDUINO = $(OUT)/arduino/ueforth

CFLAGS = -Wall -Werror \
         -O2 \
         -fno-exceptions \
         -s \
         -fomit-frame-pointer \
         -fno-stack-protector \
         -fno-ident -Wl,--build-id=none \
         -ffunction-sections -fdata-sections \
         -Wl,--gc-sections \
         -fmerge-all-constants \
         -I ./ -I $(OUT)
STRIP_ARGS = -S \
             --strip-unneeded \
             --remove-section=.note.gnu.gold-version \
             --remove-section=.comment \
             --remove-section=.note \
             --remove-section=.note.gnu.build-id \
             --remove-section=.note.ABI-tag
LIBS=-ldl

WINFLAGS = -mwindows -luser32

# Default to 32-bit Windows for maximum portability.
ifeq ($(WIN_ARCH),)
  WIN_ARCH:=i686
  #WIN_ARCH=x86_64
endif

TARGETS = $(WEB)/terminal.html \
          $(WEB)/ueforth.js \
          $(POSIX)/ueforth \
          $(ARDUINO)/ueforth.ino

# Selectively enable windows if tools available
ifneq (, $(shell which $(WIN_ARCH)-w64-mingw32-windres))
  ifneq (, $(shell which $(WIN_ARCH)-w64-mingw32-gcc))
    TARGETS += $(WINDOWS)/uEforth.exe
  else
    $(warning "Missing $(WIN_ARCH)-w64-mingw32-gcc skipping Windows.")
  endif
else
  $(warning "Missing $(WIN_ARCH)-w64-mingw32-windres skipping Windows.")
endif

all: $(TARGETS) tests

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
              common/filetools.fs posix/posix_desktop.fs
$(GEN)/posix_boot.h: common/source_to_string.js $(POSIX_BOOT) | $(GEN)
	echo "ok" | cat $(POSIX_BOOT) - | $< boot >$@

WINDOWS_BOOT = common/boot.fs common/terminal.fs windows/windows.fs
$(GEN)/windows_boot.h: common/source_to_string.js $(WINDOWS_BOOT) | $(GEN)
	echo "ok" | cat $(WINDOWS_BOOT) - | $< boot >$@

ARDUINO_BOOT = common/boot.fs arduino/arduino.fs \
               posix/posix_highlevel.fs common/filetools.fs \
               arduino/arduino_server.fs \
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

WINDOWS_RESOURCES = $(RES)/ueforth_res.o

$(WINDOWS_RESOURCES): windows/ueforth.rc $(RES)/eforth.ico
	$(WIN_ARCH)-w64-mingw32-windres $< $@

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

$(WINDOWS)/uEforth.exe: \
    windows/windows_main.c \
    common/opcodes.h \
    common/core.h \
    $(GEN)/windows_boot.h \
    $(WINDOWS_RESOURCES) | $(WINDOWS)
	$(WIN_ARCH)-w64-mingw32-gcc \
    $(CFLAGS) $(WINFLAGS) $< $(WINDOWS_RESOURCES) -o $@

# ---- ARDUINO ----

$(ARDUINO):
	mkdir -p $@

$(ARDUINO)/ueforth.ino: \
        arduino/fuse_ino.js \
        arduino/arduino.template.ino \
        common/opcodes.h \
        common/core.h \
        $(GEN)/arduino_boot.h | $(ARDUINO)
	$^ >$@

