/*
 * ESP32forth v{{VERSION}}
 * Revision: {{REVISION}}
 */

{{opcodes}}
{{calling}}

// For now, default on several options.
#define ENABLE_SPIFFS_SUPPORT
#define ENABLE_WIFI_SUPPORT
#define ENABLE_MDNS_SUPPORT
#define ENABLE_WEBSERVER_SUPPORT
#define ENABLE_SDCARD_SUPPORT
#define ENABLE_I2C_SUPPORT
#define ENABLE_SOCKETS_SUPPORT
#define ENABLE_FREERTOS_SUPPORT

// Uncomment this #define for OLED Support.
// You will need to install these libraries from the Library Manager:
//   Adafruit SSD1306
//   Adafruit GFX Library
//   Adafruit BusIO
//#define ENABLE_OLED_SUPPORT

// For now assume only boards with PSRAM (ESP32-CAM)
// will want SerialBluetooth (very large) and camera support.
// Other boards can support these if they're set to a larger
// parition size. But it's unclear the best way to configure this.
#ifdef BOARD_HAS_PSRAM
# define ENABLE_CAMERA_SUPPORT
# define ENABLE_SERIAL_BLUETOOTH_SUPPORT
#endif

#ifdef ENABLE_WEBSERVER_SUPPORT
# include "WebServer.h"
#endif

#include <errno.h>
#include <unistd.h>
#include <fcntl.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <sys/select.h>

#if defined(ESP32)
# define HEAP_SIZE (100 * 1024)
# define STACK_SIZE 512
#elif defined(ESP8266)
# define HEAP_SIZE (40 * 1024)
# define STACK_SIZE 512
#else
# define HEAP_SIZE 2 * 1024
# define STACK_SIZE 32
#endif

