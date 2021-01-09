/******************************************************************************/
/* esp32Forth, Version 6.3 : for NodeMCU ESP32S                                 */
/******************************************************************************/
/* 16jun25cht  _63                                                            */
/* web server                                                                */
/* 16jun19cht  _62                                                            */
/* structures                                                                 */
/* 14jun19cht  _61                                                            */
/* macro assembler with labels                                                */
/* 10may19cht  _54                                                            */
/* robot tests                                                                */
/* 21jan19cht  _51                                                            */
/* 8 channel electronic organ                                                 */
/* 15jan19cht  _50                                                            */
/* Clean up for AIR robot                                                     */
/* 03jan19cht  _47-49                                                         */
/* Move to ESP32                                                              */
/* 07jan19cht  _46                                                            */
/* delete UDP                                                                 */
/* 03jan19cht  _45                                                            */
/* Move to NodeMCU ESP32S Kit                                                 */
/* 18jul17cht  _44                                                            */
/* Byte code sequencer                                                        */
/* 14jul17cht  _43                                                            */
/* Stacks in circular buffers                                                 */
/* 01jul17cht  _42                                                            */
/* Compiled as an Arduino sketch                                              */
/* 20mar17cht  _41                                                            */
/* Compiled as an Arduino sketch                                              */
/* Follow the ceForth model with 64 primitives                                */
/* Serial Monitor at 115200 baud                                              */
/* Send and receive UDP packets in parallel with Serial Monitor               */
/* Case insensitive interpreter                                               */
/* data[] must be filled with rom42.h eForth dictionary                       */
/* 22jun17cht                                                                 */
/* Stacks are 256 cell circular buffers, with byte pointers R and S           */
/* All references to R and S are forced to (unsigned char)                    */
/* All multiply-divide words cleaned up                                       */
/******************************************************************************/

#include "SPIFFS.h"
#include <WiFi.h>
#include <WebServer.h>
#include "SPIFFS.h"

const char* ssid = "SVFIG";//type your ssid
const char* pass = "12345678";//type your password
// static ip address
IPAddress ip(192,168,1,201); 
IPAddress gateway(192,168,1,1);
IPAddress subnet(255,255,255,0);

WebServer server(80);

/******************************************************************************/
/* esp32Forth_51                                                              */
/******************************************************************************/

# define  FALSE 0
# define  TRUE  -1
# define  LOGICAL ? TRUE : FALSE
# define  LOWER(x,y) ((unsigned long)(x)<(unsigned long)(y))
# define  pop top = stack[(unsigned char)S--]
# define  push stack[(unsigned char)++S] = top; top =
# define  popR rack[(unsigned char)R--]
# define  pushR rack[(unsigned char)++R]

long rack_main[256] = {0};
long stack_main[256] = {0};
long rack_background[256] = {0};
long stack_background[256] = {0};
__thread long *rack;
__thread long *stack;
__thread unsigned char R, S, bytecode ;
__thread long* Pointer ;
__thread long  P, IP, WP, top, links, len ;
uint8_t* cData ;
__thread long long int d, n, m ;
String HTTPin;
String HTTPout;
TaskHandle_t background_thread;

int BRAN=0,QBRAN=0,DONXT=0,DOTQP=0,STRQP=0,TOR=0,ABORQP=0;

//#include "rom_54.h" /* load dictionary */
long data[16000] = {};
int IMEDD=0x80;
int COMPO=0x40;

void HEADER(int lex, char seq[]) {
  P=IP>>2;
  int i;
  int len=lex&31;
  data[P++]=links;
  IP=P<<2;
  Serial.println();
  Serial.print(links,HEX);
  for (i=links>>2;i<P;i++)
     {Serial.print(" ");Serial.print(data[i],HEX);}
  links=IP;
  cData[IP++]=lex;
  for (i=0;i<len;i++)
     {cData[IP++]=seq[i];}
  while (IP&3) {cData[IP++]=0;}
  Serial.println();
  Serial.print(seq);
  Serial.print(" ");
  Serial.print(IP,HEX);
}
int CODE(int len, ... ) {
  int addr=IP;
  int s;
  va_list argList;
  va_start(argList, len);
  for(; len;len--) {
    s= va_arg(argList, int);
    cData[IP++]=s;
    Serial.print(" ");
    Serial.print(s,HEX);
  }
  va_end(argList);
  return addr;
  }
int COLON(int len, ... ) {
  int addr=IP;
  P=IP>>2;
  data[P++]=6; // dolist
  va_list argList;
  va_start(argList, len);
  Serial.println();
  Serial.print(addr,HEX);
  Serial.print(" ");
  Serial.print(6,HEX);
  for(; len;len--) {
    int j=va_arg(argList, int);
    data[P++]=j;
    Serial.print(" ");
    Serial.print(j,HEX);
  }
  IP=P<<2;
  va_end(argList);
  return addr;
  }
int LABEL(int len, ... ) {
  int addr=IP;
  P=IP>>2;
  va_list argList;
  va_start(argList, len);
  Serial.println();
  Serial.print(addr,HEX);
  for(; len;len--) {
    int j=va_arg(argList, int);
    data[P++]=j;
    Serial.print(" ");
    Serial.print(j,HEX);
  }
  IP=P<<2;
  va_end(argList);
  return addr;
  }
void BEGIN(int len, ... ) {
  P=IP>>2;
  Serial.println();
  Serial.print(IP,HEX);
  Serial.print(" BEGIN ");
  pushR=P;
  va_list argList;
  va_start(argList, len);
  for(; len;len--) {
    int j=va_arg(argList, int);
    data[P++]=j;
    Serial.print(" ");
    Serial.print(j,HEX);
  }
  IP=P<<2;
  va_end(argList);
}
void AGAIN(int len, ... ) {
  P=IP>>2;
  Serial.println();
  Serial.print(IP,HEX);
  Serial.print(" AGAIN ");
  data[P++]=BRAN; 
  data[P++]=popR<<2; 
  va_list argList;
  va_start(argList, len);
  for(; len;len--) {
    int j=va_arg(argList, int);
    data[P++]=j;
    Serial.print(" ");
    Serial.print(j,HEX);
  }
  IP=P<<2;
  va_end(argList);
  }
void UNTIL(int len, ... ) {
  P=IP>>2;
  Serial.println();
  Serial.print(IP,HEX);
  Serial.print(" UNTIL ");
  data[P++]=QBRAN; 
  data[P++]=popR<<2; 
  va_list argList;
  va_start(argList, len);
  for(; len;len--) {
    int j=va_arg(argList, int);
    data[P++]=j;
    Serial.print(" ");
    Serial.print(j,HEX);
  }
  IP=P<<2;
  va_end(argList);
  }
void WHILE(int len, ... ) {
  P=IP>>2;
  int k;
  Serial.println();
  Serial.print(IP,HEX);
  Serial.print(" WHILE ");
  data[P++]=QBRAN; 
  data[P++]=0; 
  k=popR;
  pushR=(P-1);
  pushR=k;
  va_list argList;
  va_start(argList, len);
  for(; len;len--) {
    int j=va_arg(argList, int);
    data[P++]=j;
    Serial.print(" ");
    Serial.print(j,HEX);
  }
  IP=P<<2;
  va_end(argList);
  }
void REPEAT(int len, ... ) {
  P=IP>>2;
  Serial.println();
  Serial.print(IP,HEX);
  Serial.print(" REPEAT ");
  data[P++]=BRAN; 
  data[P++]=popR<<2; 
  data[popR]=P<<2;
  va_list argList;
  va_start(argList, len);
  for(; len;len--) {
    int j=va_arg(argList, int);
    data[P++]=j;
    Serial.print(" ");
    Serial.print(j,HEX);
  }
  IP=P<<2;
  va_end(argList);
  }
void IF(int len, ... ) {
  P=IP>>2;
  Serial.println();
  Serial.print(IP,HEX);
  Serial.print(" IF ");
  data[P++]=QBRAN; 
  pushR=P;
  data[P++]=0; 
  va_list argList;
  va_start(argList, len);
  for(; len;len--) {
    int j=va_arg(argList, int);
    data[P++]=j;
    Serial.print(" ");
    Serial.print(j,HEX);
  }
  IP=P<<2;
  va_end(argList);
  }
void ELSE(int len, ... ) {
  P=IP>>2;
  Serial.println();
  Serial.print(IP,HEX);
  Serial.print(" ELSE ");
  data[P++]=BRAN; 
  data[P++]=0; 
  data[popR]=P<<2; 
  pushR=P-1;
  va_list argList;
  va_start(argList, len);
  for(; len;len--) {
    int j=va_arg(argList, int);
    data[P++]=j;
    Serial.print(" ");
    Serial.print(j,HEX);
  }
  IP=P<<2;
  va_end(argList);
  }
