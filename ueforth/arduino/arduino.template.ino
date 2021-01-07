{{opcodes}}

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
  X("MS", MS, mspause(tos); DROP) \
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

// TODO: Why doesn't ftruncate exist?
//  X("RESIZE-FILE", RESIZE_FILE, cell_t fd = tos; DROP; \
//    tos = ftruncate(fd, tos); tos = tos < 0 ? errno : 0) \

static char filename[PATH_MAX];

{{core}}
{{boot}}

static void mspause(cell_t ms) {
  vTaskDelay(ms / portTICK_PERIOD_MS);
}

void setup() {
  cell_t *heap = (cell_t *) malloc(HEAP_SIZE);
  ueforth(0, 0, heap, boot, sizeof(boot));
}

void loop() {
}