#define PLATFORM_OPCODE_LIST \
  /* Memory Allocation */ \
  Y(MALLOC, SET malloc(n0)) \
  Y(SYSFREE, free(a0); DROP) \
  Y(REALLOC, SET realloc(a1, n0); NIP) \
  Y(heap_caps_malloc, SET heap_caps_malloc(n1, n0); NIP) \
  Y(heap_caps_free, heap_caps_free(a0); DROP) \
  Y(heap_caps_realloc, \
      tos = (cell_t) heap_caps_realloc(a2, n1, n0); NIPn(2)) \
  /* Serial */ \
  X("Serial.begin", SERIAL_BEGIN, Serial.begin(tos); DROP) \
  X("Serial.end", SERIAL_END, Serial.end()) \
  X("Serial.available", SERIAL_AVAILABLE, PUSH Serial.available()) \
  X("Serial.readBytes", SERIAL_READ_BYTES, n0 = Serial.readBytes(b1, n0); NIP) \
  X("Serial.write", SERIAL_WRITE, n0 = Serial.write(b1, n0); NIP) \
  X("Serial.flush", SERIAL_FLUSH, Serial.flush()) \
  /* Pins and PWM */ \
  Y(pinMode, pinMode(n1, n0); DROPn(2)) \
  Y(digitalWrite, digitalWrite(n1, n0); DROPn(2)) \
  Y(digitalRead, n0 = digitalRead(n0)) \
  Y(analogRead, n0 = analogRead(n0)) \
  Y(ledcSetup, \
      n0 = (cell_t) (1000000 * ledcSetup(n2, n1 / 1000.0, n0)); NIPn(2)) \
  Y(ledcAttachPin, ledcAttachPin(n1, n0); DROPn(2)) \
  Y(ledcDetachPin, ledcDetachPin(n0); DROP) \
  Y(ledcRead, n0 = ledcRead(n0)) \
  Y(ledcReadFreq, n0 = (cell_t) (1000000 * ledcReadFreq(n0))) \
  Y(ledcWrite, ledcWrite(n1, n0); DROPn(2)) \
  Y(ledcWriteTone, \
      n0 = (cell_t) (1000000 * ledcWriteTone(n1, n0 / 1000.0)); NIP) \
  Y(ledcWriteNote, \
      tos = (cell_t) (1000000 * ledcWriteNote(n2, (note_t) n1, n0)); NIPn(2)) \
  /* General System */ \
  Y(MS, delay(n0); DROP) \
  Y(TERMINATE, exit(n0)) \
  /* File words */ \
  X("R/O", R_O, PUSH O_RDONLY) \
  X("R/W", R_W, PUSH O_RDWR) \
  X("W/O", W_O, PUSH O_WRONLY) \
  Y(BIN, ) \
  X("CLOSE-FILE", CLOSE_FILE, tos = close(tos); tos = tos ? errno : 0) \
  X("FLUSH-FILE", FLUSH_FILE, fsync(tos); /* fsync has no impl and returns ENOSYS :-( */ tos = 0) \
  X("OPEN-FILE", OPEN_FILE, cell_t mode = n0; DROP; cell_t len = n0; DROP; \
    memcpy(filename, a0, len); filename[len] = 0; \
    n0 = open(filename, mode, 0777); PUSH n0 < 0 ? errno : 0) \
  X("CREATE-FILE", CREATE_FILE, cell_t mode = n0; DROP; cell_t len = n0; DROP; \
    memcpy(filename, a0, len); filename[len] = 0; \
    n0 = open(filename, mode | O_CREAT | O_TRUNC); PUSH n0 < 0 ? errno : 0) \
  X("DELETE-FILE", DELETE_FILE, cell_t len = n0; DROP; \
    memcpy(filename, a0, len); filename[len] = 0; \
    n0 = unlink(filename); n0 = n0 ? errno : 0) \
  X("WRITE-FILE", WRITE_FILE, cell_t fd = n0; DROP; cell_t len = n0; DROP; \
    n0 = write(fd, a0, len); n0 = n0 != len ? errno : 0) \
  X("READ-FILE", READ_FILE, cell_t fd = n0; DROP; cell_t len = n0; DROP; \
    n0 = read(fd, a0, len); PUSH n0 < 0 ? errno : 0) \
  X("FILE-POSITION", FILE_POSITION, \
    n0 = (cell_t) lseek(n0, 0, SEEK_CUR); PUSH n0 < 0 ? errno : 0) \
  X("REPOSITION-FILE", REPOSITION_FILE, cell_t fd = n0; DROP; \
    n0 = (cell_t) lseek(fd, tos, SEEK_SET); n0 = n0 < 0 ? errno : 0) \
  X("RESIZE-FILE", RESIZE_FILE, cell_t fd = n0; DROP; n0 = ResizeFile(fd, tos)) \
  X("FILE-SIZE", FILE_SIZE, struct stat st; w = fstat(n0, &st); \
    n0 = (cell_t) st.st_size; PUSH w < 0 ? errno : 0) \
  OPTIONAL_SPIFFS_SUPPORT \
  OPTIONAL_WIFI_SUPPORT \
  OPTIONAL_MDNS_SUPPORT \
  OPTIONAL_WEBSERVER_SUPPORT \
  OPTIONAL_SDCARD_SUPPORT \
  OPTIONAL_I2C_SUPPORT \
  OPTIONAL_SERIAL_BLUETOOTH_SUPPORT \
  OPTIONAL_CAMERA_SUPPORT \
  OPTIONAL_SOCKETS_SUPPORT \
  OPTIONAL_FREERTOS_SUPPORT \
  OPTIONAL_OLED_SUPPORT \

#ifndef ENABLE_SPIFFS_SUPPORT
// Provide a default failing SPIFFS.begin
# define OPTIONAL_SPIFFS_SUPPORT \
  X("SPIFFS.begin", SPIFFS_BEGIN, NIPn(2); n0 = 0)
#else
# include "SPIFFS.h"
# define OPTIONAL_SPIFFS_SUPPORT \
  X("SPIFFS.begin", SPIFFS_BEGIN, \
      tos = SPIFFS.begin(n2, c1, n0); NIPn(2)) \
  X("SPIFFS.end", SPIFFS_END, SPIFFS.end()) \
  X("SPIFFS.format", SPIFFS_FORMAT, PUSH SPIFFS.format()) \
  X("SPIFFS.totalBytes", SPIFFS_TOTAL_BYTES, PUSH SPIFFS.totalBytes()) \
  X("SPIFFS.usedBytes", SPIFFS_USED_BYTES, PUSH SPIFFS.usedBytes())
#endif

