#include "Braille.h"
#include "braille_process.h"

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
		HAL_Delay(10);
    //print_debug(0,"Size %d -------\r\n",size);  
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
        //len = (recvPacket[offset+1] & 0xffff) << 8;                                       // check packet length (MSB)
        len |= recvPacket[offset+1];                                                      // check packet length (LSB)
        total += len;                                                                     // update total analyse
        if (total > size){                                                                //check length validation
            offset++;
            total = offset;                                                               // roll back total analyse                          
            print_debug(0,"\r\nXBEE> invalid length!!\r\n");
					//break;
            continue;
        } 
//				if(Braille_checksum((char*)&recvPacket[offset+2], len) == (char)recvPacket[offset+2+len]){        // checksum error detection                   
//						print_debug(0,"checksum correct\r\n");
//            print_debug(0,"\r\nBraille Receive Packet ---\r\n");   
//            print_payload(0,&recvPacket[offset+2], len);  
//            print_debug(0,"-------\r\n"); 			    
//					  Braille_processPacket((char*)&recvPacket[offset+2], len);                               // analyse API-specific Structure 
//            offset += 2+len;
//        }
//        else{                                                                             // got a valid packet 
//            print_debug(0,"XBEE> checksum error\r\n");
//            offset++;
//            total = offset;                                                               // roll back total analyse 
//        }
				Braille_processPacket((char*)&recvPacket[offset+2], len);
				return;
				//offset += 2+len;
	}
}
int Braille_processPacket(char *buf, int len) {
		uint8_t frameType;   
    int res, i;                                          
    frameType = buf[0];            
    
    switch(frameType) { 
       
        case status:                                                                       // AT Command Response
            if(len < 1) {                                    
                // shoudn't reach here since checksum valid
                return -1;
            }  
            print_debug(0,"Status : 0x%02X\r\n", status); 
            //res = xbee_processATCMR(buf, len);
            break;  
         case config:                                                                       // AT Command Response
            if(len < 1) {                                    
                // shoudn't reach here since checksum valid
                return -1;
            }  
            print_debug(0,"Config : 0x%02X\r\n", config); 
            //res = xbee_processATCMR(buf, len);
            break;  
          case data:                                                                       // AT Command Response
            if(len < 1) {                                    
                // shoudn't reach here since checksum valid
                return -1;
            }  
            print_debug(0,"Data : 0x%02X\r\n", data);            						
						res = Braille_data(buf, len);
            break; 
					case event:                                                                       // AT Command Response
            if(len < 1) {                                    
                // shoudn't reach here since checksum valid
                return -1;
            }  
            print_debug(0,"Event : 0x%02X\r\n", event); 
            //res = xbee_processATCMR(buf, len);
            break; 						
        default:                      
            print_debug(0,"\r\nUnknown XBee Frame Type ( %02x )!!\r\n", frameType);                
            return -1;
            break;  
        
    }     
    return res; 
}

int Braille_sendAPI(char *frame, int lenght, int timeout) {       
                                                 
    uint8_t sendPacket[256];                                                                    
    int packetLen = 2+lenght+1+1;
		int cmd = data;
    //char frameID = frame[1];
    //int res;
                              
    
    sendPacket[0] = 0x7E;                                                               // Start Delimeter
    //sendPacket[1] = (lenght >> 8) & 0xFF;                                               // Braille Packet Length  - msb
    sendPacket[1] = 0x07;                                                      //                     - lsb
		sendPacket[2] = cmd;  
    memcpy(&sendPacket[3], frame, lenght);
    sendPacket[packetLen-1] = Braille_checksum((char*)&sendPacket[2],lenght+1);
		//sendPacket[packetLen-1] = 0xef;
    
    /*
    if(timeout > 0) {
        res = xbee_addWaitQueue(frameID, &sendPacket[0], packetLen, timeout);    
        if(res < 0) {               
            free(sendPacket); 
            return -1;
        }               
    }
    */
     
    print_debug(0,"\r\n----------- Send ------------\r\n");
    print_payload(0,sendPacket, packetLen);
    print_debug(0,"-----------------------------\r\n");
    
    Braille_write(sendPacket, packetLen);
		//uart2_write(sendPacket, packetLen);
    return 0;
    
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
