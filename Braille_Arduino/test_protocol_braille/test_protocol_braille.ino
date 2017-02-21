#include "print_debug.h"
extern PrintDebug debug;

//String Navigation[] = {
//"10020100000000000001001003",
//"10020800000000000001001003",
//"10020400000000000001001003",
//"10022000000000000001001003",
//"10020600000000000001001003",
//"10023000000000000001001003",
//"10020300000000000001001003",
//"10021800000000000001001003",
//"10022800000000000001001003",
//"10020500000000000001001003",
//"10020700000000000001001003",
//"10023800000000000001001003",
//"10021D00000000000001001003",
//"10020F00000000000001001003",
//"10023200000000000001001003",
//"10020006000000000000001003",
//"10020030000000000000001003",
//"1002002E000000000000001003"
//};
//
//String Reading[] = {
//"10020024000000000000001003",
//"10020001000000000000001003",
//"10020009000000000000001003",
//"10020004000000000000001003",
//"10020012000000000000001003",
//"10020002000000000000001003",
//"10020010000000000000001003",
//"1002003F000000000000001003",
//"10020025000000000000001003",
//"10020027000000000000001003"
//};
//
//String Cursor[] ={
//"10020008000000000000001003",
//"1002000A000000000000001003",
//"10020020000000000000001003"
//};
//
//String Braille[]={
//"10021200000000000001001003",
//"10020017000000000000001003",
//"1002001C000000000000001003",
//"10020023000000000000001003",
//"1002001B000000000000001003",
//"10021B00000000000001001003",
//"1002003A000000000000001003",
//"10020015000000000000001003",
//"10020016000000000000001003",
//"10020003000000000000001003"
//
//};
//
//String Infomation []={
//"10020013000000000000001003",
//"10020036000000000000001003",
//"10020034000000000000001003",
//"1002001A000000000000001003",
//"1002000E000000000000001003",
//"1002001E000000000000001003",
//"10020035000000000000001003"
//
//};
//
//String Edit []={
//"10022D00000000000001001003",
//"10020900000000000001001003",
//"10022700000000000001001003",
//"10021900000000000001001003",
//"10020037000000000000001003",
//"1002003E000000000000001003",
//"1002002C000000000000001003",
//"1002000B000000000000001003",
//"10020026000000000000001003"
//};

//String Internet[]={
//"10020037000000000000001003",
//"1002003E000000000000001003",
//"1002002C000000000000001003",
//"1002000B000000000000001003",
//"10020026000000000000001003"
//
//};

//1002 01002300000000000000000000000000000000000000000000000000000000000000000000000000 1003

#define UART_MAX_BUFFER     256
#define sizeData            40
byte uart_buff[UART_MAX_BUFFER];
unsigned int uart_index_read = 0;
unsigned int uart_index_write = 0;
unsigned int uart_index_count = 0;
int uart_update = 0;
int SS_pin = 53;
int SCK_pin = 52;
int MOSI_pin = 51;

int status_sw1 = 40;
int status_sw2 = 41;
int status_sw3 = 42;
int status_sw4 = 43;
int status_sw5 = 44;
int status_sw6 = 45;

int nav =0;
int rd = 0;
int cs = 0;
int br = 0;
int inf = 0;
int ed = 0;

void setup() {
  // put your setup code here, to run once:
   Serial.begin(9600);
   Serial2.begin(9600);
   pinMode(SS_pin, OUTPUT);
   pinMode(SCK_pin, OUTPUT);
   pinMode(MOSI_pin, OUTPUT);
   pinMode(status_sw1, INPUT);
   pinMode(status_sw2, INPUT);
   pinMode(status_sw3, INPUT);
   pinMode(status_sw4, INPUT);
   pinMode(status_sw5, INPUT);
   pinMode(status_sw6, INPUT);
  
    //digitalWrite(SS_pin, HIGH);
    digitalWrite(SS_pin, LOW);
        for(int i=319;i>=0;i--){ 
           digitalWrite(MOSI_pin,0);
           digitalWrite(SCK_pin, HIGH);
           delay (1); 
           digitalWrite(SCK_pin, LOW);
           delay (1); 
        }
    digitalWrite(SS_pin, HIGH);
    Serial.println("Start Program");
}