#ifndef ENABLE_FREERTOS_SUPPORT
# define OPTIONAL_FREERTOS_SUPPORT
#else
# include "freertos/FreeRTOS.h"
# include "freertos/task.h"
# define OPTIONAL_FREERTOS_SUPPORT \
  Y(vTaskDelete, vTaskDelete((TaskHandle_t) n0); DROP) \
  Y(xTaskCreatePinnedToCore, n0 = xTaskCreatePinnedToCore((TaskFunction_t) a6, c5, n4, a3, (UBaseType_t) n2, (TaskHandle_t *) a1, (BaseType_t) n0); NIPn(6)) \
  Y(xPortGetCoreID, PUSH xPortGetCoreID())
#endif

#ifndef ENABLE_CAMERA_SUPPORT
# define OPTIONAL_CAMERA_SUPPORT
#else
# include "esp_camera.h"
# define OPTIONAL_CAMERA_SUPPORT \
  Y(esp_camera_init, n0 = esp_camera_init((camera_config_t *) a0)) \
  Y(esp_camera_deinit, PUSH esp_camera_deinit()) \
  Y(esp_camera_fb_get, PUSH esp_camera_fb_get()) \
  Y(esp_camera_fb_return, esp_camera_fb_return((camera_fb_t *) a0); DROP) \
  Y(esp_camera_sensor_get, PUSH esp_camera_sensor_get())
#endif

#ifndef ENABLE_SOCKETS_SUPPORT
# define OPTIONAL_SOCKETS_SUPPORT
#else
# include <errno.h>
# include <sys/select.h>
# include <sys/socket.h>
# include <sys/time.h>
# include <sys/types.h>
# include <sys/un.h>
# include <sys/poll.h>
# define OPTIONAL_SOCKETS_SUPPORT \
  Y(socket, n0 = socket(n2, n1, n0); NIPn(2)) \
  Y(bind, n0 = bind(n2, (struct sockaddr *) a1, n0); NIPn(2)) \
  Y(listen, n0 = listen(n1, n0); NIP) \
  Y(connect, n0 = connect(n2, (struct sockaddr *) a1, n0); NIPn(2)) \
  Y(accept, n0 = accept(n2, (struct sockaddr *) a1, (socklen_t *) a0); NIPn(2)) \
  Y(select, n0 = select(n4, (fd_set *) a3, (fd_set *) a2, (fd_set *) a1, (struct timeval *) a0); NIPn(4)) \
  Y(poll, n0 = poll((struct pollfd *) a2, (nfds_t) n1, n0); NIPn(2)) \
  Y(errno, PUSH errno)
#endif

#ifndef ENABLE_SDCARD_SUPPORT
# define OPTIONAL_SDCARD_SUPPORT
#else
# include "SD_MMC.h"
# define OPTIONAL_SDCARD_SUPPORT \
  X("SD_MMC.begin", SD_MMC_BEGIN, tos = SD_MMC.begin(c1, n0); NIP) \
  X("SD_MMC.end", SD_MMC_END, SD_MMC.end()) \
  X("SD_MMC.cardType", SD_MMC_CARD_TYPE, PUSH SD_MMC.cardType()) \
  X("SD_MMC.totalBytes", SD_MMC_TOTAL_BYTES, PUSH SD_MMC.totalBytes()) \
  X("SD_MMC.usedBytes", SD_MMC_USED_BYTES, PUSH SD_MMC.usedBytes())
#endif

