#include "print_debug.h"


UART_HandleTypeDef *huart_intstanc;

void print_init(UART_HandleTypeDef *huart){
	huart_intstanc = huart;
}

/**
  * @brief  print_debug
  * @param  uint8_t level, char *fmtstr
  */
void print_debug(uint8_t level, char *fmtstr, ...) {
	
	char buff[256];
	uint8_t i = 0;
	va_list argptr; 

	va_start(argptr, fmtstr);
	vsprintf(buff,fmtstr,argptr);
	HAL_UART_Transmit(huart_intstanc,(uint8_t *)&buff,strlen(buff),10);
	va_end(argptr); 
	
	return;
}


/**
  * @brief  print_payload
  * @param  uint8_t level, const uint8_t * buff, const uint16_t len
  */
void print_payload(uint8_t level, const uint8_t * buff, const uint16_t len){
	
	
	int len_rem = len;
	int line_width = 16;            // number of bytes per line //
	int line_len;
	int offset = 0;                    // zero-based offset counter //
	const unsigned char *ch = buff;
	
	if(len>512){
		print_debug(0,"payload len (over max).\n");
		return;
	}

	if (len <= 0){
		return;
	}

	// data fits on one line //
	if (len <= line_width) {
			print_hex_ascii_line(ch, len, offset);
			return;
	}
	// data spans multiple lines //
	for ( ;; ) {
			// compute current line length //
			line_len = line_width % len_rem;
			// print line //
			print_hex_ascii_line(ch, line_len, offset);
			// compute total remaining //
			len_rem = len_rem - line_len;
			// shift pointer to remaining bytes to print //
			ch = ch + line_len;
			// add offset //
			offset = offset + line_width;
			// check if we have line width chars or less //
			if (len_rem <= line_width) {
					// print last line and get out //
					print_hex_ascii_line(ch, len_rem, offset);
					break;
			}
	}
	return;
}

static void print_hex_ascii_line(const unsigned char *payload, int len, int offset){

	int i;
	int gap;
	const unsigned char *ch;

	// offset //                      
	print_debug(0,"%05d   ", offset);               
	
	
	// hex //                                                                                                      
	ch = payload;
	for(i = 0; i < len; i++) {                            
			print_debug(0,"%02x ", *ch);                    
			
			ch++;
			// print extra space after 8th byte for visual aid //
			if (i == 7){                            
					print_debug(0," ");                                     
					
			}
	}
	// print space to handle line less than 8 bytes //
	if (len < 8){                            
			print_debug(0," ");                                              
			
	}
	
	// fill hex gap with spaces if not full line //
	if (len < 16) {
			gap = 16 - len;
			for (i = 0; i < gap; i++) {
					print_debug(0,"   ");                                           
					
			}
	}
	print_debug(0,"   ");                                                      
	
	
	// ascii (if printable) //
	ch = payload;
	for(i = 0; i < len; i++) {
			if (isprint(*ch)){
					print_debug(0,"%c", *ch);                                           
					
			}
			else{
					print_debug(0,".");                                                 
					
			}
			ch++;
	}

	print_debug(0,"\r\n");                                                        
    

	return;
}