void THEN(int len, ... ) {
  P=IP>>2;
  Serial.println();
  Serial.print(IP,HEX);
  Serial.print(" THEN ");
  data[popR]=P<<2; 
  va_list argList;
  va_start(argList, len);
  for(; len;len--) {
    int j=va_arg(argList, int);
    data[P++]=j;
    Serial.print(" ");
    Serial.print(j,HEX);
  }
  IP=P<<2;
  va_end(argList);
  }
void FOR(int len, ... ) {
  P=IP>>2;
  Serial.println();
  Serial.print(IP,HEX);
  Serial.print(" FOR ");
  data[P++]=TOR; 
  pushR=P;
  va_list argList;
  va_start(argList, len);
  for(; len;len--) {
    int j=va_arg(argList, int);
    data[P++]=j;
    Serial.print(" ");
    Serial.print(j,HEX);
  }
  IP=P<<2;
  va_end(argList);
  }
void NEXT(int len, ... ) {
  P=IP>>2;
  Serial.println();
  Serial.print(IP,HEX);
  Serial.print(" NEXT ");
  data[P++]=DONXT; 
  data[P++]=popR<<2; 
  va_list argList;
  va_start(argList, len);
  for(; len;len--) {
    int j=va_arg(argList, int);
    data[P++]=j;
    Serial.print(" ");
    Serial.print(j,HEX);
  }
  IP=P<<2;
  va_end(argList);
  }
void AFT(int len, ... ) {
  P=IP>>2;
  int k;
  Serial.println();
  Serial.print(IP,HEX);
  Serial.print(" AFT ");
  data[P++]=BRAN; 
  data[P++]=0; 
  k=popR;
  pushR=P;
  pushR=P-1;
  va_list argList;
  va_start(argList, len);
  for(; len;len--) {
    int j=va_arg(argList, int);
    data[P++]=j;
    Serial.print(" ");
    Serial.print(j,HEX);
  }
  IP=P<<2;
  va_end(argList);
  }
void DOTQ(char seq[]) {
  P=IP>>2;
  int i;
  int len=strlen(seq);
  data[P++]=DOTQP;
  IP=P<<2;
  cData[IP++]=len;
  for (i=0;i<len;i++)
     {cData[IP++]=seq[i];}
  while (IP&3) {cData[IP++]=0;}
  Serial.println();
  Serial.print(IP,HEX);
  Serial.print(" ");
  Serial.print(seq);
}
void STRQ(char seq[]) {
  P=IP>>2;
  int i;
  int len=strlen(seq);
  data[P++]=STRQP;
  IP=P<<2;
  cData[IP++]=len;
  for (i=0;i<len;i++)
     {cData[IP++]=seq[i];}
  while (IP&3) {cData[IP++]=0;}
  Serial.println();
  Serial.print(IP,HEX);
  Serial.print(" ");
  Serial.print(seq);
}
void ABORQ(char seq[]) {
  P=IP>>2;
  int i;
  int len=strlen(seq);
  data[P++]=ABORQP;
  IP=P<<2;
  cData[IP++]=len;
  for (i=0;i<len;i++)
     {cData[IP++]=seq[i];}
  while (IP&3) {cData[IP++]=0;}
  Serial.println();
  Serial.print(IP,HEX);
  Serial.print(" ");
  Serial.print(seq);
}

void CheckSum() {
  int i;
  char sum=0;
  Serial.println();
  Serial.printf("%4x ",IP);
  for (i=0;i<32;i++) {
    sum += cData[IP];
    Serial.printf("%2x",cData[IP++]);
  }
  Serial.printf(" %2x",sum);
}
/******************************************************************************/
/* ledc                                                                       */
/******************************************************************************/
/* LEDC Software Fade */
// use first channel of 16 channels (started from zero)
#define LEDC_CHANNEL_0     0
// use 13 bit precission for LEDC timer
#define LEDC_TIMER_13_BIT  13
// use 5000 Hz as a LEDC base frequency
#define LEDC_BASE_FREQ     5000
// fade LED PIN (replace with LED_BUILTIN constant for built-in LED)
#define LED_PIN            5
int brightness = 255;    // how bright the LED is

// Arduino like analogWrite
// value has to be between 0 and valueMax
void ledcAnalogWrite(uint8_t channel, uint32_t value, uint32_t valueMax = 255) {
  // calculate duty, 8191 from 2 ^ 13 - 1
  uint32_t duty = (8191 / valueMax) * min(value, valueMax);
  // write duty to LEDC
  ledcWrite(channel, duty);
}

/******************************************************************************/
/* PRIMITIVES                                                                 */
/******************************************************************************/

void next(void)
{ P = data[IP>>2];
  IP += 4; 
  WP = P+4;  }

void accep()
/* WiFiClient */
{ while (Serial.available()) {
    len = Serial.readBytes(cData, top); }
  Serial.write(cData, len);
  top = len;
}
void qrx(void)
  { while (Serial.available() == 0) {};
    push Serial.read();
    push -1; }

void txsto(void)
{  Serial.write( (unsigned char) top);
   char c=top;
   HTTPout += c ;
   pop; 
} 

void docon(void)
{  push data[WP>>2]; }

void dolit(void)
{   push data[IP>>2];
  IP += 4;
  next(); }

void dolist(void)
{   rack[(unsigned char)++R] = IP;
  IP = WP; 
  next(); }

void exitt(void)
{   IP = (long) rack[(unsigned char)R--];
  next(); }

void execu(void)
{  P = top;
  WP = P + 4;
  pop; }

void donext(void)
{   if(rack[(unsigned char)R]) {
    rack[(unsigned char)R] -= 1 ;
    IP = data[IP>>2]; 
  } else { IP += 4;  (unsigned char)R-- ;  }
  next(); }

void qbran(void)
{   if(top == 0) IP = data[IP>>2]; 
  else IP += 4;  pop; 
  next(); }

void bran(void)
{   IP = data[IP>>2]; 
  next(); }

void store(void)
{   data[top>>2] = stack[(unsigned char)S--];
  pop;  }

void at(void)
{   top = data[top>>2];  }

void cstor(void)
{   cData[top] = (unsigned char) stack[(unsigned char)S--];
  pop;  }

void cat(void)
{   top = (long) cData[top];  }

void rpat(void) {}
void rpsto(void) {}

void rfrom(void)
{   push rack[(unsigned char)R--];  }

void rat(void)
{   push rack[(unsigned char)R];  }

void tor(void)
{   rack[(unsigned char)++R] = top;  pop;  }

void spat(void) {}
void spsto(void) {}

void drop(void)
{   pop;  }

void dup(void)
{   stack[(unsigned char)++S] = top;  }

void swap(void)
{   WP = top;
  top = stack[(unsigned char)S];
  stack[(unsigned char)S] = WP;  }

void over(void)
{  push stack[(unsigned char)(S-1)];  }

void zless(void)
{   top = (top < 0) LOGICAL;  }

void andd(void)
{   top &= stack[(unsigned char)S--];  }

void orr(void)
{   top |= stack[(unsigned char)S--];  }

void xorr(void)
{   top ^= stack[(unsigned char)S--];  }

void uplus(void)
{   stack[(unsigned char)S] += top;
  top = LOWER(stack[(unsigned char)S], top);  } 

void nop(void)
{   next(); } 

void qdup(void)
{   if(top) stack[(unsigned char)++S] = top ;  }

void rot(void)
{   WP = stack[(unsigned char)(S-1)];
  stack[(unsigned char)(S-1)] = stack[(unsigned char)S];
  stack[(unsigned char)S] = top;
  top = WP;  }

void ddrop(void)
{   drop(); drop();  }

void ddup(void)
{   over(); over();  }

void plus(void)
{   top += stack[(unsigned char)S--];  }

void inver(void)
{   top = -top-1;  }

void negat(void)
{   top = 0 - top;  }

void dnega(void)
{   inver();
  tor();
  inver(); 
  push 1;
  uplus();
  rfrom();
  plus(); }

void subb(void)
{   top = stack[(unsigned char)S--] - top;  }

void abss(void)
{   if(top < 0)
    top = -top;  }

void great(void)
{   top = (stack[(unsigned char)S--] > top) LOGICAL;  }

void less(void)
{   top = (stack[(unsigned char)S--] < top) LOGICAL;  }

void equal(void)
{   top = (stack[(unsigned char)S--] == top) LOGICAL;  }

void uless(void)
{   top = LOWER(stack[(unsigned char)S], top) LOGICAL; S--;  }

