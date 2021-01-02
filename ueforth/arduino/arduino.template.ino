{{opcodes}}

#define PLATFORM_OPCODE_LIST \
  X("GPIO", OP_GPIO, ) \

{{core}}
{{boot}}

void setup() {
  ueforth(boot, sizeof(boot));
}

void loop() {
}
