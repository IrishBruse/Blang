#include <stdio.h>

void PrintString(char *val);
void PrintNumber(int val);

#ifdef IBLANG_IMPLEMENTATION
void PrintString(char *val) { printf("%s", val); }
void PrintNumber(int val) { printf("%d", val); }
#endif
