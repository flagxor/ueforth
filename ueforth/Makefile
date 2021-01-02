CFLAGS=-O2 -Wall -Werror -I./ -I./out
LIBS=-ldl

TARGETS = out/web/terminal.html \
          out/web/ueforth.js \
          out/posix/ueforth \
          out/arduino/ueforth.ino

all: $(TARGETS)

out/gen:
	mkdir -p out/gen

out/gen/boot.h: common/source_to_string.js common/boot.fs | out/gen
	$^ boot >$@

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
        out/gen/boot.h \
        out/gen/web_dict.js \
        out/gen/web_cases.js | out/web
	$^ >$@

out/posix:
	mkdir -p out/posix

out/posix/ueforth: posix/posix_main.c common/opcodes.h | out/posix
	$(CC) $(CFLAGS) $< -o $@ $(LIBS)

out/arduino:
	mkdir -p out/arduino

out/arduino/ueforth.ino: \
        arduino/fuse_ino.js \
        arduino/arduino.template.ino \
        common/opcodes.h \
        common/core.h \
        out/gen/boot.h | out/arduino
	$^ >$@

clean:
	rm -rf out/