#ifndef ENABLE_I2C_SUPPORT
# define OPTIONAL_I2C_SUPPORT
#else
# include <Wire.h>
# define OPTIONAL_I2C_SUPPORT \
  X("Wire.begin", WIRE_BEGIN, n0 = Wire.begin(n1, n0); NIP) \
  X("Wire.setClock", WIRE_SET_CLOCK, Wire.setClock(n0); DROP) \
  X("Wire.getClock", WIRE_GET_CLOCK, PUSH Wire.getClock()) \
  X("Wire.setTimeout", WIRE_SET_TIMEOUT, Wire.setTimeout(n0); DROP) \
  X("Wire.getTimeout", WIRE_GET_TIMEOUT, PUSH Wire.getTimeout()) \
  X("Wire.lastError", WIRE_LAST_ERROR, PUSH Wire.lastError()) \
  X("Wire.getErrorText", WIRE_GET_ERROR_TEXT, PUSH Wire.getErrorText(n0)) \
  X("Wire.beginTransmission", WIRE_BEGIN_TRANSMISSION, Wire.beginTransmission(n0); DROP) \
  X("Wire.endTransmission", WIRE_END_TRANSMISSION, PUSH Wire.endTransmission(n0)) \
  X("Wire.requestFrom", WIRE_REQUEST_FROM, n0 = Wire.requestFrom(n2, n1, n0); NIPn(2)) \
  X("Wire.writeTransmission", WIRE_WRITE_TRANSMISSION, \
      n0 = Wire.writeTransmission(n3, b2, n1, n0); NIPn(3)) \
  X("Wire.readTransmission", WIRE_READ_TRANSMISSION, \
      n0 = Wire.readTransmission(n4, b3, n2, n1, (uint32_t *) a0); NIPn(4)) \
  X("Wire.write", WIRE_WRITE, n0 = Wire.write(b1, n0); NIP) \
  X("Wire.available", WIRE_AVAILABLE, PUSH Wire.available()) \
  X("Wire.read", WIRE_READ, PUSH Wire.read()) \
  X("Wire.peek", WIRE_PEEK, PUSH Wire.peek()) \
  X("Wire.busy", WIRE_BUSY, PUSH Wire.busy()) \
  X("Wire.flush", WIRE_FLUSH, Wire.flush())
#endif

#ifndef ENABLE_SERIAL_BLUETOOTH_SUPPORT
# define OPTIONAL_SERIAL_BLUETOOTH_SUPPORT
#else
# include "esp_bt_device.h"
# include "BluetoothSerial.h"
# define bt0 ((BluetoothSerial *) a0)
# define OPTIONAL_SERIAL_BLUETOOTH_SUPPORT \
  X("SerialBT.new", SERIALBT_NEW, PUSH new BluetoothSerial()) \
  X("SerialBT.delete", SERIALBT_DELETE, delete bt0; DROP) \
  X("SerialBT.begin", SERIALBT_BEGIN, n0 = bt0->begin(c2, n1); NIPn(2)) \
  X("SerialBT.end", SERIALBT_END, bt0->end(); DROP) \
  X("SerialBT.available", SERIALBT_AVAILABLE, n0 = bt0->available()) \
  X("SerialBT.readBytes", SERIALBT_READ_BYTES, n0 = bt0->readBytes(b2, n1); NIPn(2)) \
  X("SerialBT.write", SERIALBT_WRITE, n0 = bt0->write(b2, n1); NIPn(2)) \
  X("SerialBT.flush", SERIALBT_FLUSH, bt0->flush(); DROP) \
  X("SerialBT.hasClient", SERIALBT_HAS_CLIENT, n0 = bt0->hasClient()) \
  X("SerialBT.enableSSP", SERIALBT_ENABLE_SSP, bt0->enableSSP(); DROP) \
  X("SerialBT.setPin", SERIALBT_SET_PIN, n0 = bt0->setPin(c1); NIP) \
  X("SerialBT.unpairDevice", SERIALBT_UNPAIR_DEVICE, \
      n0 = bt0->unpairDevice(b1); NIP) \
  X("SerialBT.connect", SERIALBT_CONNECT, n0 = bt0->connect(c1); NIP) \
  X("SerialBT.connectAddr", SERIALBT_CONNECT_ADDR, n0 = bt0->connect(b1); NIP) \
  X("SerialBT.disconnect", SERIALBT_DISCONNECT, n0 = bt0->disconnect()) \
  X("SerialBT.connected", SERIALBT_CONNECTED, n0 = bt0->connected(n1); NIP) \
  X("SerialBT.isReady", SERIALBT_IS_READY, n0 = bt0->isReady(n2, n1); NIPn(2)) \
  /* Bluetooth */ \
  Y(esp_bt_dev_get_address, PUSH esp_bt_dev_get_address())
#endif

#ifndef ENABLE_WIFI_SUPPORT
# define OPTIONAL_WIFI_SUPPORT
#else
# include <WiFi.h>
# include <WiFiClient.h>

