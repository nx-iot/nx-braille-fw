#include "main.h"
#include "uart1_manag.h"

int8_t Braille_read(uint8_t *value, uint16_t *len);
int8_t Braille_write(uint8_t *value, uint16_t len);
void Braille_receivePacket(void);
int Braille_checksum(char buf[],int len);