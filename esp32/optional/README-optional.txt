+----------------------------------+
|  ESP32forth - Optional Packages  |
+----------------------------------+

ESPforth supports a number of optional packages, included in this directory.

By default ESPforth will only include core functionality.
To include one or more of these modules, move them from this directory
into the parent directory, next to the ESPforth.ino file.

These are the current optional modules:
  * assemblers.h - Assemblers for ESP32 Xtensa and ESP32 RISC-V
  * camera.h - Support for the ESP32-CAM camera
  * oled.h - Support for the SSD1306 Oled
  * rmt.h - Support for RMT (Remote Control)
  * serial-bluetooth.h - Support for Bluetooth serial and
                         bterm a Bluetooth serial redirector for the terminal
  * spi-flash.h - Support for low level SPI Flash partition access

Initially ESP32forth focused on a minimal C kernel, with most functionality
built in Forth code loaded at boot. Eventually, as support for more capabilities
were added to ESPforth, this became unsustainable.

Optional modules demonstrate good patterns for use in your own extensions
to ESP32forth. You can add you own modules by #including them from
an optional userwords.h file placed next to ESPforth.ino
