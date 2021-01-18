{{opcodes}}

#include <WiFi.h>
#include <WiFiClient.h>
#include <WebServer.h>
#include <ESPmDNS.h>
#include "SPIFFS.h"

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

#define PUSH(v) (DUP, tos = (cell_t) (v))

#define PLATFORM_OPCODE_LIST \
  /* Allocation and Strings */ \
  X("MALLOC", MALLOC, tos = (cell_t) malloc(tos)) \
  X("SYSFREE", FREE, free((void *) tos); DROP) \
  X("REALLOC", REALLOC, tos = (cell_t) realloc((void *) *sp, tos); --sp) \
  /* Serial */ \
  X("Serial.begin", SERIAL_BEGIN, Serial.begin(tos); DROP) \
  X("Serial.end", SERIAL_END, Serial.end()) \
  X("Serial.available", SERIAL_AVAILABLE, DUP; tos = Serial.available()) \
  X("Serial.readBytes", SERIAL_READ_BYTES, tos = Serial.readBytes((uint8_t *) *sp, tos); --sp) \
  X("Serial.write", SERIAL_WRITE, tos = Serial.write((const uint8_t *) *sp, tos); --sp) \
  /* Pins and PWM */ \
  X("pinMode", PIN_MODE, pinMode(*sp, tos); --sp; DROP) \
  X("digitalWrite", DIGITAL_WRITE, digitalWrite(*sp, tos); --sp; DROP) \
  X("digitalRead", DIGITAL_READ, tos = (cell_t) digitalRead(tos)) \
  X("analogRead", ANALOG_READ, tos = (cell_t) analogRead(tos)) \
  X("ledcSetup", LEDC_SETUP, \
      tos = (cell_t) (1000000 * ledcSetup(sp[-1], *sp / 1000.0, tos)); sp -= 2) \
  X("ledcAttachPin", ATTACH_PIN, ledcAttachPin(*sp, tos); --sp; DROP) \
  X("ledcDetachPin", DETACH_PIN, ledcDetachPin(tos); DROP) \
  X("ledcRead", LEDC_READ, tos = (cell_t) ledcRead(tos)) \
  X("ledcReadFreq", LEDC_READ_FREQ, tos = (cell_t) (1000000 * ledcReadFreq(tos))) \
  X("ledcWrite", LEDC_WRITE, ledcWrite(*sp, tos); --sp; DROP) \
  X("ledcWriteTone", LEDC_WRITE_TONE, \
      tos = (cell_t) (1000000 * ledcWriteTone(*sp, tos / 1000.0)); --sp) \
  X("ledcWriteNote", LEDC_WRITE_NOTE, \
      tos = (cell_t) (1000000 * ledcWriteNote(sp[-1], (note_t) *sp, tos)); sp -=2) \
  X("MS", MS, delay(tos); DROP) \
  X("TERMINATE", TERMINATE, exit(tos)) \
  /* File words */ \
  X("R/O", R_O, PUSH(O_RDONLY)) \
  X("R/W", R_W, PUSH(O_RDWR)) \
  X("W/O", W_O, PUSH(O_WRONLY)) \
  X("BIN", BIN, ) \
  X("CLOSE-FILE", CLOSE_FILE, tos = close(tos); tos = tos ? errno : 0) \
  X("OPEN-FILE", OPEN_FILE, cell_t mode = tos; DROP; cell_t len = tos; DROP; \
    memcpy(filename, (void *) tos, len); filename[len] = 0; \
    tos = open(filename, mode, 0777); PUSH(tos < 0 ? errno : 0)) \
  X("CREATE-FILE", CREATE_FILE, cell_t mode = tos; DROP; cell_t len = tos; DROP; \
    memcpy(filename, (void *) tos, len); filename[len] = 0; \
    tos = open(filename, mode | O_CREAT | O_TRUNC); PUSH(tos < 0 ? errno : 0)) \
  X("DELETE-FILE", DELETE_FILE, cell_t len = tos; DROP; \
    memcpy(filename, (void *) tos, len); filename[len] = 0; \
    tos = unlink(filename); tos = tos ? errno : 0) \
  X("WRITE-FILE", WRITE_FILE, cell_t fd = tos; DROP; cell_t len = tos; DROP; \
    tos = write(fd, (void *) tos, len); tos = tos != len ? errno : 0) \
  X("READ-FILE", READ_FILE, cell_t fd = tos; DROP; cell_t len = tos; DROP; \
    tos = read(fd, (void *) tos, len); PUSH(tos != len ? errno : 0)) \
  X("FILE-POSITION", FILE_POSITION, \
    tos = (cell_t) lseek(tos, 0, SEEK_CUR); PUSH(tos < 0 ? errno : 0)) \
  X("REPOSITION-FILE", REPOSITION_FILE, cell_t fd = tos; DROP; \
    tos = (cell_t) lseek(fd, tos, SEEK_SET); tos = tos < 0 ? errno : 0) \
  X("FILE-SIZE", FILE_SIZE, struct stat st; w = fstat(tos, &st); \
    tos = (cell_t) st.st_size; PUSH(w < 0 ? errno : 0)) \
  /* WiFi */ \
  X("WiFi.config", WIFI_CONFIG, \
      WiFi.config(ToIP(sp[-2]), ToIP(sp[-1]), ToIP(*sp), ToIP(tos)); sp -= 3; DROP) \
  X("WiFi.begin", WIFI_BEGIN, \
      WiFi.begin((const char *) *sp, (const char *) tos); --sp; DROP) \
  X("WiFi.disconnect", WIFI_DISCONNECT, WiFi.disconnect()) \
  X("WiFi.status", WIFI_STATUS, DUP; tos = WiFi.status()) \
  X("WiFi.macAddress", WIFI_MAC_ADDRESS, WiFi.macAddress((uint8_t *) tos); DROP) \
  X("WiFi.localIP", WIFI_LOCAL_IPS, DUP; tos = FromIP(WiFi.localIP())) \
  X("WiFi.mode", WIFI_MODE, WiFi.mode((wifi_mode_t) tos); DROP) \
  X("WiFi.setTxPower", WIFI_SET_TX_POWER, WiFi.setTxPower((wifi_power_t) tos); DROP) \
  X("WiFi.getTxPower", WIFI_GET_TX_POWER, DUP; tos = (cell_t) WiFi.getTxPower()) \
  /* mDNS */ \
  X("MDNS.begin", MDNS_BEGIN, tos = MDNS.begin((const char *) tos)) \
  /* SPIFFS */ \
  X("SPIFFS.begin", SPIFFS_BEGIN, \
      tos = SPIFFS.begin(sp[-1], (const char *) *sp, tos); sp -=2) \
  X("SPIFFS.end", SPIFFS_END, SPIFFS.end()) \
  X("SPIFFS.format", SPIFFS_FORMAT, DUP; tos = SPIFFS.format()) \
  X("SPIFFS.totalBytes", SPIFFS_TOTAL_BYTES, DUP; tos = SPIFFS.totalBytes()) \
  X("SPIFFS.usedBytes", SPIFFS_USED_BYTES, DUP; tos = SPIFFS.usedBytes()) \
  /* WebServer */ \
  X("WebServer.new", WEBSERVER_NEW, DUP; tos = (cell_t) new WebServer(tos)) \
  X("WebServer.delete", WEBSERVER_DELETE, delete (WebServer *) tos; DROP) \
  X("WebServer.begin", WEBSERVER_BEGIN, \
      WebServer *ws = (WebServer *) tos; DROP; ws->begin(tos); DROP) \
  X("WebServer.stop", WEBSERVER_STOP, \
      WebServer *ws = (WebServer *) tos; DROP; ws->stop()) \
  X("WebServer.on", WEBSERVER_ON, \
      InvokeWebServerOn((WebServer *) tos, (const char *) sp[-1], *sp); \
      sp -= 2; DROP) \
  X("WebServer.hasArg", WEBSERVER_HAS_ARG, \
      tos = ((WebServer *) tos)->hasArg((const char *) *sp); DROP) \
  X("WebServer.arg", WEBSERVER_ARG, \
      string_value = ((WebServer *) tos)->arg((const char *) *sp); \
      *sp = (cell_t) string_value.c_str(); tos = string_value.length()) \
  X("WebServer.argi", WEBSERVER_ARGI, \
      string_value = ((WebServer *) tos)->arg(*sp); \
      *sp = (cell_t) string_value.c_str(); tos = string_value.length()) \
  X("WebServer.argName", WEBSERVER_ARG_NAME, \
      string_value = ((WebServer *) tos)->argName(*sp); \
      *sp = (cell_t) string_value.c_str(); tos = string_value.length()) \
  X("WebServer.args", WEBSERVER_ARGS, tos = ((WebServer *) tos)->args()) \
  X("WebServer.setContentLength", WEBSERVER_SET_CONTENT_LENGTH, \
      ((WebServer *) tos)->setContentLength(*sp); --sp; DROP) \
  X("WebServer.sendHeader", WEBSERVER_SEND_HEADER, \
      ((WebServer *) tos)->sendHeader((const char *) sp[-2], (const char *) sp[-1], *sp); \
      sp -= 3; DROP) \
  X("WebServer.send", WEBSERVER_SEND, \
      ((WebServer *) tos)->send(sp[-2], (const char *) sp[-1], (const char *) *sp); \
      sp -= 3; DROP) \
  X("WebServer.sendContent", WEBSERVER_SEND_CONTENT, \
      WebServerSendContent((WebServer *) tos, (const char *) sp[-1], *sp); \
      sp -= 2; DROP) \
  X("WebServer.method", WEBSERVER_METHOD, \
      tos = (cell_t) ((WebServer *) tos)->method()) \
  X("WebServer.handleClient", WEBSERVER_HANDLE_CLIENT, \
      ((WebServer *) tos)->handleClient(); DROP) \

