#ifndef PRINT_DEBUG_H
#define PRINT_DEBUG_H

//#include "main.h"
#include <Arduino.h>

#ifdef __cplusplus
extern "C"{
#endif

#include <stdio.h>
#include <string.h>
#include <stdint.h>
#include <stdarg.h>
#include <ctype.h>

static void print_hex_ascii_line(const unsigned char *payload, int len, int offset);

//va_start(argptr, fmtstr);
//extern void va_start (va_list ap, last);
  //int   vsprintf (char *__s, const char *__fmt, va_list ap);

#ifdef __cplusplus
} // extern "C"
#endif

class PrintDebug{
  public:
  PrintDebug();
  void print(uint8_t level, char *fmtstr, ...);
  void print_payload(uint8_t level, const uint8_t * buff, const uint16_t len);
};

extern PrintDebug debug;
extern uint8_t print_debug_mode;
#endif 
