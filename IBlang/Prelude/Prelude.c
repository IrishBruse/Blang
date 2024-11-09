#include <stdio.h>

typedef struct string
{
    char* data;
    int length;
} string;

void Print__int(int n)
{
    printf("%d\n", n);
}

void Print__string(string s)
{
    printf("%.*s\n", s.length, s.data);
}