void ummod(void)
{  d = (long long int)((unsigned long)top);
  m = (long long int)((unsigned long)stack[(unsigned char) S]);
  n = (long long int)((unsigned long)stack[(unsigned char) (S - 1)]);
  n += m << 32;
  pop;
  top = (unsigned long)(n / d);
  stack[(unsigned char) S] = (unsigned long)(n%d); }
void msmod(void)
{ d = (signed long long int)((signed long)top);
  m = (signed long long int)((signed long)stack[(unsigned char) S]);
  n = (signed long long int)((signed long)stack[(unsigned char) S - 1]);
  n += m << 32;
  pop;
  top = (signed long)(n / d);
  stack[(unsigned char) S] = (signed long)(n%d); }
void slmod(void)
{ if (top != 0) {
    WP = stack[(unsigned char) S] / top;
    stack[(unsigned char) S] %= top;
    top = WP;
  } }
void mod(void)
{ top = (top) ? stack[(unsigned char) S--] % top : stack[(unsigned char) S--]; }
void slash(void)
{ top = (top) ? stack[(unsigned char) S--] / top : (stack[(unsigned char) S--], 0); }
void umsta(void)
{ d = (unsigned long long int)top;
  m = (unsigned long long int)stack[(unsigned char) S];
  m *= d;
  top = (unsigned long)(m >> 32);
  stack[(unsigned char) S] = (unsigned long)m; }
void star(void)
{ top *= stack[(unsigned char) S--]; }
void mstar(void)
{ d = (signed long long int)top;
  m = (signed long long int)stack[(unsigned char) S];
  m *= d;
  top = (signed long)(m >> 32);
  stack[(unsigned char) S] = (signed long)m; }
void ssmod(void)
{ d = (signed long long int)top;
  m = (signed long long int)stack[(unsigned char) S];
  n = (signed long long int)stack[(unsigned char) (S - 1)];
  n *= m;
  pop;
  top = (signed long)(n / d);
  stack[(unsigned char) S] = (signed long)(n%d); }
void stasl(void)
{ d = (signed long long int)top;
  m = (signed long long int)stack[(unsigned char) S];
  n = (signed long long int)stack[(unsigned char) (S - 1)];
  n *= m;
  pop; pop;
  top = (signed long)(n / d); }

void pick(void)
{   top = stack[(unsigned char)(S-top)];  }

void pstor(void)
{   data[top>>2] += stack[(unsigned char)S--], pop;  }

void dstor(void)
{   data[(top>>2)+1] = stack[(unsigned char)S--];
  data[top>>2] = stack[(unsigned char)S--];
  pop;  }

void dat(void)
{   push data[top>>2];
  top = data[(top>>2)+1];  }

void count(void)
{   stack[(unsigned char)++S] = top + 1;
  top = cData[top];  }

void dovar(void)
{   push WP; }

void maxx(void)
{   if (top < stack[(unsigned char)S]) pop;
  else (unsigned char)S--; }

void minn(void)
{   if (top < stack[(unsigned char)S]) (unsigned char)S--;
  else pop; }

void audio(void)
{  WP=top; pop;
   ledcWriteTone(WP,top);
   pop;
}

void sendPacket(void)
{}

void poke(void)
{   Pointer = (long*)top; *Pointer = stack[(unsigned char)S--];
    pop;  }

void peeek(void)
{   Pointer = (long*)top; top = *Pointer;  }

void adc(void)
{  top= (long) analogRead(top); }

void pin(void)
{  WP=top; pop;
   ledcAttachPin(top,WP);
   pop;
}

void duty(void)
{  WP=top; pop;
   ledcAnalogWrite(WP,top,255);
   pop;
}

void freq(void)
{  WP=top; pop;
   ledcSetup(WP,top,13);
   pop;
}

void (*primitives[72])(void) = {
    /* case 0 */ nop,
    /* case 1 */ accep, 
    /* case 2 */ qrx,    
    /* case 3 */ txsto,  
    /* case 4 */ docon,   
    /* case 5 */ dolit,
    /* case 6 */ dolist,
    /* case 7 */ exitt,
    /* case 8 */ execu,
    /* case 9 */ donext,
    /* case 10 */ qbran,
    /* case 11 */ bran,
    /* case 12 */ store,
    /* case 13 */ at,
    /* case 14 */ cstor,
    /* case 15 */ cat,
    /* case 16 */ nop,
    /* case 17 */ nop,
    /* case 18 */ rfrom,
    /* case 19 */ rat,
    /* case 20 */ tor,
    /* case 21 */ nop, 
    /* case 22 */ nop,
    /* case 23 */ drop,
    /* case 24 */ dup,
    /* case 25 */ swap,
    /* case 26 */ over,
    /* case 27 */ zless,
    /* case 28 */ andd,
    /* case 29 */ orr,
    /* case 30 */ xorr,
    /* case 31 */ uplus,
    /* case 32 */ next, 
    /* case 33 */ qdup, 
    /* case 34 */ rot, 
    /* case 35 */ ddrop, 
    /* case 36 */ ddup, 
    /* case 37 */ plus,
    /* case 38 */ inver, 
    /* case 39 */ negat, 
    /* case 40 */ dnega, 
    /* case 41 */ subb, 
    /* case 42 */ abss,
    /* case 43 */ equal, 
    /* case 44 */ uless, 
    /* case 45 */ less,   
    /* case 46 */ ummod,
    /* case 47 */ msmod,
    /* case 48 */ slmod, 
    /* case 49 */ mod,  
    /* case 50 */ slash, 
    /* case 51 */ umsta,   
    /* case 52 */ star, 
    /* case 53 */ mstar, 
    /* case 54 */ ssmod, 
    /* case 55 */ stasl, 
    /* case 56 */ pick, 
    /* case 57 */ pstor, 
    /* case 58 */ dstor, 
    /* case 59 */ dat, 
    /* case 60 */ count, 
    /* case 61 */ dovar, 
    /* case 62 */ maxx, 
    /* case 63 */ minn,
    /* case 64 */ audio,
    /* case 65 */ sendPacket,
    /* case 66 */ poke,
    /* case 67 */ peeek, 
    /* case 68 */ adc,
    /* case 69 */ pin,
    /* case 70 */ duty, 
    /* case 71 */ freq };

int as_nop=0;
int as_accept=1;
int as_qrx=2;
int as_txsto=3;
int as_docon=4;
int as_dolit=5;
int as_dolist=6;
int as_exit=7;
int as_execu=8;
int as_donext=9;
int as_qbran=10;
int as_bran=11;
int as_store=12;
int as_at=13;
int as_cstor=14;
int as_cat=15;
int as_rpat=16;
int as_rpsto=17;
int as_rfrom=18;
int as_rat=19;
int as_tor=20;
int as_spat=21;
int as_spsto=22;
int as_drop=23;
int as_dup=24;
int as_swap=25;
int as_over=26;
int as_zless=27;
int as_andd=28;
int as_orr=29;
int as_xorr=30;
int as_uplus=31;
int as_next=32;
int as_qdup=33;
int as_rot=34;
int as_ddrop=35;
int as_ddup=36;
int as_plus=37;
int as_inver=38;
int as_negat=39;
int as_dnega=40;
int as_subb=41;
int as_abss=42;
int as_equal=43;
int as_uless=44;
int as_less=45;
int as_ummod=46;
int as_msmod=47;
int as_slmod=48;
int as_mod=49;
int as_slash=50;
int as_umsta=51;
int as_star=52;
int as_mstar=53;
int as_ssmod=54;
int as_stasl=55;
int as_pick=56;
int as_pstor=57;
int as_dstor=58;
int as_dat=59;
int as_count=60;
int as_dovar=61;
int as_max=62;
int as_min=63;
int as_tone=64;
int as_sendPacket=65;
int as_poke=66;
int as_peek=67;
int as_adc=68;
int as_pin=69;
int as_duty=70;
int as_freq=71;

//void evaluate()
//{ while (true){
//    bytecode=(unsigned char)cData[P++];
//    if (bytecode) {primitives[bytecode]();}
//    else {break;} 
//  }                 // break on NOP
//}
  
__thread int counter = 0;
void evaluate()
{ while (true){
    if (counter++ > 10000) {
      delay(1);
      counter = 0;
    }
    bytecode=(unsigned char)cData[P++];
    if (bytecode) {primitives[bytecode]();}
    else {break;}
  }                 // break on NOP
}

