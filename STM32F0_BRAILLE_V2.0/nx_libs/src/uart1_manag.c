#include "uart1_manag.h"

extern UART_HandleTypeDef huart1;
extern recv_uart_t recv_uart_1;
uint8_t uart_1_buff[5];

void uart1_callback(){
	if(HAL_UART_Receive_IT(&huart1,uart_1_buff,1)==HAL_OK){
		if(recv_uart_1.size < MAX_BUFF_UART1){
			recv_uart_1.value[recv_uart_1.idx_w++] = uart_1_buff[0];
			recv_uart_1.size++;
		}else{
			//HAL_UART_Transmit(&huart1,(uint8_t*)"Uart1 Over flow\n",16,100);
			print_debug(0,"[ERROR]{Uart1}Over flow\n");
			recv_uart_1.idx_w = 0;
			recv_uart_1.idx_r = 0;
			recv_uart_1.size = 0;
		}
		//////////////////////////////////////////
		//HAL_UART_Transmit(&huart2,uart_1_buff,1,10);
	}
}

int8_t uart1_read(uint8_t *value,uint16_t *len,uint16_t size){
	int8_t ret = 0;
	
	if(recv_uart_1.size>0){
		if((size > 0) && (size <= recv_uart_1.size)){
			memcpy(value,&recv_uart_1.value[recv_uart_1.idx_r],size);
			memcpy(len,&recv_uart_1.size,sizeof(uint16_t));
			recv_uart_1.idx_r += recv_uart_1.size;
			recv_uart_1.size -= recv_uart_1.size;
			ret = 1;
			if(recv_uart_1.idx_r > MAX_BUFF_UART1){
				//HAL_UART_Transmit(&huart1,(uint8_t*)"Uart2 Read Over flow\n",16,100);
				print_debug(0,"[ERROR]{Uart1}Over flow\n");
				recv_uart_1.idx_w = 0;
				recv_uart_1.idx_r = 0;
				recv_uart_1.size = 0;
				ret = 0;
			}
		}else{
			//HAL_UART_Transmit(&huart2,&recv_uart_2.value[recv_uart_2.idx_r],recv_uart_2.size,10);
			memcpy(value,&recv_uart_1.value[recv_uart_1.idx_r],recv_uart_1.size);
			memcpy(len,&recv_uart_1.size,sizeof(uint16_t));
			recv_uart_1.idx_r += recv_uart_1.size;
			recv_uart_1.size -= recv_uart_1.size;
			ret = 1;
			if(recv_uart_1.idx_r > MAX_BUFF_UART1){
				//HAL_UART_Transmit(&huart1,(uint8_t*)"Uart1 Read Over flow\n",16,100);
				print_debug(0,"[ERROR]{Uart1}Over flow\n");
				recv_uart_1.idx_w = 0;
				recv_uart_1.idx_r = 0;
				recv_uart_1.size = 0;
				ret = 0;
			}					
		}

	}
	return ret;
}

int8_t uart1_write(uint8_t *value,uint16_t len){
	int8_t ret = 0;
	HAL_UART_Transmit(&huart1,value,len,100);
	
	ret = 1;
	return ret;
}

void uart1_flush(){
	recv_uart_1.idx_w = 0;
	recv_uart_1.idx_r = 0;
	recv_uart_1.size = 0;
}