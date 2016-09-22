#ifndef UART2_MANAG_H
#define UART2_MANAG_H

#include "main.h"
#include "print_debug.h"

#define MAX_BUFF_UART2	512

void uart2_callback();
int8_t uart2_read(uint8_t *value,uint16_t *len,uint16_t size);
int8_t uart2_write(uint8_t *value,uint16_t len);

extern void uart2_callback();

#endif 