static const char *index_html =
"<!html>\n"
"<head>\n"
"<title>esp32forth</title>\n"
"<style>\n"
"body {\n"
"  padding: 5px;\n"
"  background-color: #111;\n"
"  color: #2cf;\n"
"}\n"
"#prompt {\n"
"  width: 100%;\n"
"  padding: 5px;\n"
"  font-family: monospace;\n"
"  background-color: #ff8;\n"
"}\n"
"#output {\n"
"  width: 100%;\n"
"  height: 80%;\n"
"  resize: none;\n"
"}\n"
"</style>\n"
"</head>\n"
"<h2>esp32forth</h2>\n"
"<link rel=\"icon\" href=\"data:,\">\n"
"<body>\n"
"Upload File: <input id=\"filepick\" type=\"file\" name=\"files[]\"></input><br/>\n"
"<button onclick=\"ask('hex')\">hex</button>\n"
"<button onclick=\"ask('decimal')\">decimal</button>\n"
"<button onclick=\"ask('words')\">words</button>\n"
"<button onclick=\"ask('$100 init hush')\">init</button>\n"
"<button onclick=\"ask('ride')\">ride</button>\n"
"<button onclick=\"ask('blow')\">blow</button>\n"
"<button onclick=\"ask('$50000 p0')\">fore</button>\n"
"<button onclick=\"ask('$a0000 p0')\">back</button>\n"
"<button onclick=\"ask('$10000 p0')\">left</button>\n"
"<button onclick=\"ask('$40000 p0')\">right</button>\n"
"<button onclick=\"ask('$90000 p0')\">spin</button>\n"
"<button onclick=\"ask('0 p0')\">stop</button>\n"
"<button onclick=\"ask('4 p0s')\">LED</button>\n"
"<button onclick=\"ask('$24 ADC . $27 ADC . $22 ADC . $23 ADC .')\">ADC</button>\n"
"<br/>\n"
"<textarea id=\"output\" readonly></textarea>\n"
"<input id=\"prompt\" type=\"prompt\"></input><br/>\n"
"<script>\n"
"var prompt = document.getElementById('prompt');\n"
"var filepick = document.getElementById('filepick');\n"
"var output = document.getElementById('output');\n"
"function httpPost(url, items, callback) {\n"
"  var fd = new FormData();\n"
"  for (k in items) {\n"
"    fd.append(k, items[k]);\n"
"  }\n"
"  var r = new XMLHttpRequest();\n"
"  r.onreadystatechange = function() {\n"
"    if (this.readyState == XMLHttpRequest.DONE) {\n"
"      if (this.status === 200) {\n"
"        callback(this.responseText);\n"
"      } else {\n"
"        callback(null);\n"
"      }\n"
"    }\n"
"  };\n"
"  r.open('POST', url);\n"
"  r.send(fd);\n"
"}\n"
"function ask(cmd, callback) {\n"
"  httpPost('/input',\n"
"           {cmd: cmd + '\\n'}, function(data) {\n"
"    if (data !== null) { output.value += data; }\n"
"    output.scrollTop = output.scrollHeight;  // Scroll to the bottom\n"
"    if (callback !== undefined) { callback(); }\n"
"  });\n"
"}\n"
"prompt.onkeyup = function(event) {\n"
"  if (event.keyCode === 13) {\n"
"    event.preventDefault();\n"
"    ask(prompt.value);\n"
"    prompt.value = '';\n"
"  }\n"
"};\n"
"filepick.onchange = function(event) {\n"
"  if (event.target.files.length > 0) {\n"
"    var reader = new FileReader();\n"
"    reader.onload = function(e) {\n"
"      var parts = e.target.result.split('\\n');\n"
"      function upload() {\n"
"        if (parts.length === 0) { filepick.value = ''; return; }\n"
"        ask(parts.shift(), upload);\n"
"      }\n"
"      upload();\n"
"    }\n"
"    reader.readAsText(event.target.files[0]);\n"
"  }\n"
"};\n"
"window.onload = function() {\n"
"  ask('');\n"
"  prompt.focus();\n"
"};\n"
"</script>\n"
;

static void returnFail(String msg) {
  server.send(500, "text/plain", msg + "\r\n");
}

static void handleInput() {
  if (!server.hasArg("cmd")) {
    return returnFail("Missing Input");
  }
  HTTPin = server.arg("cmd");
  HTTPout = "";
  Serial.println(HTTPin);  // line cleaned up
  len = HTTPin.length();
  HTTPin.getBytes(cData, len);
  //Serial.println("Enter Forth.");
  data[0x66] = 0;                   // >IN
  data[0x67] = len;                 // #TIB
  data[0x68] = 0;                   // 'TIB
  if (len > 3 && memcmp(cData, "bg ", 3) == 0) {
    if (background_thread) {
      vTaskDelete(background_thread);
      background_thread = 0;
    }
    data[0x66] = 3; // Skip "bg "
    // Start background thread 1024 byte stack.
    xTaskCreate(background, "background", 1024, &IP, tskIDLE_PRIORITY, &background_thread);
  } else {
    P = 0x180;                        // EVAL
    WP = 0x184;
    evaluate();
  }
//  Serial.println();
//  Serial.println("Return from Forth.");           // line cleaned up
//  Serial.print("Returning ");
  Serial.print(HTTPout.length());
//  Serial.println(" characters");
  server.setContentLength(HTTPout.length());
  server.send(200, "text/plain", HTTPout);
}

