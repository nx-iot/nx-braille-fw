#include "braille_process.h"

// ******************************** Braille_status ******************************** 
int Braille_status(char buf[],int len){
		uint8_t delay;   
    int res, i;                                          
    delay = buf[0]; 
}
//*********************************************************************************
// ******************************** Braille_config ******************************** 
int Braille_config(char buf[],int len){
		
}
//*********************************************************************************
// ******************************** Braille_data ********************************
int Braille_data(char buf[],int len){
		int Num[8] = {0x01, 0x02, 0x04, 0x08, 0x10, 0x20, 0x40, 0x80};
		int order[24];
		uint8_t delay;   
    int res,j,i ,n,m,seg,chk; 
		print_payload(0,(uint8_t*)buf, len); 
    //delay = buf[1];
		n=0;
		m=8;
				for(seg=1;seg<=3;seg++){
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
				print_debug(0,"Value \n");
				HAL_GPIO_WritePin(latch_pin_GPIO_Port, latch_pin_Pin, 0);
				for(i=23;i>=0;i--){ //15(2)
					print_debug(0,"%d ",order[i]);
					HAL_GPIO_WritePin(GPIOC, GPIO_PIN_3,order[i]);//data
					HAL_GPIO_WritePin(GPIOC, clock_Pin, 1); //clock
					HAL_Delay(1);
					HAL_GPIO_WritePin(GPIOC, clock_Pin, 0); //clock
					HAL_Delay(1);
				}
				print_debug(0,"\n");			
				HAL_GPIO_WritePin(latch_pin_GPIO_Port, latch_pin_Pin, 1);
				print_debug(0,"Compleate \n");	
				
				HAL_Delay(3000);
				
				HAL_GPIO_WritePin(latch_pin_GPIO_Port, latch_pin_Pin, 0);
				for(i=23;i>=0;i--){ //15(2)
					print_debug(0,"%d ",order[i]);
					HAL_GPIO_WritePin(GPIOC, GPIO_PIN_3,0);//data
					HAL_GPIO_WritePin(GPIOC, clock_Pin, 1); //clock
					HAL_Delay(1);
					HAL_GPIO_WritePin(GPIOC, clock_Pin, 0); //clock
					HAL_Delay(1);
				}
				print_debug(0,"\n");			
				HAL_GPIO_WritePin(latch_pin_GPIO_Port, latch_pin_Pin, 1);
				print_debug(0,"Clear \n");
}
//*********************************************************************************
// ******************************** Braille_event ********************************
int Braille_event(char buf[],int len){
		
}
//*********************************************************************************