// TODO: Why doesn't ftruncate exist?
//  X("RESIZE-FILE", RESIZE_FILE, cell_t fd = tos; DROP; \
//    tos = ftruncate(fd, tos); tos = tos < 0 ? errno : 0) \

static char filename[PATH_MAX];
static String string_value;

{{core}}
{{boot}}

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

static void InvokeWebServerOn(WebServer *ws, const char *url, cell_t xt) {
  ws->on(url, [xt]() {
    cell_t *old_ip = g_sys.ip;
    cell_t *old_rp = g_sys.rp;
    cell_t *old_sp = g_sys.sp;
    cell_t stack[16];
    cell_t rstack[16];
    g_sys.sp = stack + 1;
    g_sys.rp = rstack;
    cell_t code[2];
    code[0] = xt;
    code[1] = g_sys.YIELD_XT;
    g_sys.ip = code;
    ueforth_run();
    g_sys.ip = old_ip;
    g_sys.rp = old_rp;
    g_sys.sp = old_sp;
  });
}

static void WebServerSendContent(WebServer *ws, const char *data, cell_t len) {
  char buffer[256];
  while (len) {
    if (len < sizeof(buffer) - 1) {
      memcpy(buffer, data, len);
      buffer[len] = 0;
      ws->sendContent(buffer);
      len = 0;
    } else {
      memcpy(buffer, data, sizeof(buffer) - 1);
      buffer[sizeof(buffer)] = 0;
      ws->sendContent(buffer);
      len -= (sizeof(buffer) - 1);
    }
  }
}

void setup() {
  cell_t *heap = (cell_t *) malloc(HEAP_SIZE);
  ueforth(0, 0, heap, boot, sizeof(boot));
}

void loop() {
  ueforth_run();
}
