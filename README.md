# µEforth / ESP32forth

This EForth inspired implementation of Forth is bootstraped from a minimalist C kernel.

Compiled site documentation at [https://eforth.appspot.com](https://eforth.appspot.com/)

## Building from Source

To build from source:

```
sudo apt install ninja-build gcc-arm-none-eabi
git clone https://github.com/flagxor/ueforth
cd ueforth
./configure.py
ninja
```

The resulting output will have this structure:

* out/deploy - A copy of the eforth.appspot.com / esp32forth.appspot.com ready to deploy.
* out/esp32 - A source build for ESP32.
* out/esp32-sim - A POSIX build approximating ESP32 for testing.
* out/pico-ice - A build for pico-ice.
* out/pico-ice-sim - A POSIX build approximating pico-ice for testing.
* out/gen - Intermediate / generated files.
* out/posix - A build for Linux / POSIX.
* out/resources - Intermediate / generated resources.
* out/web - A build for Web.
* out/windows - A build for Windows.

Individual platforms can be built as follows:

```
ninja posix
ninja esp32
ninja pico-ice
ninja win32
ninja win64
ninja web
```

A build that excludes the slower components can be configured with:

```
./configure.py -f
```

To install to /usr/bin on Linux / POSIX do:

```
ninja install
```

ESP32 boards can be compiled and flashed with:

```
ninja esp32-flash
ninja esp32s2-flash
ninja esp32s3-flash
ninja esp32c3-flash
ninja esp32cam-flash
```

Set PORT=com3 etc. to select board.
