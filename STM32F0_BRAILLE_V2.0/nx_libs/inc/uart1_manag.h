#ifndef UART1_MANAG_H
#define UART1_MANAG_H

#include "main.h"
#include "print_debug.h"

#define MAX_BUFF_UART1	255

void uart1_callback();
int8_t uart1_read(uint8_t *value,uint16_t *len,uint16_t size);
int8_t uart1_write(uint8_t *value,uint16_t len);
void uart1_flush();

extern void uart1_callback();

#endif 