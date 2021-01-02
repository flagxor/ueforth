CFLAGS=-O2 -Wall -Werror -I./
LIBS=-ldl

TARGETS = out/web/terminal.html \
          out/web/ueforth.js \
          out/posix/ueforth \
          out/arduino/ueforth.ino

all: $(TARGETS)

out/gen:
	mkdir -p out/gen

out/gen/dump_web_opcodes: web/dump_web_opcodes.c opcodes.h | out/gen
	$(CC) $(CFLAGS) $< -o $@

out/gen/web_cases.js: out/gen/dump_web_opcodes | out/gen
	$< cases >$@

out/gen/web_dict.js: out/gen/dump_web_opcodes | out/gen
	$< dict >$@

out/web:
	mkdir -p out/web

out/web/terminal.html: web/terminal.html | out/web
	cp $< $@

out/web/ueforth.js: web/fuse_web.js web/web.template.js boot.fs \
        out/gen/web_dict.js out/gen/web_cases.js | out/web
	$^ >$@

out/posix:
	mkdir -p out/posix

out/posix/ueforth: posix/posix.c opcodes.h | out/posix
	$(CC) $(CFLAGS) $< -o $@ $(LIBS)

out/arduino:
	mkdir -p out/arduino

out/arduino/ueforth.ino: arduino/fuse_ino.js \
        arduino/arduino.template.ino \
        opcodes.h \
        core.h \
        boot.fs | out/arduino
	$^ >$@

clean:
	rm -rf out/
