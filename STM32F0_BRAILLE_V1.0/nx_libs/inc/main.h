#ifndef MAIN_H
#define MAIN_H

#include "stm32f0xx_hal.h"
#include "string.h"
#include "uart1_manag.h"
#include "uart2_manag.h"

typedef struct{
	uint16_t length;
	uint8_t value[255];
}data_t;

typedef struct{
	uint16_t idx_r;	//index read
	uint16_t idx_w;	//index write
	uint16_t size;	//size of data
	uint8_t value[400]; //512
}recv_uart_t;

extern recv_uart_t recv_uart_1,recv_uart_2;
extern UART_HandleTypeDef huart1;
extern UART_HandleTypeDef huart2;
void Braille_data(void);
void manag_sw(void);

#endif 