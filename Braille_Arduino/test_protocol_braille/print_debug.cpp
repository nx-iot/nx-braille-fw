#include "print_debug.h"

PrintDebug debug;
PrintDebug::PrintDebug(){}

#define PRINT_DEBUG_8_MAX     8
#define PRINT_DEBUG_16_MAX    16
#define PRINT_DEBUG_32_MAX    32
#define PRINT_DEBUG_64_MAX    64

uint8_t print_debug_mode = 1;
char buf[PRINT_DEBUG_8_MAX];

void PrintDebug::print(uint8_t level, char *fmtstr, ...) {
  
  
  uint8_t i = 0;



  va_list argptr; 

if(print_debug_mode == 1){
  va_start(argptr, fmtstr);
  //vsprintf(buf,fmtstr,argptr);
  int siz = vsnprintf(buf,sizeof(buf),fmtstr,argptr);
  if(siz > 0){
    Serial.write((byte *)&buf,siz);
  }else{
    Serial.println("[ERR]print is fail.");
  }
//  Serial.write((byte *)&buff,strlen(buff));
  va_end(argptr); 


//  int siz = vsnprintf(buf, sizeof(buf), fmtstr, args);
//  Serial.write((byte *)&buf,siz);
//  va_end(argptr); 
}
  return;
}

void PrintDebug::print_payload(uint8_t level, const uint8_t * buff, const uint16_t len){
  
  
  int len_rem = len;
  int line_width = 16;            // number of bytes per line //
  int line_len;
  int offset = 0;                    // zero-based offset counter //
  const unsigned char *ch = buff;

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
  debug.print(0,"%05d   ", offset);               
  
  
  // hex //                                                                                                      
  ch = payload;
  for(i = 0; i < len; i++) {                            
      debug.print(0,"%02x ", *ch);                    
      
      ch++;
      // print extra space after 8th byte for visual aid //
      if (i == 7){                            
          debug.print(0," ");                                     
          
      }
  }
  // print space to handle line less than 8 bytes //
  if (len < 8){                            
      debug.print(0," ");                                              
      
  }
  
  // fill hex gap with spaces if not full line //
  if (len < 16) {
      gap = 16 - len;
      for (i = 0; i < gap; i++) {
          debug.print(0,"   ");                                           
          
      }
  }
  debug.print(0,"   ");                                                      
  
  
  // ascii (if printable) //
  ch = payload;
  for(i = 0; i < len; i++) {
      if (isprint(*ch)){
          debug.print(0,"%c", *ch);                                           
          
      }
      else{
          debug.print(0,".");                                                 
          
      }
      ch++;
  }

  debug.print(0,"\r\n");                                                        
    

  return;
}