static IPAddress ToIP(cell_t ip) {
  return IPAddress(ip & 0xff, ((ip >> 8) & 0xff), ((ip >> 16) & 0xff), ((ip >> 24) & 0xff));
}

static cell_t FromIP(IPAddress ip) {
  cell_t ret = 0;
  ret = (ret << 8) | ip[3];
  ret = (ret << 8) | ip[2];
  ret = (ret << 8) | ip[1];
  ret = (ret << 8) | ip[0];
  return ret;
}

# define OPTIONAL_WIFI_SUPPORT \
  /* WiFi */ \
  X("WiFi.config", WIFI_CONFIG, \
      WiFi.config(ToIP(n3), ToIP(n2), ToIP(n1), ToIP(n0)); DROPn(4)) \
  X("WiFi.begin", WIFI_BEGIN, WiFi.begin(c1, c0); DROPn(2)) \
  X("WiFi.disconnect", WIFI_DISCONNECT, WiFi.disconnect()) \
  X("WiFi.status", WIFI_STATUS, PUSH WiFi.status()) \
  X("WiFi.macAddress", WIFI_MAC_ADDRESS, WiFi.macAddress(b0); DROP) \
  X("WiFi.localIP", WIFI_LOCAL_IPS, PUSH FromIP(WiFi.localIP())) \
  X("WiFi.mode", WIFI_MODE, WiFi.mode((wifi_mode_t) n0); DROP) \
  X("WiFi.setTxPower", WIFI_SET_TX_POWER, WiFi.setTxPower((wifi_power_t) n0); DROP) \
  X("WiFi.getTxPower", WIFI_GET_TX_POWER, PUSH WiFi.getTxPower())
#endif

#ifndef ENABLE_MDNS_SUPPORT
# define OPTIONAL_MDNS_SUPPORT
#else
# include <ESPmDNS.h>
# define OPTIONAL_MDNS_SUPPORT \
  /* mDNS */ \
  X("MDNS.begin", MDNS_BEGIN, n0 = MDNS.begin(c0))
#endif

#ifndef ENABLE_WEBSERVER_SUPPORT
# define OPTIONAL_WEBSERVER_SUPPORT
#else
# include <WebServer.h>
# define ws0 ((WebServer *) a0)
# define OPTIONAL_WEBSERVER_SUPPORT \
  /* WebServer */ \
  X("WebServer.new", WEBSERVER_NEW, PUSH new WebServer(tos)) \
  X("WebServer.delete", WEBSERVER_DELETE, delete ws0; DROP) \
  X("WebServer.begin", WEBSERVER_BEGIN, ws0->begin(n1); DROPn(2)) \
  X("WebServer.stop", WEBSERVER_STOP, ws0->stop(); DROP) \
  X("WebServer.on", WEBSERVER_ON, InvokeWebServerOn(ws0, c2, n1); DROPn(3)) \
  X("WebServer.hasArg", WEBSERVER_HAS_ARG, n0 = ws0->hasArg(c1); DROP) \
  X("WebServer.arg", WEBSERVER_ARG, \
      string_value = ws0->arg(c1); \
      c1 = &string_value[0]; n0 = string_value.length()) \
  X("WebServer.argi", WEBSERVER_ARGI, \
      string_value = ws0->arg(n1); \
      c1 = &string_value[0]; n0 = string_value.length()) \
  X("WebServer.argName", WEBSERVER_ARG_NAME, \
      string_value = ws0->argName(n1); \
      c1 = &string_value[0]; n0 = string_value.length()) \
  X("WebServer.args", WEBSERVER_ARGS, n0 = ws0->args()) \
  X("WebServer.setContentLength", WEBSERVER_SET_CONTENT_LENGTH, \
      ws0->setContentLength(n1); DROPn(2)) \
  X("WebServer.sendHeader", WEBSERVER_SEND_HEADER, \
      ws0->sendHeader(c3, c2, n1); DROPn(4)) \
  X("WebServer.send", WEBSERVER_SEND, ws0->send(n3, c2, c1); DROPn(4)) \
  X("WebServer.sendContent", WEBSERVER_SEND_CONTENT, \
      ws0->sendContent(c1); DROPn(2)) \
  X("WebServer.method", WEBSERVER_METHOD, n0 = ws0->method()) \
  X("WebServer.handleClient", WEBSERVER_HANDLE_CLIENT, ws0->handleClient(); DROP)
