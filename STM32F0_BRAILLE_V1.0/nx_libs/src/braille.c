#include "Braille.h"

int8_t Braille_read(uint8_t *value, uint16_t *len){
	uint16_t lenn = 0;
	int8_t ret = 0;
	ret = uart1_read(value,len,255);
	memcpy(&lenn,len,sizeof(uint16_t));
	uart1_flush();
	return ret;
}

int8_t Braille_write(uint8_t *value, uint16_t len){
	int8_t ret = 0;
	//print_payload(0,value,len);
	ret = uart1_write(value, len);
	return ret;
}

void Braille_receivePacket(void) {
		uint8_t recvPacket[256];           
    int ret = 0;           
            
    // -- for packet validation -- //
    int offset = 0;
    uint16_t size = 0x0000;
		uint16_t len = 0x0000;
    int total = 0;
    
    // --------------- Process Braille Data ---------------- // UART0
      
    ret = Braille_read(recvPacket,&size);    
    print_debug(0,"Size %d -------\r\n",size);  
		if(size <= 0 && len <=0) {
        return;
		}
		 if(recvPacket[0] != 0x7E){
			 //recvPacket[0] = 0x7E;
			 return;
		 }
     print_debug(0,"rawpacket\r\n");
            print_debug(0,"\r\nBraille Receive Packet ---\r\n");   
            print_payload(0,recvPacket, size);  
            print_debug(0,"-------\r\n"); 
	
		 while(total < size){
        offset += strcspn((char*)&recvPacket[offset], "\x7E");                                   // seach for header
        if(offset == size){                 
            //printDebug("invalid coordinator packet");                                   // not found Start Delimiter 0x7E
            break;
        }
        len = (recvPacket[offset+1] & 0xffff) << 8;                                       // check packet length (MSB)
        len |= recvPacket[offset+2];                                                      // check packet length (LSB)
        total += len;                                                                     // update total analyse
        if (total > size){                                                                //check length validation
            offset++;
            total = offset;                                                               // roll back total analyse                          
            print_debug(0,"\r\nXBEE> invalid length!!\r\n");
					//break;
            continue;
        } 
				if(Braille_checksum((char*)&recvPacket[offset+3], len) == (char)recvPacket[offset+3+len]){        // checksum error detection                   
						print_debug(0,"checksum correct\r\n");
            print_debug(0,"\r\nBraille Receive Packet ---\r\n");   
            print_payload(0,&recvPacket[offset+3], len);  
            print_debug(0,"-------\r\n"); 			
            //xbee_processPacket((char*)&recvPacket[offset+3], len);                               // analyse API-specific Structure 
            offset += 3+len;
        }
        else{                                                                             // got a valid packet 
            print_debug(0,"XBEE> checksum error\r\n");
            offset++;
            total = offset;                                                               // roll back total analyse 
        }
	}
}
int Braille_checksum(char buf[],int len) {

    int i;
    char sum = 0;                                          
    //print_payload(buf,len);
    for (i = 0; i < len; i++) {
        sum += buf[i];
    }                 
    return (0xff - (sum & 0xff));

}