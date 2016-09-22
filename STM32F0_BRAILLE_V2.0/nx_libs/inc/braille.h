#include "main.h"
#include "uart1_manag.h"
//#include "braille_process.h"

#define status 0x01
#define config 0x02
#define data 	 0x03
#define event  0x04

int8_t Braille_read(uint8_t *value, uint16_t *len);
int8_t Braille_write(uint8_t *value, uint16_t len);
void Braille_receivePacket(void);
int Braille_checksum(char buf[],int len);
int Braille_processPacket(char *buf, int len) ;
int Braille_sendAPI(char *frame, int lenght, int timeout);