#endif

#ifndef ENABLE_OLED_SUPPORT
# define OPTIONAL_OLED_SUPPORT
#else
#  include <Adafruit_GFX.h>
#  include <Adafruit_SSD1306.h>
static Adafruit_SSD1306 *oled_display = 0;
# define OPTIONAL_OLED_SUPPORT \
  Y(OledAddr, PUSH &oled_display) \
  Y(OledNew, oled_display = new Adafruit_SSD1306(n2, n1, &Wire, n0); DROPn(3)) \
  Y(OledDelete, delete oled_display) \
  Y(OledBegin, n0 = oled_display->begin(n1, n0); NIP) \
  Y(OledHOME, oled_display->setCursor(0,0); DROP) \
  Y(OledCLS, oled_display->clearDisplay()) \
  Y(OledTextc, oled_display->setTextColor(n0); DROP) \
  Y(OledPrintln, oled_display->println(c0); DROP) \
  Y(OledNumln, oled_display->println(n0); DROP) \
  Y(OledNum, oled_display->print(n0); DROP) \
  Y(OledDisplay, oled_display->display()) \
  Y(OledPrint, oled_display->write(c0); DROP) \
  Y(OledInvert, oled_display->invertDisplay(n0); DROP) \
  Y(OledTextsize, oled_display->setTextSize(n0); DROP) \
  Y(OledSetCursor, oled_display->setCursor(n1,n0); DROPn(2)) \
  Y(OledPixel, oled_display->drawPixel(n2, n1, n0); DROPn(2)) \
  Y(OledDrawL, oled_display->drawLine(n4, n3, n2, n1, n0); DROPn(4)) \
  Y(OledCirc, oled_display->drawCircle(n3,n2, n1, n0); DROPn(3)) \
  Y(OledCircF, oled_display->fillCircle(n3, n2, n1, n0); DROPn(3)) \
  Y(OledRect, oled_display->drawRect(n4, n3, n2, n1, n0); DROPn(4)) \
  Y(OledRectF, oled_display->fillRect(n4, n3, n2, n1, n0); DROPn(3)) \
  Y(OledRectR, oled_display->drawRoundRect(n5, n4, n3, n2, n1, n0); DROPn(5)) \
  Y(OledRectRF, oled_display->fillRoundRect(n5, n4, n3, n2, n1, n0 ); DROPn(5))
#endif

static char filename[PATH_MAX];
static String string_value;

{{core}}
{{interp}}
{{boot}}

// Work around lack of ftruncate
static cell_t ResizeFile(cell_t fd, cell_t size) {
  struct stat st;
  char buf[256];
  cell_t t = fstat(fd, &st);
  if (t < 0) { return errno; }
  if (size < st.st_size) {
    // TODO: Implement truncation
    return ENOSYS;
  }
  cell_t oldpos = lseek(fd, 0, SEEK_CUR);
  if (oldpos < 0) { return errno; }
  t = lseek(fd, 0, SEEK_END);
  if (t < 0) { return errno; }
  memset(buf, 0, sizeof(buf));
  while (st.st_size < size) {
    cell_t len = sizeof(buf);
    if (size - st.st_size < len) {
      len = size - st.st_size;
    }
    t = write(fd, buf, len);
    if (t != len) {
      return errno;
    }
    st.st_size += t;
  }
  t = lseek(fd, oldpos, SEEK_SET);
  if (t < 0) { return errno; }
  return 0;
}

#ifdef ENABLE_WEBSERVER_SUPPORT
static void InvokeWebServerOn(WebServer *ws, const char *url, cell_t xt) {
  ws->on(url, [xt]() {
    cell_t code[2];
    code[0] = xt;
    code[1] = g_sys.YIELD_XT;
    cell_t stack[16];
    cell_t rstack[16];
    cell_t *rp = rstack;
    *++rp = (cell_t) (stack + 1);
    *++rp = (cell_t) code;
    forth_run(rp);
  });
}
#endif

void setup() {
  cell_t *heap = (cell_t *) malloc(HEAP_SIZE);
  forth_init(0, 0, heap, boot, sizeof(boot));
}

void loop() {
  g_sys.rp = forth_run(g_sys.rp);
}
