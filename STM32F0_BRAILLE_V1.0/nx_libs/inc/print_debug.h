#ifndef PRINT_DEBUG_H
#define PRINT_DEBUG_H

//#include "stm32f4xx_hal.h"

#include "main.h"

#include "stdio.h"
#include "string.h"
#include "stdint.h"
#include "stdarg.h"
#include "ctype.h"




#define DEBG_L0			(uint8_t)0x00
#define DEBG_L1			(uint8_t)0x01
#define DEBG_L2			(uint8_t)0x02
#define DEBG_L3			(uint8_t)0x03


void print_init(UART_HandleTypeDef *huart);
void print_debug(uint8_t level, char *fmtstr, ...);
static void print_hex_ascii_line(const unsigned char *payload, int len, int offset);
void print_payload(uint8_t level, const uint8_t * buff, const uint16_t len);
void print_hex_ascii_line(const unsigned char *payload, int len, int offset);

#endif