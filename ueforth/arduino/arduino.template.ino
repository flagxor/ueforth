{{opcodes}}

#include "SPIFFS.h"
#include <WiFi.h>
#include <WebServer.h>

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
  /* Serial */ \
  X("Serial.begin", SERIAL_BEGIN, Serial.begin(tos); DROP) \
  X("Serial.end", SERIAL_END, Serial.end()) \
  X("Serial.available", SERIAL_AVAILABLE, DUP; tos = Serial.available()) \
  X("Serial.readBytes", SERIAL_READ_BYTES, tos = Serial.readBytes((uint8_t *) *sp, tos); --sp) \
  X("Serial.write", SERIAL_WRITE, tos = Serial.write((const uint8_t *) *sp, tos); --sp) \
  /* Pins and PWM */ \
  X("pinMode", PIN_MODE, pinMode(*sp, tos); --sp; DROP) \
  X("digitalWrite", DIGITAL_WRITE, digitalWrite(*sp, tos); --sp; DROP) \
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
  X("R/O", R_O, *++sp = O_RDONLY) \
  X("R/W", R_W, *++sp = O_RDWR) \
  X("W/O", W_O, *++sp = O_WRONLY) \
  X("BIN", BIN, ) \
  X("CLOSE-FILE", CLOSE_FILE, tos = close(tos); tos = tos ? errno : 0) \
  X("OPEN-FILE", OPEN_FILE, cell_t mode = tos; DROP; cell_t len = tos; DROP; \
    memcpy(filename, (void *) tos, len); filename[len] = 0; \
    tos = open(filename, mode, 0777); PUSH tos < 0 ? errno : 0) \
  X("CREATE-FILE", CREATE_FILE, cell_t mode = tos; DROP; cell_t len = tos; DROP; \
    memcpy(filename, (void *) tos, len); filename[len] = 0; \
    tos = open(filename, mode | O_CREAT | O_TRUNC); PUSH tos < 0 ? errno : 0) \
  X("DELETE-FILE", DELETE_FILE, cell_t len = tos; DROP; \
    memcpy(filename, (void *) tos, len); filename[len] = 0; \
    tos = unlink(filename); tos = tos ? errno : 0) \
  X("WRITE-FILE", WRITE_FILE, cell_t fd = tos; DROP; cell_t len = tos; DROP; \
    tos = write(fd, (void *) tos, len); tos = tos != len ? errno : 0) \
  X("READ-FILE", READ_FILE, cell_t fd = tos; DROP; cell_t len = tos; DROP; \
    tos = read(fd, (void *) tos, len); PUSH tos != len ? errno : 0) \
  X("FILE-POSITION", FILE_POSITION, \
    tos = (cell_t) lseek(tos, 0, SEEK_CUR); PUSH tos < 0 ? errno : 0) \
  X("REPOSITION-FILE", REPOSITION_FILE, cell_t fd = tos; DROP; \
    tos = (cell_t) lseek(fd, tos, SEEK_SET); tos = tos < 0 ? errno : 0) \
  X("FILE-SIZE", FILE_SIZE, struct stat st; w = fstat(tos, &st); \
    tos = (cell_t) st.st_size; PUSH w < 0 ? errno : 0) \
  /* WiFi */ \
  X("WiFi.config", WIFI_CONFIG, \
      WiFi.config(ToIP(sp[-1]), ToIP(*sp), ToIP(tos)); sp -= 2; DROP) \
  X("WiFi.begin", WIFI_BEGIN, \
      WiFi.begin((const char *) *sp, (const char *) tos); --sp; DROP) \
  X("WiFi.disconnect", WIFI_DISCONNECT, WiFi.disconnect()) \
  X("WiFi.status", WIFI_STATUS, DUP; tos = WiFi.status()) \
  X("WiFi.macAddress", WIFI_MAC_ADDRESS, WiFi.macAddress((uint8_t *) tos); DROP) \
  X("WiFi.localIP", WIFI_LOCAL_IPS, DUP; tos = FromIP(WiFi.localIP())) \
  /* SPIFFS */ \
  X("SPIFFS.begin", SPIFFS_BEGIN, tos = SPIFFS.begin(tos)) \
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

// TODO: Why doesn't ftruncate exist?
//  X("RESIZE-FILE", RESIZE_FILE, cell_t fd = tos; DROP; \
//    tos = ftruncate(fd, tos); tos = tos < 0 ? errno : 0) \

static char filename[PATH_MAX];

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

void setup() {
  cell_t *heap = (cell_t *) malloc(HEAP_SIZE);
  ueforth(0, 0, heap, boot, sizeof(boot));
}

void loop() {
}
