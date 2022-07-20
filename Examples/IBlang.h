#include <stdio.h>

void Print(char* str);

#ifdef IBLANG_IMPLEMENTATION
void Print(char* str)
{
    printf("%s", str);
}
#endif
