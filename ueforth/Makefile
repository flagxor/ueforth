OUT = out
GEN = $(OUT)/gen
RES = $(OUT)/resources
WEB = $(OUT)/web
POSIX = $(OUT)/posix
ARDUINO = $(OUT)/arduino

CFLAGS=-O2 -Wall -Werror -I ./ -I $(OUT)
LIBS=-ldl

TARGETS = $(WEB)/terminal.html \
          $(WEB)/ueforth.js \
          $(POSIX)/ueforth \
          $(ARDUINO)/ueforth.ino

all: $(TARGETS) $(RES)/eforth.ico tests

tests: core_test

core_test: $(POSIX)/ueforth common/core_test.fs \
        common/core_test.fs.golden
	echo "include common/core_test.fs" | $< | \
        diff - common/core_test.fs.golden

$(GEN):
	mkdir -p $@

POSIX_BOOT = common/boot.fs posix/posix.fs
$(GEN)/posix_boot.h: common/source_to_string.js $(POSIX_BOOT) | $(GEN)
	echo "ok" | cat $(POSIX_BOOT) - | $< boot >$@

ARDUINO_BOOT = common/boot.fs
$(GEN)/arduino_boot.h: common/source_to_string.js $(ARDUINO_BOOT) | $(GEN)
	echo "ok" | cat $(ARDUINO_BOOT) - | $< boot >$@

$(GEN)/dump_web_opcodes: web/dump_web_opcodes.c common/opcodes.h | $(GEN)
	$(CC) $(CFLAGS) $< -o $@

$(GEN)/web_cases.js: $(GEN)/dump_web_opcodes | $(GEN)
	$< cases >$@

$(GEN)/web_dict.js: $(GEN)/dump_web_opcodes | $(GEN)
	$< dict >$@

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

$(POSIX):
	mkdir -p $@

$(POSIX)/ueforth: \
    posix/posix_main.c \
    common/opcodes.h \
    common/core.h \
    $(GEN)/posix_boot.h | $(POSIX)
	$(CC) $(CFLAGS) $< -o $@ $(LIBS)

$(ARDUINO):
	mkdir -p $@

$(ARDUINO)/ueforth.ino: \
        arduino/fuse_ino.js \
        arduino/arduino.template.ino \
        common/opcodes.h \
        common/core.h \
        $(GEN)/arduino_boot.h | $(ARDUINO)
	$^ >$@

clean:
	rm -rf $(OUT)
