#include <stdio.h>

void PrintNumber(int n)
{
    printf("%d\n", n);
}

void PrintString(const char* s)
{
    printf("%s\n", s);
}

typedef struct
{
    char* data;
    int length;
} string;
