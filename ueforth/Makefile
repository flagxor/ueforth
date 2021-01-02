all: out/web/ueforth.js out/posix/ueforth out/arduino/ueforth.ino

out/web:
	mkdir -p out/web

out/web/ueforth.js: web/fuse_web.js web/web.template.js boot.fs | out/web
	$^ >$@

out/posix:
	mkdir -p out/posix

CFLAGS=-O2 -Wall -Werror
LIBS=-ldl

out/posix/ueforth: ueforth.c | out/posix
	$(CC) $(CFLAGS) $< -o $@ $(LIBS)

out/arduino:
	mkdir -p out/arduino

out/arduino/ueforth.ino: arduino/fuse_ino.js \
        arduino/arduino.template.ino boot.fs | out/arduino
	$^ >$@

clean:
	rm -rf out/
