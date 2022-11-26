# µEforth / ESP32forth

This EForth inspired implementation of Forth is bootstraped from a minimalist C kernel.

## Building from Source

To build from source:

```
git clone https://github.com/flagxor/ueforth
cd ueforth
make
```

The resulting output will have this structure:

* out/deploy - A copy of the eforth.appspot.com / esp32forth.appspot.com ready to deploy.
* out/esp32 - A source build for ESP32.
* out/esp32 - A source build for ESP32.
* out/esp32-sim - A POSIX build approximating ESP32.
* out/gen - Intermediate / generated files.
* out/posix - A build for Linux / POSIX.
* out/resources - Intermediate / generated resources.
* out/web - A build for Web.
* out/windows - A build for Windows.

Individual platforms can be built as follows:

```
make posix
make esp32
make win32
make win64
make web
```

To install to /usr/bin on Linux / POSIX do:

```
make install
```

ESP32 boards can be compiled and flashed with:

```
make esp32-flash
make esp32s2-flash
make esp32s3-flash
make esp32c3-flash
make esp32cam-flash
```

Set PORT=com3 etc. to select board.
