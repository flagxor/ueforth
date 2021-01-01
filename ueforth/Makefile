all: out/ueforth

out:
	mkdir -p out

out/ueforth: ueforth.c | out
	$(CC) $(CFLAGS) $< -o $@ $(LIBS)

CFLAGS=-O2 -Wall -Werror
LIBS=-ldl

clean:
	rm -rf out/