void background(void *ipp) {
  long *ipv = (long*) ipp;
  rack = rack_background;
  stack = stack_background;
  Serial.println("background!!");
  IP = *ipv;
  S = 0;
  R = 0;
  top = 0;
  P = 0x180;                        // EVAL
  WP = 0x184;
  evaluate();
  for(;;) {
  }
}
void setup() {
  
  rack = rack_main;
  stack = stack_main;
  P = 0x180;
  WP = 0x184;
  IP = 0;
  S = 0;
  R = 0;
  top = 0;
  cData = (uint8_t *) data;
  Serial.begin(115200);
  delay(100);
//  WiFi.config(ip, gateway, subnet);
  WiFi.mode(WIFI_STA);
// attempt to connect to Wifi network:
  WiFi.begin(ssid, pass);
  while (WiFi.status() != WL_CONNECTED) {
    delay(500);
    Serial.print(".");
  }
  Serial.println("");
  Serial.println("WiFi connected");
  Serial.print("IP Address: ");
  Serial.println(WiFi.localIP());
  // if you get a connection, report back via serial:
  server.begin();
  Serial.println("Booting esp32Forth v6.3 ...");

// Setup timer and attach timer to a led pin
  ledcSetup(0, 100, LEDC_TIMER_13_BIT);
  ledcAttachPin(5, 0);
  ledcAnalogWrite(0, 250, brightness);
  pinMode(2,OUTPUT);
  digitalWrite(2, HIGH);   // turn the LED2 on 
  pinMode(16,OUTPUT);
  digitalWrite(16, LOW);   // motor1 forward
  pinMode(17,OUTPUT);
  digitalWrite(17, LOW);   // motor1 backward 
  pinMode(18,OUTPUT);
  digitalWrite(18, LOW);   // motor2 forward 
  pinMode(19,OUTPUT);
  digitalWrite(19, LOW);   // motor2 bacward

  IP=512;
  R=0;
  HEADER(3,"HLD");
  int HLD=CODE(8,as_docon,as_next,0,0,0X90,1,0,0);
  HEADER(4,"SPAN");
  int SPAN=CODE(8,as_docon,as_next,0,0,0X94,1,0,0);
  HEADER(3,">IN");
  int INN=CODE(8,as_docon,as_next,0,0,0X98,1,0,0);
  HEADER(4,"#TIB");
  int NTIB=CODE(8,as_docon,as_next,0,0,0X9C,1,0,0);
  HEADER(4,"'TIB");
  int TTIB=CODE(8,as_docon,as_next,0,0,0XA0,1,0,0);
  HEADER(4,"BASE");
  int BASE=CODE(8,as_docon,as_next,0,0,0XA4,1,0,0);
  HEADER(7,"CONTEXT");
  int CNTXT=CODE(8,as_docon,as_next,0,0,0XA8,1,0,0);
  HEADER(2,"CP");
  int CP=CODE(8,as_docon,as_next,0,0,0XAC,1,0,0);
  HEADER(4,"LAST");
  int LAST=CODE(8,as_docon,as_next,0,0,0XB0,1,0,0);
  HEADER(5,"'EVAL");
  int TEVAL=CODE(8,as_docon,as_next,0,0,0XB4,1,0,0);
  HEADER(6,"'ABORT");
  int TABRT=CODE(8,as_docon,as_next,0,0,0XB8,1,0,0);
  HEADER(3,"tmp");
  int TEMP=CODE(8,as_docon,as_next,0,0,0XBC,1,0,0);
  HEADER(1,"Z");
  int Z=CODE(8,as_docon,as_next,0,0,0,0,0,0);
  HEADER(4,"ppqn");
  int PPQN=CODE(8,as_docon,as_next,0,0,0XC0,1,0,0);
  HEADER(7,"channel");
  int CHANN=CODE(8,as_docon,as_next,0,0,0XC4,1,0,0);

  HEADER(3,"NOP");
  int NOP=CODE(4,as_nop,as_next,0,0);
  HEADER(6,"ACCEPT");
  int ACCEP=CODE(4,as_accept,as_next,0,0);
  HEADER(4,"?KEY");
  int QKEY=CODE(4,as_qrx,as_next,0,0);  
  HEADER(4,"EMIT");
  int EMIT=CODE(4,as_txsto,as_next,0,0);  
  HEADER(5,"DOLIT");
  int DOLIT=CODE(4,as_dolit,as_next,0,0); 
  HEADER(6,"DOLIST");
  int DOLST=CODE(4,as_dolist,as_next,0,0);
  HEADER(4,"EXIT");
  int EXITT=CODE(4,as_exit,as_next,0,0);
  HEADER(7,"EXECUTE");
  int EXECU=CODE(4,as_execu,as_next,0,0);
  HEADER(6,"DONEXT");
  DONXT=CODE(4,as_donext,as_next,0,0);
  HEADER(7,"QBRANCH");
  QBRAN=CODE(4,as_qbran,as_next,0,0);
  HEADER(6,"BRANCH");
  BRAN=CODE(4,as_bran,as_next,0,0);
  HEADER(1,"!");
  int STORE=CODE(4,as_store,as_next,0,0);
  HEADER(1,"@");
  int AT=CODE(4,as_at,as_next,0,0);
  HEADER(2,"C!");
  int CSTOR=CODE(4,as_cstor,as_next,0,0);
  HEADER(2,"C@");
  int CAT=CODE(4,as_cat,as_next,0,0);
  HEADER(2,"R>");
  int RFROM=CODE(4,as_rfrom,as_next,0,0);
  HEADER(2,"R@");
  int RAT=CODE(4,as_rat,as_next,0,0);
  HEADER(2,">R");
  TOR=CODE(4,as_tor,as_next,0,0);
  HEADER(4,"DROP");
  int DROP=CODE(4,as_drop,as_next,0,0);
  HEADER(3,"DUP");
  int DUPP=CODE(4,as_dup,as_next,0,0);
  HEADER(4,"SWAP");
  int SWAP=CODE(4,as_swap,as_next,0,0);
  HEADER(4,"OVER");
  int OVER=CODE(4,as_over,as_next,0,0);
  HEADER(2,"0<");
  int ZLESS=CODE(4,as_zless,as_next,0,0);
  HEADER(3,"AND");
  int ANDD=CODE(4,as_andd,as_next,0,0);
  HEADER(2,"OR");
  int ORR=CODE(4,as_orr,as_next,0,0);
  HEADER(3,"XOR");
  int XORR=CODE(4,as_xorr,as_next,0,0);
  HEADER(3,"UM+");
  int UPLUS=CODE(4,as_uplus,as_next,0,0);
  HEADER(4,"?DUP");
  int QDUP=CODE(4,as_qdup,as_next,0,0); 
  HEADER(3,"ROT");
  int ROT=CODE(4,as_rot,as_next,0,0); 
  HEADER(5,"2DROP");
  int DDROP=CODE(4,as_ddrop,as_next,0,0); 
  HEADER(4,"2DUP");
  int DDUP=CODE(4,as_ddup,as_next,0,0); 
  HEADER(1,"+");
  int PLUS=CODE(4,as_plus,as_next,0,0);
  HEADER(3,"NOT");
  int INVER=CODE(4,as_inver,as_next,0,0);
  HEADER(6,"NEGATE");
  int NEGAT=CODE(4,as_negat,as_next,0,0); 
  HEADER(7,"DNEGATE");
  int DNEGA=CODE(4,as_dnega,as_next,0,0); 
  HEADER(1,"-");
  int SUBBB=CODE(4,as_subb,as_next,0,0); 
  HEADER(3,"ABS");
  int ABSS=CODE(4,as_abss,as_next,0,0);
  HEADER(1,"=");
  int EQUAL=CODE(4,as_equal,as_next,0,0); 
  HEADER(2,"U<");
  int ULESS=CODE(4,as_uless,as_next,0,0); 
  HEADER(1,"<");
  int LESS=CODE(4,as_less,as_next,0,0);
  HEADER(6,"UM/MOD");
  int UMMOD=CODE(4,as_ummod,as_next,0,0);
  HEADER(5,"M/MOD");
  int MSMOD=CODE(4,as_msmod,as_next,0,0);
  HEADER(4,"/MOD");
  int SLMOD=CODE(4,as_slmod,as_next,0,0); 
  HEADER(3,"MOD");
  int MODD=CODE(4,as_mod,as_next,0,0);   
  HEADER(1,"/");
  int SLASH=CODE(4,as_slash,as_next,0,0);
  HEADER(3,"UM*");
  int UMSTA=CODE(4,as_umsta,as_next,0,0);   
  HEADER(1,"*");
  int STAR=CODE(4,as_star,as_next,0,0); 
  HEADER(2,"M*");
  int MSTAR=CODE(4,as_mstar,as_next,0,0); 
  HEADER(5,"*/MOD");
  int SSMOD=CODE(4,as_ssmod,as_next,0,0); 
  HEADER(2,"*/");
  int STASL=CODE(4,as_stasl,as_next,0,0);
  HEADER(4,"PICK");
  int PICK=CODE(4,as_pick,as_next,0,0); 
  HEADER(2,"+!");
  int PSTOR=CODE(4,as_pstor,as_next,0,0); 
  HEADER(2,"2!");
  int DSTOR=CODE(4,as_dstor,as_next,0,0); 
  HEADER(2,"2@");
  int DAT=CODE(4,as_dat,as_next,0,0);
  HEADER(5,"COUNT");
  int COUNT=CODE(4,as_count,as_next,0,0); 
  HEADER(3,"MAX");
  int MAX=CODE(4,as_max,as_next,0,0);
  HEADER(3,"MIN");
  int MIN=CODE(4,as_min,as_next,0,0);
  HEADER(2,"BL");
  int BLANK=CODE(8,as_docon,as_next,0,0,32,0,0,0);
  HEADER(4,"CELL");
  int CELL=CODE(8,as_docon,as_next,0,0, 4,0,0,0);
  HEADER(5,"CELL+");
  int CELLP=CODE(8,as_docon,as_plus,as_next,0, 4,0,0,0);
  HEADER(5,"CELL-");
  int CELLM=CODE(8,as_docon,as_subb,as_next,0,4,0,0,0);
  HEADER(5,"CELLS");
  int CELLS=CODE(8,as_docon,as_star,as_next,0,4,0,0,0);
  HEADER(5,"CELL/");
  int CELLD=CODE(8,as_docon,as_slash,as_next,0,4,0,0,0);
  HEADER(2,"1+");
  int ONEP=CODE(8,as_docon,as_plus,as_next,0,1,0,0,0);
  HEADER(2,"1-");
  int ONEM=CODE(8,as_docon,as_subb,as_next,0,1,0,0,0);
  HEADER(2,"2+");
  int TWOP=CODE(8,as_docon,as_plus,as_next,0,2,0,0,0);
  HEADER(2,"2-");
  int TWOM=CODE(8,as_docon,as_subb,as_next,0,2,0,0,0);
  HEADER(2,"2*");
  int TWOST=CODE(8,as_docon,as_star,as_next,0,2,0,0,0);
  HEADER(2,"2/");
  int TWOS=CODE(8,as_docon,as_slash,as_next,0,2,0,0,0);
  HEADER(10,"sendPacket");
  int SENDP=CODE(4,as_sendPacket,as_next,0,0);
  HEADER(4,"POKE");
  int POKE=CODE(4,as_poke,as_next,0,0);
  HEADER(4,"PEEK");
  int PEEK=CODE(4,as_peek,as_next,0,0);
  HEADER(3,"ADC");
  int ADC=CODE(4,as_adc,as_next,0,0);
  HEADER(3,"PIN");
  int PIN=CODE(4,as_pin,as_next,0,0);
  HEADER(4,"TONE");
  int TONE=CODE(4,as_tone,as_next,0,0);
  HEADER(4,"DUTY");
  int DUTY=CODE(4,as_duty,as_next,0,0);
  HEADER(4,"FREQ");
  int FREQ=CODE(4,as_freq,as_next,0,0);

  HEADER(3,"KEY");
  int KEY=COLON(0);
  BEGIN(1,QKEY);
  UNTIL(1,EXITT);
  HEADER(6,"WITHIN");
  int WITHI=COLON(7,OVER,SUBBB,TOR,SUBBB,RFROM,ULESS,EXITT);
  HEADER(5,">CHAR");
  int TCHAR=COLON(8,DOLIT,0x7F,ANDD,DUPP,DOLIT,127,BLANK,WITHI);
  IF(3,DROP,DOLIT,0X5F);
  THEN(1,EXITT);
  HEADER(7,"ALIGNED");
  int ALIGN=COLON(7,DOLIT,3,PLUS,DOLIT,0XFFFFFFFC,ANDD,EXITT);
  HEADER(4,"HERE");
  int HERE=COLON(3,CP,AT,EXITT);
  HEADER(3,"PAD");
  int PAD=COLON(5,HERE,DOLIT,80,PLUS,EXITT);
  HEADER(3,"TIB");
  int TIB=COLON(3,TTIB,AT,EXITT);
  HEADER(8,"@EXECUTE");
  int ATEXE=COLON(2,AT,QDUP);
  IF(1,EXECU);
  THEN(1,EXITT);
  HEADER(5,"CMOVE");
  int CMOVEE=COLON(0);
  FOR(0);
  AFT(8,OVER,CAT,OVER,CSTOR,TOR,ONEP,RFROM,ONEP);
  THEN(0);
  NEXT(2,DDROP,EXITT);
  HEADER(4,"MOVE");
  int MOVE=COLON(1,CELLD);
  FOR(0);
  AFT(8,OVER,AT,OVER,STORE,TOR,CELLP,RFROM,CELLP);
  THEN(0);
  NEXT(2,DDROP,EXITT);
  HEADER(4,"FILL");
  int FILL=COLON(1,SWAP);
  FOR(1,SWAP);
  AFT(3,DDUP,CSTOR,ONEP);
  THEN(0);
  NEXT(2,DDROP,EXITT);
  HEADER(5,"DIGIT");
  int DIGIT=COLON(12,DOLIT,9,OVER,LESS,DOLIT,7,ANDD,PLUS,DOLIT,0X30,PLUS,EXITT);
  HEADER(7,"EXTRACT");
  int EXTRC=COLON(7,DOLIT,0,SWAP,UMMOD,SWAP,DIGIT,EXITT);
  HEADER(2,"<#");
  int BDIGS=COLON(4,PAD,HLD,STORE,EXITT);
  HEADER(4,"HOLD");
  int HOLD=COLON(8,HLD,AT,ONEM,DUPP,HLD,STORE,CSTOR,EXITT);
  HEADER(1,"#");
  int DIG=COLON(5,BASE,AT,EXTRC,HOLD,EXITT);
  HEADER(2,"#S");
  int DIGS=COLON(0);
  BEGIN(2,DIG,DUPP);
  WHILE(0);
  REPEAT(1,EXITT);
  HEADER(4,"SIGN");
  int SIGN=COLON(1,ZLESS);
  IF(3,DOLIT,0X2D,HOLD);
  THEN(1,EXITT);
  HEADER(2,"#>");
  int EDIGS=COLON(7,DROP,HLD,AT,PAD,OVER,SUBBB,EXITT);
  HEADER(3,"str");
  int STRR=COLON(9,DUPP,TOR,ABSS,BDIGS,DIGS,RFROM,SIGN,EDIGS,EXITT);
  HEADER(3,"HEX");
  int HEXX=COLON(5,DOLIT,16,BASE,STORE,EXITT);
  HEADER(7,"DECIMAL");
  int DECIM=COLON(5,DOLIT,10,BASE,STORE,EXITT);
  HEADER(6,"wupper");
  int UPPER=COLON(4,DOLIT,0x5F5F5F5F,ANDD,EXITT);
  HEADER(6,">upper");
  int TOUPP=COLON(6,DUPP,DOLIT,0x61,DOLIT,0x7B,WITHI);
  IF(3,DOLIT,0x5F,ANDD);
  THEN(1,EXITT);
  HEADER(6,"DIGIT?");
  int DIGTQ=COLON(9,TOR,TOUPP,DOLIT,0X30,SUBBB,DOLIT,9,OVER,LESS);
  IF(8,DOLIT,7,SUBBB,DUPP,DOLIT,10,LESS,ORR);
  THEN(4,DUPP,RFROM,ULESS,EXITT);
  HEADER(7,"NUMBER?");
  int NUMBQ=COLON(12,BASE,AT,TOR,DOLIT,0,OVER,COUNT,OVER,CAT,DOLIT,0X24,EQUAL);
  IF(5,HEXX,SWAP,ONEP,SWAP,ONEM);
  THEN(13,OVER,CAT,DOLIT,0X2D,EQUAL,TOR,SWAP,RAT,SUBBB,SWAP,RAT,PLUS,QDUP);
  IF(1,ONEM);
  FOR(6,DUPP,TOR,CAT,BASE,AT,DIGTQ);
  WHILE(7,SWAP,BASE,AT,STAR,PLUS,RFROM,ONEP);
  NEXT(2,DROP,RAT);
  IF(1,NEGAT);
  THEN(1,SWAP);
  ELSE(6,RFROM,RFROM,DDROP,DDROP,DOLIT,0);
  THEN(1,DUPP);
  THEN(6,RFROM,DDROP,RFROM,BASE,STORE,EXITT);
  HEADER(5,"SPACE");
  int SPACE=COLON(3,BLANK,EMIT,EXITT);
  HEADER(5,"CHARS");
  int CHARS=COLON(4,SWAP,DOLIT,0,MAX);
  FOR(0);
  AFT(2,DUPP,EMIT);
  THEN(0);
  NEXT(2,DROP,EXITT);
  HEADER(6,"SPACES");
  int SPACS=COLON(3,BLANK,CHARS,EXITT);
  HEADER(4,"TYPE");
  int TYPES=COLON(0);
  FOR(0);
  AFT(5,DUPP,CAT,TCHAR,EMIT,ONEP);
  THEN(0);
  NEXT(2,DROP,EXITT);
  HEADER(2,"CR");
  int CR=COLON(7,DOLIT,10,DOLIT,13,EMIT,EMIT,EXITT);
  HEADER(3,"do$");
  int DOSTR=COLON(10,RFROM,RAT,RFROM,COUNT,PLUS,ALIGN,TOR,SWAP,TOR,EXITT);
  HEADER(3,"$\"|");
  int STRQP=COLON(2,DOSTR,EXITT);
  HEADER(3,".\"|");
  DOTQP=COLON(4,DOSTR,COUNT,TYPES,EXITT);
  HEADER(2,".R");
  int DOTR=COLON(8,TOR,STRR,RFROM,OVER,SUBBB,SPACS,TYPES,EXITT);
  HEADER(3,"U.R");
  int UDOTR=COLON(10,TOR,BDIGS,DIGS,EDIGS,RFROM,OVER,SUBBB,SPACS,TYPES,EXITT);
  HEADER(2,"U.");
  int UDOT=COLON(6,BDIGS,DIGS,EDIGS,SPACE,TYPES,EXITT);
  HEADER(1,".");
  int DOT=COLON(5,BASE,AT,DOLIT,10,XORR);
  IF(3,UDOT,EXITT);
  THEN(4,STRR,SPACE,TYPES,EXITT);
  HEADER(1,"?");
  int QUEST=COLON(3,AT,DOT,EXITT);
  HEADER(7,"(parse)");
  int PARS=COLON(5,TEMP,CSTOR,OVER,TOR,DUPP);
  IF(5,ONEM,TEMP,CAT,BLANK,EQUAL);
  IF(0);
  FOR(6,BLANK,OVER,CAT,SUBBB,ZLESS,INVER);
  WHILE(1,ONEP);
  NEXT(6,RFROM,DROP,DOLIT,0,DUPP,EXITT);
  THEN(1,RFROM);
  THEN(2,OVER,SWAP);
  FOR(9,TEMP,CAT,OVER,CAT,SUBBB,TEMP,CAT,BLANK,EQUAL);
  IF(1,ZLESS);
  THEN(0);
  WHILE(1,ONEP);
  NEXT(2,DUPP,TOR);
  ELSE(5,RFROM,DROP,DUPP,ONEP,TOR);
  THEN(6,OVER,SUBBB,RFROM,RFROM,SUBBB,EXITT);
  THEN(4,OVER,RFROM,SUBBB,EXITT);
  HEADER(5,"PACK$");
  int PACKS=COLON(18,DUPP,TOR,DDUP,PLUS,DOLIT,0xFFFFFFFC,ANDD,DOLIT,0,SWAP,STORE,DDUP,CSTOR,ONEP,SWAP,CMOVEE,RFROM,EXITT);
  HEADER(5,"PARSE");
  int PARSE=COLON(15,TOR,TIB,INN,AT,PLUS,NTIB,AT,INN,AT,SUBBB,RFROM,PARS,INN,PSTOR,EXITT);
  HEADER(5,"TOKEN");
  int TOKEN=COLON(9,BLANK,PARSE,DOLIT,0x1F,MIN,HERE,CELLP,PACKS,EXITT);
  HEADER(4,"WORD");
  int WORDD=COLON(5,PARSE,HERE,CELLP,PACKS,EXITT);
  HEADER(5,"NAME>");
  int NAMET=COLON(7,COUNT,DOLIT,0x1F,ANDD,PLUS,ALIGN,EXITT);
  HEADER(5,"SAME?");
  int SAMEQ=COLON(4,DOLIT,0x1F,ANDD,CELLD);
  FOR(0);
  AFT(18,OVER,RAT,DOLIT,4,STAR,PLUS,AT,UPPER,OVER,RAT,DOLIT,4,STAR,PLUS,AT,UPPER,SUBBB,QDUP);
  IF(3,RFROM,DROP,EXITT);
  THEN(0);
  THEN(0);
  NEXT(3,DOLIT,0,EXITT);
  HEADER(4,"find");
  int FIND=COLON(10,SWAP,DUPP,AT,TEMP,STORE,DUPP,AT,TOR,CELLP,SWAP);
  BEGIN(2,AT,DUPP);
  IF(9,DUPP,AT,DOLIT,0xFFFFFF3F,ANDD,UPPER,RAT,UPPER,XORR);
  IF(3,CELLP,DOLIT,0XFFFFFFFF);
  ELSE(4,CELLP,TEMP,AT,SAMEQ);
  THEN(0);
  ELSE(6,RFROM,DROP,SWAP,CELLM,SWAP,EXITT);
  THEN(0);
  WHILE(2,CELLM,CELLM);
  REPEAT(9,RFROM,DROP,SWAP,DROP,CELLM,DUPP,NAMET,SWAP,EXITT);
  HEADER(5,"NAME?");
  int NAMEQ=COLON(3,CNTXT,FIND,EXITT);
  HEADER(6,"EXPECT");
  int EXPEC=COLON(5,ACCEP,SPAN,STORE,DROP,EXITT);
  HEADER(5,"QUERY");
  int QUERY=COLON(12,TIB,DOLIT,0X100,ACCEP,NTIB,STORE,DROP,DOLIT,0,INN,STORE,EXITT);
  HEADER(5,"ABORT");
  int ABORT=COLON(4,NOP,TABRT,ATEXE,EXITT);
  HEADER(6,"abort\"");
  ABORQP=COLON(0);
  IF(4,DOSTR,COUNT,TYPES,ABORT);
  THEN(3,DOSTR,DROP,EXITT);
  HEADER(5,"ERROR");
  int ERRORR=COLON(8,SPACE,COUNT,TYPES,DOLIT,0x3F,EMIT,CR,ABORT);
  HEADER(10,"$INTERPRET");
  int INTER=COLON(2,NAMEQ,QDUP);
  IF(4,CAT,DOLIT,COMPO,ANDD);
  ABORQ(" compile only");
  int INTER0=LABEL(2,EXECU,EXITT);
  THEN(1,NUMBQ);
  IF(1,EXITT);
  THEN(1,ERRORR);
  HEADER(IMEDD+1,"[");
  int LBRAC=COLON(5,DOLIT,INTER,TEVAL,STORE,EXITT);
  HEADER(3,".OK");
  int DOTOK=COLON(6,CR,DOLIT,INTER,TEVAL,AT,EQUAL);
  IF(14,TOR,TOR,TOR,DUPP,DOT,RFROM,DUPP,DOT,RFROM,DUPP,DOT,RFROM,DUPP,DOT);
  DOTQ(" ok>");
  THEN(1,EXITT);
  HEADER(4,"EVAL");
  int EVAL=COLON(1,LBRAC);
  BEGIN(3,TOKEN,DUPP,AT);
  WHILE(2,TEVAL,ATEXE);
  REPEAT(4,DROP,DOTOK,NOP,EXITT);
  HEADER(4,"QUIT");
  int QUITT=COLON(1,LBRAC);
  BEGIN(2,QUERY,EVAL);
  AGAIN(0);
  HEADER(4,"LOAD");
  int LOAD=COLON(10,NTIB,STORE,TTIB,STORE,DOLIT,0,INN,STORE,EVAL,EXITT);
  HEADER(1,",");
  int COMMA=COLON(7,HERE,DUPP,CELLP,CP,STORE,STORE,EXITT);
  HEADER(IMEDD+7,"LITERAL");
  int LITER=COLON(5,DOLIT,DOLIT,COMMA,COMMA,EXITT);
  HEADER(5,"ALLOT");
  int ALLOT=COLON(4,ALIGN,CP,PSTOR,EXITT);
  HEADER(3,"$,\"");
  int STRCQ=COLON(9,DOLIT,0X22,WORDD,COUNT,PLUS,ALIGN,CP,STORE,EXITT);
  HEADER(7,"?UNIQUE");
  int UNIQU=COLON(3,DUPP,NAMEQ,QDUP);
  IF(6,COUNT,DOLIT,0x1F,ANDD,SPACE,TYPES);
  DOTQ(" reDef");
  THEN(2,DROP,EXITT);
  HEADER(3,"$,n");
  int SNAME=COLON(2,DUPP,AT);
  IF(14,UNIQU,DUPP,NAMET,CP,STORE,DUPP,LAST,STORE,CELLM,CNTXT,AT,SWAP,STORE,EXITT);
  THEN(1,ERRORR);
  HEADER(1,"'");
  int TICK=COLON(2,TOKEN,NAMEQ);
  IF(1,EXITT);
  THEN(1,ERRORR);
  HEADER(IMEDD+9,"[COMPILE]");
  int BCOMP=COLON(3,TICK,COMMA,EXITT);
  HEADER(7,"COMPILE");
  int COMPI=COLON(7,RFROM,DUPP,AT,COMMA,CELLP,TOR,EXITT);
  HEADER(8,"$COMPILE");
  int SCOMP=COLON(2,NAMEQ,QDUP);
  IF(4,AT,DOLIT,IMEDD,ANDD);
  IF(1,EXECU);
  ELSE(1,COMMA);
  THEN(1,EXITT);
  THEN(1,NUMBQ);
  IF(2,LITER,EXITT);
  THEN(1,ERRORR);
  HEADER(5,"OVERT");
  int OVERT=COLON(5,LAST,AT,CNTXT,STORE,EXITT);
  HEADER(1,"]");
  int RBRAC=COLON(5,DOLIT,SCOMP,TEVAL,STORE,EXITT);
  HEADER(1,":");
  int COLN=COLON(7,TOKEN,SNAME,RBRAC,DOLIT,0x6,COMMA,EXITT);
  HEADER(IMEDD+1,";");
  int SEMIS=COLON(6,DOLIT,EXITT,COMMA,LBRAC,OVERT,EXITT);
  HEADER(3,"dm+");
  int DMP=COLON(4,OVER,DOLIT,6,UDOTR);
  FOR(0);
  AFT(6,DUPP,AT,DOLIT,9,UDOTR,CELLP);
  THEN(0);
  NEXT(1,EXITT);
  HEADER(4,"DUMP");
  int DUMP=COLON(10,BASE,AT,TOR,HEXX,DOLIT,0x1F,PLUS,DOLIT,0x20,SLASH);
  FOR(0);
  AFT(10,CR,DOLIT,8,DDUP,DMP,TOR,SPACE,CELLS,TYPES,RFROM);
  THEN(0);
  NEXT(5,DROP,RFROM,BASE,STORE,EXITT);
  HEADER(5,">NAME");
  int TNAME=COLON(1,CNTXT);
  BEGIN(2,AT,DUPP);
  WHILE(3,DDUP,NAMET,XORR);
  IF(1,ONEM);
  ELSE(3,SWAP,DROP,EXITT);
  THEN(0);
  REPEAT(3,SWAP,DROP,EXITT);
  HEADER(3,".ID");
  int DOTID=COLON(7,COUNT,DOLIT,0x1F,ANDD,TYPES,SPACE,EXITT);
  HEADER(5,"WORDS");
  int WORDS=COLON(6,CR,CNTXT,DOLIT,0,TEMP,STORE);
  BEGIN(2,AT,QDUP);
  WHILE(9,DUPP,SPACE,DOTID,CELLM,TEMP,AT,DOLIT,0x10,LESS);
  IF(4,DOLIT,1,TEMP,PSTOR);
  ELSE(5,CR,DOLIT,0,TEMP,STORE);
  THEN(0);
  REPEAT(1,EXITT);
  HEADER(6,"FORGET");
  int FORGT=COLON(3,TOKEN,NAMEQ,QDUP);
  IF(12,CELLM,DUPP,CP,STORE,AT,DUPP,CNTXT,STORE,LAST,STORE,DROP,EXITT);
  THEN(1,ERRORR);
  HEADER(4,"COLD");
  int COLD=COLON(1,CR);
  DOTQ("esp32forth V6.3, 2019 ");
  int DOTQ1=LABEL(2,CR,EXITT);
  HEADER(4,"LINE");
  int LINE=COLON(2,DOLIT,0x7);
  FOR(6,DUPP,PEEK,DOLIT,0x9,UDOTR,CELLP);
  NEXT(1,EXITT);
  HEADER(2,"PP");
  int PP=COLON(0);
  FOR(0);
  AFT(7,CR,DUPP,DOLIT,0x9,UDOTR,SPACE,LINE);
  THEN(0);
  NEXT(1,EXITT);
  HEADER(2,"P0");
  int P0=COLON(4,DOLIT,0x3FF44004,POKE,EXITT);
  HEADER(3,"P0S");
  int P0S=COLON(4,DOLIT,0x3FF44008,POKE,EXITT);
  HEADER(3,"P0C");
  int P0C=COLON(4,DOLIT,0x3FF4400C,POKE,EXITT);
  HEADER(2,"P1");
  int P1=COLON(4,DOLIT,0x3FF44010,POKE,EXITT);
  HEADER(3,"P1S");
  int P1S=COLON(4,DOLIT,0x3FF44014,POKE,EXITT);
  HEADER(3,"P1C");
  int P1C=COLON(4,DOLIT,0x3FF44018,POKE,EXITT);
  HEADER(4,"P0EN");
  int P0EN=COLON(4,DOLIT,0x3FF44020,POKE,EXITT);
  HEADER(5,"P0ENS");
  int P0ENS=COLON(4,DOLIT,0x3FF44024,POKE,EXITT);
  HEADER(5,"P0ENC");
  int P0ENC=COLON(4,DOLIT,0x3FF44028,POKE,EXITT);
  HEADER(4,"P1EN");
  int P1EN=COLON(4,DOLIT,0x3FF4402C,POKE,EXITT);
  HEADER(5,"P1ENS");
  int P1ENS=COLON(4,DOLIT,0x3FF44030,POKE,EXITT);
  HEADER(5,"P1ENC");
  int P1ENC=COLON(4,DOLIT,0x3FF44034,POKE,EXITT);
  HEADER(4,"P0IN");
  int P0IN=COLON(5,DOLIT,0x3FF4403C,PEEK,DOT,EXITT);
  HEADER(4,"P1IN");
  int P1IN=COLON(5,DOLIT,0x3FF44040,PEEK,DOT,EXITT);
  HEADER(3,"PPP");
  int PPP=COLON(7,DOLIT,0x3FF44000,DOLIT,3,PP,DROP,EXITT);
  HEADER(5,"EMITT");
  int EMITT=COLON(2,DOLIT,0x3);
  FOR(8,DOLIT,0,DOLIT,0x100,MSMOD,SWAP,TCHAR,EMIT);
  NEXT(2,DROP,EXITT);
  HEADER(5,"TYPEE");
  int TYPEE=COLON(3,SPACE,DOLIT,0x7);
  FOR(4,DUPP,PEEK,EMITT,CELLP);
  NEXT(2,DROP,EXITT);
  HEADER(4,"PPPP");
  int PPPP=COLON(0);
  FOR(0);
  AFT(10,CR,DUPP,DUPP,DOLIT,0x9,UDOTR,SPACE,LINE,SWAP,TYPEE);
  THEN(0);
  NEXT(1,EXITT);
  HEADER(3,"KKK");
  int KKK=COLON(7,DOLIT,0x3FF59000,DOLIT,0x10,PP,DROP,EXITT);
  HEADER(IMEDD+4,"THEN");
  int THENN=COLON(4,HERE,SWAP,STORE,EXITT);
  HEADER(IMEDD+3,"FOR");
  int FORR=COLON(4,COMPI,TOR,HERE,EXITT);
  HEADER(IMEDD+5,"BEGIN");
  int BEGIN=COLON(2,HERE,EXITT);
  HEADER(IMEDD+4,"NEXT");
  int NEXT=COLON(4,COMPI,DONXT,COMMA,EXITT);
  HEADER(IMEDD+5,"UNTIL");
  int UNTIL=COLON(4,COMPI,QBRAN,COMMA,EXITT);
  HEADER(IMEDD+5,"AGAIN");
  int AGAIN=COLON(4,COMPI,BRAN,COMMA,EXITT);
  HEADER(IMEDD+2,"IF");
  int IFF=COLON(7,COMPI,QBRAN,HERE,DOLIT,0,COMMA,EXITT);
  HEADER(IMEDD+5,"AHEAD");
  int AHEAD=COLON(7,COMPI,BRAN,HERE,DOLIT,0,COMMA,EXITT);
  HEADER(IMEDD+6,"REPEAT");
  int REPEA=COLON(3,AGAIN,THENN,EXITT);
  HEADER(IMEDD+3,"AFT");
  int AFT=COLON(5,DROP,AHEAD,HERE,SWAP,EXITT);
  HEADER(IMEDD+4,"ELSE");
  int ELSEE=COLON(4,AHEAD,SWAP,THENN,EXITT);
  HEADER(IMEDD+5,"WHILE");
  int WHILEE=COLON(3,IFF,SWAP,EXITT);
  HEADER(IMEDD+6,"ABORT\"");
  int ABRTQ=COLON(6,DOLIT,ABORQP,HERE,STORE,STRCQ,EXITT);
  HEADER(IMEDD+2,"$\"");
  int STRQ=COLON(6,DOLIT,STRQP,HERE,STORE,STRCQ,EXITT);
  HEADER(IMEDD+2,".\"");
  int DOTQQ=COLON(6,DOLIT,DOTQP,HERE,STORE,STRCQ,EXITT);
  HEADER(4,"CODE");
  int CODE=COLON(5,TOKEN,SNAME,OVERT,ALIGN,EXITT);
  HEADER(6,"CREATE");
  int CREAT=COLON(5,CODE,DOLIT,0x203D,COMMA,EXITT);
  HEADER(8,"VARIABLE");
  int VARIA=COLON(5,CREAT,DOLIT,0,COMMA,EXITT);
  HEADER(8,"CONSTANT");
  int CONST=COLON(6,CODE,DOLIT,0x2004,COMMA,COMMA,EXITT);
  HEADER(IMEDD+2,".(");
  int DOTPR=COLON(5,DOLIT,0X29,PARSE,TYPES,EXITT);
  HEADER(IMEDD+1,"\\");
  int BKSLA=COLON(5,DOLIT,0xA,WORDD,DROP,EXITT);
  HEADER(IMEDD+1,"(");
  int PAREN=COLON(5,DOLIT,0X29,PARSE,DDROP,EXITT);
  HEADER(12,"COMPILE-ONLY");
  int ONLY=COLON(6,DOLIT,0x40,LAST,AT,PSTOR,EXITT);
  HEADER(9,"IMMEDIATE");
  int IMMED=COLON(6,DOLIT,0x80,LAST,AT,PSTOR,EXITT);
  int ENDD=IP;
  Serial.println();
  Serial.print("IP=");
  Serial.print(IP);
  Serial.print(" R-stack= ");
  Serial.print(popR<<2,HEX);
  IP=0x180;
  int USER=LABEL(16,6,EVAL,0,0,0,0,0,0,0,0x10,IMMED-12,ENDD,IMMED-12,INTER,EVAL,0);

// dump dictionary
  IP=0;
  for (len=0;len<0x120;len++){CheckSum();}

// compile \data\load.txt  
  if(!SPIFFS.begin(true)){Serial.println("Error mounting SPIFFS"); }
  File file = SPIFFS.open("/load.txt");
  if(file) {
    Serial.print("Load file: ");
    len = file.read(cData+0x8000,0x7000); 
    Serial.print(len);
    Serial.println(" bytes.");
    data[0x66] = 0;                   // >IN
    data[0x67] = len;                 // #TIB
    data[0x68] = 0x8000;              // 'TIB
    P = 0x180;                        // EVAL
    WP = 0x184;
    evaluate();
    Serial.println(" Done loading."); 
    file.close();
    SPIFFS.end();
  }
  // Setup web server handlers
  server.on("/", HTTP_GET, []() {
    server.send(200, "text/html", index_html);
  });
  server.on("/input", HTTP_POST, handleInput);
  server.begin();
  Serial.println("HTTP server started");
}

void loop() {
  server.handleClient();
}
