CFLAGS=-O2 -Wall -Werror -I./ -I./out
LIBS=-ldl

TARGETS = out/web/terminal.html \
          out/web/ueforth.js \
          out/posix/ueforth \
          out/arduino/ueforth.ino

all: $(TARGETS) tests

tests: core_test

core_test: out/posix/ueforth common/core_test.fs \
        common/core_test.fs.golden
	echo "include common/core_test.fs" | $< | \
        diff - common/core_test.fs.golden

out/gen:
	mkdir -p out/gen

POSIX_BOOT = common/boot.fs posix/posix.fs
out/gen/posix_boot.h: common/source_to_string.js $(POSIX_BOOT) | out/gen
	echo "ok" | cat $(POSIX_BOOT) - | $< boot >$@

ARDUINO_BOOT = common/boot.fs
out/gen/arduino_boot.h: common/source_to_string.js $(ARDUINO_BOOT) | out/gen
	echo "ok" | cat $(ARDUINO_BOOT) - | $< boot >$@

out/gen/dump_web_opcodes: web/dump_web_opcodes.c common/opcodes.h | out/gen
	$(CC) $(CFLAGS) $< -o $@

out/gen/web_cases.js: out/gen/dump_web_opcodes | out/gen
	$< cases >$@

out/gen/web_dict.js: out/gen/dump_web_opcodes | out/gen
	$< dict >$@

out/web:
	mkdir -p out/web

out/web/terminal.html: web/terminal.html | out/web
	cp $< $@

out/web/ueforth.js: \
        web/fuse_web.js \
        web/web.template.js \
        common/boot.fs \
        out/gen/web_dict.js \
        out/gen/web_cases.js | out/web
	$^ >$@

out/posix:
	mkdir -p out/posix

out/posix/ueforth: \
    posix/posix_main.c \
    common/opcodes.h \
    common/core.h \
    out/gen/posix_boot.h | out/posix
	$(CC) $(CFLAGS) $< -o $@ $(LIBS)

out/arduino:
	mkdir -p out/arduino

out/arduino/ueforth.ino: \
        arduino/fuse_ino.js \
        arduino/arduino.template.ino \
        common/opcodes.h \
        common/core.h \
        out/gen/arduino_boot.h | out/arduino
	$^ >$@

clean:
	rm -rf out/
