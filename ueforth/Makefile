all: out/ueforth

out:
	mkdir -p out

out/ueforth: ueforth.c | out
	$(CC) $(CFLAGS) $< -o $@

CFLAGS=-O2 -Wall -Werror

clean:
	rm -rf out/