void loop() {
     if (uart_index_write > 1) {
        callback();
     }
     if(digitalRead(status_sw1)==0){
        Serial.println("Left Arrow");
        byte xx[]={0x10,0x02,0x04,0x00,0x00,0x00,0x00,0x00,0x00,0x01,0x00,0x10,0x03};
        debug.print_payload(0,xx,13);
        Serial2.write(xx, sizeof(xx));
        delay(1000); 
     }
     else if(digitalRead(status_sw2)==0){
       Serial.println("Up Arrow");
        byte xx[]={0x10,0x02,0x01,0x00,0x00,0x00,0x00,0x00,0x00,0x01,0x00,0x10,0x03};
        debug.print_payload(0,xx,13);
        Serial2.write(xx, sizeof(xx));
        delay(1000);
     }
     else if(digitalRead(status_sw3)==0){
        Serial.println("Right Arrow");
        byte xx[]={0x10,0x02,0x20,0x00,0x00,0x00,0x00,0x00,0x00,0x01,0x00,0x10,0x03};
        debug.print_payload(0,xx,13);
        Serial2.write(xx, sizeof(xx));
        delay(1000);
     }
     else if(digitalRead(status_sw4)==0){
        Serial.println("Down Arrow");
        byte xx[]={0x10,0x02,0x08,0x00,0x00,0x00,0x00,0x00,0x00,0x01,0x00,0x10,0x03};
        debug.print_payload(0,xx,13);
        Serial2.write(xx, sizeof(xx));
        delay(1000);  
     }
     else if(digitalRead(status_sw5)==0){
        Serial.println("A");
        byte xx[]={0x10,0x02,0x41,0x10,0x03};
        debug.print_payload(0,xx,5);
        Serial2.write(xx, sizeof(xx));
        delay(1000);
     }
     else if(digitalRead(status_sw6)==0){
        Serial.println("ก");
        byte xx[]={0x10,0x02,0x1b,0x10,0x03};
        debug.print_payload(0,xx,5);
        Serial2.write(xx, sizeof(xx));
        delay(1000);
     }
     
}

void serialEvent2() {
  delay(100);
  while (Serial2.available()) {
    uart_buff[uart_index_write++] = (byte)Serial2.read();
    uart_index_count++;
    if (uart_index_write >= UART_MAX_BUFFER) {
      uart_index_read = 0;
      uart_index_write = 0;
    }
  }
}

void callback(){
  if(uart_index_write == 57){
    debug.print_payload(0,uart_buff,uart_index_write);

      if((byte)uart_buff[0] == 0x10 && (byte)uart_buff[1] == 0x02 && (byte)uart_buff[55] == 0x10 && (byte)uart_buff[56] == 0x03){
          Serial.println("Pass");
          byte recvPacket[sizeData];  
          memcpy(recvPacket,&uart_buff[8],sizeData); // Start byte 8
          debug.print_payload(0,recvPacket,sizeData);
          event(recvPacket);
      }
      else {
       Serial.println("Not Pass");
      } 
  }
      uart_index_write = 0;
      uart_index_read = 0;
}

void event(byte buf[]){
      int Num[8] = {0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80};
      int order[320]; // bit
      int res,j,i ,n,m,seg,chk; 
      int count = 39; // data - 1 
      n=0;
      m=8;
        for(seg=0;seg<=count;seg++){
          j=0;
          for(i=n;i<m;i++){
            chk = buf[seg] & Num[j];
            if(chk != 0){
              order[i] = 1;
            }
            else{
              order[i] = 0;
            }
            j++;
          }
          n=m;
          m+=8;
        }
      digitalWrite(SS_pin, LOW);
      for(i=319;i>=0;i--){ 
         //Serial.print(order[i]);
         digitalWrite(MOSI_pin,order[i]);
         digitalWrite(SCK_pin, HIGH);
         delay (1); 
         digitalWrite(SCK_pin, LOW);
         delay (1); 
         //SPI.transfer (order[i]);
      }
      digitalWrite(SS_pin, HIGH); 
      Serial.println("");    
      delay (1000);      
}
// 10 20 BC 00 00 00 00 00 01 01 01 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 00 10 30


