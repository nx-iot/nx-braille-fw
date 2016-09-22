#include "uart2_manag.h"

extern UART_HandleTypeDef huart2;
extern recv_uart_t recv_uart_2;
uint8_t uart_2_buff[5];

void uart2_callback(){
	if(HAL_UART_Receive_IT(&huart2,uart_2_buff,1)==HAL_OK){
		if(recv_uart_2.size < MAX_BUFF_UART2){
			recv_uart_2.value[recv_uart_2.idx_w++] = uart_2_buff[0];
			recv_uart_2.size++;
		}else{
			HAL_UART_Transmit(&huart2,(uint8_t*)"Uart2 Over flow\n",16,100);
			recv_uart_2.idx_w = 0;
			recv_uart_2.idx_r = 0;
			recv_uart_2.size = 0;
		}
		
		//////////////////////////////
		//HAL_UART_Transmit(&huart1,uart_2_buff,1,10);
	}
}

int8_t uart2_read(uint8_t *value,uint16_t *len,uint16_t size){
	int8_t ret = 0;
	
	if(recv_uart_2.size>0){
		if((size > 0) && (size <= recv_uart_2.size)){
			memcpy(value,&recv_uart_2.value[recv_uart_2.idx_r],size);
			memcpy(len,&recv_uart_2.size,sizeof(uint16_t));
			recv_uart_2.idx_r += recv_uart_2.size;
			recv_uart_2.size -= recv_uart_2.size;
			ret = 1;
			if(recv_uart_2.idx_r > MAX_BUFF_UART2){
				HAL_UART_Transmit(&huart2,(uint8_t*)"Uart2 Read Over flow\n",16,100);
				recv_uart_2.idx_w = 0;
				recv_uart_2.idx_r = 0;
				recv_uart_2.size = 0;
				ret = 0;
			}
		}else{
			//HAL_UART_Transmit(&huart2,&recv_uart_2.value[recv_uart_2.idx_r],recv_uart_2.size,10);
			memcpy(value,&recv_uart_2.value[recv_uart_2.idx_r],recv_uart_2.size);
			memcpy(len,&recv_uart_2.size,sizeof(uint16_t));
			recv_uart_2.idx_r += recv_uart_2.size;
			recv_uart_2.size -= recv_uart_2.size;
			ret = 1;
			if(recv_uart_2.idx_r > MAX_BUFF_UART2){
				HAL_UART_Transmit(&huart2,(uint8_t*)"Uart2 Read Over flow\n",16,100);
				recv_uart_2.idx_w = 0;
				recv_uart_2.idx_r = 0;
				recv_uart_2.size = 0;
				ret = 0;
			}					
		}

	}
	return ret;
}

int8_t uart2_write(uint8_t *value,uint16_t len){
	int8_t ret = 0;
	HAL_UART_Transmit(&huart2,value,len,100);
	
	ret = 1;
	return ret;
}