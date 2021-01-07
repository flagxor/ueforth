{{opcodes}}

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
  X("GPIO", OP_GPIO, ) \

{{core}}
{{boot}}

void setup() {
  cell_t *heap = (cell_t *) malloc(HEAP_SIZE);
  ueforth(0, 0, heap, boot, sizeof(boot));
}

void loop() {
}
